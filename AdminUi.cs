namespace TaskTap;

/// <summary>Shared visual helpers for the admin portal so the pages stay consistent.</summary>
public static class AdminUi
{
    public static readonly Color Navy = Color.FromArgb(9, 30, 60);
    public static readonly Color Blue = Color.FromArgb(8, 117, 232);
    public static readonly Color Accent = Color.FromArgb(255, 138, 61);
    public static readonly Color Muted = Color.FromArgb(110, 128, 150);
    public static readonly Color Page = Color.FromArgb(244, 248, 252);
    public static readonly Color CardBack = Color.White;
    public static readonly Color Line = Color.FromArgb(225, 233, 241);
    public static readonly Color SidebarNavy = Color.FromArgb(16, 24, 40);
    public static readonly Color ActiveNavy = Color.FromArgb(36, 52, 78);
    public static readonly Color AdminBadgeBack = Color.FromArgb(255, 176, 102);
    public static readonly Color AdminBadgeFore = Color.FromArgb(33, 24, 12);
    public static readonly Color Good = Color.FromArgb(20, 153, 111);
    public static readonly Color Warn = Color.FromArgb(219, 108, 32);
    public static readonly Color Bad = Color.FromArgb(185, 55, 67);

    public static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
    };

    public static Label MakeWrappedLabel(string text, float size, Color color, int maxWidth, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold);
        label.MaximumSize = new Size(maxWidth, 0);
        return label;
    }

    public static Panel MakeCard() => new()
    {
        BackColor = CardBack,
        BorderStyle = BorderStyle.FixedSingle
    };

    public static Button MakePrimaryButton(string text, string name, int width = 150)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(width, 38),
            BackColor = Blue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    public static Button MakeSecondaryButton(string text, string name, int width = 130)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(width, 38),
            BackColor = Color.White,
            ForeColor = Navy,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = Line;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    public static Button MakeLinkButton(string text, string name)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Blue,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    public static Label MakePill(string text, Color back, Color fore) => new()
    {
        Text = text,
        AutoSize = true,
        BackColor = back,
        ForeColor = fore,
        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
        Padding = new Padding(8, 3, 8, 3),
        TextAlign = ContentAlignment.MiddleCenter
    };

    public static Panel MakeDivider() => new() { BackColor = Line, Height = 1 };

    public static T? Find<T>(Control parent, string name) where T : Control =>
        parent.Controls.Find(name, false).OfType<T>().FirstOrDefault();
}
