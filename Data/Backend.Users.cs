using System.Globalization;

namespace TaskTap.Data;

/// <summary>Database-backed repositories used by <see cref="DemoDataStore"/> and the pages.</summary>
public static partial class Backend
{
    private static readonly Color[] AvatarPalette =
    [
        Color.FromArgb(8, 117, 232), Color.FromArgb(109, 131, 215), Color.FromArgb(10, 154, 115),
        Color.FromArgb(223, 129, 80), Color.FromArgb(123, 97, 209), Color.FromArgb(219, 108, 32),
        Color.FromArgb(229, 83, 110), Color.FromArgb(19, 166, 183)
    ];

    public static Color AvatarFor(int id) => AvatarPalette[Math.Abs(id) % AvatarPalette.Length];

    public static string UserKey(int id) => $"USR-{id:000}";

    public static int ParseUserKey(string key) =>
        key.StartsWith("USR-", StringComparison.Ordinal) && int.TryParse(key.AsSpan(4), out var id) ? id : 0;

    /// <summary>Barangays are shown as "Brgy. 176" in the session and stored as the bare number.</summary>
    public static string BarangayNumber(string value) =>
        value.Replace("Brgy.", "", StringComparison.OrdinalIgnoreCase).Replace("Barangay", "", StringComparison.OrdinalIgnoreCase).Trim();

    private static string Initials(string first, string last)
    {
        var a = first.Length > 0 ? char.ToUpperInvariant(first[0]).ToString() : "";
        var b = last.Length > 0 ? char.ToUpperInvariant(last[0]).ToString() : "";
        return a + b;
    }

    private static string RelativeTime(DateTime? value)
    {
        if (value is null) return "Never";
        var days = (DateTime.Today - value.Value.Date).Days;
        if (days <= 0) return "Today";
        if (days == 1) return "Yesterday";
        if (days < 7) return $"{days} days ago";
        if (days < 30) return $"{days / 7} week{(days / 7 == 1 ? "" : "s")} ago";
        return value.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
    }

    public sealed record AuthResult(bool Success, string Error, AppUser? User = null);

    // ----- Login throttling (per identity, in memory) -------------------------

    private static readonly Dictionary<string, (int Failures, DateTime LockedUntil)> LoginAttempts = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsLocked(string identity, out TimeSpan remaining)
    {
        remaining = TimeSpan.Zero;
        if (!LoginAttempts.TryGetValue(identity, out var entry) || entry.LockedUntil <= DateTime.Now)
            return false;
        remaining = entry.LockedUntil - DateTime.Now;
        return true;
    }

    private static void RecordFailure(string identity)
    {
        LoginAttempts.TryGetValue(identity, out var entry);
        var failures = entry.LockedUntil != default && entry.LockedUntil <= DateTime.Now ? 1 : entry.Failures + 1;
        LoginAttempts[identity] = failures >= 5 ? (0, DateTime.Now.AddMinutes(1)) : (failures, default);
    }

    private sealed record UserRow(int Id, string First, string Last, string Email, string Phone, string Hash, string Role,
        string AdminRole, string Barangay, string Status, DateTime Created);

    private static UserRow ReadUserRow(MySqlConnector.MySqlDataReader r) => new(
        r.Int("id"), r.Str("first_name"), r.Str("last_name"), r.Str("email"), r.Str("phone"), r.Str("password_hash"),
        r.Str("role"), r.Str("admin_role"), r.Str("barangay"), r.Str("status"), r.Date("created_at"));

    private static AppUser ToAppUser(UserRow u) => new(
        FullName: $"{u.First} {u.Last}".Trim(),
        FirstName: u.First,
        LastName: u.Last,
        Initials: Initials(u.First, u.Last),
        Role: u.Role,
        Barangay: u.Barangay.Length == 0 ? "" : $"Brgy. {u.Barangay}",
        Email: u.Email,
        MemberSince: u.Created.Year.ToString(CultureInfo.InvariantCulture),
        Id: u.Id,
        AdminRole: u.AdminRole);

