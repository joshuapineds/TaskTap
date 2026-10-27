using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TaskTap;

/// <summary>What a map needs to know about a task. Exact coordinates are never drawn; see <see cref="Geo.DisplayPoint"/>.</summary>
public sealed record MapTask(int Id, string Title, string Subtitle, string Barangay, double? Latitude, double? Longitude, string Color)
{
    public static MapTask From(DemoTask task) => new(
        task.Id, task.Title, $"₱{task.Price:N0} · {task.Category}", task.Barangay, task.Latitude, task.Longitude, ColorFor(task.Category));

    public static string ColorFor(string category) => category switch
    {
        "Repairs" => "#0875e8",
        "Cleaning" => "#13a673",
        "Delivery" or "Errands" => "#f59c2d",
        "Tutoring" => "#7b61d1",
        "Pet Care" => "#e5536e",
        _ => "#0875e8"
    };
}

/// <summary>
/// An OpenStreetMap map (Leaflet running inside WebView2). If the WebView2 runtime is missing or the
/// page fails to load, <see cref="Unavailable"/> is raised so the host can keep its static fallback.
/// Call <see cref="Suspend"/> when the page is hidden and <see cref="Resume"/> when it returns, so only
/// the visible page keeps a browser running.
/// </summary>
public sealed class MapView : Panel
{
    private const string VirtualHost = "tasktap.map";
    private static Task<CoreWebView2Environment>? _environment;

