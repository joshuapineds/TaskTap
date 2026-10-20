using TaskTap.Data;

namespace TaskTap;

/// <summary>
/// Admin-side slice of the shared repository (backed by MySQL). Everything here is read and
/// written by the admin portal, and the user-facing pages read the same lists.
/// </summary>
public static partial class DemoDataStore
{
    private static int _activitySequence = 2002;

    /// <summary>Administrator sub-roles available in the demo.</summary>
    public static readonly string[] AdminRoles = ["Super Admin", "Moderator", "Finance", "Support"];

    /// <summary>Permission keys shown in the role matrix.</summary>
    public static readonly string[] AdminPermissionKeys =
    [
        "Users", "Verification", "Tasks", "Categories", "Barangays",
        "Reports", "Disputes", "Payments", "Archives", "Activity", "Settings", "Roles"
    ];

    /// <summary>Role to granted permission keys.</summary>
    public static Dictionary<string, HashSet<string>> AdminPermissions { get; } = [];

    internal static Dictionary<string, HashSet<string>> CreateSeedPermissions() => new()
    {
        ["Super Admin"] = [.. AdminPermissionKeys],
        ["Moderator"] = ["Users", "Verification", "Tasks", "Reports", "Disputes", "Archives", "Activity"],
        ["Finance"] = ["Payments", "Disputes", "Archives", "Activity"],
        ["Support"] = ["Users", "Reports", "Activity"]
    };

    /// <summary>Community members managed by the admin portal.</summary>
    public static List<DemoUser> Users { get; } = [];

    /// <summary>Task categories shared with the user app's tiles and filters.</summary>
    public static List<DemoCategory> Categories { get; } = [];

    /// <summary>Barangays shared with the user app's address dropdowns.</summary>
    public static List<DemoBarangay> Barangays { get; } = [];

    /// <summary>Moderation reports raised against tasks, users, messages or reviews.</summary>
    public static List<DemoReport> Reports { get; } = [];

    /// <summary>Task disputes between posters and doers.</summary>
    public static List<DemoDispute> Disputes { get; } = [];

    /// <summary>Audit trail written by every admin mutation.</summary>
    public static List<DemoActivity> ActivityLog { get; } = [];

    /// <summary>Platform-wide settings edited from the admin Settings page.</summary>
    public static AdminSettings Settings { get; private set; } = new();

    /// <summary>Raised after any admin mutation so pages and the shell can refresh.</summary>
    public static event Action? DataChanged;

    public static void RaiseDataChanged() => DataChanged?.Invoke();

    /// <summary>Appends an audit entry and refreshes subscribers.</summary>
    public static void LogActivity(string actor, string action, string target, string category, string detail)
    {
        var entry = new DemoActivity
        {
            Id = $"ACT-{NextActivityNumber()}",
            Actor = actor,
            Action = action,
            Target = target,
            Category = category,
            Timestamp = DateTime.Now,
            Detail = detail
        };
        ActivityLog.Insert(0, entry);
        Persist(() => Backend.InsertActivity(entry));
        DataChanged?.Invoke();
    }

    private static int NextActivityNumber()
    {
        _activitySequence = Math.Max(_activitySequence, Db.IsReady ? Backend.MaxIdNumber("activity_log", "ACT-") : _activitySequence) + 1;
        return _activitySequence;
    }

    /// <summary>Saves a user row after an admin or verification change.</summary>
    private static void SaveUser(DemoUser user) => Persist(() => Backend.SaveUserAdminState(user));

    // ----- Users -------------------------------------------------------------

    public static void SuspendUser(DemoUser user)
    {
        user.Status = "Suspended";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Suspended user", user.FullName, "User", "Account suspended from the admin portal.");
    }

    public static void ActivateUser(DemoUser user)
    {
        user.Status = "Active";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Activated user", user.FullName, "User", "Account reactivated from the admin portal.");
    }

    public static void VerifyUser(DemoUser user)
    {
        user.Verified = true;
        user.VerificationRequested = false;
        user.RequestedAt = "";
        user.VerificationNote = "";
        if (user.Status == "Unverified")
            user.Status = "Active";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Verified user", user.FullName, "User", "Identity and contact details approved.");
    }

