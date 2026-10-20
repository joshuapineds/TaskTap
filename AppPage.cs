namespace TaskTap;

/// <summary>
/// Base class for every page hosted inside <see cref="MainShellForm"/>. Pages are
/// <see cref="UserControl"/>s that fill the shell's content host; the shell owns
/// the sidebar, topbar, back stack and toast. Pages never open or hide windows.
/// </summary>
public abstract class AppPage : UserControl
{
    /// <summary>The canonical route this page represents, used for shell chrome.</summary>
    public abstract Route RouteName { get; }

    protected AppPage()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(244, 248, 252);
        Font = new Font("Segoe UI", 9F);
    }

    /// <summary>The application navigation owner.</summary>
    protected NavigationService Nav => NavigationService.Current;

    /// <summary>Called each time the page is shown by the shell.</summary>
    public virtual void OnNavigatedTo()
    {
    }
}
