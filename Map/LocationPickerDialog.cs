namespace TaskTap;

/// <summary>Lets a poster drop a pin on the map or search for a place. Returns the chosen coordinates.</summary>
public sealed class LocationPickerDialog : Form
{
    private readonly MapView _map = new() { Dock = DockStyle.Fill };
    private readonly TextBox _query = new() { PlaceholderText = "Search a street, landmark or place in Caloocan" };
    private readonly Button _search = new() { Text = "Search" };
    private readonly ListBox _results = new() { Visible = false, IntegralHeight = false, Height = 92 };
    private readonly Label _hint = new() { AutoSize = false, ForeColor = Color.FromArgb(91, 111, 134) };
    private readonly Button _ok = new() { Text = "Use this location", Enabled = false };
    private readonly Button _cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };
    private CancellationTokenSource? _searchCancellation;

    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private LocationPickerDialog(double? lat, double? lng, string barangay)
    {
        Text = "Pin the task location";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 560);
        MinimumSize = new Size(640, 480);
        MinimizeBox = false;
        MaximizeBox = true;
        ShowInTaskbar = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        var top = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 8, 12, 4) };
        _search.Dock = DockStyle.Right;
        _search.Width = 90;
        _query.Dock = DockStyle.Fill;
        top.Controls.Add(_query);
        top.Controls.Add(_search);

        _results.Dock = DockStyle.Top;

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(12, 8, 12, 8) };
        _hint.Dock = DockStyle.Fill;
        _hint.Text = "Click the map to place the pin. Other members only see an approximate area (about 300 m), never your exact pin.";
        _ok.Dock = DockStyle.Right;
        _ok.Width = 150;
        _ok.BackColor = Color.FromArgb(8, 117, 232);
        _ok.ForeColor = Color.White;
        _ok.FlatStyle = FlatStyle.Flat;
        _ok.FlatAppearance.BorderSize = 0;
        _cancel.Dock = DockStyle.Right;
        _cancel.Width = 90;
        bottom.Controls.Add(_hint);
        bottom.Controls.Add(_cancel);
        bottom.Controls.Add(_ok);

        Controls.Add(_map);
        Controls.Add(_results);
        Controls.Add(top);
        Controls.Add(bottom);
        AcceptButton = _search;
        CancelButton = _cancel;

        _search.Click += async (_, _) => await RunSearchAsync();
        _results.SelectedIndexChanged += (_, _) =>
        {
            if (_results.SelectedItem is GeoResult result)
                Choose(result.Latitude, result.Longitude, fly: true);
        };
        _ok.Click += (_, _) => DialogResult = DialogResult.OK;
        _map.LocationPicked += (la, ln) => Choose(la, ln, fly: false);
        _map.Unavailable += reason => _hint.Text = "The map could not load: " + reason;
        _map.Ready += () =>
        {
            _map.BeginPicking(lat, lng);
            if (lat is null)
            {
                if (Geo.TryGetBarangay(barangay, out var center))
                    _map.FlyTo(center.Lat, center.Lng, 15);
                else
                    Geo.RequestBarangays([barangay]);
            }
        };

        if (lat is { } la0 && lng is { } ln0)
        {
            Latitude = la0;
            Longitude = ln0;
            _ok.Enabled = true;
        }
    }

    private void Choose(double lat, double lng, bool fly)
    {
        Latitude = lat;
        Longitude = lng;
        _ok.Enabled = true;
        _hint.Text = FormattableString.Invariant($"Pin placed at {lat:0.00000}, {lng:0.00000}. Click elsewhere to move it.");
        if (fly)
            _map.SetPicked(lat, lng);
    }

    private async Task RunSearchAsync()
    {
        var text = _query.Text.Trim();
        if (text.Length < 3)
        {
            _hint.Text = "Type at least 3 characters to search.";
            return;
        }

        _searchCancellation?.Cancel();
        _searchCancellation = new CancellationTokenSource();
        _search.Enabled = false;
        _hint.Text = "Searching OpenStreetMap…";
        try
        {
            var found = await Geo.SearchAsync(text, _searchCancellation.Token);
            _results.Items.Clear();
            foreach (var item in found)
                _results.Items.Add(item);
            _results.Visible = found.Count > 0;
            _hint.Text = found.Count == 0 ? "No places found. Try a street or landmark name, or click the map." : "Choose a result, or click the map to fine-tune the pin.";
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _hint.Text = "Search failed (" + ex.Message + "). You can still click the map.";
        }
        finally
        {
            _search.Enabled = true;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _searchCancellation?.Cancel();
        base.OnFormClosed(e);
    }

    /// <summary>Shows the picker; returns the pin, or null when cancelled.</summary>
    public static (double Lat, double Lng)? Pick(IWin32Window owner, double? lat, double? lng, string barangay)
    {
        using var dialog = new LocationPickerDialog(lat, lng, barangay);
        return dialog.ShowDialog(owner) == DialogResult.OK && dialog.Latitude is { } la && dialog.Longitude is { } ln
            ? (la, ln)
            : null;
    }
}