    /// <summary>Records a member's request for identity verification.</summary>
    public static void RequestVerification(DemoUser user, string note)
    {
        user.VerificationRequested = true;
        user.RequestedAt = DateTime.Today.ToString("MMM d, yyyy");
        user.VerificationNote = note;
        SaveUser(user);
        LogActivity(
            AppSession.CurrentUser.FullName,
            "Requested verification",
            user.FullName,
            "User",
            string.IsNullOrWhiteSpace(note) ? "Identity verification requested." : note);
    }

    public static void RejectVerification(DemoUser user, string? reason = null)
    {
        user.Verified = false;
        user.VerificationRequested = false;
        user.RequestedAt = "";
        user.VerificationNote = "";
        user.Status = "Unverified";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Rejected verification", user.FullName, "User", reason ?? "Verification request declined.");
    }

    public static void DeleteUser(DemoUser user)
    {
        Users.Remove(user);
        Persist(() => Backend.DeleteUser(user));
        LogActivity("TaskTap Admin", "Deleted user", user.FullName, "User", "Account permanently removed.");
    }

    /// <summary>Soft-deletes a member into the archives.</summary>
    public static void ArchiveUser(DemoUser user)
    {
        user.Status = "Deactivated";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Archived user", user.FullName, "User", "Account moved to archives.");
    }

    /// <summary>Restores an archived member to active status.</summary>
    public static void RestoreUser(DemoUser user)
    {
        user.Status = "Active";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Restored user", user.FullName, "User", "Account restored from archives.");
    }

    /// <summary>Removes a report from the resolved archive.</summary>
    public static void DeleteReport(DemoReport report)
    {
        Reports.Remove(report);
        Persist(() => Backend.DeleteReport(report));
        LogActivity("TaskTap Admin", "Deleted report", report.Id, "Report", "Report removed from the archive.");
    }

    // ----- Administrator roles ----------------------------------------------

    public static void PromoteToAdmin(DemoUser user, string adminRole)
    {
        user.Role = "Administrator";
        user.AdminRole = adminRole;
        if (user.Status == "Unverified")
            user.Status = "Active";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Promoted administrator", user.FullName, "Roles", $"{adminRole} access granted.");
    }

    public static void DemoteAdmin(DemoUser user)
    {
        if (Users.Count(candidate => candidate.IsAdministrator) <= 1)
            return;
        user.Role = "Standard User";
        user.AdminRole = "";
        SaveUser(user);
        LogActivity("TaskTap Admin", "Removed administrator", user.FullName, "Roles", "Administrator access removed.");
    }

    public static void SetAdminRole(DemoUser user, string adminRole)
    {
        user.AdminRole = adminRole;
        SaveUser(user);
        LogActivity("TaskTap Admin", "Changed administrator role", user.FullName, "Roles", $"Role set to {adminRole}.");
    }

    public static void SetRolePermission(string role, string permission, bool allowed)
    {
        if (!AdminPermissions.TryGetValue(role, out var granted))
            return;
        var changed = allowed ? granted.Add(permission) : granted.Remove(permission);
        if (changed)
        {
            Persist(() => Backend.SetPermission(role, permission, allowed));
            LogActivity("TaskTap Admin", "Updated permissions", role, "Roles", $"{permission} {(allowed ? "granted" : "revoked")}.");
        }
    }

    /// <summary>Active category names in display order, shared with the user app.</summary>
    public static string[] ActiveCategoryNames() =>
        [.. Categories.Where(category => category.Active).OrderBy(category => category.SortOrder).Select(category => category.Name)];

    /// <summary>Active barangay names, shared with the user app.</summary>
    public static string[] ActiveBarangayNames() =>
        [.. Barangays.Where(barangay => barangay.Active).OrderBy(barangay => barangay.Id).Select(barangay => barangay.Name)];

    // ----- Categories --------------------------------------------------------

    public static void AddCategory(DemoCategory category)
    {
        category.SortOrder = Categories.Count == 0 ? 1 : Categories.Max(c => c.SortOrder) + 1;
        Categories.Add(category);
        Persist(() => Backend.SaveCategory(category));
        LogActivity("TaskTap Admin", "Added category", category.Name, "Category", "New category available to the user app.");
    }

