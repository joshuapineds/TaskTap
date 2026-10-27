using System.Globalization;
using System.Text.Json;
using TaskTap.Data;

namespace TaskTap;

/// <summary>One place returned by the OpenStreetMap Nominatim search.</summary>
public sealed record GeoResult(string Name, double Latitude, double Longitude)
{
    public override string ToString() => Name;
}

/// <summary>
/// Location helpers for the map features: barangay centres (geocoded once through OpenStreetMap
/// Nominatim and cached in MySQL), address search for the location picker, and the privacy offset
/// that keeps a task's exact pin from ever being shown to other members.
/// </summary>
public static class Geo
{
    /// <summary>Radius of the approximate area shown for a task that has an exact pin.</summary>
    public const int PinnedAreaMeters = 300;

    /// <summary>Radius of the area shown when only the barangay is known.</summary>
    public const int BarangayAreaMeters = 700;

    private static readonly Dictionary<string, (double Lat, double Lng)> Barangays = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> Requested = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object Gate = new();
    private static readonly SemaphoreSlim Throttle = new(1, 1);
    private static DateTime _lastRequest = DateTime.MinValue;
    private static bool _workerRunning;
    private static HttpClient? _http;
    private static MapSettings _settings = new();

    /// <summary>Raised (on a background thread) when new barangay coordinates become known.</summary>
    public static event Action? CoordinatesUpdated;

    internal static MapSettings Settings => _settings;

    public static (double Lat, double Lng) DefaultCenter => (_settings.DefaultLatitude, _settings.DefaultLongitude);

    /// <summary>Reads settings and cached barangay coordinates. Call once after the database is ready.</summary>
    internal static void Initialize(AppConfig config)
    {
        _settings = config.Maps;
        try
        {
            var cached = Backend.LoadBarangayCoordinates();
            lock (Gate)
            {
                foreach (var pair in cached)
                    Barangays[pair.Key] = pair.Value;
            }
        }
        catch
        {
            // The map still works without cached coordinates (it falls back to the city centre).
        }
    }

