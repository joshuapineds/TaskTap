namespace TaskTap;

/// <summary>Signed-in renderer for the short Help and policy articles.</summary>
public sealed class InfoArticlePage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);

    private readonly Route _route;
    private readonly InfoArticle _article;
    private readonly Panel page = new();
    private int _lastWidth = -1;

    public override Route RouteName => _route;

    public InfoArticlePage(Route route, InfoArticle article)
    {
        _route = route;
        _article = article;

        page.Dock = DockStyle.Fill;
        page.BackColor = PageColor;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        LayoutPage();
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(600, Math.Min(880, page.ClientSize.Width - 64));
        if (contentWidth == _lastWidth)
            return;
        _lastWidth = contentWidth;

        Rebuild(contentWidth);
    }

    private void Rebuild(int width)
    {
        page.SuspendLayout();
        page.Controls.Clear();

        var left = Math.Max(32, (page.ClientSize.Width - width) / 2);
        var y = 24;

        var eyebrow = MakeLabel(_article.Eyebrow, 8, Blue, true);
        eyebrow.Location = new Point(left, y);
        page.Controls.Add(eyebrow);
        y += 24;

        var title = MakeLabel(_article.Title, 28, Navy, true);
        title.Location = new Point(left, y);
        page.Controls.Add(title);
        y += 48;

        var summary = MakeWrapped(_article.Summary, 11, Muted, width);
        summary.Location = new Point(left, y);
        page.Controls.Add(summary);
        y += summary.GetPreferredSize(new Size(width, 0)).Height + 28;

        foreach (var section in _article.Sections)
        {
            var heading = MakeLabel(section.Heading, 12, Navy, true);
            heading.Location = new Point(left, y);
            page.Controls.Add(heading);
            y += 28;

            var body = MakeWrapped(section.Body, 9.5F, Muted, width);
            body.Location = new Point(left, y);
            page.Controls.Add(body);
            y += body.GetPreferredSize(new Size(width, 0)).Height + 20;
        }

        var relatedLabel = MakeLabel("RELATED", 8, Muted, true);
        relatedLabel.Location = new Point(left, y);
        page.Controls.Add(relatedLabel);
        y += 24;

        var related = new FlowLayoutPanel
        {
            Location = new Point(left, y),
            Size = new Size(width, 40),
            WrapContents = false,
            BackColor = Color.Transparent
        };
        foreach (var other in InfoArticleCatalog.All.Where(article => article.Key != _article.Key))
            related.Controls.Add(MakeLink(other.Title, other.Key));
        page.Controls.Add(related);
        y += 46;

        page.AutoScrollMinSize = new Size(0, y + 40);
        page.ResumeLayout();
    }

    private Button MakeLink(string text, string key)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Blue,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 12, 0),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => Nav.Navigate(InfoArticleCatalog.RouteFor(key));
        return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
    };

    private static Label MakeWrapped(string text, float size, Color color, int maxWidth, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold);
        label.MaximumSize = new Size(maxWidth, 0);
        return label;
    }
}
