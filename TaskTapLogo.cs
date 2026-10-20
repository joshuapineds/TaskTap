using System.Drawing.Drawing2D;

namespace TaskTap;

/// <summary>
/// The shared TaskTap logo used across the app: the two overlapping brand dots plus the
/// "TaskTap" wordmark, matching the logo shown on the How It Works and About pages.
/// </summary>
public sealed class TaskTapLogo : Control
{
    public Color TextColor = Color.FromArgb(9, 35, 76);
    public float FontSize = 16F;

    public TaskTapLogo()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        Size = new Size(160, 34);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var blue = new SolidBrush(Color.FromArgb(8, 112, 229));
        using var orange = new SolidBrush(Color.FromArgb(255, 134, 57));
        using var white = new SolidBrush(Color.White);
        e.Graphics.FillEllipse(blue, 0, 0, 18, 18);
        e.Graphics.FillEllipse(orange, 10, 13, 18, 18);
        e.Graphics.FillEllipse(white, 6, 6, 6, 6);
        e.Graphics.FillEllipse(white, 16, 19, 6, 6);
        TextRenderer.DrawText(e.Graphics, "TaskTap", new Font("Segoe UI", FontSize, FontStyle.Bold), new Point(34, 3), TextColor);
    }
}
