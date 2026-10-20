namespace TaskTap;

/// <summary>Announcements: publish and pause the notices shown to members.</summary>
public sealed class AdminAnnouncementsPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private int _lastWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Announcements;

    public AdminAnnouncementsPage()
    {
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("COMMUNICATION", 8, AdminUi.Blue, true);
        eyebrow.Name = "announcementsEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Announcements", 26, AdminUi.Navy, true);
        title.Name = "announcementsTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Publish notices that appear in the member Notifications page.", 9, AdminUi.Muted);
        subtitle.Name = "announcementsSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var add = AdminUi.MakePrimaryButton("+ New announcement", "announcementsAddButton", 190);
        add.Click += (_, _) => NewAnnouncement();
        page.Controls.Add(add);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var heading = AdminUi.MakeLabel("Published announcements", 11, AdminUi.Navy, true);
        heading.Location = new Point(18, 14);
        listCard.Controls.Add(heading);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        RefreshList();
        LayoutPage();
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        if (contentWidth == _lastWidth)
            return;
        _lastWidth = contentWidth;

        const int left = 32;
        var eyebrow = AdminUi.Find<Label>(page, "announcementsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "announcementsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "announcementsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var add = AdminUi.Find<Button>(page, "announcementsAddButton");
        if (add is not null) add.Location = new Point(left + contentWidth - 190, 40);

        listCard.Bounds = new Rectangle(left, 132, contentWidth, 620);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);
        page.AutoScrollMinSize = new Size(0, 790);

        RefreshList();
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        var announcements = DemoDataStore.Announcements.ToList();

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var announcement in announcements)
        {
            var row = BuildRow(announcement, width);
            row.SetBounds(0, y, width, 96);
            listBody.Controls.Add(row);
            y += 102;
        }

        if (announcements.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No announcements yet. Publish one to reach members.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(DemoAnnouncement announcement, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var title = AdminUi.MakeLabel(announcement.Title, 10, AdminUi.Navy, true);
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var body = AdminUi.MakeLabel(announcement.Body, 8, AdminUi.Muted);
        body.MaximumSize = new Size(Math.Max(200, width - 320), 34);
        body.Location = new Point(14, 34);
        row.Controls.Add(body);

        var meta = AdminUi.MakeLabel($"{announcement.Audience} · published {announcement.PublishedAt} · {announcement.Author}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(14, 72);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(
            announcement.Active ? "Published" : "Paused",
            announcement.Active ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 247, 224),
            announcement.Active ? AdminUi.Good : Color.FromArgb(180, 130, 20));
        status.Cursor = Cursors.Hand;
        status.Location = new Point(width - 220, 16);
        status.Click += (_, _) =>
        {
            DemoDataStore.ToggleAnnouncement(announcement);
            RefreshList();
        };
        row.Controls.Add(status);

        var delete = AdminUi.MakeLinkButton("Delete", $"announcementDelete{announcement.Id}");
        delete.ForeColor = AdminUi.Bad;
        delete.Location = new Point(width - 116, 20);
        delete.Click += (_, _) => DeleteAnnouncement(announcement);
        row.Controls.Add(delete);

        return row;
    }

    private void NewAnnouncement()
    {
        using var dialog = new AnnouncementDialog();
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.PublishAnnouncement(new DemoAnnouncement
        {
            Id = DemoDataStore.NextAnnouncementId(),
            Title = dialog.AnnouncementTitle,
            Body = dialog.AnnouncementBody,
            Audience = dialog.AnnouncementAudience,
            Active = dialog.AnnouncementActive,
            PublishedAt = DateTime.Today.ToString("MMM d, yyyy"),
            Author = AppSession.CurrentAdmin?.FullName ?? "System"
        });

        RefreshList();
        Toast("Announcement published.");
    }

    private void DeleteAnnouncement(DemoAnnouncement announcement)
    {
        var answer = MessageBox.Show(
            this,
            $"Delete the \"{announcement.Title}\" announcement?",
            "Delete announcement",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        DemoDataStore.DeleteAnnouncement(announcement);
        RefreshList();
        Toast("Announcement deleted.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
