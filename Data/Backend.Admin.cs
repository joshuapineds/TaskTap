namespace TaskTap.Data;

/// <summary>Persistence for the admin-managed lists. Each method is an upsert or delete keyed by the model's id.</summary>
public static partial class Backend
{
    public static List<DemoCategory> LoadCategories() =>
        Db.Query("SELECT * FROM categories ORDER BY sort_order, id",
            r => new DemoCategory
            {
                Id = r.Str("id"), Name = r.Str("name"), Icon = r.Str("icon"),
                Color = Color.FromArgb(r.Int("color_argb")), Accent = Color.FromArgb(r.Int("accent_argb")),
                Active = r.Bool("active"), SortOrder = r.Int("sort_order")
            });

    public static void SaveCategory(DemoCategory c) =>
        Db.NonQuery(
            "INSERT INTO categories (id, name, icon, color_argb, accent_argb, active, sort_order) VALUES (@id, @n, @i, @c, @a, @act, @s) " +
            "ON DUPLICATE KEY UPDATE name = VALUES(name), icon = VALUES(icon), color_argb = VALUES(color_argb), " +
            "accent_argb = VALUES(accent_argb), active = VALUES(active), sort_order = VALUES(sort_order)",
            ("@id", c.Id), ("@n", c.Name), ("@i", c.Icon), ("@c", c.Color.ToArgb()), ("@a", c.Accent.ToArgb()),
            ("@act", c.Active ? 1 : 0), ("@s", c.SortOrder));

    public static void DeleteCategory(DemoCategory c) => Db.NonQuery("DELETE FROM categories WHERE id = @id", ("@id", c.Id));

    public static List<DemoBarangay> LoadBarangays() =>
        Db.Query("SELECT * FROM barangays ORDER BY id",
            r => new DemoBarangay { Id = r.Str("id"), Name = r.Str("name"), City = r.Str("city"), Active = r.Bool("active") });

    public static void SaveBarangay(DemoBarangay b) =>
        Db.NonQuery(
            "INSERT INTO barangays (id, name, city, active) VALUES (@id, @n, @c, @a) " +
            "ON DUPLICATE KEY UPDATE name = VALUES(name), city = VALUES(city), active = VALUES(active)",
            ("@id", b.Id), ("@n", b.Name), ("@c", b.City), ("@a", b.Active ? 1 : 0));

    public static List<DemoReport> LoadReports() =>
        Db.Query("SELECT * FROM reports ORDER BY created_ts DESC, id DESC",
            r => new DemoReport
            {
                Id = r.Str("id"), ReporterName = r.Str("reporter_name"), TargetType = r.Str("target_type"),
                TargetLabel = r.Str("target_label"), Reason = r.Str("reason"), Severity = r.Str("severity"),
                Status = r.Str("status"), CreatedAt = r.Str("created_at"), Description = r.Str("description"),
                Resolution = r.Str("resolution")
            });

    public static void SaveReport(DemoReport x) =>
        Db.NonQuery(
            "INSERT INTO reports (id, reporter_name, target_type, target_label, reason, severity, status, created_at, description, resolution) " +
            "VALUES (@id, @rn, @tt, @tl, @re, @sev, @st, @ca, @d, @res) " +
            "ON DUPLICATE KEY UPDATE severity = VALUES(severity), status = VALUES(status), resolution = VALUES(resolution)",
            ("@id", x.Id), ("@rn", x.ReporterName), ("@tt", x.TargetType), ("@tl", x.TargetLabel), ("@re", x.Reason),
            ("@sev", x.Severity), ("@st", x.Status), ("@ca", x.CreatedAt), ("@d", x.Description), ("@res", x.Resolution));

    public static void DeleteReport(DemoReport x) => Db.NonQuery("DELETE FROM reports WHERE id = @id", ("@id", x.Id));

    public static List<DemoDispute> LoadDisputes() =>
        Db.Query("SELECT * FROM disputes ORDER BY created_ts DESC, id DESC",
            r => new DemoDispute
            {
                Id = r.Str("id"), TaskTitle = r.Str("task_title"), PosterName = r.Str("poster_name"), DoerName = r.Str("doer_name"),
                Amount = r.Int("amount"), Reason = r.Str("reason"), Status = r.Str("status"), OpenedAt = r.Str("opened_at"),
                Resolution = r.Str("resolution")
            });

    public static void SaveDispute(DemoDispute x) =>
        Db.NonQuery(
            "INSERT INTO disputes (id, task_title, poster_name, doer_name, amount, reason, status, opened_at, resolution) " +
            "VALUES (@id, @t, @p, @d, @a, @r, @s, @o, @res) " +
            "ON DUPLICATE KEY UPDATE status = VALUES(status), resolution = VALUES(resolution)",
            ("@id", x.Id), ("@t", x.TaskTitle), ("@p", x.PosterName), ("@d", x.DoerName), ("@a", x.Amount),
            ("@r", x.Reason), ("@s", x.Status), ("@o", x.OpenedAt), ("@res", x.Resolution));

