using TaskTap.Data;

namespace TaskTap;

public sealed class NotificationsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(104, 121, 144);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel preferenceCard = new();
    private readonly Panel safetyCard = new();
    private readonly Panel notificationList = new();
    private readonly ComboBox periodFilter = new();
    private readonly Label unreadCount = new();
    private readonly Label sidebarUnread = new();
    private readonly Label emptyState = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private readonly List<NotificationItem> notices = [];
    private readonly Dictionary<string, Button> filterButtons = [];
    private readonly Dictionary<NotificationItem, Panel> noticeRows = [];
    private readonly Dictionary<NotificationItem, Button> readButtons = [];
    private string activeFilter = "all";
    private bool pageBuilt;
    private Dictionary<string, bool> preferenceValues = new(StringComparer.OrdinalIgnoreCase);

    private sealed class NotificationItem(string title, string time, string description, string action, string type, string period, string icon, Color iconColor, bool unread)
    {
        public string Title { get; } = title;
        public string Time { get; } = time;
        public string Description { get; } = description;
        public string Action { get; } = action;
        public string Type { get; } = type;
        public string Period { get; } = period;
        public string Icon { get; } = icon;
        public Color IconColor { get; } = iconColor;
        public bool Unread { get; set; } = unread;
        public int Id { get; init; }
    }

    public override Route RouteName => Route.Notifications;

    public NotificationsPage()
    {
        LoadNotifications();
        BuildPage();
    }

    private static string AgeLabel(DateTime created)
    {
        var age = DateTime.Now - created;
        if (age.TotalMinutes < 1) return "Just now";
        if (age.TotalMinutes < 60) return $"{(int)age.TotalMinutes} min ago";
        if (created.Date == DateTime.Today) return $"{(int)age.TotalHours} hr{((int)age.TotalHours == 1 ? "" : "s")} ago";
        if (created.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
        return created.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
    }

    private void LoadNotifications()
    {
        if (AppSession.IsSignedIn)
        {
            Db.Try(() => preferenceValues = Backend.LoadNotificationPreferences(AppSession.CurrentUser.Id));
            List<Backend.NotificationRow> rows = [];
            Db.Try(() => rows = Backend.LoadNotifications(AppSession.CurrentUser.Id));
            foreach (var row in rows)
            {
                var (icon, color) = row.Kind switch
                {
                    "messages" => ("▱", Color.FromArgb(132, 101, 190)),
                    "payments" => ("₱", Color.FromArgb(224, 132, 73)),
                    _ => ("✓", Color.FromArgb(77, 132, 214))
                };
                notices.Add(new NotificationItem(row.Title, AgeLabel(row.CreatedAt), row.Body, row.ActionLabel, row.Kind,
                    row.CreatedAt.Date == DateTime.Today ? "today" : "week", icon, color, row.Unread) { Id = row.Id });
            }
        }

        foreach (var announcement in DemoDataStore.Announcements.Where(announcement => announcement.Active))
        {
            notices.Add(new NotificationItem(
                announcement.Title,
                announcement.PublishedAt,
                announcement.Body,
                "View announcements",
                "announcements",
                "week",
                "✉",
                Color.FromArgb(77, 132, 214),
                false));
        }
    }

    private void BuildPage()
    {
        page.Dock = DockStyle.Fill;
        page.AutoScroll = true;
        page.BackColor = PageColor;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);
        BuildContent();

        toast.Size = new Size(440, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);

        pageBuilt = true;
        UpdateCounts();
        ApplyFilters();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Dashboard   /   Notifications", 8, Muted, true);
        crumb.Name = "notificationsCrumb";
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);
        AddPageLabel("notificationsEyebrow", "ACTIVITY CENTER", 8, Blue, true);
        AddPageLabel("notificationsHeading", "Notifications", 25, Navy, true);
        AddPageLabel("notificationsSubtitle", "Stay on top of requests, payments, and task updates.", 9, Muted, false);
        var markAll = MakeButton("✓  Mark all as read", "NotificationsMarkAllButton", false, 150);
        markAll.Click += (_, _) => MarkAllRead();
        page.Controls.Add(markAll);

        listCard.Name = "notificationsListCard";
        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);
        var toolbar = new Panel { Name = "notificationsToolbar", Height = 65, Dock = DockStyle.Top, BackColor = Color.White };
        toolbar.Paint += (_, e) => { using var pen = new Pen(Line); e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1); };
        listCard.Controls.Add(toolbar);
        var filters = new[] { ("all", "All"), ("unread", "Unread"), ("tasks", "Tasks"), ("payments", "Payments"), ("announcements", "Announcements") };
        var x = 14;
        foreach (var (key, text) in filters)
        {
            var button = MakeFilterButton(key, text);
            button.Location = new Point(x, 17);
            filterButtons.Add(key, button);
            toolbar.Controls.Add(button);
            x += button.Width + 6;
        }
        periodFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        periodFilter.Items.AddRange(["All activity", "Today", "This week"]);
        periodFilter.SelectedIndex = 0;
        periodFilter.Size = new Size(132, 30);
        periodFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        periodFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        toolbar.Controls.Add(periodFilter);

        notificationList.Name = "notificationsList";
        notificationList.AutoScroll = true;
        notificationList.BackColor = Color.White;
        listCard.Controls.Add(notificationList);
        emptyState.Text = "♧\r\nNo notifications here\r\nTry another filter or check back later.";
        emptyState.TextAlign = ContentAlignment.MiddleCenter;
        emptyState.ForeColor = Muted;
        emptyState.Font = new Font("Segoe UI", 10F);
        emptyState.Visible = false;
        listCard.Controls.Add(emptyState);

        preferenceCard.Name = "notificationsPreferenceCard";
        preferenceCard.BackColor = Color.White;
        preferenceCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(preferenceCard);
        BuildPreferences();
        safetyCard.Name = "notificationsSafetyCard";
        safetyCard.BackColor = Color.FromArgb(240, 248, 255);
        safetyCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(safetyCard);
        BuildSafetyCard();
        foreach (var notice in notices)
        {
            var row = BuildNotificationRow(notice);
            noticeRows.Add(notice, row);
            notificationList.Controls.Add(row);
        }
    }

    private void BuildPreferences()
    {
        AddCardLabel(preferenceCard, "Notification preferences", 13, Navy, true, new Point(18, 18), new Size(250, 24));
        AddCardLabel(preferenceCard, "Choose which updates you want to see in your TaskTap activity center.", 8, Muted, false, new Point(18, 48), new Size(275, 43));
        var preferences = new[]
        {
            ("Task requests", "New requests and decisions"),
            ("Payments", "Payment records and reminders"),
            ("Deadlines", "Upcoming task reminders"),
            ("Community tips", "Occasional TaskTap tips")
        };
        for (var i = 0; i < preferences.Length; i++)
        {
            var (title, subtitle) = preferences[i];
            var y = 105 + i * 67;
            var divider = new Panel { BackColor = Line, Location = new Point(18, y), Size = new Size(275, 1) };
            preferenceCard.Controls.Add(divider);
            AddCardLabel(preferenceCard, title, 9, Navy, true, new Point(18, y + 11), new Size(190, 20));
            AddCardLabel(preferenceCard, subtitle, 7.5F, Muted, false, new Point(18, y + 31), new Size(200, 18));
            var preferenceKey = title.ToLowerInvariant().Replace(" ", "_");
            var toggle = new CheckBox { Name = $"NotificationPreference_{preferenceKey}", Checked = !preferenceValues.TryGetValue(preferenceKey, out var enabled) || enabled, Appearance = Appearance.Button, Text = "ON", TextAlign = ContentAlignment.MiddleCenter, FlatStyle = FlatStyle.Flat, BackColor = Blue, ForeColor = Color.White, Font = new Font("Segoe UI", 7F, FontStyle.Bold), Size = new Size(46, 24), Location = new Point(244, y + 17), Cursor = Cursors.Hand };
            toggle.FlatAppearance.BorderSize = 0;
            toggle.CheckedChanged += (_, _) => { toggle.Text = toggle.Checked ? "ON" : "OFF"; toggle.BackColor = toggle.Checked ? Blue : Color.FromArgb(170, 184, 200);
                if (AppSession.IsSignedIn) Db.Try(() => Backend.SetNotificationPreference(AppSession.CurrentUser.Id, preferenceKey, toggle.Checked));
                ShowToast($"{title} notifications {(toggle.Checked ? "enabled" : "disabled")}."); };
            preferenceCard.Controls.Add(toggle);
        }
    }

    private void BuildSafetyCard()
    {
        var icon = new Label { Text = "✓", Location = new Point(18, 17), Size = new Size(38, 38), BackColor = Color.FromArgb(229, 246, 239), ForeColor = Color.FromArgb(41, 159, 120), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
        safetyCard.Controls.Add(icon);
        AddCardLabel(safetyCard, "Stay safe", 13, Navy, true, new Point(18, 64), new Size(250, 24));
        AddCardLabel(safetyCard, "Keep communication and payment records inside TaskTap. Never share passwords, OTPs, or sensitive account information.", 8, Muted, false, new Point(18, 94), new Size(278, 68));
        var tips = MakeButton("Learn more about safe task handling →", "NotificationsSafetyTipsButton", false, 250);
        tips.Location = new Point(14, 168);
        tips.FlatAppearance.BorderSize = 0;
        tips.Click += (_, _) => Nav.Navigate(Route.CommunityGuidelines);
        safetyCard.Controls.Add(tips);
    }

    private Panel BuildNotificationRow(NotificationItem notice)
    {
        var row = new Panel { BackColor = notice.Unread ? Color.FromArgb(248, 251, 255) : Color.White, Height = 98, BorderStyle = BorderStyle.None };
        row.Paint += (_, e) => { using var pen = new Pen(Line); e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1); if (notice.Unread) using (var unreadPen = new Pen(Blue, 3)) e.Graphics.DrawLine(unreadPen, 1, 0, 1, row.Height); };
        var icon = new Label { Text = notice.Icon, Location = new Point(16, 17), Size = new Size(40, 40), BackColor = Color.FromArgb(237, 244, 252), ForeColor = notice.IconColor, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F, FontStyle.Bold) };
        row.Controls.Add(icon);
        var title = new Label { Text = notice.Title, Location = new Point(68, 14), Size = new Size(380, 22), ForeColor = Navy, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoEllipsis = true };
        row.Controls.Add(title);
        var time = new Label { Text = notice.Time, Size = new Size(95, 20), ForeColor = Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        row.Controls.Add(time);
        var description = new Label { Text = notice.Description, Location = new Point(68, 37), Size = new Size(440, 28), ForeColor = Muted, Font = new Font("Segoe UI", 8F), AutoEllipsis = true };
        row.Controls.Add(description);
        var action = MakeLinkButton(notice.Action + "  →");
        action.Location = new Point(64, 66);
        action.Click += (_, _) => ActivateNotification(notice);
        row.Controls.Add(action);
        var read = new Button { Text = notice.Unread ? "●" : "○", Size = new Size(30, 30), ForeColor = notice.Unread ? Blue : Color.FromArgb(194, 205, 218), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F), Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand, TabStop = false, AccessibleName = notice.Unread ? "Mark as read" : "Mark as unread" };
        read.FlatAppearance.BorderSize = 0;
        read.Click += (_, _) => ToggleRead(notice);
        readButtons.Add(notice, read);
        row.Controls.Add(read);
        row.Resize += (_, _) => { time.Location = new Point(row.ClientSize.Width - 138, 14); title.Size = new Size(Math.Max(120, row.ClientSize.Width - 245), 22); description.Size = new Size(Math.Max(120, row.ClientSize.Width - 150), 28); read.Location = new Point(row.ClientSize.Width - 38, 10); };
        return row;
    }

    private Button MakeFilterButton(string key, string text)
    {
        var button = new Button { Name = "NotificationsFilter_" + key, Text = text, Size = new Size(text == "Unread" ? 92 : 72, 31), FlatStyle = FlatStyle.Flat, BackColor = key == "all" ? Color.FromArgb(234, 244, 255) : Color.FromArgb(245, 248, 252), ForeColor = key == "all" ? Blue : Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = key == "all" ? Color.FromArgb(213, 234, 255) : Color.FromArgb(245, 248, 252);
        button.FlatAppearance.BorderSize = key == "all" ? 1 : 0;
        button.Click += (_, _) => { activeFilter = key; UpdateFilterButtons(); ApplyFilters(); };
        return button;
    }

    private void ApplyFilters()
    {
        if (!pageBuilt) return;
        var period = periodFilter.SelectedIndex == 1 ? "today" : periodFilter.SelectedIndex == 2 ? "week" : "all";
        var visible = 0;
        foreach (var notice in notices)
        {
            var typeMatches = activeFilter is "all" or "unread" || notice.Type == activeFilter;
            var unreadMatches = activeFilter != "unread" || notice.Unread;
            var periodMatches = period == "all" || notice.Period == period;
            var show = typeMatches && unreadMatches && periodMatches;
            noticeRows[notice].Visible = show;
            if (show) visible++;
        }
        emptyState.Visible = visible == 0;
        LayoutNotificationRows();
    }

    private void ToggleRead(NotificationItem notice)
    {
        notice.Unread = !notice.Unread;
        if (notice.Id != 0)
            Db.Try(() => { if (notice.Unread) Backend.MarkNotificationUnread(notice.Id); else Backend.MarkNotificationRead(notice.Id); });
        UpdateCounts();
        ApplyFilters();
        ShowToast(notice.Unread ? "Marked as unread" : "Marked as read");
    }

    private void MarkAllRead()
    {
        foreach (var notice in notices) notice.Unread = false;
        Db.Try(() => Backend.MarkAllNotificationsRead(AppSession.CurrentUser.Id));
        UpdateCounts();
        ApplyFilters();
        ShowToast("All notifications marked as read");
    }

    private void UpdateCounts()
    {
        var count = notices.Count(notice => notice.Unread);
        unreadCount.Text = count.ToString();
        sidebarUnread.Text = count.ToString();
        sidebarUnread.Visible = count > 0;
        DemoDataStore.SetUnreadNotifications(count);
        if (filterButtons.TryGetValue("all", out var all)) all.Text = $"All  {notices.Count}";
        if (filterButtons.TryGetValue("unread", out var unread)) unread.Text = $"Unread  {count}";
        foreach (var (notice, row) in noticeRows)
        {
            row.BackColor = notice.Unread ? Color.FromArgb(248, 251, 255) : Color.White;
            var read = readButtons[notice];
            read.Text = notice.Unread ? "●" : "○";
            read.ForeColor = notice.Unread ? Blue : Color.FromArgb(194, 205, 218);
            read.AccessibleName = notice.Unread ? "Mark as read" : "Mark as unread";
            row.Invalidate();
        }
    }

    private void UpdateFilterButtons()
    {
        foreach (var (key, button) in filterButtons)
        {
            var active = key == activeFilter;
            button.BackColor = active ? Color.FromArgb(234, 244, 255) : Color.FromArgb(245, 248, 252);
            button.ForeColor = active ? Blue : Muted;
            button.FlatAppearance.BorderColor = active ? Color.FromArgb(213, 234, 255) : Color.FromArgb(245, 248, 252);
            button.FlatAppearance.BorderSize = active ? 1 : 0;
        }
    }

    private void ActivateNotification(NotificationItem notice)
    {
        if (notice.Unread)
        {
            notice.Unread = false;
            if (notice.Id != 0)
                Db.Try(() => Backend.MarkNotificationRead(notice.Id));
            UpdateCounts();
            ApplyFilters();
        }
        var destination = notice.Type == "payments" ? "Payments" : "My Tasks";
        if (destination == "Payments") { Nav.Navigate(Route.PaymentHistory); return; }
        switch (notice.Action)
        {
            case "Review request": Nav.Navigate(new ManageRequestsPage()); return;
            case "View request": Nav.Navigate(new MyRequestsPage()); return;
            case "View reviews": Nav.Navigate(new ReviewsPage()); return;
            case "Continue editing": Nav.Navigate(Route.PostTask); return;
            case "View announcements": return;
            case "Open guidelines": Nav.Navigate(Route.CommunityGuidelines); return;
            default: Nav.Navigate(Route.MyTasks); return;
        }
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var left = 22;
        var contentWidth = width - 44;
        Position("notificationsCrumb", left, 12);
        Position("notificationsEyebrow", left, 40);
        Position("notificationsHeading", left, 56);
        Position("notificationsSubtitle", left, 104);
        var markAll = page.Controls.Find("NotificationsMarkAllButton", false).FirstOrDefault();
        if (markAll is not null) markAll.Location = new Point(width - left - markAll.Width, 61);
        var sideWidth = Math.Min(320, Math.Max(270, contentWidth / 3));
        var listWidth = contentWidth - sideWidth - 18;
        var listHeight = Math.Max(610, notices.Count(notice => noticeRows[notice].Visible) * 98 + 65);
        listCard.Bounds = new Rectangle(left, 138, listWidth, listHeight);
        preferenceCard.Bounds = new Rectangle(left + listWidth + 18, 138, sideWidth, 390);
        safetyCard.Bounds = new Rectangle(left + listWidth + 18, 546, sideWidth, 240);
        var toolbar = listCard.Controls.Find("notificationsToolbar", false).FirstOrDefault();
        if (toolbar is not null)
        {
            var x = 12;
            foreach (var button in filterButtons.Values) { button.Location = new Point(x, 17); x += button.Width + 4; }
            periodFilter.Location = new Point(Math.Max(x + 2, toolbar.ClientSize.Width - periodFilter.Width - 10), 17);
        }
        notificationList.Bounds = new Rectangle(0, 65, listCard.Width, listCard.Height - 65);
        emptyState.Bounds = new Rectangle(0, 65, listCard.Width, listCard.Height - 65);
        page.AutoScrollMinSize = new Size(0, Math.Max(listCard.Bottom, safetyCard.Bottom) + 30);
        LayoutNotificationRows();
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void LayoutNotificationRows()
    {
        if (notificationList.Width <= 0) return;
        var y = notificationList.AutoScrollPosition.Y;
        foreach (var notice in notices)
        {
            var row = noticeRows[notice];
            row.Width = notificationList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            row.Location = new Point(0, y);
            if (row.Visible) y += row.Height;
        }
    }

    private void Position(string name, int x, int y)
    {
        var control = page.Controls.Find(name, false).FirstOrDefault();
        if (control is not null) control.Location = new Point(x, y);
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private Label MakeLabel(string text, float size, Color color, bool bold) => new() { Text = text, AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent };

    private Button MakeButton(string text, string name, bool active, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, BackColor = active ? Color.FromArgb(234, 244, 255) : Color.White, ForeColor = active ? Blue : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = Line;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private Button MakeLinkButton(string text)
    {
        var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, TabStop = false };
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(0);
        return button;
    }

    private void AddPageLabel(string name, string text, float size, Color color, bool bold)
    {
        var label = MakeLabel(text, size, color, bold);
        label.Name = name;
        page.Controls.Add(label);
    }

    private void AddCardLabel(Control parent, string text, float size, Color color, bool bold, Point location, Size dimensions)
    {
        var label = new Label { Text = text, Location = location, Size = dimensions, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent, AutoEllipsis = true };
        parent.Controls.Add(label);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