    public static void UpdateCategory(DemoCategory category)
    {
        Persist(() => Backend.SaveCategory(category));
        LogActivity("TaskTap Admin", "Updated category", category.Name, "Category", "Category details changed.");
    }

    public static void DeleteCategory(DemoCategory category)
    {
        Categories.Remove(category);
        Persist(() => Backend.DeleteCategory(category));
        LogActivity("TaskTap Admin", "Deleted category", category.Name, "Category", "Category removed from the user app.");
    }

    public static void ToggleCategory(DemoCategory category)
    {
        category.Active = !category.Active;
        Persist(() => Backend.SaveCategory(category));
        LogActivity("TaskTap Admin", category.Active ? "Activated category" : "Paused category", category.Name, "Category", "Category visibility changed.");
    }

    /// <summary>Moves a category one slot up (-1) or down (+1) in display order.</summary>
    public static void MoveCategory(DemoCategory category, int delta)
    {
        var ordered = Categories.OrderBy(c => c.SortOrder).ToList();
        var index = ordered.IndexOf(category);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= ordered.Count)
            return;
        (ordered[index], ordered[target]) = (ordered[target], ordered[index]);
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SortOrder = i + 1;
        Persist(() => { foreach (var item in ordered) Backend.SaveCategory(item); });
        LogActivity("TaskTap Admin", "Reordered categories", category.Name, "Category", "Category display order changed.");
    }

    // ----- Barangays ---------------------------------------------------------

    public static void AddBarangay(DemoBarangay barangay)
    {
        Barangays.Add(barangay);
        Persist(() => Backend.SaveBarangay(barangay));
        LogActivity("TaskTap Admin", "Added barangay", barangay.Name, "Barangay", "New service area available to the user app.");
    }

    public static void UpdateBarangay(DemoBarangay barangay)
    {
        Persist(() => Backend.SaveBarangay(barangay));
        LogActivity("TaskTap Admin", "Updated barangay", barangay.Name, "Barangay", "Barangay details changed.");
    }

    public static void DeactivateBarangay(DemoBarangay barangay)
    {
        barangay.Active = false;
        Persist(() => Backend.SaveBarangay(barangay));
        LogActivity("TaskTap Admin", "Deactivated barangay", barangay.Name, "Barangay", "Service area paused.");
    }

    public static void ActivateBarangay(DemoBarangay barangay)
    {
        barangay.Active = true;
        Persist(() => Backend.SaveBarangay(barangay));
        LogActivity("TaskTap Admin", "Activated barangay", barangay.Name, "Barangay", "Service area re-opened.");
    }

    // ----- Reports -----------------------------------------------------------

    public static void ResolveReport(DemoReport report, string resolution)
    {
        report.Status = "Resolved";
        report.Resolution = resolution;
        Persist(() => Backend.SaveReport(report));
        LogActivity("TaskTap Admin", "Resolved report", report.Id, "Report", resolution);
    }

    public static void DismissReport(DemoReport report)
    {
        report.Status = "Dismissed";
        report.Resolution = "No action needed.";
        Persist(() => Backend.SaveReport(report));
        LogActivity("TaskTap Admin", "Dismissed report", report.Id, "Report", "Report reviewed and dismissed.");
    }

    public static void EscalateReport(DemoReport report)
    {
        report.Status = "Investigating";
        Persist(() => Backend.SaveReport(report));
        LogActivity("TaskTap Admin", "Escalated report", report.Id, "Report", "Report moved to investigating.");
    }

    /// <summary>Files a report from a member (task, user, message or review) for the admin team.</summary>
    public static void SubmitReport(string targetType, string targetLabel, string reason, string description)
    {
        var next = Math.Max(1000, Db.IsReady ? Backend.MaxIdNumber("reports", "RPT-") : 1000) + 1;
        var report = new DemoReport
        {
            Id = $"RPT-{next}",
            ReporterName = AppSession.CurrentUser.FullName,
            TargetType = targetType,
            TargetLabel = targetLabel,
            Reason = reason,
            Severity = "Medium",
            Status = "Open",
            CreatedAt = DateTime.Now.ToString("MMM d, yyyy h:mm tt", System.Globalization.CultureInfo.InvariantCulture),
            Description = description
        };
        Reports.Insert(0, report);
        Persist(() => Backend.SaveReport(report));
        LogActivity(AppSession.CurrentUser.FullName, "Filed report", report.Id, "Report", reason);
    }

    // ----- Disputes ----------------------------------------------------------

    public static void ResolveDispute(DemoDispute dispute, string resolution)
    {
        dispute.Status = "Resolved";
        dispute.Resolution = resolution;
        Persist(() => Backend.SaveDispute(dispute));
        LogActivity("TaskTap Admin", "Resolved dispute", dispute.Id, "Dispute", resolution);
    }

    public static void RefundDispute(DemoDispute dispute)
    {
        dispute.Status = "Resolved";
        dispute.Resolution = "Refund approved.";
        Persist(() => Backend.SaveDispute(dispute));
        LogActivity("TaskTap Admin", "Refunded dispute", dispute.Id, "Dispute", "Refund approved for the poster.");
    }

    public static void DismissDispute(DemoDispute dispute)
    {
        dispute.Status = "Dismissed";
        dispute.Resolution = "No action needed.";
        Persist(() => Backend.SaveDispute(dispute));
        LogActivity("TaskTap Admin", "Dismissed dispute", dispute.Id, "Dispute", "Dispute reviewed and dismissed.");
    }

    /// <summary>Next dispute identifier for a user-opened dispute.</summary>
    public static string NextDisputeId()
    {
        var next = Disputes
            .Select(dispute => int.TryParse(dispute.Id.Replace("DSP-", string.Empty), out var number) ? number : 0)
            .DefaultIfEmpty(300)
            .Append(Db.IsReady ? Backend.MaxIdNumber("disputes", "DSP-") : 0)
            .Max() + 1;
        return $"DSP-{next}";
    }

    /// <summary>Records a dispute opened from a completed task.</summary>
    public static void OpenDispute(DemoDispute dispute)
    {
        Disputes.Insert(0, dispute);
        Persist(() => Backend.SaveDispute(dispute));
        LogActivity(AppSession.CurrentUser.FullName, "Opened dispute", dispute.Id, "Dispute", dispute.Reason);
    }

    // ----- Support tickets ---------------------------------------------------

    /// <summary>Support requests raised by members.</summary>
    public static List<DemoSupportTicket> SupportTickets { get; } = [];

    /// <summary>Next support ticket identifier.</summary>
    public static string NextTicketId()
    {
        var next = SupportTickets
            .Select(ticket => int.TryParse(ticket.Id.Replace("SUP-", string.Empty), out var number) ? number : 0)
            .DefaultIfEmpty(1000)
            .Append(Db.IsReady ? Backend.MaxIdNumber("support_tickets", "SUP-") : 0)
            .Max() + 1;
        return $"SUP-{next}";
    }

    /// <summary>Records a member support request.</summary>
    public static void RequestSupport(DemoSupportTicket ticket)
    {
        SupportTickets.Insert(0, ticket);
        Persist(() => Backend.SaveTicket(ticket));
        LogActivity(AppSession.CurrentUser.FullName, "Opened support ticket", ticket.Id, "Support", ticket.Subject);
    }

    public static void ReplyToTicket(DemoSupportTicket ticket, string reply)
    {
        ticket.Reply = reply;
        ticket.Status = "Answered";
        Persist(() => Backend.SaveTicket(ticket));
        LogActivity("TaskTap Admin", "Replied to ticket", ticket.Id, "Support", reply);
    }

    public static void ResolveTicket(DemoSupportTicket ticket)
    {
        ticket.Status = "Resolved";
        Persist(() => Backend.SaveTicket(ticket));
        LogActivity("TaskTap Admin", "Resolved ticket", ticket.Id, "Support", "Ticket closed as resolved.");
    }

    public static void CloseTicket(DemoSupportTicket ticket)
    {
        ticket.Status = "Closed";
        Persist(() => Backend.SaveTicket(ticket));
        LogActivity("TaskTap Admin", "Closed ticket", ticket.Id, "Support", "Ticket closed without a reply.");
    }

    // ----- Announcements -----------------------------------------------------

    /// <summary>Announcements published to members.</summary>
    public static List<DemoAnnouncement> Announcements { get; } = [];

    /// <summary>Next announcement identifier.</summary>
    public static string NextAnnouncementId()
    {
        var next = Announcements
            .Select(announcement => int.TryParse(announcement.Id.Replace("ANN-", string.Empty), out var number) ? number : 0)
            .DefaultIfEmpty(100)
            .Append(Db.IsReady ? Backend.MaxIdNumber("announcements", "ANN-") : 0)
            .Max() + 1;
        return $"ANN-{next}";
    }

    public static void PublishAnnouncement(DemoAnnouncement announcement)
    {
        Announcements.Insert(0, announcement);
        Persist(() => Backend.SaveAnnouncement(announcement));
        LogActivity("TaskTap Admin", "Published announcement", announcement.Id, "Announcement", announcement.Title);
    }

    public static void ToggleAnnouncement(DemoAnnouncement announcement)
    {
        announcement.Active = !announcement.Active;
        Persist(() => Backend.SaveAnnouncement(announcement));
        LogActivity(
            "TaskTap Admin",
            announcement.Active ? "Resumed announcement" : "Paused announcement",
            announcement.Id,
            "Announcement",
            announcement.Title);
    }

    public static void DeleteAnnouncement(DemoAnnouncement announcement)
    {
        Announcements.Remove(announcement);
        Persist(() => Backend.DeleteAnnouncement(announcement));
        LogActivity("TaskTap Admin", "Deleted announcement", announcement.Id, "Announcement", announcement.Title);
    }

    // ----- Payments ----------------------------------------------------------

    public static void MarkPaymentPaid(PaymentRecordDetails payment)
    {
        var index = Payments.IndexOf(payment);
        if (index < 0)
            return;
        Payments[index] = payment with { Status = "Paid" };
        Persist(() => Backend.SetPaymentStatus(payment.Reference, "Paid"));
        LogActivity("TaskTap Admin", "Marked payment paid", payment.Reference, "Payment", $"{payment.Amount:N0} recorded as paid.");
    }

    public static void VoidPayment(PaymentRecordDetails payment)
    {
        var index = Payments.IndexOf(payment);
        if (index < 0)
            return;
        Payments[index] = payment with { Status = "Void" };
        Persist(() => Backend.SetPaymentStatus(payment.Reference, "Void"));
        LogActivity("TaskTap Admin", "Voided payment", payment.Reference, "Payment", "Payment record voided.");
    }

    public static void RefundPayment(PaymentRecordDetails payment)
    {
        var index = Payments.IndexOf(payment);
        if (index < 0)
            return;
        Payments[index] = payment with { Status = "Refunded" };
        Persist(() => Backend.SetPaymentStatus(payment.Reference, "Refunded"));
        LogActivity("TaskTap Admin", "Refunded payment", payment.Reference, "Payment", "Payment refunded to the poster.");
    }

    // ----- Tasks -------------------------------------------------------------

    /// <summary>Admin-only highlight flags for the task table.</summary>
    public static HashSet<DemoTask> FeaturedTasks { get; } = [];

    /// <summary>Admin-only pause flags for the task table.</summary>
    public static HashSet<DemoTask> PausedTasks { get; } = [];

    private static readonly Dictionary<DemoTask, string> ArchivedFrom = new();

    /// <summary>Archives a task from either the marketplace or the posted list.</summary>
    public static void ArchiveAdminTask(DemoTask task)
    {
        if (PostedTasks.Remove(task))
            ArchivedFrom[task] = "Posted";
        else if (AvailableTasks.Remove(task))
            ArchivedFrom[task] = "Available";

        if (!ArchivedTasks.Contains(task))
            ArchivedTasks.Add(task);
        Persist(() => Backend.SetTaskStatus(task, "Archived"));

        LogActivity("TaskTap Admin", "Archived task", task.Title, "Task", "Listing moved to archives.");
    }

    /// <summary>Restores an archived task to the list it came from.</summary>
    public static void RestoreAdminTask(DemoTask task)
    {
        ArchivedTasks.Remove(task);
        var from = ArchivedFrom.Remove(task, out var value) ? value : (AppSession.IsAdminSession ? "Available" : "Posted");
        Persist(() => Backend.SetTaskStatus(task, "Active"));
        if (from == "Available")
        {
            if (!AvailableTasks.Contains(task))
                AvailableTasks.Add(task);
        }
        else
        {
            if (!PostedTasks.Contains(task))
                PostedTasks.Add(task);
        }

        LogActivity("TaskTap Admin", "Restored task", task.Title, "Task", "Listing restored to the marketplace.");
    }

    public static void ToggleFeatured(DemoTask task)
    {
        var featured = !FeaturedTasks.Remove(task);
        if (featured)
            FeaturedTasks.Add(task);
        Persist(() => Backend.SetFeatured(task, featured));
        LogActivity("TaskTap Admin", featured ? "Featured task" : "Unfeatured task", task.Title, "Task", "Marketplace highlight changed.");
    }

    public static void TogglePaused(DemoTask task)
    {
        var paused = !PausedTasks.Remove(task);
        if (paused)
            PausedTasks.Add(task);
        Persist(() => Backend.SetTaskStatus(task, paused ? "Paused" : "Active"));
        LogActivity("TaskTap Admin", paused ? "Paused task" : "Resumed task", task.Title, "Task", "Listing visibility changed.");
    }

    public static void DeleteTask(DemoTask task)
    {
        AvailableTasks.Remove(task);
        PostedTasks.Remove(task);
        ArchivedTasks.Remove(task);
        Persist(() => Backend.DeleteTask(task));
        LogActivity("TaskTap Admin", "Deleted task", task.Title, "Task", "Task removed from the marketplace.");
    }

    // ----- Settings and demo reset ------------------------------------------

    public static void UpdateSettings(AdminSettings settings)
    {
        Settings = settings;
        Persist(() => Backend.SaveSettings(settings));
        LogActivity("TaskTap Admin", "Updated settings", settings.PlatformName, "Settings", "Platform settings saved.");
    }

    // ----- Seeds -------------------------------------------------------------

    internal static List<DemoCategory> CreateSeedCategories() =>
    [
        new() { Id = "CAT-01", Name = "Cleaning", Icon = "✦", Color = Color.FromArgb(232, 241, 255), Accent = Color.FromArgb(8, 117, 232), Active = true, SortOrder = 1 },
        new() { Id = "CAT-02", Name = "Tutoring", Icon = "▤", Color = Color.FromArgb(234, 247, 242), Accent = Color.FromArgb(20, 153, 111), Active = true, SortOrder = 2 },
        new() { Id = "CAT-03", Name = "Repairs", Icon = "↗", Color = Color.FromArgb(255, 240, 230), Accent = Color.FromArgb(219, 108, 32), Active = true, SortOrder = 3 },
        new() { Id = "CAT-04", Name = "Shopping", Icon = "▥", Color = Color.FromArgb(233, 249, 251), Accent = Color.FromArgb(19, 166, 183), Active = true, SortOrder = 4 },
        new() { Id = "CAT-05", Name = "Pet Care", Icon = "♞", Color = Color.FromArgb(255, 243, 232), Accent = Color.FromArgb(238, 162, 14), Active = true, SortOrder = 5 },
        new() { Id = "CAT-06", Name = "Moving", Icon = "⇄", Color = Color.FromArgb(241, 239, 255), Accent = Color.FromArgb(139, 85, 223), Active = true, SortOrder = 6 },
        new() { Id = "CAT-07", Name = "Delivery", Icon = "▣", Color = Color.FromArgb(241, 239, 255), Accent = Color.FromArgb(123, 97, 209), Active = true, SortOrder = 7 },
        new() { Id = "CAT-08", Name = "Others", Icon = "•••", Color = Color.FromArgb(244, 246, 249), Accent = Color.FromArgb(110, 128, 150), Active = true, SortOrder = 8 }
    ];

    internal static List<DemoBarangay> CreateSeedBarangays() =>
    [
        new() { Id = "BRY-176", Name = "Brgy. 176", City = "Caloocan City", Active = true },
        new() { Id = "BRY-177", Name = "Brgy. 177", City = "Caloocan City", Active = true },
        new() { Id = "BRY-178", Name = "Brgy. 178", City = "Caloocan City", Active = true },
        new() { Id = "BRY-179", Name = "Brgy. 179", City = "Caloocan City", Active = true },
        new() { Id = "BRY-180", Name = "Brgy. 180", City = "Caloocan City", Active = true }
    ];
}
