using System.Globalization;

namespace TaskTap.Data;

public static partial class Backend
{
    public sealed record ChatMessageRow(int Id, int SenderId, string Body, DateTime SentAt);

    public sealed record ConversationRow(
        int Id, int TaskId, int OtherId, string OtherName, string OtherInitials, string Presence,
        string TaskTitle, string Banner, string Status, int UnreadCount, List<ChatMessageRow> Messages);

    public sealed record ComposeTarget(int TaskId, string TaskTitle, int OtherId, string OtherName)
    {
        public override string ToString() => $"{TaskTitle} — {OtherName}";
    }

    private static (int Low, int High) Ordered(int a, int b) => a < b ? (a, b) : (b, a);

    public static List<ConversationRow> LoadConversations(int userId)
    {
        var heads = Db.Query(
            """
            SELECT c.id, c.task_id, c.user_low, c.user_high FROM conversations c
            WHERE (c.user_low = @u OR c.user_high = @u)
              AND NOT EXISTS (SELECT 1 FROM conversation_hidden h WHERE h.conversation_id = c.id AND h.user_id = @u)
            """,
            r => (Id: r.Int("id"), TaskId: r.Int("task_id"), Low: r.Int("user_low"), High: r.Int("user_high")),
            ("@u", userId));

        var rows = new List<ConversationRow>();
        foreach (var head in heads)
        {
            var otherId = head.Low == userId ? head.High : head.Low;
            var other = Db.Query("SELECT first_name, last_name, last_active FROM users WHERE id = @id",
                r => (First: r.Str("first_name"), Last: r.Str("last_name"), Active: r.DateOrNull("last_active")), ("@id", otherId)).FirstOrDefault();
            if (other.First is null) continue;

            var task = Db.Query(
                "SELECT title, price, category, barangay, deadline, status FROM tasks WHERE id = @t",
                r => (Title: r.Str("title"), Price: r.Int("price"), Category: r.Str("category"), Barangay: r.Str("barangay"),
                      Deadline: r.Date("deadline"), Status: r.Str("status")),
                ("@t", head.TaskId)).FirstOrDefault();

            var requestStatus = Db.Scalar(
                "SELECT status FROM task_requests WHERE task_id = @t AND (doer_id = @a OR doer_id = @b) ORDER BY (status = 'accepted') DESC LIMIT 1",
                ("@t", head.TaskId), ("@a", userId), ("@b", otherId))?.ToString() ?? "pending";

            var status = task.Status == "Completed" ? "Completed"
                : requestStatus == "accepted" ? "Accepted"
                : requestStatus == "declined" ? "Closed" : "Pending";

            var messages = Db.Query(
                "SELECT id, sender_id, body, sent_at FROM messages WHERE conversation_id = @c ORDER BY sent_at, id",
                r => new ChatMessageRow(r.Int("id"), r.Int("sender_id"), r.Str("body"), r.Date("sent_at")),
                ("@c", head.Id));
            var unread = Convert.ToInt32(Db.Scalar(
                "SELECT COUNT(*) FROM messages WHERE conversation_id = @c AND sender_id <> @u AND read_at IS NULL",
                ("@c", head.Id), ("@u", userId)) ?? 0);

            rows.Add(new ConversationRow(
                head.Id, head.TaskId, otherId, $"{other.First} {other.Last}".Trim(), Initials(other.First, other.Last),
                other.Active is null ? "Not active yet" : $"Active {RelativeTime(other.Active).ToLowerInvariant()}",
                task.Title ?? "Task", task.Title is null ? "" : $"₱{task.Price:N0} · {task.Category} · Brgy. {task.Barangay} · Due {FormatDate(task.Deadline)}",
                status, unread, messages));
        }

        return [.. rows.OrderByDescending(row => row.Messages.Count == 0 ? DateTime.MinValue : row.Messages[^1].SentAt)];
    }

