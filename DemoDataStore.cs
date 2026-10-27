using TaskTap.Data;

namespace TaskTap;

/// <summary>A task discoverable through Find Tasks / the dashboard, or posted by the user.</summary>
public sealed record DemoTask(
    string Title,
    string Category,
    int Price,
    string Barangay,
    int Days,
    string Deadline,
    string Icon,
    string Description,
    Color IconBack,
    Point MapPoint)
{
    /// <summary>Database id (0 until the task has been saved).</summary>
    public int Id { get; set; }
    public int PosterId { get; set; }
    public string PosterName { get; set; } = "";
    public string Requirements { get; set; } = "";
    public string TaskType { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string BudgetType { get; set; } = "";
    public string Duration { get; set; } = "";
    public string ScheduleNotes { get; set; } = "";
    public string StartTime { get; set; } = "";

    /// <summary>Exact pin chosen by the poster (null = barangay area only). Never shown to other members.</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>A task the poster completed or cancelled, kept for history and disputes.</summary>
public sealed record DemoTaskOutcome(
    string Title,
    string Category,
    int Amount,
    string Barangay,
    string Status,
    DateTime Date,
    string Reason,
    string Description,
    string Icon,
    Color IconBack);

/// <summary>A request the current user has sent (doer journey).</summary>
public sealed class DemoRequest
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskStatus { get; set; } = "Active";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "";
    public string Barangay { get; init; } = "";
    public int Amount { get; init; }
    public string Status { get; set; } = "pending";
    public string Deadline { get; init; } = "";
    public DateTime Date { get; init; }
    public string RequestDate { get; init; } = "";
    public string Description { get; init; } = "";
    public string AttachmentName { get; init; } = "";
    public int AttachmentSize { get; init; }
    public string Icon { get; init; } = "";
    public Color IconBack { get; init; }
    public bool Withdrawn { get; set; }
}

/// <summary>An application the current user received on a posted task (poster journey).</summary>
public sealed class DemoApplicant
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int DoerId { get; set; }
    public string TaskTitle { get; init; } = "";
    public string Name { get; init; } = "";
    public string Initials { get; init; } = "";
    public double Rating { get; init; }
    public int Reviews { get; init; }
    public int Completed { get; init; }
    public string RequestedAt { get; init; } = "";
    public string AttachmentName { get; init; } = "";
    public int AttachmentSize { get; init; }
    public string[] Skills { get; init; } = [];
    public string Barangay { get; init; } = "";
    public string MemberSince { get; init; } = "";
    public DateTime Date { get; init; }
    public Color AvatarColor { get; init; }
    public bool Shortlisted { get; set; }
    public bool Declined { get; set; }
    public bool Accepted { get; set; }
}

/// <summary>
/// Shared application state. Pages read these lists; every mutation here is written through to
/// MySQL (see <see cref="Backend"/>), and the lists are reloaded from the database at sign-in
/// (see DemoDataStore.Data.cs). The class name is kept so existing pages keep working.
/// </summary>
public static partial class DemoDataStore
{
    /// <summary>Tasks a signed-in user can browse (Find Tasks + Dashboard "Tasks for You").</summary>
    public static List<DemoTask> AvailableTasks { get; } = [];

    /// <summary>Tasks the current user has posted (My Tasks "Posted" tab).</summary>
    public static List<DemoTask> PostedTasks { get; } = [];

    /// <summary>Requests the current user has sent (My Requests).</summary>
    public static List<DemoRequest> RequestsSent { get; } = [];

    /// <summary>Applications the current user received on posted tasks (Manage Requests).</summary>
    public static List<DemoApplicant> RequestsReceived { get; } = [];

    /// <summary>Recorded payments (Payment History reads this; Record Payment appends to it).</summary>
    public static List<PaymentRecordDetails> Payments { get; } = [];

    private static int _unreadNotifications;

    /// <summary>Unread badge counts shown in the shared sidebar (kept live by the pages).</summary>
    public static int UnreadNotifications => _unreadNotifications;

    /// <summary>Raised when unread counts change so the shell can refresh its badges.</summary>
    public static event Action? BadgesChanged;

    public static void SetUnreadNotifications(int count)
    {
        if (_unreadNotifications == count) return;
        _unreadNotifications = Math.Max(0, count);
        BadgesChanged?.Invoke();
    }

    /// <summary>Tasks the user archived (moved out of the active posted list).</summary>
    public static List<DemoTask> ArchivedTasks { get; } = [];

    private static int CurrentUserId => AppSession.CurrentUser.Id;

    /// <summary>The task id for a snapshot, resolving by title when the snapshot was built without one.</summary>
    public static int ResolveTaskId(DemoTask task)
    {
        if (task.Id > 0)
            return task.Id;
        var match = AvailableTasks.Concat(PostedTasks).Concat(ArchivedTasks).FirstOrDefault(candidate => candidate.Title == task.Title);
        return match?.Id ?? 0;
    }

