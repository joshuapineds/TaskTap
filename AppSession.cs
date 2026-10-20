using TaskTap.Data;

namespace TaskTap;

/// <summary>A signed-in member or administrator (loaded from the users table).</summary>
public sealed record AppUser(
    string FullName,
    string FirstName,
    string LastName,
    string Initials,
    string Role,
    string Barangay,
    string Email,
    string MemberSince,
    int Id = 0,
    string AdminRole = "",
    string LastActive = "Never");

/// <summary>
/// The shared session. Every page reads the current user from here. Signing in loads that
/// user's data from MySQL into <see cref="DemoDataStore"/>; signing out clears it.
/// </summary>
public static class AppSession
{
    private static readonly AppUser GuestUser = new("Guest", "Guest", "", "G", "Guest", "", "", "", 0);

    private static AppUser? _currentUser;
    private static AppUser? _currentAdmin;

    /// <summary>The signed-in member (a placeholder "Guest" while nobody is signed in).</summary>
    public static AppUser CurrentUser => _currentUser ?? GuestUser;

    /// <summary>True once a member has signed in.</summary>
    public static bool IsSignedIn => _currentUser is not null;

    /// <summary>The signed-in admin (null while the admin portal is closed).</summary>
    public static AppUser? CurrentAdmin => _currentAdmin;

    /// <summary>True while an admin session is active.</summary>
    public static bool IsAdminSession => _currentAdmin is not null;

    /// <summary>A task the guest selected before signing in, reopened after authentication.</summary>
    public static DemoTask? PendingTaskAfterLogin { get; set; }

    /// <summary>Establishes the member session and loads that member's data.</summary>
    public static void SignInAs(AppUser user)
    {
        _currentUser = user;
        DemoDataStore.LoadForUser(user);
    }

    /// <summary>Replaces the member's cached profile after an edit (no data reload).</summary>
    public static void UpdateCurrentUser(AppUser user) => _currentUser = user;

    /// <summary>Starts an admin session and loads the admin portal's data.</summary>
    public static void SignInAdminAs(AppUser admin)
    {
        _currentAdmin = admin;
        DemoDataStore.LoadForAdmin(admin);
    }

    /// <summary>Replaces the admin's cached profile after an edit.</summary>
    public static void UpdateCurrentAdmin(AppUser admin) => _currentAdmin = admin;

    /// <summary>Ends the admin session only.</summary>
    public static void SignOutAdmin()
    {
        _currentAdmin = null;
        if (_currentUser is not null)
            DemoDataStore.LoadForUser(_currentUser);
        else
            DemoDataStore.ClearSessionData();
    }

    /// <summary>Clears the session on logout.</summary>
    public static void SignOut()
    {
        _currentUser = null;
        _currentAdmin = null;
        PendingTaskAfterLogin = null;
        DemoDataStore.ClearSessionData();
    }
}