    public static List<DemoActivity> LoadActivity() =>
        Db.Query("SELECT * FROM activity_log ORDER BY created_at DESC, id DESC LIMIT 500",
            r => new DemoActivity
            {
                Id = r.Str("id"), Actor = r.Str("actor"), Action = r.Str("action"), Target = r.Str("target"),
                Category = r.Str("category"), Timestamp = r.Date("created_at"), Detail = r.Str("detail")
            });

    public static void InsertActivity(DemoActivity x) =>
        Db.NonQuery(
            "INSERT INTO activity_log (id, actor, action, target, category, detail, created_at) VALUES (@id, @a, @ac, @t, @c, @d, @ts)",
            ("@id", x.Id), ("@a", x.Actor), ("@ac", x.Action), ("@t", x.Target), ("@c", x.Category), ("@d", x.Detail), ("@ts", x.Timestamp));

    /// <summary>Highest numeric suffix stored for an id prefix such as "ACT-" (used to continue sequences after a restart).</summary>
    public static int MaxIdNumber(string table, string prefix)
    {
        var table_ = table is "activity_log" or "reports" or "disputes" or "support_tickets" or "announcements"
            ? table : throw new ArgumentException("Unknown table", nameof(table));
        var value = Db.Scalar($"SELECT MAX(CAST(SUBSTRING(id, @len) AS UNSIGNED)) FROM {table_} WHERE id LIKE @p",
            ("@len", prefix.Length + 1), ("@p", prefix + "%"));
        return value is null ? 0 : Convert.ToInt32(value);
    }

    public static List<DemoSupportTicket> LoadTickets(int userId, string fullName, bool all) =>
        Db.Query(
            "SELECT * FROM support_tickets WHERE @all = 1 OR name = @n ORDER BY created_ts DESC, id DESC",
            r => new DemoSupportTicket
            {
                Id = r.Str("id"), Name = r.Str("name"), Email = r.Str("email"), Category = r.Str("category"), Subject = r.Str("subject"),
                Message = r.Str("message"), Status = r.Str("status"), CreatedAt = r.Str("created_at"), Reply = r.Str("reply")
            },
            ("@all", all ? 1 : 0), ("@n", fullName));

    public static void SaveTicket(DemoSupportTicket x) =>
        Db.NonQuery(
            "INSERT INTO support_tickets (id, name, email, category, subject, message, status, created_at, reply) " +
            "VALUES (@id, @n, @e, @c, @s, @m, @st, @ca, @rep) " +
            "ON DUPLICATE KEY UPDATE status = VALUES(status), reply = VALUES(reply)",
            ("@id", x.Id), ("@n", x.Name), ("@e", x.Email), ("@c", x.Category), ("@s", x.Subject), ("@m", x.Message),
            ("@st", x.Status), ("@ca", x.CreatedAt), ("@rep", x.Reply));

    public static List<DemoAnnouncement> LoadAnnouncements() =>
        Db.Query("SELECT * FROM announcements ORDER BY created_ts DESC, id DESC",
            r => new DemoAnnouncement
            {
                Id = r.Str("id"), Title = r.Str("title"), Body = r.Str("body"), Audience = r.Str("audience"),
                Active = r.Bool("active"), PublishedAt = r.Str("published_at"), Author = r.Str("author")
            });

    public static void SaveAnnouncement(DemoAnnouncement x) =>
        Db.NonQuery(
            "INSERT INTO announcements (id, title, body, audience, active, published_at, author) VALUES (@id, @t, @b, @a, @act, @p, @au) " +
            "ON DUPLICATE KEY UPDATE title = VALUES(title), body = VALUES(body), audience = VALUES(audience), active = VALUES(active)",
            ("@id", x.Id), ("@t", x.Title), ("@b", x.Body), ("@a", x.Audience), ("@act", x.Active ? 1 : 0), ("@p", x.PublishedAt), ("@au", x.Author));

    public static void DeleteAnnouncement(DemoAnnouncement x) => Db.NonQuery("DELETE FROM announcements WHERE id = @id", ("@id", x.Id));

    public static AdminSettings LoadSettings()
    {
        var map = Db.Query("SELECT skey, svalue FROM app_settings", r => (Key: r.Str("skey"), Value: r.Str("svalue")))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        var defaults = new AdminSettings();
        return new AdminSettings
        {
            PlatformName = map.GetValueOrDefault("PlatformName", defaults.PlatformName),
            SupportEmail = map.GetValueOrDefault("SupportEmail", defaults.SupportEmail),
            DefaultBarangay = map.GetValueOrDefault("DefaultBarangay", defaults.DefaultBarangay),
            AutoApproveThreshold = int.TryParse(map.GetValueOrDefault("AutoApproveThreshold"), out var threshold) ? threshold : defaults.AutoApproveThreshold,
            RequireVerification = map.TryGetValue("RequireVerification", out var rv) ? rv == "1" : defaults.RequireVerification,
            MaintenanceMode = map.TryGetValue("MaintenanceMode", out var mm) ? mm == "1" : defaults.MaintenanceMode,
            SessionTimeoutMinutes = int.TryParse(map.GetValueOrDefault("SessionTimeoutMinutes"), out var timeout) ? timeout : defaults.SessionTimeoutMinutes
        };
    }

