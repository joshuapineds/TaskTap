namespace TaskTap;

/// <summary>
/// Generic fallback for admin routes that are normally reached contextually from
/// another page (for example, a user detail that needs a selected member).
/// </summary>
public sealed class AdminPlaceholderPage : AdminPage
{
    private readonly AdminRoute _route;
    private readonly string _title;

    public AdminPlaceholderPage(AdminRoute route, string title)
    {
        _route = route;
        _title = title;
        BuildContent();
    }

    public override AdminRoute AdminRouteName => _route;

    private void BuildContent()
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = AdminUi.Page, AutoScroll = true };
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("ADMIN CONSOLE", 8, AdminUi.Blue, true);
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var heading = AdminUi.MakeLabel(_title, 26, AdminUi.Navy, true);
        heading.Location = new Point(32, 44);
        page.Controls.Add(heading);

        var subtitle = AdminUi.MakeWrappedLabel("This view opens from a related admin screen.", 10, AdminUi.Muted, 700);
        subtitle.Location = new Point(32, 92);
        page.Controls.Add(subtitle);

        var card = AdminUi.MakeCard();
        card.Location = new Point(32, 136);
        card.Size = new Size(620, 140);
        page.Controls.Add(card);

        var message = AdminUi.MakeWrappedLabel($"Use the link on the related admin page to open {_title}.", 9, AdminUi.Muted, 560);
        message.Location = new Point(22, 22);
        card.Controls.Add(message);

        var home = AdminUi.MakePrimaryButton("Back to dashboard", "AdminPlaceholderHomeButton", 180);
        home.Location = new Point(22, 78);
        home.Click += (_, _) => Nav.GoHome();
        card.Controls.Add(home);
    }
}
