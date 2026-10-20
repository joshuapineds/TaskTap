namespace TaskTap;

/// <summary>
/// The single admin window. It owns the admin sidebar, topbar, content host and
/// admin sign-out, and delegates all page transitions to
/// <see cref="AdminNavigationService"/>.
/// </summary>
public sealed class AdminShellForm : Form
{
    private readonly Panel sidebar = new();
    private readonly Panel topbar = new();
    private readonly Panel contentHost = new();
    private readonly Button[] navButtons = new Button[13];
    private readonly Button backButton = new();
    private readonly Label pageTitleLabel = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private readonly AdminNavigationService navigation = AdminNavigationService.Current;
    private bool _exitHandled;

    private static readonly (string Glyph, string Label, AdminRoute Route)[] NavItems =
    [
        ("⌂", "Dashboard", AdminRoute.Dashboard),
        ("◉", "Users", AdminRoute.Users),
        ("▤", "Tasks", AdminRoute.Tasks),
        ("✦", "Categories", AdminRoute.Categories),
        ("⚑", "Reports", AdminRoute.Reports),
        ("₱", "Payments", AdminRoute.Payments),
        ("▣", "Archives", AdminRoute.Archives),
        ("⌖", "Barangays", AdminRoute.Barangays),
        ("◷", "Activity Log", AdminRoute.ActivityLog),
        ("⚙", "Settings", AdminRoute.Settings),
        ("✉", "Announcements", AdminRoute.Announcements),
        ("☎", "Support Tickets", AdminRoute.SupportTickets),
        ("◎", "Admin Profile", AdminRoute.AdminProfile)
    ];

    public Panel ContentHost => contentHost;

    public AdminShellForm()
    {
        BuildShell();
        navigation.Attach(this);
        navigation.Navigate(AdminRoute.Dashboard);
    }

    private void BuildShell()
    {
        Text = "TaskTap Admin";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 760);
        ClientSize = new Size(1540, 900);
        BackColor = AdminUi.Page;
        Font = new Font("Segoe UI", 9F);

        contentHost.Dock = DockStyle.Fill;
        contentHost.BackColor = AdminUi.Page;
        Controls.Add(contentHost);