    /// <summary>Signs in with email or phone. <paramref name="adminOnly"/> restricts the result to administrators.</summary>
    public static AuthResult Authenticate(string identity, string password, bool adminOnly)
    {
        identity = identity.Trim();
        if (identity.Length == 0 || password.Length == 0)
            return new AuthResult(false, "Enter your email or phone number and password.");
        if (IsLocked(identity, out var remaining))
            return new AuthResult(false, $"Too many attempts. Try again in {Math.Ceiling(remaining.TotalSeconds)} seconds.");

        UserRow? row;
        try
        {
            row = Db.Query(
                "SELECT * FROM users WHERE LOWER(email) = @email OR " +
                "REPLACE(REPLACE(REPLACE(REPLACE(phone, ' ', ''), '-', ''), '(', ''), ')', '') = @phone LIMIT 1",
                ReadUserRow,
                ("@email", identity.ToLowerInvariant()), ("@phone", NormalizePhone(identity))).FirstOrDefault();
        }
        catch (Exception ex)
        {
            return new AuthResult(false, $"Cannot reach the database: {ex.Message}");
        }

        if (row is null)
        {
            RecordFailure(identity);
            return new AuthResult(false, adminOnly ? "No admin account is associated with that email or phone." : "No account is associated with that email or phone.");
        }

        if (!PasswordHasher.Verify(password, row.Hash))
        {
            RecordFailure(identity);
            return new AuthResult(false, adminOnly ? "Incorrect admin password." : "Incorrect password.");
        }

        if (adminOnly && row.Role != "Administrator")
        {
            RecordFailure(identity);
            return new AuthResult(false, "Incorrect admin email or password.");
        }

        if (row.Status == "Suspended")
            return new AuthResult(false, "This account is suspended. Please contact TaskTap support.");
        if (row.Status == "Deactivated")
            return new AuthResult(false, "This account has been deactivated.");

        LoginAttempts.Remove(identity);
        Db.Try(() => Db.NonQuery("UPDATE users SET last_active = NOW() WHERE id = @id", ("@id", row.Id)));
        return new AuthResult(true, "", ToAppUser(row));
    }