    private static HttpClient Http
    {
        get
        {
            if (_http is not null)
                return _http;
            var contact = string.IsNullOrWhiteSpace(_settings.ContactEmail) ? "school project" : _settings.ContactEmail.Trim();
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"TaskTap/1.0 ({contact})");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
            return _http = client;
        }
    }

    // ----- Coordinates ----------------------------------------------------------

    public static bool TryGetBarangay(string barangay, out (double Lat, double Lng) center)
    {
        lock (Gate)
            return Barangays.TryGetValue(Backend.BarangayNumber(barangay), out center);
    }

    /// <summary>Exact pin when the poster set one, otherwise the barangay centre, otherwise the city centre.</summary>
    public static (double Lat, double Lng, bool Exact) TaskPoint(double? lat, double? lng, string barangay)
    {
        if (lat is { } la && lng is { } ln)
            return (la, ln, true);
        if (TryGetBarangay(barangay, out var center))
            return (center.Lat, center.Lng, false);
        return (_settings.DefaultLatitude, _settings.DefaultLongitude, false);
    }

    public static int AreaRadius(bool exact) => exact ? PinnedAreaMeters : BarangayAreaMeters;

    /// <summary>
    /// The point actually drawn on a map. It is shifted by a stable, task-specific offset (at most 55% of the
    /// area radius) so the true location always lies inside the shown area but is never revealed.
    /// </summary>
    public static (double Lat, double Lng, int Radius) DisplayPoint(int taskId, string barangay, double? lat, double? lng)
    {
        var (baseLat, baseLng, exact) = TaskPoint(lat, lng, barangay);
        var radius = AreaRadius(exact);
        var seed = unchecked(taskId * 7919 + (exact ? 17 : 31));
        var random = new Random(seed);
        var angle = random.NextDouble() * Math.PI * 2;
        var distance = radius * 0.55 * Math.Sqrt(random.NextDouble());
        var dLat = distance * Math.Cos(angle) / 111_320.0;
        var dLng = distance * Math.Sin(angle) / (111_320.0 * Math.Cos(baseLat * Math.PI / 180.0));
        return (baseLat + dLat, baseLng + dLng, radius);
    }

    // ----- Nominatim -------------------------------------------------------------

    private static async Task WaitForTurnAsync(CancellationToken cancellation)
    {
        // Nominatim's usage policy allows at most one request per second.
        await Throttle.WaitAsync(cancellation);
        try
        {
            var wait = _lastRequest.AddMilliseconds(1100) - DateTime.UtcNow;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, cancellation);
            _lastRequest = DateTime.UtcNow;
        }
        finally
        {
            Throttle.Release();
        }
    }

    /// <summary>Searches places in the Philippines (biased to Caloocan). Only call this on an explicit user action.</summary>
    public static async Task<List<GeoResult>> SearchAsync(string query, CancellationToken cancellation = default)
    {
        query = query.Trim();
        if (query.Length < 3)
            return [];

        await WaitForTurnAsync(cancellation);
        var url = "https://nominatim.openstreetmap.org/search?format=jsonv2&limit=5&countrycodes=ph" +
                  "&viewbox=120.93,14.80,121.09,14.62&q=" + Uri.EscapeDataString(query);
        using var response = await Http.GetAsync(url, cancellation);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellation);

        var results = new List<GeoResult>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("lat", out var latElement) || !item.TryGetProperty("lon", out var lonElement))
                continue;
            if (!double.TryParse(latElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(lonElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
                continue;
            var name = item.TryGetProperty("display_name", out var nameElement) ? nameElement.GetString() ?? "" : "";
            results.Add(new GeoResult(name.Length > 90 ? name[..87] + "..." : name, lat, lon));
        }
        return results;
    }

    private static bool InsideCaloocanArea(double lat, double lng) => lat is > 14.55 and < 14.90 && lng is > 120.88 and < 121.17;

    /// <summary>
    /// Looks up the centre of each barangay once (in the background, one request per second),
    /// stores it in MySQL, and raises <see cref="CoordinatesUpdated"/> so open maps redraw.
    /// </summary>
    public static void RequestBarangays(IEnumerable<string> barangays)
    {
        var missing = new List<string>();
        lock (Gate)
        {
            foreach (var number in barangays.Select(Backend.BarangayNumber).Where(number => number.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Barangays.ContainsKey(number) && Requested.Add(number))
                    missing.Add(number);
            }
        }
        if (missing.Count == 0)
            return;

        lock (Gate)
        {
            foreach (var number in missing)
                PendingLookups.Enqueue(number);
            if (_workerRunning)
                return;
            _workerRunning = true;
        }

        _ = Task.Run(ResolvePendingAsync);
    }

    private static readonly Queue<string> PendingLookups = new();

    private static async Task ResolvePendingAsync()
    {
        try
        {
            while (true)
            {
                string? next;
                lock (Gate)
                {
                    if (!PendingLookups.TryDequeue(out next))
                    {
                        _workerRunning = false;
                        return;
                    }
                }
                var number = next;

                try
                {
                    var results = await SearchAsync($"Barangay {number}, Caloocan, Metro Manila");
                    var hit = results.FirstOrDefault(result => InsideCaloocanArea(result.Latitude, result.Longitude));
                    if (hit is null)
                        continue;
                    lock (Gate)
                        Barangays[number] = (hit.Latitude, hit.Longitude);
                    try { Backend.SaveBarangayCoordinates(number, hit.Latitude, hit.Longitude); } catch { /* cache only */ }
                    CoordinatesUpdated?.Invoke();
                }
                catch
                {
                    // Offline or rate limited: keep the fallback position and try again next session.
                }
            }
        }
        catch
        {
            lock (Gate)
                _workerRunning = false;
        }
    }
}