    /// <summary>Finds or creates the conversation between two members about a task.</summary>
    public static int EnsureConversation(int taskId, int me, int other)
    {
        var (low, high) = Ordered(me, other);
        Db.NonQuery("INSERT IGNORE INTO conversations (task_id, user_low, user_high) VALUES (@t, @l, @h)",
            ("@t", taskId), ("@l", low), ("@h", high));
        var id = Convert.ToInt32(Db.Scalar(
            "SELECT id FROM conversations WHERE task_id = @t AND user_low = @l AND user_high = @h",
            ("@t", taskId), ("@l", low), ("@h", high)));
        Db.NonQuery("DELETE FROM conversation_hidden WHERE conversation_id = @c AND user_id = @u", ("@c", id), ("@u", me));
        return id;
    }

    public static ChatMessageRow SendMessage(int conversationId, int senderId, string body)
    {
        var id = (int)Db.Insert("INSERT INTO messages (conversation_id, sender_id, body) VALUES (@c, @s, @b)",
            ("@c", conversationId), ("@s", senderId), ("@b", body));

        // A new message brings a hidden conversation back for the recipient and notifies them.
        Db.NonQuery("DELETE FROM conversation_hidden WHERE conversation_id = @c AND user_id <> @s", ("@c", conversationId), ("@s", senderId));
        var info = Db.Query(
            "SELECT user_low, user_high FROM conversations WHERE id = @c",
            r => (Low: r.Int("user_low"), High: r.Int("user_high")), ("@c", conversationId)).FirstOrDefault();
        var recipient = info.Low == senderId ? info.High : info.Low;
        var sender = GetAppUser(senderId);
        Notify(recipient, "New message", $"{sender?.FullName ?? "A member"} sent you a message.", "messages", "Open conversation");
        return new ChatMessageRow(id, senderId, body, DateTime.Now);
    }

    public static void MarkConversationRead(int conversationId, int userId) =>
        Db.NonQuery("UPDATE messages SET read_at = NOW() WHERE conversation_id = @c AND sender_id <> @u AND read_at IS NULL",
            ("@c", conversationId), ("@u", userId));

    public static void HideConversation(int conversationId, int userId) =>
        Db.NonQuery("INSERT IGNORE INTO conversation_hidden (conversation_id, user_id) VALUES (@c, @u)", ("@c", conversationId), ("@u", userId));

    /// <summary>Number of conversations that hold at least one unread message for the member.</summary>
    public static int UnreadConversationCount(int userId) =>
        Convert.ToInt32(Db.Scalar(
            """
            SELECT COUNT(DISTINCT m.conversation_id) FROM messages m JOIN conversations c ON c.id = m.conversation_id
            WHERE (c.user_low = @u OR c.user_high = @u) AND m.sender_id <> @u AND m.read_at IS NULL
            """, ("@u", userId)) ?? 0);

    /// <summary>People the member can start a task conversation with: applicants on their tasks and posters they applied to.</summary>
    public static List<ComposeTarget> LoadComposeTargets(int userId) =>
        Db.Query(
            """
            SELECT t.id AS task_id, t.title, u.id AS other_id, CONCAT(u.first_name, ' ', u.last_name) AS other_name
            FROM task_requests r JOIN tasks t ON t.id = r.task_id JOIN users u ON u.id = r.doer_id
            WHERE t.poster_id = @u AND r.status IN ('pending', 'accepted')
            UNION
            SELECT t.id, t.title, p.id, CONCAT(p.first_name, ' ', p.last_name)
            FROM task_requests r JOIN tasks t ON t.id = r.task_id JOIN users p ON p.id = t.poster_id
            WHERE r.doer_id = @u AND r.status IN ('pending', 'accepted')
            """,
            r => new ComposeTarget(r.Int("task_id"), r.Str("title"), r.Int("other_id"), r.Str("other_name")),
            ("@u", userId));

    public static string DayLabel(DateTime value)
    {
        if (value.Date == DateTime.Today) return "Today";
        if (value.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
        return value.ToString("MMM d", CultureInfo.InvariantCulture);
    }
}
