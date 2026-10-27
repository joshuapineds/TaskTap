namespace TaskTap;

/// <summary>A community member managed through the admin portal.</summary>
public sealed class DemoUser
{
    public string Id { get; init; } = "";
    public string FullName { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Initials { get; init; } = "";
    public string Email { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Role { get; set; } = "Standard User";
    public string AdminRole { get; set; } = "";
    public string Barangay { get; set; } = "";
    public string Status { get; set; } = "Active";
    public bool Verified { get; set; }
    public bool VerificationRequested { get; set; }
    public string RequestedAt { get; set; } = "";
    public string VerificationNote { get; set; } = "";
    public double Rating { get; init; }
    public int Reviews { get; init; }
    public int CompletedTasks { get; init; }
    public int PostedTasks { get; init; }
    public string Joined { get; init; } = "";
    public string LastActive { get; init; } = "";
    public Color AvatarColor { get; init; }
    public string[] Skills { get; init; } = [];

    public bool IsAdministrator => Role == "Administrator";
    public bool IsPendingVerification => VerificationRequested || (!Verified && Status == "Active");
}

/// <summary>A task category the user app reads for tiles, filters and pickers.</summary>
public sealed class DemoCategory
{
    public string Id { get; init; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public Color Color { get; set; }
    public Color Accent { get; set; }
    public bool Active { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>A barangay the user app reads for address dropdowns and filters.</summary>
public sealed class DemoBarangay
{
    public string Id { get; init; } = "";
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public bool Active { get; set; } = true;
}

/// <summary>A moderation report raised against a task, user, message or review.</summary>
public sealed class DemoReport
{
    public string Id { get; init; } = "";
    public string ReporterName { get; init; } = "";
    public string TargetType { get; init; } = "";
    public string TargetLabel { get; init; } = "";
    public string Reason { get; init; } = "";
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public string CreatedAt { get; init; } = "";
    public string Description { get; init; } = "";
    public string Resolution { get; set; } = "";
}

/// <summary>A task dispute between a poster and a doer.</summary>
public sealed class DemoDispute
{
    public string Id { get; init; } = "";
    public string TaskTitle { get; init; } = "";
    public string PosterName { get; init; } = "";
    public string DoerName { get; init; } = "";
    public int Amount { get; init; }
    public string Reason { get; init; } = "";
    public string Status { get; set; } = "Open";
    public string OpenedAt { get; init; } = "";
    public string Resolution { get; set; } = "";
}

/// <summary>An audit entry written by every admin mutation.</summary>
public sealed class DemoActivity
{
    public string Id { get; init; } = "";
    public string Actor { get; init; } = "";
    public string Action { get; init; } = "";
    public string Target { get; init; } = "";
    public string Category { get; init; } = "";
    public DateTime Timestamp { get; init; }
    public string Detail { get; init; } = "";
}

/// <summary>Platform-wide settings edited from the admin Settings page.</summary>
public sealed class AdminSettings
{
    public string PlatformName { get; set; } = "TaskTap";
    public string SupportEmail { get; set; } = "support@tasktap.local";
    public string DefaultBarangay { get; set; } = "Brgy. 176";
    public int AutoApproveThreshold { get; set; } = 90;
    public bool RequireVerification { get; set; } = true;
    public bool MaintenanceMode { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 30;
}

/// <summary>A support request raised by a member.</summary>
public sealed class DemoSupportTicket
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Category { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; set; } = "Open";
    public string CreatedAt { get; init; } = "";
    public string Reply { get; set; } = "";
}

/// <summary>An announcement published from the admin portal and shown to members.</summary>
public sealed class DemoAnnouncement
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public string Audience { get; init; } = "All members";
    public bool Active { get; set; } = true;
    public string PublishedAt { get; init; } = "";
    public string Author { get; init; } = "";
}
