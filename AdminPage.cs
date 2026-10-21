namespace TaskTap;

/// <summary>
/// Base class for every page hosted inside <see cref="AdminShellForm"/>. Pages are
/// <see cref="UserControl"/>s that fill the shell's content host; the shell owns the
/// sidebar, topbar, back stack and toast. Pages never open or hide windows.
/// </summary>
public abstract class AdminPage : UserControl
{
    /// <summary>The canonical route this page represents, used for shell chrome.</summary>
    public abstract AdminRoute AdminRouteName { get; }

    protected AdminPage()
    {
        Dock = DockStyle.Fill;
        BackColor = AdminUi.Page;
        Font = new Font("Segoe UI", 9F);
    }

    /// <summary>The admin navigation owner.</summary>
    protected AdminNavigationService Nav => AdminNavigationService.Current;

    /// <summary>Called each time the page is shown by the shell.</summary>
    public virtual void OnNavigatedTo()
    {
    }
}