    /// <summary>Publishes a newly created task: saves it to MySQL, then adds it to the posted list.</summary>
    public static void PostTask(DemoTask task)
    {
        task.PosterId = CurrentUserId;
        task.PosterName = AppSession.CurrentUser.FullName;
        var saved = Db.Try(() => task.Id = Backend.InsertTask(task, CurrentUserId));
        if (!saved)
            return;
        PostedTasks.Insert(0, task);
        LogActivity(AppSession.CurrentUser.FullName, "Posted task", task.Title, "Task", "New task published.");
    }

    /// <summary>Replaces a posted task with an edited copy (and saves it).</summary>
    public static void UpdatePostedTask(DemoTask original, DemoTask updated, bool paused)
    {
        updated.Id = original.Id;
        updated.PosterId = original.PosterId;
        updated.PosterName = original.PosterName;
        Persist(() =>
        {
            Backend.UpdateTask(updated);
            Backend.SetTaskStatus(updated, paused ? "Paused" : "Active");
        });

        var wasFeatured = FeaturedTasks.Remove(original);
        PausedTasks.Remove(original);
        var index = PostedTasks.IndexOf(original);
        if (index >= 0)
            PostedTasks[index] = updated;
        if (wasFeatured)
            FeaturedTasks.Add(updated);
        if (paused)
            PausedTasks.Add(updated);
        LogActivity(AppSession.CurrentUser.FullName, "Updated task", updated.Title, "Task", "Task details changed.");
    }

    /// <summary>Archives a posted task (a history/visibility action, distinct from completion).</summary>
    public static void ArchiveTask(DemoTask task)
    {
        Persist(() => Backend.SetTaskStatus(task, "Archived"));
        PostedTasks.Remove(task);
        if (!ArchivedTasks.Contains(task))
            ArchivedTasks.Add(task);
    }

    /// <summary>Restores an archived task back to the active posted list.</summary>
    public static void RestoreTask(DemoTask task)
    {
        Persist(() => Backend.SetTaskStatus(task, "Active"));
        ArchivedTasks.Remove(task);
        if (!PostedTasks.Contains(task))
            PostedTasks.Add(task);
    }

    /// <summary>Permanently removes an archived task from history.</summary>
    public static void RemoveArchivedTask(DemoTask task)
    {
        Persist(() => Backend.DeleteTask(task));
        ArchivedTasks.Remove(task);
    }

    /// <summary>Tasks the current user saved for later.</summary>
    public static List<DemoTask> SavedTasks { get; } = [];

    /// <summary>True when a task with this title is in the saved list.</summary>
    public static bool IsSaved(string title) => SavedTasks.Any(task => task.Title == title);

    /// <summary>Adds or removes a task from the saved list and notifies subscribers.</summary>
    public static void ToggleSaved(DemoTask task)
    {
        var existing = SavedTasks.FirstOrDefault(saved => saved.Title == task.Title);
        var taskId = ResolveTaskId(existing ?? task);
        if (existing is not null)
        {
            SavedTasks.Remove(existing);
            Persist(() => Backend.SetSaved(CurrentUserId, taskId, false));
        }
        else
        {
            if (task.Id == 0 && taskId > 0)
                task.Id = taskId;
            SavedTasks.Add(task);
            Persist(() => Backend.SetSaved(CurrentUserId, taskId, true));
        }
        DataChanged?.Invoke();
    }

    /// <summary>Outcomes recorded when a poster completes or cancels a task.</summary>
    public static List<DemoTaskOutcome> TaskOutcomes { get; } = [];

