namespace TaskTap;

/// <summary>Guest renderer for the short Help and policy articles.</summary>
public sealed class InfoArticleForm : PublicInfoFormBase
{
    private readonly InfoArticle _article;

    public InfoArticleForm(InfoArticle article) : base(article.Title)
    {
        _article = article;
        BuildArticle();
    }

    private void BuildArticle()
    {
        const int textWidth = 820;
        var body = new Panel { BackColor = PageColor };
        var y = 8;

        var eyebrow = MakeLabel(_article.Eyebrow, 8, BlueColor, true);
        eyebrow.Location = new Point(0, y);
        body.Controls.Add(eyebrow);
        y += 22;

        var title = MakeLabel(_article.Title, 30, NavyColor, true);
        title.Location = new Point(0, y);
        body.Controls.Add(title);
        y += 50;

        var summary = MakeLabel(_article.Summary, 11, MutedColor);
        summary.MaximumSize = new Size(textWidth, 0);
        summary.Location = new Point(0, y);
        body.Controls.Add(summary);
        y += summary.GetPreferredSize(new Size(textWidth, 0)).Height + 26;

        foreach (var section in _article.Sections)
        {
            var heading = MakeLabel(section.Heading, 13, NavyColor, true);
            heading.Location = new Point(0, y);
            body.Controls.Add(heading);
            y += 28;

            var text = MakeLabel(section.Body, 10, MutedColor);
            text.MaximumSize = new Size(textWidth, 0);
            text.Location = new Point(0, y);
            body.Controls.Add(text);
            y += text.GetPreferredSize(new Size(textWidth, 0)).Height + 22;
        }

        var related = new FlowLayoutPanel
        {
            Location = new Point(0, y),
            Size = new Size(textWidth, 40),
            WrapContents = false,
            BackColor = PageColor
        };
        foreach (var other in InfoArticleCatalog.All.Where(article => article.Key != _article.Key))
        {
            var link = MakeLink(other.Title, $"InfoLink{other.Key}");
            var captured = other;
            link.Click += (_, _) => OpenArticle(captured);
            related.Controls.Add(link);
        }
        body.Controls.Add(related);
        y += 46;

        AddSection(body, y + 30, 0);
    }

    private void OpenArticle(InfoArticle article)
    {
        var next = new InfoArticleForm(article);
        next.FormClosed += (_, _) =>
        {
            if (IsDisposed)
                return;
            Show();
            Activate();
        };
        Hide();
        next.Show();
    }
}