    public static void SaveSettings(AdminSettings s)
    {
        var pairs = new (string Key, string Value)[]
        {
            ("PlatformName", s.PlatformName), ("SupportEmail", s.SupportEmail), ("DefaultBarangay", s.DefaultBarangay),
            ("AutoApproveThreshold", s.AutoApproveThreshold.ToString()), ("RequireVerification", s.RequireVerification ? "1" : "0"),
            ("MaintenanceMode", s.MaintenanceMode ? "1" : "0"), ("SessionTimeoutMinutes", s.SessionTimeoutMinutes.ToString())
        };
        Db.Transaction(tx =>
        {
            foreach (var (key, value) in pairs)
                tx.Run("INSERT INTO app_settings (skey, svalue) VALUES (@k, @v) ON DUPLICATE KEY UPDATE svalue = VALUES(svalue)", ("@k", key), ("@v", value));
        });
    }

    public static Dictionary<string, HashSet<string>> LoadPermissions()
    {
        var result = new Dictionary<string, HashSet<string>>();
        foreach (var (role, permission) in Db.Query("SELECT role, permission FROM role_permissions", r => (Role: r.Str("role"), Permission: r.Str("permission"))))
        {
            if (!result.TryGetValue(role, out var set))
                result[role] = set = [];
            set.Add(permission);
        }
        return result;
    }

    public static void SetPermission(string role, string permission, bool allowed)
    {
        if (allowed)
            Db.NonQuery("INSERT IGNORE INTO role_permissions (role, permission) VALUES (@r, @p)", ("@r", role), ("@p", permission));
        else
            Db.NonQuery("DELETE FROM role_permissions WHERE role = @r AND permission = @p", ("@r", role), ("@p", permission));
    }

    // ----- Payments -----------------------------------------------------------

    public static List<PaymentRecordDetails> LoadPayments(int userId, string fullName, bool all) =>
        Db.Query(
            "SELECT p.*, CONCAT(owner.first_name, ' ', owner.last_name) AS owner_name " +
            "FROM payments p JOIN users owner ON owner.id = p.owner_id " +
            "WHERE @all = 1 OR p.owner_id = @u OR EXISTS (" +
            "SELECT 1 FROM tasks t JOIN task_requests accepted ON accepted.task_id = t.id " +
            "WHERE t.title = p.task_title AND t.status = 'Completed' AND accepted.doer_id = @u AND accepted.status = 'accepted') " +
            "ORDER BY p.paid_on DESC, p.id DESC",
            r => new PaymentRecordDetails(
                r.Str("task_title"), r.Str("reference"), r.Str("method"), r.Str("status"),
                Convert.ToDecimal(r.GetValue(r.GetOrdinal("amount"))), r.Date("paid_on"), r.Str("category"),
                r.Str("barangay"), all || r.Int("owner_id") == userId ? r.Str("person") : r.Str("owner_name"), r.Str("description")),
            ("@all", all ? 1 : 0), ("@u", userId));

    public static void InsertPayment(PaymentRecordDetails p, int ownerId) =>
        Db.NonQuery(
            "INSERT INTO payments (reference, owner_id, task_title, method, status, amount, paid_on, category, barangay, person, description) " +
            "VALUES (@ref, @o, @t, @m, @s, @a, @d, @c, @b, @p, @desc) " +
            "ON DUPLICATE KEY UPDATE method = VALUES(method), status = VALUES(status), amount = VALUES(amount), description = VALUES(description)",
            ("@ref", p.Reference), ("@o", ownerId == 0 ? DBNull.Value : (object)ownerId), ("@t", p.Task), ("@m", "Cash"), ("@s", p.Status),
            ("@a", p.Amount), ("@d", p.Date.Date), ("@c", p.Category), ("@b", p.Barangay), ("@p", p.Person), ("@desc", p.Description));

    public static void SetPaymentStatus(string reference, string status) =>
        Db.NonQuery("UPDATE payments SET status = @s WHERE reference = @r", ("@s", status), ("@r", reference));

    /// <summary>Next free PAY-xxxxx reference.</summary>
    public static string NextPaymentReference()
    {
        var max = Db.Scalar("SELECT MAX(CAST(SUBSTRING(reference, 5) AS UNSIGNED)) FROM payments WHERE reference LIKE 'PAY-%'");
        return $"PAY-{(max is null ? 100 : Convert.ToInt32(max)) + 1:00000}";
    }
}
