namespace TaskTap;

/// <summary>
/// Admin counterpart to <see cref="NavigationService"/>. It swaps pages inside the
/// admin shell's content host, keeps a back stack, and owns admin logout.
/// </summary>
public sealed class AdminNavigationService
{
    private readonly Stack<AdminPage> _backStack = new();
    private readonly Dictionary<AdminRoute, AdminPage> _singletons = new();
    private AdminShellForm? _shell;

    /// <summary>Process-wide admin navigation owner (one admin shell at a time).</summary>
    public static AdminNavigationService Current { get; } = new();

    public AdminShellForm? Shell => _shell;
    public AdminPage? CurrentPage { get; private set; }
    public bool CanGoBack => _backStack.Count > 0;

    public event Action<AdminRoute>? Navigated;

    public void Attach(AdminShellForm shell) => _shell = shell;

    /// <summary>Navigate to a global (singleton) admin route, reusing its page instance.</summary>
    public void Navigate(AdminRoute route)
    {
        if (!CanAccess(route))
        {
            _shell?.ShowToast("You do not have permission to open this section.");
            return;
        }
        if (!_singletons.TryGetValue(route, out var page))
        {
            page = Create(route);
            _singletons[route] = page;
        }

        Show(page, pushCurrent: true);
    }

    /// <summary>Navigate to a contextual admin page instance supplied by the caller.</summary>
    public void Navigate(AdminPage page) => Show(page, pushCurrent: true);

    /// <summary>Return to the previous admin page, restoring it exactly as it was left.</summary>
    public bool Back()
    {
        if (_backStack.Count == 0)
            return false;

        DemoDataStore.RefreshIfStale(TimeSpan.Zero);
        Show(_backStack.Pop(), pushCurrent: false);
        return true;
    }

    public void GoHome() => Navigate(AdminRoute.Dashboard);

    public bool CanAccess(AdminRoute route) => route switch
    {
        AdminRoute.Dashboard or AdminRoute.AdminProfile => true,
        AdminRoute.Users or AdminRoute.UserDetail or AdminRoute.VerificationQueue => DemoDataStore.HasAdminPermission("Users"),
        AdminRoute.Tasks => DemoDataStore.HasAdminPermission("Tasks"),
        AdminRoute.Categories => DemoDataStore.HasAdminPermission("Categories"),
        AdminRoute.Barangays => DemoDataStore.HasAdminPermission("Barangays"),
        AdminRoute.Reports => DemoDataStore.HasAdminPermission("Reports"),
        AdminRoute.Disputes => DemoDataStore.HasAdminPermission("Disputes"),
        AdminRoute.Payments => DemoDataStore.HasAdminPermission("Payments"),
        AdminRoute.Archives => DemoDataStore.HasAdminPermission("Archives"),
        AdminRoute.ActivityLog => DemoDataStore.HasAdminPermission("Activity"),
        AdminRoute.Settings => DemoDataStore.HasAdminPermission("Settings"),
        AdminRoute.RolesPermissions => DemoDataStore.HasAdminPermission("Roles"),
        AdminRoute.Announcements => DemoDataStore.HasAdminPermission("Settings"),
        AdminRoute.SupportTickets => DemoDataStore.HasAdminPermission("Support"),
        _ => false
    };

    public void Reset()
    {
        _backStack.Clear();
        _singletons.Clear();
        CurrentPage = null;
    }

    public void Logout()
    {
        Reset();
        _shell?.ExitToAdminLogin();
    }

    private void Show(AdminPage page, bool pushCurrent)
    {
        var host = _shell?.ContentHost;
        if (host is null)
            return;

        if (pushCurrent && CurrentPage is not null && !ReferenceEquals(CurrentPage, page))
            _backStack.Push(CurrentPage);

        if (CurrentPage is not null && CurrentPage != page)
            host.Controls.Remove(CurrentPage);

        CurrentPage = page;
        page.Dock = DockStyle.Fill;
        if (!host.Controls.Contains(page))
            host.Controls.Add(page);
        page.BringToFront();
        page.OnNavigatedTo();
        _shell!.UpdateChrome(page.AdminRouteName);
        Navigated?.Invoke(page.AdminRouteName);
    }

    private static AdminPage Create(AdminRoute route) => route switch
    {
        AdminRoute.Dashboard => new AdminDashboardPage(),
        AdminRoute.Users => new UserManagementPage(),
        AdminRoute.Tasks => new TaskManagementPage(),
        AdminRoute.Categories => new CategoryManagementPage(),
        AdminRoute.Reports => new ReportManagementPage(),
        AdminRoute.Payments => new PaymentRecordsPage(),
        AdminRoute.Archives => new ArchivesPage(),
        AdminRoute.Barangays => new BarangayManagementPage(),
        AdminRoute.ActivityLog => new ActivityLogPage(),
        AdminRoute.Settings => new AdminSettingsPage(),
        AdminRoute.Announcements => new AdminAnnouncementsPage(),
        AdminRoute.SupportTickets => new SupportTicketsPage(),
        AdminRoute.AdminProfile => new AdminProfilePage(),
        AdminRoute.UserDetail => new AdminPlaceholderPage(AdminRoute.UserDetail, "User Detail"),
        AdminRoute.VerificationQueue => new VerificationQueuePage(),
        AdminRoute.RolesPermissions => new RolesPermissionsPage(),
        AdminRoute.Disputes => new DisputesPage(),
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Not a singleton admin route.")
    };
}
