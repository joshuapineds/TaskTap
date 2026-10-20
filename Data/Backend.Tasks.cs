using System.Globalization;

namespace TaskTap.Data;

public static partial class Backend
{
    private static string FormatDate(DateTime value) => value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private static DemoTask ReadTask(MySqlConnector.MySqlDataReader r)
    {
        var deadline = r.Date("deadline");
        return new DemoTask(
            r.Str("title"),
            r.Str("category"),
            r.Int("price"),
            r.Str("barangay"),
            Math.Max(0, (deadline.Date - DateTime.Today).Days),
            FormatDate(deadline),
            r.Str("icon"),
            r.Str("description"),
            Color.FromArgb(r.Int("icon_back_argb")),
            new Point(r.Int("map_x"), r.Int("map_y")))
        {
            Id = r.Int("id"),
            PosterId = r.Int("poster_id"),
            PosterName = r.Str("poster_name"),
            Requirements = r.Str("requirements"),
            TaskType = r.Str("task_type"),
            PaymentMethod = r.Str("payment_method"),
            BudgetType = r.Str("budget_type"),
            Duration = r.Str("duration"),
            ScheduleNotes = r.Str("schedule_notes")
        };
    }

    private const string TaskSelect = """
        SELECT t.*, CONCAT(u.first_name, ' ', u.last_name) AS poster_name
        FROM tasks t JOIN users u ON u.id = t.poster_id
        """;

    /// <summary>Marketplace tasks visible to a member: other people's active tasks.</summary>
    public static List<DemoTask> LoadMarketplace(int userId) =>
        Db.Query($"{TaskSelect} WHERE t.status = 'Active' AND t.poster_id <> @u ORDER BY t.created_at DESC, t.id DESC", ReadTask, ("@u", userId));

    /// <summary>Everything the admin portal manages: active and paused listings.</summary>
    public static List<DemoTask> LoadAllLiveTasks() =>
        Db.Query($"{TaskSelect} WHERE t.status IN ('Active', 'Paused') ORDER BY t.created_at DESC, t.id DESC", ReadTask);

    public static List<DemoTask> LoadPosted(int userId) =>
        Db.Query($"{TaskSelect} WHERE t.poster_id = @u AND t.status IN ('Active', 'Paused') ORDER BY t.created_at DESC, t.id DESC", ReadTask, ("@u", userId));

    /// <summary>Archived, completed and cancelled tasks (one member, or everyone when userId is 0).</summary>
    public static List<DemoTask> LoadClosed(int userId) =>
        Db.Query(
            $"{TaskSelect} WHERE t.status IN ('Archived', 'Completed', 'Cancelled') AND (@u = 0 OR t.poster_id = @u) ORDER BY t.closed_at DESC, t.id DESC",
            ReadTask, ("@u", userId));

    public static List<DemoTaskOutcome> LoadOutcomes(int userId) =>
        Db.Query(
            "SELECT * FROM tasks WHERE poster_id = @u AND status IN ('Completed', 'Cancelled') ORDER BY closed_at DESC",
            r => new DemoTaskOutcome(
                r.Str("title"), r.Str("category"), r.Int("price"), r.Str("barangay"), r.Str("status"),
                (r.DateOrNull("closed_at") ?? DateTime.Today).Date, r.Str("cancel_reason"), r.Str("description"),
                r.Str("icon"), Color.FromArgb(r.Int("icon_back_argb"))),
            ("@u", userId));

    public static List<DemoTask> LoadSaved(int userId) =>
        Db.Query(
            $"{TaskSelect} JOIN saved_tasks s ON s.task_id = t.id WHERE s.user_id = @u ORDER BY s.saved_at DESC",
            ReadTask, ("@u", userId));

    public static HashSet<int> LoadTaskIdsWhere(string column, string value)
    {
        var safe = column is "status" or "featured" ? column : throw new ArgumentException("Unknown column", nameof(column));
        return [.. Db.Query($"SELECT id FROM tasks WHERE {safe} = @v", r => r.Int("id"), ("@v", value))];
    }

    public static int ResolveTaskIdByTitle(string title, int excludePoster = 0) =>
        Convert.ToInt32(Db.Scalar(
            "SELECT id FROM tasks WHERE title = @t AND poster_id <> @p AND status IN ('Active', 'Paused') ORDER BY id DESC LIMIT 1",
            ("@t", title), ("@p", excludePoster)) ?? 0);

