namespace TaskTap;

/// <summary>
/// Application-level navigation owner. It swaps pages inside the shell's content
/// host, keeps a back stack so returning from a sibling restores the origin page
/// (with its filters/tab/scroll state intact), and owns logout.
/// </summary>
public sealed class NavigationService
{
    private readonly Stack<AppPage> _backStack = new();
    private readonly Dictionary<Route, AppPage> _singletons = new();
    private MainShellForm? _shell;

    /// <summary>Process-wide navigation owner (one shell at a time).</summary>
    public static NavigationService Current { get; } = new();

    public MainShellForm? Shell => _shell;
    public AppPage? CurrentPage { get; private set; }
    public bool CanGoBack => _backStack.Count > 0;

    public event Action<Route>? Navigated;

    public void Attach(MainShellForm shell) => _shell = shell;

    /// <summary>Navigate to a route; global pages are cached, contextual pages are created fresh.</summary>
    public void Navigate(Route route)
    {
        DemoDataStore.RefreshIfStale(TimeSpan.FromSeconds(10));
        Show(Resolve(route), pushCurrent: true);
    }

    /// <summary>Navigate to a contextual page instance supplied by the caller.</summary>
    public void Navigate(AppPage page) => Show(page, pushCurrent: true);

    /// <summary>Switch between sibling tabs (e.g. Payments › History / Earnings) without growing the back stack.</summary>
    public void Swap(Route route) => Show(Resolve(route), pushCurrent: false);

    /// <summary>Resolves a route to a page, reusing singletons and creating contextual pages on demand.</summary>
    private AppPage Resolve(Route route)
    {
        if (_singletons.TryGetValue(route, out var cached))
            return cached;

        if (!IsSingleton(route))
            return CreateContextual(route);

        var created = Create(route);
        _singletons[route] = created;
        return created;
    }

    // Only static content is cached. Pages that show database data are rebuilt on every visit
    // so they always reflect what is stored in MySQL (including changes from other members).
    private static bool IsSingleton(Route route) => route switch
    {
        Route.HelpFaq or Route.CommunityGuidelines or Route.Terms or Route.Privacy => true,
        _ => false
    };

    /// <summary>Creates the parameterless pages that are rebuilt on every visit.</summary>
    private static AppPage CreateContextual(Route route) => route switch
    {
        Route.Dashboard => new DashboardPage(),
        Route.FindTasks => new FindTasksPage(),
        Route.PostTask => new PostTaskPage(),
        Route.MyTasks => new MyTasksPage(),
        Route.Messages => new MessagesPage(),
        Route.Notifications => new NotificationsPage(),
        Route.Profile => new ProfilePage(),
        Route.SavedTasks => new SavedTasksPage(),
        Route.MyDisputes => new MyDisputesPage(),
        Route.Earnings => new EarningsPage(),
        Route.Support => new SupportPage(),
        Route.AccountSettings => new AccountSettingsPage(),
        Route.PublicProfile => new PublicProfilePage(),
        Route.Reviews => new ReviewsPage(),
        Route.PaymentHistory => new PaymentHistoryPage(),
        Route.ArchivedTasks => new ArchivedTasksPage(),
        Route.MyRequests => new MyRequestsPage(),
        Route.ManageRequests => new ManageRequestsPage(),
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Not a navigable route.")
    };

    /// <summary>Return to the previous page, restoring it exactly as it was left.</summary>
    public bool Back()
    {
        if (_backStack.Count == 0)
            return false;

        Show(_backStack.Pop(), pushCurrent: false);
        return true;
    }

    public void GoHome() => Navigate(Route.Dashboard);

    public void Reset()
    {
        _backStack.Clear();
        _singletons.Clear();
        CurrentPage = null;
    }

    public void Logout()
    {
        Reset();
        _shell?.ExitToLanding();
    }

    private void Show(AppPage page, bool pushCurrent)
    {
        var host = _shell?.ContentHost;
        if (host is null)
            return;

        if (pushCurrent && CurrentPage is not null && !ReferenceEquals(CurrentPage, page))
            _backStack.Push(CurrentPage);

        if (CurrentPage is not null && CurrentPage != page)
        {
            CurrentPage.OnNavigatedFrom();
            host.Controls.Remove(CurrentPage);
        }

        CurrentPage = page;
        page.Dock = DockStyle.Fill;
        if (!host.Controls.Contains(page))
            host.Controls.Add(page);
        page.BringToFront();
        page.OnNavigatedTo();
        _shell!.UpdateChrome(page.RouteName);
        Navigated?.Invoke(page.RouteName);
    }

    /// <summary>Creates a cached (singleton) page; only static content is cached.</summary>
    private static AppPage Create(Route route) => route switch
    {
        Route.HelpFaq => new InfoArticlePage(Route.HelpFaq, InfoArticleCatalog.HelpFaq),
        Route.CommunityGuidelines => new InfoArticlePage(Route.CommunityGuidelines, InfoArticleCatalog.CommunityGuidelines),
        Route.Terms => new InfoArticlePage(Route.Terms, InfoArticleCatalog.Terms),
        Route.Privacy => new InfoArticlePage(Route.Privacy, InfoArticleCatalog.Privacy),
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Not a singleton route.")
    };
}