    private WebView2 _web;
    private readonly Label _status = new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.FromArgb(91, 111, 134),
        BackColor = Color.FromArgb(238, 243, 248),
        Font = new Font("Segoe UI", 9F),
        Text = "Loading map…"
    };

    // The current content, re-applied whenever the browser (re)starts or barangay coordinates arrive.
    private Action? _scene;
    private string? _pickScript;
    private string? _viewScript;
    private string? _highlightScript;

    private bool _started;
    private bool _ready;
    private bool _failed;
    private bool _suspended;

    public MapView()
    {
        BackColor = Color.FromArgb(233, 242, 231);
        _web = CreateWeb();
        Controls.Add(_web);
        Controls.Add(_status);
        _status.BringToFront();
        Geo.CoordinatesUpdated += OnCoordinatesUpdated;
    }

    public bool IsReady => _ready;
    public bool IsFailed => _failed;

    /// <summary>Raised once when the map cannot be shown (no WebView2 runtime, page failed to load...).</summary>
    public event Action<string>? Unavailable;

    public event Action? Ready;

    /// <summary>A marker was clicked.</summary>
    public event Action<int>? MarkerSelected;

    /// <summary>"View details" was pressed in a marker popup.</summary>
    public event Action<int>? MarkerOpened;

    /// <summary>The user clicked the map while picking a location.</summary>
    public event Action<double, double>? LocationPicked;

    private static WebView2 CreateWeb() => new() { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.FromArgb(233, 242, 231) };

    // ----- Public API ------------------------------------------------------------

    /// <summary>Shows tasks as approximate markers.</summary>
    public void ShowTasks(IReadOnlyList<MapTask> tasks, bool fit = true)
    {
        Geo.RequestBarangays(tasks.Where(task => task.Latitude is null).Select(task => task.Barangay));
        _highlightScript = null;
        SetScene(() =>
        {
            var markers = tasks.Select(task =>
            {
                var point = Geo.DisplayPoint(task.Id, task.Barangay, task.Latitude, task.Longitude);
                return new { id = task.Id, lat = point.Lat, lng = point.Lng, title = task.Title, subtitle = task.Subtitle, color = task.Color };
            });
            Execute($"TT.setMarkers({JsonSerializer.Serialize(markers)}, {(fit ? "true" : "false")})");
        });
    }

    /// <summary>Shows one task as an approximate circle (never the exact pin).</summary>
    public void ShowArea(MapTask task)
    {
        if (task.Latitude is null)
            Geo.RequestBarangays([task.Barangay]);
        SetScene(() =>
        {
            var point = Geo.DisplayPoint(task.Id, task.Barangay, task.Latitude, task.Longitude);
            var label = task.Barangay.Length > 0 ? $"Brgy. {task.Barangay}" : "Approximate area";
            Execute($"TT.showArea({JsonSerializer.Serialize(new { lat = point.Lat, lng = point.Lng, radius = point.Radius, label })})");
        });
    }

    public void Highlight(int taskId)
    {
        _highlightScript = $"TT.highlight({taskId}, false)";
        Execute(_highlightScript);
    }

    public void FitAll() => Execute("TT.fitAll()");

    public void FlyTo(double lat, double lng, int zoom = 16)
    {
        _viewScript = FormattableString.Invariant($"TT.setView({lat}, {lng}, {zoom})");
        Execute(_viewScript);
    }

    /// <summary>Turns on click-to-pick; an existing pin can be shown as the starting point.</summary>
    public void BeginPicking(double? lat, double? lng) =>
        SetScene(() => Execute(lat is { } la && lng is { } ln
            ? FormattableString.Invariant($"TT.setPicker(true, {la}, {ln})")
            : "TT.setPicker(true)"));

    public void SetPicked(double lat, double lng)
    {
        _pickScript = FormattableString.Invariant($"TT.setPicked({lat}, {lng}, 16)");
        Execute(_pickScript);
    }

    /// <summary>Stops the browser (frees its memory) while the page is not on screen.</summary>
    public void Suspend()
    {
        if (_suspended || _failed || !_started)
            return;
        _suspended = true;
        _ready = false;
        _started = false;
        Controls.Remove(_web);
        _web.Dispose();
        _status.Text = "Loading map…";
        _status.Visible = true;
        _status.BringToFront();
    }

    /// <summary>Restarts the browser after <see cref="Suspend"/> and redraws the current content.</summary>
    public void Resume()
    {
        if (!_suspended || _failed || IsDisposed)
            return;
        _suspended = false;
        _web = CreateWeb();
        Controls.Add(_web);
        _status.BringToFront();
        _started = true;
        _ = InitializeAsync(_web);
    }

    // ----- WebView2 plumbing -----------------------------------------------------

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (_started || _suspended)
            return;
        _started = true;
        _ = InitializeAsync(_web);
    }

    private async Task InitializeAsync(WebView2 web)
    {
        try
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "assets", "map");
            if (!File.Exists(Path.Combine(folder, "map.html")))
                throw new FileNotFoundException("assets/map/map.html is missing next to TaskTap.exe.");

            _environment ??= CoreWebView2Environment.CreateAsync(
                null, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskTap", "WebView2"));
            var environment = await _environment;
            await web.EnsureCoreWebView2Async(environment);
            if (IsDisposed || !ReferenceEquals(web, _web))
                return;

            var core = web.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.SetVirtualHostNameToFolderMapping(VirtualHost, folder, CoreWebView2HostResourceAccessKind.Allow);
            core.WebMessageReceived += OnWebMessage;
            core.NavigationStarting += (_, args) =>
            {
                if (!args.Uri.StartsWith($"https://{VirtualHost}/", StringComparison.OrdinalIgnoreCase) && args.Uri != "about:blank")
                    args.Cancel = true;
            };
            core.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                OpenExternal(args.Uri);
            };
            core.NavigationCompleted += (_, args) =>
            {
                if (!args.IsSuccess)
                    Fail("The map page could not be loaded.");
            };

            var center = Geo.DefaultCenter;
            var query = FormattableString.Invariant($"?lat={center.Lat}&lng={center.Lng}&zoom={Geo.Settings.DefaultZoom}");
            core.Navigate($"https://{VirtualHost}/map.html{query}");
        }
        catch (Exception ex)
        {
            if (IsDisposed || !ReferenceEquals(web, _web))
                return;
            Fail(ex is WebView2RuntimeNotFoundException
                ? "The Microsoft Edge WebView2 Runtime is not installed."
                : ex.Message);
        }
    }

    private static void OpenExternal(string uri)
    {
        // Only the attribution links (OpenStreetMap / Leaflet) may open, and only in the default browser.
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || parsed.Scheme != Uri.UriSchemeHttps)
            return;
        var host = parsed.Host;
        if (host.EndsWith("openstreetmap.org", StringComparison.OrdinalIgnoreCase) || host.EndsWith("leafletjs.com", StringComparison.OrdinalIgnoreCase))
            Process.Start(new ProcessStartInfo(parsed.AbsoluteUri) { UseShellExecute = true });
    }

    private void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            // The page posts JSON.stringify(...) strings, so the payload arrives as a string.
            using var document = JsonDocument.Parse(args.TryGetWebMessageAsString());
            var root = document.RootElement;
            switch (root.GetProperty("type").GetString())
            {
                case "ready":
                    _ready = true;
                    _status.Visible = false;
                    _scene?.Invoke();
                    foreach (var script in new[] { _viewScript, _pickScript, _highlightScript })
                    {
                        if (script is not null)
                            Execute(script);
                    }
                    Ready?.Invoke();
                    break;
                case "select":
                    MarkerSelected?.Invoke(root.GetProperty("id").GetInt32());
                    break;
                case "open":
                    MarkerOpened?.Invoke(root.GetProperty("id").GetInt32());
                    break;
                case "picked":
                    var lat = root.GetProperty("lat").GetDouble();
                    var lng = root.GetProperty("lng").GetDouble();
                    _pickScript = FormattableString.Invariant($"TT.setPicked({lat}, {lng}, 16)");
                    LocationPicked?.Invoke(lat, lng);
                    break;
                case "error":
                    Fail(root.TryGetProperty("message", out var message) ? message.GetString() ?? "The map failed." : "The map failed.");
                    break;
            }
        }
        catch (Exception)
        {
            // Ignore malformed messages.
        }
    }

    private void Fail(string reason)
    {
        if (_failed || IsDisposed)
            return;
        _failed = true;
        _status.Text = "The interactive map is unavailable.\r\n" + reason;
        _status.Visible = true;
        _status.BringToFront();
        Unavailable?.Invoke(reason);
    }

    private void SetScene(Action scene)
    {
        _scene = scene;
        if (_ready)
            scene();
    }

    private void Execute(string script)
    {
        if (!_ready || _failed || IsDisposed)
            return;
        _ = _web.CoreWebView2.ExecuteScriptAsync(script);
    }

    private void OnCoordinatesUpdated()
    {
        if (IsDisposed || !IsHandleCreated)
            return;
        try
        {
            BeginInvoke(() =>
            {
                if (_ready)
                    _scene?.Invoke();
            });
        }
        catch (InvalidOperationException)
        {
            // Closing.
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Geo.CoordinatesUpdated -= OnCoordinatesUpdated;
            _web.Dispose();
        }
        base.Dispose(disposing);
    }
}
