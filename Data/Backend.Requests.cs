using System.Globalization;

namespace TaskTap.Data;

public static partial class Backend
{
    /// <summary>Requests the member sent to other people's tasks.</summary>
    public static List<DemoRequest> LoadSentRequests(int doerId) =>
        Db.Query(
            """
            SELECT r.id AS request_id, r.task_id, r.attachment_name, r.attachment_size, r.status AS request_status, r.created_at AS requested_at,
                   t.*, t.status AS task_status
            FROM task_requests r JOIN tasks t ON t.id = r.task_id
            WHERE r.doer_id = @u ORDER BY r.created_at DESC
            """,
            r =>
            {
                var deadline = r.Date("deadline");
                var requested = r.Date("requested_at");
                return new DemoRequest
                {
                    Id = r.Int("request_id"),
                    TaskId = r.Int("task_id"),
                    TaskStatus = r.Str("task_status"),
                    Title = r.Str("title"),
                    Category = r.Str("category"),
                    Barangay = r.Str("barangay"),
                    Amount = r.Int("price"),
                    Status = r.Str("request_status"),
                    Deadline = deadline.Date < DateTime.Today ? "Deadline passed" : FormatDate(deadline),
                    Date = requested,
                    RequestDate = requested.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Description = r.Str("description"),
                    AttachmentName = r.Str("attachment_name"),
                    AttachmentSize = r.Int("attachment_size"),
                    Icon = r.Str("icon"),
                    IconBack = Color.FromArgb(r.Int("icon_back_argb")),
                    Withdrawn = r.Str("request_status") == "withdrawn"
                };
            },
            ("@u", doerId));

