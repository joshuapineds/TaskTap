namespace TaskTap.Data;

/// <summary>Upgrades databases created by earlier versions of TaskTap. Safe to run on every start.</summary>
internal static class Migrations
{
    public static void Apply()
    {
        Db.EnsureColumn("users", "bio", "VARCHAR(240) NOT NULL DEFAULT ''");
        Db.EnsureColumn("tasks", "latitude", "DOUBLE NULL");
        Db.EnsureColumn("tasks", "longitude", "DOUBLE NULL");
        Db.EnsureColumn("barangays", "latitude", "DOUBLE NULL");
        Db.EnsureColumn("barangays", "longitude", "DOUBLE NULL");
        Db.EnsureColumn("tasks", "start_time", "VARCHAR(12) NOT NULL DEFAULT ''");
        Db.EnsureColumn("task_requests", "attachment_name", "VARCHAR(200) NULL");
        Db.EnsureColumn("task_requests", "attachment_size", "INT NULL");
        Db.EnsureColumn("task_requests", "attachment", "LONGBLOB NULL");
        Db.NonQuery("UPDATE tasks SET payment_method = 'Cash' WHERE payment_method <> 'Cash'");
        Db.NonQuery("UPDATE payments SET method = 'Cash' WHERE method <> 'Cash'");
    }
}
