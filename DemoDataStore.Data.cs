using TaskTap.Data;

namespace TaskTap;

/// <summary>Loads the shared lists from MySQL for the guest, member or admin session.</summary>
public static partial class DemoDataStore
{
    private static DateTime _lastLoad = DateTime.MinValue;

    /// <summary>The posted task whose applicants Manage Requests is showing.</summary>
    public static DemoTask? ManagedTask { get; set; }

    /// <summary>The posted task the Edit Task page is editing.</summary>
    public static DemoTask? EditingTask { get; set; }

    private static void Replace<T>(List<T> target, IEnumerable<T> source)
    {
        var items = source.ToList();
        target.Clear();
        target.AddRange(items);
    }

    /// <summary>Lookup lists every visitor needs (categories, areas, announcements, settings, open tasks).</summary>
    public static void LoadPublicData()
    {
        if (!Db.IsReady) return;
        Db.Try(() =>
        {
            Replace(Categories, Backend.LoadCategories());
            Replace(Barangays, Backend.LoadBarangays());
            Replace(Announcements, Backend.LoadAnnouncements());
            Settings = Backend.LoadSettings();
            Replace(AvailableTasks, Backend.LoadMarketplace(0));
            _lastLoad = DateTime.Now;
        });
    }

    /// <summary>Loads everything a signed-in member sees. Guest saves made before login are carried over.</summary>
    public static void LoadForUser(AppUser user)
    {
        if (!Db.IsReady || user.Id == 0) return;
        var guestSaves = SavedTasks.ToList();
        var isAdminView = false;

        // Start from empty lists so a failed query can never leave another account's data on screen.
        ClearUserLists();

        Db.Try(() =>
        {
            LoadPublicData();
            Replace(AvailableTasks, Backend.LoadMarketplace(user.Id));
            Replace(PostedTasks, Backend.LoadPosted(user.Id));
            Replace(ArchivedTasks, Backend.LoadClosed(user.Id));
            Replace(TaskOutcomes, Backend.LoadOutcomes(user.Id));
            Replace(SavedTasks, Backend.LoadSaved(user.Id));
            Replace(RequestsSent, Backend.LoadSentRequests(user.Id));
            Replace(RequestsReceived, Backend.LoadReceivedRequests(user.Id));
            Replace(Payments, Backend.LoadPayments(user.Id, user.FullName, isAdminView));
            Replace(SupportTickets, Backend.LoadTickets(user.Id, user.FullName, false));
            Replace(Users, Backend.LoadUsers(false));
            Replace(Reports, []);
            Replace(Disputes, Backend.LoadDisputes().Where(d => d.PosterName == user.FullName || d.DoerName == user.FullName));
            Replace(ActivityLog, []);

            var pausedIds = Backend.LoadTaskIdsWhere("status", "Paused");
            PausedTasks.Clear();
            foreach (var task in PostedTasks.Where(task => pausedIds.Contains(task.Id)))
                PausedTasks.Add(task);
            FeaturedTasks.Clear();

            SetUnreadNotifications(Backend.UnreadNotificationCount(user.Id));
            _lastLoad = DateTime.Now;
        });

        foreach (var guest in guestSaves.Where(guest => !SavedTasks.Any(saved => saved.Title == guest.Title)))
        {
            var match = AvailableTasks.FirstOrDefault(task => task.Title == guest.Title);
            if (match is null) continue;
            SavedTasks.Add(match);
            Persist(() => Backend.SetSaved(user.Id, match.Id, true));
        }
        DataChanged?.Invoke();
    }

    /// <summary>Loads everything the admin portal manages.</summary>
    public static void LoadForAdmin(AppUser admin)
    {
        if (!Db.IsReady) return;
        Db.Try(() =>
        {
            LoadPublicData();
            Replace(AvailableTasks, Backend.LoadAllLiveTasks());
            Replace(PostedTasks, []);
            Replace(ArchivedTasks, Backend.LoadClosed(0));
            Replace(Users, Backend.LoadUsers(true));
            Replace(Reports, Backend.LoadReports());
            Replace(Disputes, Backend.LoadDisputes());
            Replace(ActivityLog, Backend.LoadActivity());
            Replace(SupportTickets, Backend.LoadTickets(0, "", true));
            Replace(Payments, Backend.LoadPayments(0, "", true));
            Replace(RequestsSent, []);
            Replace(RequestsReceived, []);
            Replace(SavedTasks, []);
            Replace(TaskOutcomes, []);

            var pausedIds = Backend.LoadTaskIdsWhere("status", "Paused");
            var featuredIds = Backend.LoadTaskIdsWhere("featured", "1");
            PausedTasks.Clear();
            FeaturedTasks.Clear();
            foreach (var task in AvailableTasks)
            {
                if (pausedIds.Contains(task.Id)) PausedTasks.Add(task);
                if (featuredIds.Contains(task.Id)) FeaturedTasks.Add(task);
            }

            AdminPermissions.Clear();
            foreach (var (role, permissions) in Backend.LoadPermissions())
                AdminPermissions[role] = permissions;

            _activitySequence = Math.Max(2002, Backend.MaxIdNumber("activity_log", "ACT-"));
            _lastLoad = DateTime.Now;
        });
        DataChanged?.Invoke();
    }

    /// <summary>Reloads the current session's data if it is older than <paramref name="maxAge"/>.</summary>
    public static void RefreshIfStale(TimeSpan maxAge)
    {
        if (!Db.IsReady || DateTime.Now - _lastLoad < maxAge) return;
        if (AppSession.IsAdminSession && AppSession.CurrentAdmin is { } admin)
            LoadForAdmin(admin);
        else if (AppSession.IsSignedIn)
            LoadForUser(AppSession.CurrentUser);
    }

    private static void ClearUserLists()
    {
        PostedTasks.Clear();
        ArchivedTasks.Clear();
        TaskOutcomes.Clear();
        SavedTasks.Clear();
        RequestsSent.Clear();
        RequestsReceived.Clear();
        Payments.Clear();
        SupportTickets.Clear();
        PausedTasks.Clear();
        FeaturedTasks.Clear();
    }

    /// <summary>Drops all user-specific data (used on sign-out).</summary>
    public static void ClearSessionData()
    {
        PostedTasks.Clear();
        ArchivedTasks.Clear();
        TaskOutcomes.Clear();
        SavedTasks.Clear();
        RequestsSent.Clear();
        RequestsReceived.Clear();
        Payments.Clear();
        SupportTickets.Clear();
        Users.Clear();
        Reports.Clear();
        Disputes.Clear();
        ActivityLog.Clear();
        PausedTasks.Clear();
        FeaturedTasks.Clear();
        SetUnreadNotifications(0);
        LoadPublicData();
    }
}