    /// <summary>Applications other members sent to the poster's tasks.</summary>
    public static List<DemoApplicant> LoadReceivedRequests(int posterId) =>
        Db.Query(
            """
            SELECT r.id AS request_id, r.task_id, r.attachment_name, r.attachment_size, r.status AS request_status, r.shortlisted, r.created_at AS requested_at,
                   t.title AS task_title, u.id AS doer_id, u.first_name, u.last_name, u.barangay AS doer_barangay,
                   u.skills, u.created_at AS member_since,
                   COALESCE(review_stats.rating_avg, 0) AS rating_avg,
                   COALESCE(review_stats.review_count, 0) AS review_count,
                   (SELECT COUNT(*) FROM task_requests x JOIN tasks tt ON tt.id = x.task_id
                     WHERE x.doer_id = u.id AND x.status = 'accepted' AND tt.status = 'Completed') AS completed_count
            FROM task_requests r
            JOIN tasks t ON t.id = r.task_id
            JOIN users u ON u.id = r.doer_id
            LEFT JOIN (
                SELECT reviewee_id, AVG(rating) AS rating_avg, COUNT(*) AS review_count
                FROM reviews
                GROUP BY reviewee_id
            ) review_stats ON review_stats.reviewee_id = u.id
            WHERE t.poster_id = @u AND r.status IN ('pending', 'accepted', 'declined')
            ORDER BY r.created_at DESC
            """,
            r =>
            {
                var first = r.Str("first_name");
                var last = r.Str("last_name");
                var requested = r.Date("requested_at");
                var skills = r.Str("skills");
                return new DemoApplicant
                {
                    Id = r.Int("request_id"),
                    TaskId = r.Int("task_id"),
                    TaskTitle = r.Str("task_title"),
                    Name = $"{first} {last}".Trim(),
                    Initials = Initials(first, last),
                    Rating = Convert.ToDouble(r.GetValue(r.GetOrdinal("rating_avg")), CultureInfo.InvariantCulture),
                    Reviews = r.Int("review_count"),
                    Completed = r.Int("completed_count"),
                    RequestedAt = requested.Date == DateTime.Today
                        ? $"Today, {requested:h:mm tt}"
                        : requested.Date == DateTime.Today.AddDays(-1) ? $"Yesterday, {requested:h:mm tt}" : requested.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture),
                    AttachmentName = r.Str("attachment_name"),
                    AttachmentSize = r.Int("attachment_size"),
                    Skills = skills.Length == 0 ? [] : skills.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                    Barangay = r.Str("doer_barangay"),
                    MemberSince = r.Date("member_since").Year.ToString(CultureInfo.InvariantCulture),
                    Date = requested,
                    AvatarColor = AvatarFor(r.Int("doer_id")),
                    Shortlisted = r.Bool("shortlisted"),
                    Declined = r.Str("request_status") == "declined",
                    Accepted = r.Str("request_status") == "accepted",
                    DoerId = r.Int("doer_id")
                };
            },
            ("@u", posterId));

    /// <summary>Largest CV accepted (kept small so it fits MySQL's default max_allowed_packet).</summary>
    public const int MaxCvBytes = 2 * 1024 * 1024;

    /// <summary>Checks that a file really is a PDF within the size limit. Returns an error message, or null when valid.</summary>
    public static string? ValidateCv(string fileName, byte[] bytes)
    {
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return "Please choose a PDF file.";
        if (bytes.Length == 0)
            return "That file is empty.";
        if (bytes.Length > MaxCvBytes)
            return $"The PDF is too large. The limit is {MaxCvBytes / (1024 * 1024)} MB.";
        if (bytes.Length < 5 || bytes[0] != '%' || bytes[1] != 'P' || bytes[2] != 'D' || bytes[3] != 'F' || bytes[4] != '-')
            return "That file is not a valid PDF.";
        return null;
    }

    /// <summary>Creates (or re-opens a withdrawn) request with the applicant's CV. Returns the request id, or 0 when the member owns the task.</summary>
    public static int SendRequest(int taskId, int doerId, string fileName, byte[] pdf)
    {
        var error = ValidateCv(fileName, pdf);
        if (error is not null)
            throw new InvalidOperationException(error);

        var owner = Db.Scalar("SELECT poster_id FROM tasks WHERE id = @t", ("@t", taskId));
        if (owner is null || Convert.ToInt32(owner) == doerId)
            return 0;

        var safeName = Path.GetFileName(fileName);
        if (safeName.Length > 120)
            safeName = safeName[^120..];
        Db.NonQuery(
            "INSERT INTO task_requests (task_id, doer_id, attachment_name, attachment_size, attachment, status) VALUES (@t, @d, @n, @s, @a, 'pending') " +
            "ON DUPLICATE KEY UPDATE attachment_name = VALUES(attachment_name), attachment_size = VALUES(attachment_size), attachment = VALUES(attachment), " +
            "status = 'pending', shortlisted = 0, created_at = NOW()",
            ("@t", taskId), ("@d", doerId), ("@n", safeName), ("@s", pdf.Length), ("@a", pdf));
        var id = Convert.ToInt32(Db.Scalar("SELECT id FROM task_requests WHERE task_id = @t AND doer_id = @d", ("@t", taskId), ("@d", doerId)));

        var doer = GetAppUser(doerId);
        var title = Db.Scalar("SELECT title FROM tasks WHERE id = @t", ("@t", taskId))?.ToString() ?? "your task";
        Notify(Convert.ToInt32(owner), "New task request", $"{doer?.FullName ?? "A member"} applied for “{title}” and attached a CV.", "tasks", "Review request");
        return id;
    }

    /// <summary>The CV of a request. Only the applicant and the task's poster can read it.</summary>
    public static (string Name, byte[] Bytes)? GetAttachment(int requestId, int userId)
    {
        using var connection = Db.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT r.attachment_name, r.attachment FROM task_requests r JOIN tasks t ON t.id = r.task_id " +
            "WHERE r.id = @id AND (r.doer_id = @u OR t.poster_id = @u) AND r.attachment IS NOT NULL";
        command.Parameters.AddWithValue("@id", requestId);
        command.Parameters.AddWithValue("@u", userId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;
        return (reader.GetString(0), (byte[])reader["attachment"]);
    }

    /// <summary>After a request is accepted, each side may see the other's phone/email so they can arrange the task.</summary>
    public static (string Name, string Phone, string Email)? GetAcceptedContact(int requestId, int userId) =>
        Db.Query(
            """
            SELECT CASE WHEN r.doer_id = @u THEN CONCAT(p.first_name, ' ', p.last_name) ELSE CONCAT(d.first_name, ' ', d.last_name) END AS other_name,
                   CASE WHEN r.doer_id = @u THEN p.phone ELSE d.phone END AS other_phone,
                   CASE WHEN r.doer_id = @u THEN p.email ELSE d.email END AS other_email
            FROM task_requests r
            JOIN tasks t ON t.id = r.task_id
            JOIN users p ON p.id = t.poster_id
            JOIN users d ON d.id = r.doer_id
            WHERE r.id = @id AND r.status = 'accepted' AND (r.doer_id = @u OR t.poster_id = @u)
            """,
            r => ((string Name, string Phone, string Email)?)(r.Str("other_name"), r.Str("other_phone"), r.Str("other_email")),
            ("@id", requestId), ("@u", userId)).FirstOrDefault();

    public static void SetRequestStatus(int requestId, string status)
    {
        if (requestId != 0)
            Db.NonQuery("UPDATE task_requests SET status = @s WHERE id = @id", ("@s", status), ("@id", requestId));
    }

    public static void SetShortlisted(int requestId, bool shortlisted)
    {
        if (requestId != 0)
            Db.NonQuery("UPDATE task_requests SET shortlisted = @s WHERE id = @id", ("@s", shortlisted ? 1 : 0), ("@id", requestId));
    }

    /// <summary>Accepts one applicant and declines every other open request for the same task, atomically.</summary>
    public static void AcceptRequest(int taskId, int requestId)
    {
        if (taskId == 0 || requestId == 0) return;
        Db.Transaction(tx =>
        {
            tx.Run("UPDATE task_requests SET status = 'accepted' WHERE id = @id", ("@id", requestId));
            tx.Run("UPDATE task_requests SET status = 'declined' WHERE task_id = @t AND id <> @id AND status IN ('pending', 'accepted')",
                ("@t", taskId), ("@id", requestId));
        });

        var winner = Db.Query(
            "SELECT r.doer_id, t.title FROM task_requests r JOIN tasks t ON t.id = r.task_id WHERE r.id = @id",
            r => (DoerId: r.Int("doer_id"), Title: r.Str("title")), ("@id", requestId)).FirstOrDefault();
        if (winner.DoerId != 0)
            Notify(winner.DoerId, "Request accepted", $"You were selected for “{winner.Title}”. Open My Requests to see how to reach the poster.", "tasks", "Open task");

        var declined = Db.Query(
            "SELECT doer_id FROM task_requests WHERE task_id = @t AND id <> @id AND status = 'declined'",
            r => r.Int("doer_id"), ("@t", taskId), ("@id", requestId));
        foreach (var doerId in declined)
            Notify(doerId, "Request not selected", "The poster chose another member for this task.", "tasks", "Find tasks");
    }
}