        BuildTopbar();
        BuildSidebar();
        BuildToast();
    }

    private void BuildSidebar()
    {
        sidebar.Dock = DockStyle.Left;
        sidebar.Width = 240;
        sidebar.BackColor = AdminUi.SidebarNavy;
        Controls.Add(sidebar);

        var brand = new TaskTapLogo
        {
            Name = "adminShellBrand",
            TextColor = Color.White,
            BackColor = AdminUi.SidebarNavy,
            Location = new Point(22, 20),
            Size = new Size(170, 36)
        };
        sidebar.Controls.Add(brand);

        var adminBadge = new Label
        {
            Name = "adminSidebarBadge",
            Text = "ADMIN",
            AutoSize = true,
            BackColor = AdminUi.AdminBadgeBack,
            ForeColor = AdminUi.AdminBadgeFore,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Padding = new Padding(8, 3, 8, 3),
            Location = new Point(22, 64)
        };
        sidebar.Controls.Add(adminBadge);

        for (var index = 0; index < NavItems.Length; index++)
        {
            var item = NavItems[index];
            var button = MakeSidebarButton($"{item.Glyph}   {item.Label}");
            button.Location = new Point(12, 106 + index * 43);
            var captured = item.Route;
            button.Click += (_, _) => navigation.Navigate(captured);
            navButtons[index] = button;
            sidebar.Controls.Add(button);
        }

        var returnLink = MakeSidebarButton("↩   Return to TaskTap");
        returnLink.Name = "adminReturnLink";
        returnLink.Click += (_, _) => ReturnToTaskTap();
        sidebar.Controls.Add(returnLink);

        var signOut = MakeSidebarButton("↪   Sign out");
        signOut.Name = "adminSignOut";
        signOut.Click += (_, _) => ExitToAdminLogin();
        sidebar.Controls.Add(signOut);

        for (var index = 0; index < navButtons.Length; index++)
            navButtons[index].Visible = navigation.CanAccess(NavItems[index].Route);

        sidebar.Resize += (_, _) =>
        {
            returnLink.Location = new Point(12, Math.Max(560, sidebar.Height - 96));
            signOut.Location = new Point(12, Math.Max(600, sidebar.Height - 56));
        };
    }

    private void BuildTopbar()
    {
        topbar.Dock = DockStyle.Top;
        topbar.Height = 62;
        topbar.BackColor = Color.White;
        topbar.Paint += (_, e) =>
        {
            using var pen = new Pen(AdminUi.Line);
            e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
        };
        Controls.Add(topbar);

        backButton.Text = "←";
        backButton.Size = new Size(38, 38);
        backButton.Location = new Point(16, 12);
        backButton.FlatStyle = FlatStyle.Flat;
        backButton.BackColor = AdminUi.Page;
        backButton.ForeColor = AdminUi.Navy;
        backButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        backButton.Cursor = Cursors.Hand;
        backButton.UseVisualStyleBackColor = false;
        backButton.FlatAppearance.BorderSize = 0;
        backButton.Click += (_, _) => navigation.Back();
        topbar.Controls.Add(backButton);

        pageTitleLabel.Location = new Point(62, 18);
        pageTitleLabel.AutoSize = true;
        pageTitleLabel.ForeColor = AdminUi.Navy;
        pageTitleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        topbar.Controls.Add(pageTitleLabel);

        var adminPill = new Label
        {
            Name = "adminTopbarPill",
            Text = "ADMIN",
            AutoSize = true,
            BackColor = AdminUi.AdminBadgeBack,
            ForeColor = AdminUi.AdminBadgeFore,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Padding = new Padding(8, 3, 8, 3),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topbar.Controls.Add(adminPill);

        var avatar = new Label
        {
            Name = "adminTopbarAvatar",
            Text = AppSession.CurrentAdmin?.Initials ?? "TA",
            Size = new Size(36, 36),
            BackColor = Color.FromArgb(255, 243, 232),
            ForeColor = AdminUi.Warn,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topbar.Controls.Add(avatar);

        var adminName = new Label
        {
            Name = "adminTopbarName",
            Text = $"{AppSession.CurrentAdmin?.FullName ?? "TaskTap Admin"}\nAdministrator",
            Size = new Size(150, 43),
            ForeColor = AdminUi.Navy,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topbar.Controls.Add(adminName);

        topbar.Resize += (_, _) =>
        {
            adminPill.Location = new Point(topbar.ClientSize.Width - 400, 20);
            avatar.Location = new Point(topbar.ClientSize.Width - 300, 13);
            adminName.Location = new Point(topbar.ClientSize.Width - 256, 10);
        };
    }

    private void BuildToast()
    {
        toast.Size = new Size(430, 42);
        toast.BackColor = AdminUi.Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
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
    public void UpdateChrome(AdminRoute route)
    {
        var activeIndex = route switch
        {
            AdminRoute.Dashboard => 0,
            AdminRoute.Users or AdminRoute.UserDetail or AdminRoute.VerificationQueue => 1,
            AdminRoute.Tasks => 2,
            AdminRoute.Categories => 3,
            AdminRoute.Reports or AdminRoute.Disputes => 4,
            AdminRoute.Payments => 5,
            AdminRoute.Archives => 6,
            AdminRoute.Barangays => 7,
            AdminRoute.ActivityLog => 8,
            AdminRoute.Settings or AdminRoute.RolesPermissions => 9,
            AdminRoute.Announcements => 10,
            AdminRoute.SupportTickets => 11,
            AdminRoute.AdminProfile => 12,
            _ => 0
        };

        for (var index = 0; index < navButtons.Length; index++)
            navButtons[index].BackColor = index == activeIndex ? AdminUi.ActiveNavy : Color.Transparent;

        pageTitleLabel.Text = route switch
        {
            AdminRoute.Dashboard => "Dashboard",
            AdminRoute.Users => "User Management",
            AdminRoute.UserDetail => "Users › User Detail",
            AdminRoute.VerificationQueue => "Users › Verification Queue",
            AdminRoute.RolesPermissions => "Settings › Roles & Permissions",
            AdminRoute.Tasks => "Task Management",
            AdminRoute.Categories => "Category Management",
            AdminRoute.Reports => "Report Management",
            AdminRoute.Disputes => "Reports › Disputes",
            AdminRoute.Payments => "Payment Records",
            AdminRoute.Archives => "Archives",
            AdminRoute.Barangays => "Barangay Management",
            AdminRoute.ActivityLog => "Activity Log",
            AdminRoute.Settings => "Settings",
            AdminRoute.Announcements => "Announcements",
            AdminRoute.SupportTickets => "Support Tickets",
            AdminRoute.AdminProfile => "Admin Profile",
            _ => "Admin"
        };

        backButton.Enabled = navigation.CanGoBack;
    }

    /// <summary>Ends the admin session and returns to the admin login form.</summary>
    public void ExitToAdminLogin()
    {
        if (_exitHandled)
            return;
        _exitHandled = true;
        navigation.Reset();
        AppSession.SignOutAdmin();
        Close();
    }

    /// <summary>Ends the admin session and returns to the TaskTap landing page.</summary>
    public void ReturnToTaskTap()
    {
        if (_exitHandled)
            return;
        _exitHandled = true;
        navigation.Reset();
        AppSession.SignOutAdmin();

        var landing = Application.OpenForms.OfType<Form1>().FirstOrDefault() ?? new Form1();
        foreach (var form in Application.OpenForms.Cast<Form>().ToList())
        {
            if (form is not Form1)
                form.Close();
        }

        landing.Show();
        landing.Activate();
        landing.BringToFront();
    }

    public void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private static Button MakeSidebarButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(216, 39),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(222, 234, 250),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        toastTimer.Dispose();
        if (!_exitHandled)
        {
            navigation.Reset();
            AppSession.SignOutAdmin();
        }
        base.OnFormClosed(e);
    }
}
