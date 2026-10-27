namespace TaskTap;

/// <summary>
/// The single signed-in window. It owns the shared sidebar, topbar, content host
/// and logout, and delegates all page transitions to <see cref="NavigationService"/>.
/// </summary>
public sealed class MainShellForm : Form
{
    private static readonly Color SidebarNavy = Color.FromArgb(8, 35, 77);
    private static readonly Color ActiveNavy = Color.FromArgb(31, 63, 108);
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);

    private readonly Panel sidebar = new();
    private readonly Panel topbar = new();
    private readonly Panel contentHost = new();
    private readonly Button[] navButtons = new Button[6];
    private readonly Button backButton = new();
    private readonly Label pageTitleLabel = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private readonly NavigationService navigation = NavigationService.Current;
    private bool _exitHandled;
    private Label? _messagesBadge;

    private static readonly (string Glyph, string Label, Route Route)[] NavItems =
    [
        ("⌂", "Dashboard", Route.Dashboard),
        ("⌕", "Find Tasks", Route.FindTasks),
        ("▤", "My Tasks", Route.MyTasks),
        ("◔", "Notifications", Route.Notifications),
        ("₱", "Payments", Route.PaymentHistory),
        ("◉", "Profile", Route.Profile)
    ];

    public Panel ContentHost => contentHost;

    public MainShellForm()
    {
        BuildShell();
        navigation.Attach(this);
        navigation.Navigate(Route.Dashboard);
        ReopenPendingTask();
    }

    private void ReopenPendingTask()
    {
        var pending = AppSession.PendingTaskAfterLogin;
        if (pending is null)
            return;
        AppSession.PendingTaskAfterLogin = null;
        navigation.Navigate(new UserTaskDetailsPage(
            pending.Title, pending.Category, pending.Price, pending.Barangay,
            pending.Days, pending.Deadline, pending.Icon, pending.Description, pending.IconBack));
    }

    private void BuildShell()
    {
        Text = "TaskTap";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 740);
        ClientSize = new Size(1540, 900);
        BackColor = Page;
        Font = new Font("Segoe UI", 9F);

        contentHost.Dock = DockStyle.Fill;
        contentHost.BackColor = Page;
        Controls.Add(contentHost);

        BuildTopbar();
        BuildSidebar();
        BuildToast();
    }

    private void BuildSidebar()
    {
        sidebar.Dock = DockStyle.Left;
        sidebar.Width = 220;
        sidebar.BackColor = SidebarNavy;
        Controls.Add(sidebar);

        var brand = new TaskTapLogo
        {
            Name = "shellBrand",
            TextColor = Color.White,
            BackColor = SidebarNavy,
            Location = new Point(12, 16),
            Size = new Size(196, 42),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        brand.Click += (_, _) => navigation.GoHome();
        sidebar.Controls.Add(brand);

        var postTask = MakeSidebarButton("＋   Post a Task");
        postTask.Name = "ShellPostTaskButton";
        postTask.Location = new Point(12, 66);
        postTask.Size = new Size(196, 40);
        postTask.BackColor = Blue;
        postTask.ForeColor = Color.White;
        postTask.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        postTask.FlatAppearance.BorderSize = 0;
        postTask.Click += (_, _) => navigation.Navigate(Route.PostTask);
        sidebar.Controls.Add(postTask);

        for (var index = 0; index < NavItems.Length; index++)
        {
            var item = NavItems[index];
            var button = MakeSidebarButton($"{item.Glyph}   {item.Label}");
            button.Location = new Point(12, 120 + index * 43);
            var captured = item.Route;
            button.Click += (_, _) => navigation.Navigate(captured);
            navButtons[index] = button;
            sidebar.Controls.Add(button);
        }

        _messagesBadge = AddSidebarBadge(120 + 3 * 43);
        DemoDataStore.BadgesChanged += UpdateBadges;
        UpdateBadges();

        var accountLabel = new Label
        {
            Text = "ACCOUNT",
            Location = new Point(23, 393),
            AutoSize = true,
            ForeColor = Color.FromArgb(126, 159, 198),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            BackColor = Color.Transparent
        };
        sidebar.Controls.Add(accountLabel);

        var logout = MakeSidebarButton("↪   Log out");
        logout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        logout.Click += (_, _) => navigation.Logout();
        sidebar.Controls.Add(logout);
        sidebar.Resize += (_, _) => LayoutSidebarControls(brand, postTask, logout);
        LayoutSidebarControls(brand, postTask, logout);
    }

    private void LayoutSidebarControls(Control brand, Button postTask, Button logout)
    {
        var width = Math.Max(120, sidebar.ClientSize.Width - 24);
        brand.Bounds = new Rectangle(12, 16, width, 42);
        postTask.Width = width;
        postTask.Location = new Point(12, 66);
        for (var index = 0; index < navButtons.Length; index++)
        {
            if (navButtons[index] is null)
                continue;
            navButtons[index].Width = width;
            navButtons[index].Location = new Point(12, 120 + index * 43);
        }
        if (_messagesBadge is not null)
            _messagesBadge.Left = Math.Max(12, sidebar.ClientSize.Width - _messagesBadge.Width - 22);
        logout.Width = width;
        logout.Location = new Point(12, Math.Max(0, sidebar.ClientSize.Height - logout.Height - 18));
    }

    private void BuildTopbar()
    {
        topbar.Dock = DockStyle.Top;
        topbar.Height = 62;
        topbar.BackColor = Color.White;
        topbar.Paint += (_, e) =>
        {
            using var pen = new Pen(Line);
            e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
        };
        Controls.Add(topbar);

        backButton.Text = "←";
        backButton.Size = new Size(38, 38);
        backButton.Location = new Point(16, 12);
        backButton.FlatStyle = FlatStyle.Flat;
        backButton.BackColor = Page;
        backButton.ForeColor = Navy;
        backButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        backButton.Cursor = Cursors.Hand;
        backButton.UseVisualStyleBackColor = false;
        backButton.FlatAppearance.BorderSize = 0;
        backButton.Click += (_, _) => navigation.Back();
        topbar.Controls.Add(backButton);

        pageTitleLabel.Location = new Point(62, 18);
        pageTitleLabel.AutoSize = true;
        pageTitleLabel.ForeColor = Navy;
        pageTitleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        topbar.Controls.Add(pageTitleLabel);

        var search = MakeTopbarButton("⌕", "ShellSearchButton", 42, 38);
        search.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        search.Click += (_, _) => navigation.Navigate(Route.FindTasks);
        topbar.Controls.Add(search);

        var notifications = MakeTopbarButton("♧  •", "ShellNotificationsButton", 48, 38);
        notifications.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        notifications.Click += (_, _) => navigation.Navigate(Route.Notifications);
        topbar.Controls.Add(notifications);

        var avatar = new Label
        {
            Text = AppSession.CurrentUser.Initials,
            Size = new Size(36, 36),
            BackColor = Color.FromArgb(224, 240, 255),
            ForeColor = Blue,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topbar.Controls.Add(avatar);

        var user = new Label
        {
            Text = $"{AppSession.CurrentUser.FullName}\n{AppSession.CurrentUser.Role}",
            Size = new Size(135, 43),
            ForeColor = Navy,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        user.Click += (_, _) => navigation.Navigate(Route.Profile);
        topbar.Controls.Add(user);

        topbar.Resize += (_, _) =>
        {
            search.Location = new Point(topbar.ClientSize.Width - 396, 12);
            notifications.Location = new Point(topbar.ClientSize.Width - 344, 12);
            avatar.Location = new Point(topbar.ClientSize.Width - 288, 13);
            user.Location = new Point(topbar.ClientSize.Width - 244, 10);
        };
    }

    private void BuildToast()
    {
        toast.Size = new Size(430, 42);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        toast.Anchor = AnchorStyles.Bottom;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) =>
        {
            toastTimer.Stop();
            toast.Visible = false;
        };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    /// <summary>Updates sidebar highlight, topbar title and back-button state for a route.</summary>
    public void UpdateChrome(Route route)
    {
        var activeIndex = route switch
        {
            Route.Dashboard => 0,
            Route.PostTask => -1,
            Route.FindTasks or Route.UserTaskDetails or Route.SavedTasks => 1,
            Route.MyTasks or Route.MyRequests or Route.ManageRequests or Route.EditTask or Route.ArchivedTasks or Route.MyDisputes => 2,
            Route.Notifications => 3,
            Route.PaymentHistory or Route.PaymentRecord or Route.RecordPayment or Route.RateReview or Route.Earnings => 4,
            _ => 5
        };

        for (var index = 0; index < navButtons.Length; index++)
            navButtons[index].BackColor = index == activeIndex ? ActiveNavy : Color.Transparent;

        var postTask = sidebar.Controls.Find("ShellPostTaskButton", false).OfType<Button>().FirstOrDefault();
        if (postTask is not null)
            postTask.BackColor = route == Route.PostTask ? Color.FromArgb(6, 96, 196) : Blue;

        pageTitleLabel.Text = route switch
        {
            Route.Dashboard => "Dashboard",
            Route.FindTasks => "Find Tasks",
            Route.PostTask => "Post Task",
            Route.MyTasks => "My Tasks",
            Route.MyRequests => "My Tasks › Requests I Sent",
            Route.ManageRequests => "My Tasks › Requests Received",
            Route.EditTask => "My Tasks › Edit Task",
            Route.ArchivedTasks => "My Tasks › Archived Tasks",
            Route.Notifications => "Notifications",
            Route.Profile => "Profile",
            Route.AccountSettings => "Profile › Account Settings",
            Route.PublicProfile => "Profile › Public Profile",
            Route.Reviews => "Profile › Reviews",
            Route.PaymentHistory => "Payments › History",
            Route.PaymentRecord => "Payments › Record",
            Route.RecordPayment => "Payments › Record Payment",
            Route.RateReview => "Payments › Rate & Review",
            Route.Earnings => "Payments › Earnings",
            Route.UserTaskDetails => "Find Tasks › Task Details",
            Route.SavedTasks => "Saved Tasks",
            Route.MyDisputes => "My Disputes",
            Route.Support => "Support",
            Route.HelpFaq => "Help & FAQ",
            Route.CommunityGuidelines => "Community Guidelines",
            Route.Terms => "Terms of Service",
            Route.Privacy => "Privacy Policy",
            _ => "TaskTap"
        };

        backButton.Enabled = navigation.CanGoBack;
    }

    /// <summary>Ends the session and returns to the landing page, closing hidden windows.</summary>
    public void ExitToLanding()
    {
        if (_exitHandled)
            return;
        _exitHandled = true;
        DemoDataStore.BadgesChanged -= UpdateBadges;
        navigation.Reset();
        AppSession.SignOut();

        var landing = Application.OpenForms.OfType<Form1>().FirstOrDefault() ?? new Form1();
        foreach (var form in Application.OpenForms.Cast<Form>().ToList())
        {
            if (form is not Form1)
                form.Close();
        }

        landing.Show();
        landing.Activate();
    }

    private static Button MakeSidebarButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(196, 39),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(222, 234, 250),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            AutoEllipsis = false,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private Label AddSidebarBadge(int top)
    {
        var badge = new Label
        {
            Text = "",
            AutoSize = false,
            Size = new Size(20, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(255, 132, 77),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(178, top),
            Visible = false
        };
        sidebar.Controls.Add(badge);
        return badge;
    }

    private void UpdateBadges() => UpdateBadge(_messagesBadge, DemoDataStore.UnreadNotifications);

    private static void UpdateBadge(Label? badge, int count)
    {
        if (badge is null || badge.IsDisposed)
            return;
        badge.Text = count.ToString();
        badge.Visible = count > 0;
    }

    private static Button MakeTopbarButton(string text, string name, int width, int height)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = Page,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        toastTimer.Dispose();
        // Closing the shell (e.g. via the window X) must not leave hidden windows behind.
        ExitToLanding();
        base.OnFormClosed(e);
    }
}
