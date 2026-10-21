namespace TaskTap;

/// <summary>Helpers for opening another member's public profile.</summary>
public static class ProfileLinks
{
    /// <summary>Finds a store member by full name.</summary>
    public static DemoUser? Find(string? fullName) =>
        DemoDataStore.Users.FirstOrDefault(user => string.Equals(user.FullName, fullName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Builds a public-profile page for the named member (with a safe fallback).</summary>
    public static PublicProfilePage For(string? fullName, string fallbackInitials = "TM")
    {
        var user = Find(fullName);
        if (user is not null)
            return new PublicProfilePage(user);

        var name = string.IsNullOrWhiteSpace(fullName) ? "TaskTap member" : fullName.Trim();
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new PublicProfilePage(new DemoUser
        {
            Id = "GUEST",
            FullName = name,
            FirstName = parts.Length > 0 ? parts[0] : name,
            LastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "",
            Initials = fallbackInitials,
            Role = "Standard User",
            Barangay = "",
            Status = "Active",
            Verified = false,
            Rating = 0,
            Reviews = 0,
            CompletedTasks = 0,
            PostedTasks = 0,
            Joined = "2025",
            LastActive = "Recently",
            AvatarColor = Color.FromArgb(8, 117, 232)
        });
    }
}
