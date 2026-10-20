namespace TaskTap;

/// <summary>
/// Simple horizontal bar chart painted with GDI+. Deliberately dependency-free so the
/// demo behaves the same everywhere.
/// </summary>
public sealed class AdminBarChart : Control
{
    private readonly List<(string Label, int Value, Color Color)> _items = new();

    public AdminBarChart()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw,
            true);
        BackColor = Color.White;
    }

    public void SetItems(IReadOnlyList<(string Label, int Value, Color Color)> items)
    {
        _items.Clear();
        _items.AddRange(items);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_items.Count == 0)
        {
            using var emptyFont = new Font("Segoe UI", 8F);
            TextRenderer.DrawText(e.Graphics, "No data for this view yet.", emptyFont, new Point(0, 4), Color.FromArgb(140, 156, 175));
            return;
        }

        var max = Math.Max(1, _items.Max(item => item.Value));
        var rowHeight = Math.Min(44, Math.Max(20, (Height - 6) / _items.Count));

        using var labelFont = new Font("Segoe UI", 8F);
        using var valueFont = new Font("Segoe UI", 8F, FontStyle.Bold);
        using var trackBrush = new SolidBrush(Color.FromArgb(236, 242, 248));

        const int labelWidth = 100;
        const int valueWidth = 36;

        for (var index = 0; index < _items.Count; index++)
        {
            var item = _items[index];
            var y = 2 + index * rowHeight;

            TextRenderer.DrawText(e.Graphics, item.Label, labelFont, new Point(0, y + 2), Color.FromArgb(90, 108, 130));

            var barLeft = labelWidth;
            var barWidth = Math.Max(24, Width - labelWidth - valueWidth - 10);
            e.Graphics.FillRectangle(trackBrush, barLeft, y + 4, barWidth, 12);

            var fill = (int)Math.Round(barWidth * (item.Value / (double)max));
            using var brush = new SolidBrush(item.Color);
            e.Graphics.FillRectangle(brush, barLeft, y + 4, Math.Max(3, fill), 12);

            TextRenderer.DrawText(e.Graphics, item.Value.ToString(), valueFont, new Point(barLeft + barWidth + 6, y + 2), Color.FromArgb(60, 80, 105));
        }
    }
}
