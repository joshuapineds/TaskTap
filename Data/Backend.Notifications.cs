namespace TaskTap.Data;

public static partial class Backend
{
    public sealed record NotificationRow(int Id, string Title, string Body, string Kind, string ActionLabel, DateTime CreatedAt, bool Unread);

    /// <summary>Adds a notification for a member. Failures never block the action that triggered it.</summary>
    public static void Notify(int userId, string title, string body, string kind, string actionLabel)
    {
        if (userId == 0) return;
        try
        {
            // Respect the member's notification preferences (Notifications page / registration tips box).
            var preferenceKey = kind switch { "tasks" => "task_requests", "payments" => "payments", "tips" => "community_tips", _ => "" };
            if (preferenceKey.Length > 0)
            {
                var enabled = Db.Scalar("SELECT enabled FROM notification_preferences WHERE user_id = @u AND preference_key = @k",
                    ("@u", userId), ("@k", preferenceKey));
                if (enabled is not null && Convert.ToInt32(enabled) == 0)
                    return;
            }

            Db.NonQuery(
                "INSERT INTO notifications (user_id, title, body, kind, action_label) VALUES (@u, @t, @b, @k, @a)",
                ("@u", userId), ("@t", title), ("@b", body), ("@k", kind), ("@a", actionLabel));
        }
        catch
        {
            // Notifications are best-effort.
        }
    }

    public static List<NotificationRow> LoadNotifications(int userId) =>
        Db.Query(
            "SELECT * FROM notifications WHERE user_id = @u ORDER BY created_at DESC, id DESC LIMIT 200",
            r => new NotificationRow(r.Int("id"), r.Str("title"), r.Str("body"), r.Str("kind"), r.Str("action_label"),
                r.Date("created_at"), r.DateOrNull("read_at") is null),
            ("@u", userId));

    public static void MarkNotificationRead(int id) =>
        Db.NonQuery("UPDATE notifications SET read_at = NOW() WHERE id = @id AND read_at IS NULL", ("@id", id));

    public static void MarkNotificationUnread(int id) =>
        Db.NonQuery("UPDATE notifications SET read_at = NULL WHERE id = @id", ("@id", id));

    public static void MarkAllNotificationsRead(int userId) =>
        Db.NonQuery("UPDATE notifications SET read_at = NOW() WHERE user_id = @u AND read_at IS NULL", ("@u", userId));

    public static Dictionary<string, bool> LoadNotificationPreferences(int userId) =>
        Db.Query("SELECT preference_key, enabled FROM notification_preferences WHERE user_id = @u",
            r => (Key: r.Str("preference_key"), Enabled: r.Bool("enabled")), ("@u", userId))
          .ToDictionary(x => x.Key, x => x.Enabled, StringComparer.OrdinalIgnoreCase);

    public static void SetNotificationPreference(int userId, string key, bool enabled) =>
        Db.NonQuery("INSERT INTO notification_preferences (user_id, preference_key, enabled) VALUES (@u, @k, @e) " +
                    "ON DUPLICATE KEY UPDATE enabled = VALUES(enabled)",
            ("@u", userId), ("@k", key), ("@e", enabled ? 1 : 0));

    public static int UnreadNotificationCount(int userId) =>
        Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM notifications WHERE user_id = @u AND read_at IS NULL", ("@u", userId)) ?? 0);
}
