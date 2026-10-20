namespace TaskTap.Data;

/// <summary>First-run data: lookup lists, role permissions, default settings and the first administrator.</summary>
internal static class Seeder
{
    /// <summary>Set when this run created the bootstrap administrator (so the app can tell the owner once).</summary>
    public static string? AdminCreatedFor { get; private set; }

    private static int Count(string table)
    {
        var safe = table is "categories" or "barangays" or "role_permissions" or "app_settings"
            ? table : throw new ArgumentException("Unknown table", nameof(table));
        return Convert.ToInt32(Db.Scalar($"SELECT COUNT(*) FROM {safe}") ?? 0);
    }

    public static void Run(AppConfig config)
    {
        if (Count("categories") == 0)
            foreach (var category in DemoDataStore.CreateSeedCategories())
                Backend.SaveCategory(category);

        if (Count("barangays") == 0)
            foreach (var barangay in DemoDataStore.CreateSeedBarangays())
                Backend.SaveBarangay(barangay);

        if (Count("role_permissions") == 0)
            foreach (var (role, permissions) in DemoDataStore.CreateSeedPermissions())
                foreach (var permission in permissions)
                    Backend.SetPermission(role, permission, true);

        if (Count("app_settings") == 0)
            Backend.SaveSettings(new AdminSettings());

        var admins = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM users WHERE role = 'Administrator'") ?? 0);
        if (admins == 0)
        {
            var admin = config.Admin;
            Db.NonQuery(
                "INSERT INTO users (first_name, last_name, email, phone, password_hash, role, admin_role, barangay, status, verified) " +
                "VALUES (@f, @l, @e, '', @h, 'Administrator', 'Super Admin', '176', 'Active', 1) " +
                "ON DUPLICATE KEY UPDATE role = 'Administrator', admin_role = 'Super Admin', status = 'Active', verified = 1",
                ("@f", admin.FirstName), ("@l", admin.LastName), ("@e", admin.Email.Trim().ToLowerInvariant()),
                ("@h", PasswordHasher.Hash(admin.Password)));
            AdminCreatedFor = admin.Email;
        }
    }
}