    public static AuthResult Register(string first, string last, string email, string phone, string barangay, string password)
    {
        email = email.Trim().ToLowerInvariant();
        phone = phone.Trim();
        first = first.Trim();
        last = last.Trim();
        if (first.Length == 0 || last.Length == 0 || first.Length > 60 || last.Length > 60)
            return new AuthResult(false, "Enter valid first and last names (1–60 characters each).");
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return new AuthResult(false, "Enter a valid email address.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?[0-9 ()-]{7,20}$"))
            return new AuthResult(false, "Enter a valid phone number.");
        try
        {
            if (Db.Scalar("SELECT COUNT(*) FROM users WHERE email = @e", ("@e", email)) is { } e && Convert.ToInt32(e) > 0)
                return new AuthResult(false, "An account with this email already exists.");
            if (phone.Length > 0 && Db.Scalar("SELECT COUNT(*) FROM users WHERE REPLACE(phone, ' ', '') = @p", ("@p", phone.Replace(" ", ""))) is { } p && Convert.ToInt32(p) > 0)
                return new AuthResult(false, "An account with this phone number already exists.");

            var id = (int)Db.Insert(
                "INSERT INTO users (first_name, last_name, email, phone, password_hash, role, barangay, status, last_active) " +
                "VALUES (@f, @l, @e, @p, @h, 'Standard User', @b, 'Active', NOW())",
                ("@f", first.Trim()), ("@l", last.Trim()), ("@e", email), ("@p", phone),
                ("@h", PasswordHasher.Hash(password)), ("@b", BarangayNumber(barangay)));

            var row = Db.Query("SELECT * FROM users WHERE id = @id", ReadUserRow, ("@id", id)).First();
            return new AuthResult(true, "", ToAppUser(row));
        }
        catch (Exception ex)
        {
            return new AuthResult(false, $"Could not create the account: {ex.Message}");
        }
    }

    public static bool EmailExists(string email) =>
        Db.Scalar("SELECT COUNT(*) FROM users WHERE email = @e", ("@e", email.Trim().ToLowerInvariant())) is { } v && Convert.ToInt32(v) > 0;

    public sealed record PasswordResetAccount(int Id, string DisplayName);

    public static PasswordResetAccount? FindPasswordResetAccount(string contact)
    {
        var value = contact.Trim();
        if (value.Length == 0) return null;
        return Db.Query(
            "SELECT id, first_name, last_name FROM users WHERE LOWER(email) = @email OR " +
            "REPLACE(REPLACE(REPLACE(REPLACE(phone, ' ', ''), '-', ''), '(', ''), ')', '') = @phone LIMIT 1",
            r => new PasswordResetAccount(r.Int("id"), $"{r.Str("first_name")} {r.Str("last_name")}".Trim()),
            ("@email", value.ToLowerInvariant()), ("@phone", NormalizePhone(value))).FirstOrDefault();
    }

    private static string NormalizePhone(string value) =>
        new(value.Where(character => char.IsDigit(character) || character == '+').ToArray());

    public static (bool Success, string Error) ResetPassword(int userId, string newPassword)
    {
        try
        {
            if (userId <= 0) return (false, "The recovery session is invalid. Start again.");
            if (newPassword.Length < 8) return (false, "The new password must be at least 8 characters.");
            if (!newPassword.Any(char.IsDigit) && !newPassword.Any(ch => !char.IsLetterOrDigit(ch)))
                return (false, "Include a number or special character.");
            var changed = Db.NonQuery("UPDATE users SET password_hash = @h WHERE id = @id",
                ("@h", PasswordHasher.Hash(newPassword)), ("@id", userId));
            return changed == 1 ? (true, "") : (false, "That account could not be updated.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not update the password: {ex.Message}");
        }
    }

    /// <summary>Changes a password after verifying the current one.</summary>
    public static (bool Success, string Error) ChangePassword(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var hash = Db.Scalar("SELECT password_hash FROM users WHERE id = @id", ("@id", userId))?.ToString();
            if (hash is null || !PasswordHasher.Verify(currentPassword, hash))
                return (false, "Your current password is incorrect.");
            if (newPassword.Length < 8)
                return (false, "The new password must be at least 8 characters.");
            Db.NonQuery("UPDATE users SET password_hash = @h WHERE id = @id", ("@h", PasswordHasher.Hash(newPassword)), ("@id", userId));
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>Saves name/email edits made by the signed-in user or admin.</summary>
    public static (bool Success, string Error) UpdateProfile(int userId, string first, string last, string email, string? phone, string barangay)
    {
        try
        {
            email = email.Trim().ToLowerInvariant();
            var clash = Db.Scalar("SELECT COUNT(*) FROM users WHERE email = @e AND id <> @id", ("@e", email), ("@id", userId));
            if (clash is not null && Convert.ToInt32(clash) > 0)
                return (false, "Another account already uses this email.");
            Db.NonQuery(
                "UPDATE users SET first_name = @f, last_name = @l, email = @e, phone = COALESCE(@p, phone), barangay = @b WHERE id = @id",
                ("@f", first.Trim()), ("@l", last.Trim()), ("@e", email), ("@p", phone?.Trim()), ("@b", BarangayNumber(barangay)), ("@id", userId));
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static (string Phone, string Bio) GetPhoneAndBio(int userId) =>
        Db.Query("SELECT phone, bio FROM users WHERE id = @id", r => (Phone: r.Str("phone"), Bio: r.Str("bio")), ("@id", userId)).FirstOrDefault();

    public static void SetBio(int userId, string bio) =>
        Db.NonQuery("UPDATE users SET bio = @b WHERE id = @id", ("@b", bio), ("@id", userId));

    /// <summary>Lets a member deactivate their own account (an admin can restore it).</summary>
    public static void DeactivateOwnAccount(int userId) =>
        Db.NonQuery("UPDATE users SET status = 'Deactivated' WHERE id = @id", ("@id", userId));

    public static AppUser? GetAppUser(int userId) =>
        Db.Query("SELECT * FROM users WHERE id = @id", ReadUserRow, ("@id", userId)).Select(ToAppUser).FirstOrDefault();

    /// <summary>Loads every member for the admin portal; non-admin sessions get contact details blanked.</summary>
    public static List<DemoUser> LoadUsers(bool includeContactDetails)
    {
        const string sql = """
            SELECT u.*,
                   COALESCE((SELECT AVG(rv.rating) FROM reviews rv WHERE rv.reviewee_id = u.id), 0) AS rating_avg,
                   (SELECT COUNT(*) FROM reviews rv WHERE rv.reviewee_id = u.id) AS review_count,
                   (SELECT COUNT(*) FROM tasks t WHERE t.poster_id = u.id) AS posted_count,
                   (SELECT COUNT(*) FROM task_requests r JOIN tasks t ON t.id = r.task_id
                     WHERE r.doer_id = u.id AND r.status = 'accepted' AND t.status = 'Completed') AS completed_count
            FROM users u ORDER BY u.id
            """;
        return Db.Query(sql, r =>
        {
            var id = r.Int("id");
            var first = r.Str("first_name");
            var last = r.Str("last_name");
            var skills = r.Str("skills");
            return new DemoUser
            {
                Id = UserKey(id),
                FullName = $"{first} {last}".Trim(),
                FirstName = first,
                LastName = last,
                Initials = Initials(first, last),
                Email = includeContactDetails ? r.Str("email") : "",
                Phone = includeContactDetails ? r.Str("phone") : "",
                Role = r.Str("role"),
                AdminRole = r.Str("admin_role"),
                Barangay = r.Str("barangay"),
                Status = r.Str("status"),
                Verified = r.Bool("verified"),
                VerificationRequested = r.Bool("verification_requested"),
                RequestedAt = r.Str("verification_requested_at"),
                VerificationNote = r.Str("verification_note"),
                Rating = Math.Round(Convert.ToDouble(r.GetValue(r.GetOrdinal("rating_avg")), System.Globalization.CultureInfo.InvariantCulture), 1),
                Reviews = r.Int("review_count"),
                CompletedTasks = r.Int("completed_count"),
                PostedTasks = r.Int("posted_count"),
                Joined = r.Date("created_at").Year.ToString(CultureInfo.InvariantCulture),
                LastActive = RelativeTime(r.DateOrNull("last_active")),
                AvatarColor = AvatarFor(id),
                Skills = skills.Length == 0 ? [] : skills.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            };
        });
    }

    public static void SaveUserAdminState(DemoUser user)
    {
        var id = ParseUserKey(user.Id);
        if (id == 0) return;
        Db.NonQuery(
            "UPDATE users SET role = @role, admin_role = @arole, status = @status, verified = @ver, " +
            "verification_requested = @req, verification_requested_at = @reqAt, verification_note = @note WHERE id = @id",
            ("@role", user.Role), ("@arole", user.AdminRole), ("@status", user.Status), ("@ver", user.Verified ? 1 : 0),
            ("@req", user.VerificationRequested ? 1 : 0), ("@reqAt", user.RequestedAt), ("@note", user.VerificationNote), ("@id", id));
    }

    public static void DeleteUser(DemoUser user)
    {
        var id = ParseUserKey(user.Id);
        if (id != 0)
            Db.NonQuery("DELETE FROM users WHERE id = @id", ("@id", id));
    }
}