    /// <summary>Marks a posted task complete and moves it into history.</summary>
    public static bool CompleteTask(DemoTask task)
    {
        if (!Backend.CanCloseTask(task.Id, CurrentUserId))
        {
            MessageBox.Show("You can only close your own task after accepting a task-doer.", "Task cannot be completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        CloseTask(task, "Completed", "");
        return true;
    }

    /// <summary>Cancels a posted task with a reason and moves it into history.</summary>
    public static bool CancelTask(DemoTask task, string reason)
    {
        if (!Backend.CanCloseTask(task.Id, CurrentUserId))
        {
            MessageBox.Show("You can only cancel your own task after accepting a task-doer.", "Task cannot be cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        CloseTask(task, "Cancelled", reason);
        return true;
    }

    /// <summary>Returns a closed task to the active posted list.</summary>
    public static void ReopenTask(DemoTask task)
    {
        Persist(() => Backend.SetTaskStatus(task, "Active"));
        TaskOutcomes.RemoveAll(outcome => outcome.Title == task.Title);
        ArchivedTasks.Remove(task);
        if (!PostedTasks.Contains(task))
            PostedTasks.Add(task);
        LogActivity(AppSession.CurrentUser.FullName, "Reopened task", task.Title, "Task", "Task returned to active posts.");
    }

    private static void CloseTask(DemoTask task, string status, string reason)
    {
        Persist(() =>
        {
            Backend.SetTaskStatus(task, status, reason);
            Backend.NotifyAcceptedDoers(task.Id, status == "Completed" ? "Task completed" : "Task cancelled",
                status == "Completed" ? $"“{task.Title}” was marked complete by the poster." : $"“{task.Title}” was cancelled. {reason}".Trim());
            if (status == "Completed")
            {
                // The payment record is created for you, so payment history always matches real completed work.
                Backend.EnsureCompletionPayment(task, CurrentUserId);
                var refreshed = Backend.LoadPayments(CurrentUserId, AppSession.CurrentUser.FullName, false);
                Payments.Clear();
                Payments.AddRange(refreshed);
            }
        });

        PostedTasks.Remove(task);
        AvailableTasks.Remove(task);
        if (!ArchivedTasks.Contains(task))
            ArchivedTasks.Add(task);

        TaskOutcomes.RemoveAll(outcome => outcome.Title == task.Title);
        TaskOutcomes.Insert(0, new DemoTaskOutcome(
            task.Title,
            task.Category,
            task.Price,
            task.Barangay,
            status,
            DateTime.Today,
            reason,
            task.Description,
            task.Icon,
            task.IconBack));

        LogActivity(
            AppSession.CurrentUser.FullName,
            status == "Completed" ? "Completed task" : "Cancelled task",
            task.Title,
            "Task",
            reason.Length == 0 ? "Task closed by the poster." : reason);
        DataChanged?.Invoke();
    }

    /// <summary>Records a new sent request (newest first). Returns false when it could not be sent.</summary>
    public static bool SendRequest(DemoRequest request, string cvFileName, byte[] cvBytes)
    {
        var taskId = request.TaskId > 0
            ? request.TaskId
            : AvailableTasks.FirstOrDefault(task => task.Title == request.Title)?.Id ?? 0;
        if (taskId == 0)
        {
            MessageBox.Show("This task is no longer available.", "Request not sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        var requestId = 0;
        if (!Db.Try(() => requestId = Backend.SendRequest(taskId, CurrentUserId, cvFileName, cvBytes)))
            return false;
        if (requestId == 0)
        {
            MessageBox.Show("You cannot send a request to your own task.", "Request not sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        request.Id = requestId;
        request.TaskId = taskId;
        RequestsSent.RemoveAll(existing => existing.TaskId == taskId);
        RequestsSent.Insert(0, request);
        return true;
    }

    /// <summary>Marks a sent request as withdrawn.</summary>
    public static bool WithdrawRequest(DemoRequest request)
    {
        if (!Db.Try(() => Backend.SetRequestStatus(request.Id, "withdrawn")))
            return false;
        request.Withdrawn = true;
        request.Status = "withdrawn";
        return true;
    }

    /// <summary>Marks a received applicant as accepted and declines the rest of that task's applicants.</summary>
    public static void AcceptApplicant(DemoApplicant accepted)
    {
        if (!Db.Try(() => Backend.AcceptRequest(accepted.TaskId, accepted.Id)))
            return;
        foreach (var applicant in RequestsReceived.Where(applicant => applicant.TaskId == accepted.TaskId))
        {
            applicant.Accepted = ReferenceEquals(applicant, accepted);
            applicant.Declined = !applicant.Accepted;
        }
        AvailableTasks.RemoveAll(task => task.Id == accepted.TaskId);
        LogActivity(AppSession.CurrentUser.FullName, "Selected performer", accepted.TaskTitle, "Task", $"{accepted.Name} was selected.");
        DataChanged?.Invoke();
    }

    /// <summary>Shortlists or un-shortlists an applicant.</summary>
    public static void ToggleShortlist(DemoApplicant applicant)
    {
        applicant.Shortlisted = !applicant.Shortlisted;
        Persist(() => Backend.SetShortlisted(applicant.Id, applicant.Shortlisted));
    }

    /// <summary>Declines an applicant's request.</summary>
    public static void DeclineApplicant(DemoApplicant applicant)
    {
        applicant.Declined = true;
        Persist(() =>
        {
            Backend.SetRequestStatus(applicant.Id, "declined");
            Backend.Notify(applicant.DoerId, "Request not selected", $"The poster declined your request for “{applicant.TaskTitle}”.", "tasks", "Find tasks");
        });
    }

    /// <summary>Number of open applications for a posted task.</summary>
    public static int RequestsForTask(string title) => RequestsReceived.Count(a => a.TaskTitle == title && !a.Declined && !a.Accepted);

    /// <summary>Appends a newly recorded payment to the shared history.</summary>
    public static void AddPayment(PaymentRecordDetails payment)
    {
        Persist(() => Backend.InsertPayment(payment, CurrentUserId));
        Payments.RemoveAll(existing => existing.Reference == payment.Reference);
        Payments.Add(payment);
        LogActivity(AppSession.CurrentUser.FullName, "Payment recorded", payment.Reference, "Payment", $"₱{payment.Amount:N0} {payment.Method} payment for {payment.Task}.");
    }

    /// <summary>Runs a database write; failures are reported once and never crash the page.</summary>
    private static void Persist(Action action) => Db.Try(action);
}