    /// <summary>Tells accepted performers about a change to their task (completed, cancelled...).</summary>
    public static void NotifyAcceptedDoers(int taskId, string title, string body)
    {
        var doers = Db.Query("SELECT doer_id FROM task_requests WHERE task_id = @t AND status = 'accepted'", r => r.Int("doer_id"), ("@t", taskId));
        foreach (var doer in doers)
            Notify(doer, title, body, "tasks", "Open task");
    }

    /// <summary>Inserts a new task and returns its generated id.</summary>
    public static int InsertTask(DemoTask task, int posterId)
    {
        var deadline = DateTime.ParseExact(task.Deadline, "MMM d, yyyy", CultureInfo.InvariantCulture);
        return (int)Db.Insert(
            "INSERT INTO tasks (poster_id, title, category, price, barangay, deadline, icon, description, requirements, task_type, " +
            "payment_method, budget_type, duration, schedule_notes, icon_back_argb, map_x, map_y, status) " +
            "VALUES (@p, @title, @cat, @price, @brgy, @deadline, @icon, @desc, @req, @type, @pay, @budget, @dur, @notes, @back, @mx, @my, 'Active')",
            ("@p", posterId), ("@title", task.Title), ("@cat", task.Category), ("@price", task.Price), ("@brgy", task.Barangay),
            ("@deadline", deadline), ("@icon", task.Icon), ("@desc", task.Description), ("@req", task.Requirements),
            ("@type", task.TaskType), ("@pay", task.PaymentMethod), ("@budget", task.BudgetType), ("@dur", task.Duration),
            ("@notes", task.ScheduleNotes), ("@back", task.IconBack.ToArgb()), ("@mx", task.MapPoint.X), ("@my", task.MapPoint.Y));
    }

    public static void UpdateTask(DemoTask task)
    {
        if (task.Id == 0) return;
        var deadline = DateTime.ParseExact(task.Deadline, "MMM d, yyyy", CultureInfo.InvariantCulture);
        Db.NonQuery(
            "UPDATE tasks SET title = @title, category = @cat, price = @price, barangay = @brgy, deadline = @deadline, description = @desc, " +
            "requirements = @req, task_type = @type, payment_method = @pay, budget_type = @budget, duration = @dur, schedule_notes = @notes " +
            "WHERE id = @id",
            ("@title", task.Title), ("@cat", task.Category), ("@price", task.Price), ("@brgy", task.Barangay), ("@deadline", deadline),
            ("@desc", task.Description), ("@req", task.Requirements), ("@type", task.TaskType), ("@pay", task.PaymentMethod),
            ("@budget", task.BudgetType), ("@dur", task.Duration), ("@notes", task.ScheduleNotes), ("@id", task.Id));
    }

    public static void SetTaskStatus(DemoTask task, string status, string reason = "")
    {
        if (task.Id == 0) return;
        var closing = status is "Archived" or "Completed" or "Cancelled";
        Db.NonQuery(
            "UPDATE tasks SET status = @s, cancel_reason = @r, closed_at = CASE WHEN @closing = 1 THEN NOW() ELSE NULL END WHERE id = @id",
            ("@s", status), ("@r", reason), ("@closing", closing ? 1 : 0), ("@id", task.Id));
    }

    public static void SetFeatured(DemoTask task, bool featured)
    {
        if (task.Id != 0)
            Db.NonQuery("UPDATE tasks SET featured = @f WHERE id = @id", ("@f", featured ? 1 : 0), ("@id", task.Id));
    }

    public static void DeleteTask(DemoTask task)
    {
        if (task.Id != 0)
            Db.NonQuery("DELETE FROM tasks WHERE id = @id", ("@id", task.Id));
    }

    public static void SetSaved(int userId, int taskId, bool saved)
    {
        if (userId == 0 || taskId == 0) return;
        if (saved)
            Db.NonQuery("INSERT IGNORE INTO saved_tasks (user_id, task_id) VALUES (@u, @t)", ("@u", userId), ("@t", taskId));
        else
            Db.NonQuery("DELETE FROM saved_tasks WHERE user_id = @u AND task_id = @t", ("@u", userId), ("@t", taskId));
    }
}
