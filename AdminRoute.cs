namespace TaskTap;

/// <summary>
/// Canonical destinations for the admin shell. The global routes mirror the admin
/// sidebar; the remainder are contextual subpages reached from those routes.
/// </summary>
public enum AdminRoute
{
    Dashboard,
    Users,
    UserDetail,
    VerificationQueue,
    RolesPermissions,
    Tasks,
    Categories,
    Reports,
    Disputes,
    Payments,
    Archives,
    Barangays,
    ActivityLog,
    Settings,
    Announcements,
    SupportTickets,
    AdminProfile
}
