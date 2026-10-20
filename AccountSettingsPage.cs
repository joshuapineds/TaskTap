using TaskTap.Data;

namespace TaskTap;

public sealed class AccountSettingsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(104, 121, 144);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel page = new();
    private readonly Panel settingsNav = new();
    private readonly Panel sectionHost = new();
    private readonly Label saveState = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private readonly Dictionary<string, Button> sectionButtons = [];
    private readonly Dictionary<string, Panel> sections = [];
    private readonly FlowLayoutPanel skillsPanel = new();
    private readonly TextBox firstName = new() { Text = AppSession.CurrentUser.FirstName };
    private readonly TextBox lastName = new() { Text = AppSession.CurrentUser.LastName };
    private readonly TextBox bio = new() { Text = "Friendly and dependable TaskTap member who enjoys helping neighbors with practical errands, tutoring, home tasks, and short-term community work. I communicate clearly, arrive on time, and keep task details organized.", Multiline = true, MaxLength = 240, ScrollBars = ScrollBars.Vertical };
    private readonly Label bioCount = new();
    private readonly Label verificationStatus = new();
    private readonly TextBox email = new() { Text = AppSession.CurrentUser.Email };
    private readonly TextBox phone = new() { Text = Backend.GetPhoneAndBio(AppSession.CurrentUser.Id).Phone };
    private readonly ComboBox barangay = new();
    private readonly ComboBox contactMethod = new();
    private readonly TextBox currentPassword = new() { UseSystemPasswordChar = true };
    private readonly TextBox newPassword = new() { UseSystemPasswordChar = true };
    private readonly TextBox confirmPassword = new() { UseSystemPasswordChar = true };
    private readonly ComboBox emailDelivery = new();
    private readonly List<string> skills = ["Errands", "Basic repairs", "Cleaning", "Tutoring", "Pet care"];
    private bool pageBuilt;
    private string activeSection = "profile";

    public override Route RouteName => Route.AccountSettings;

    public AccountSettingsPage()
    {
        BuildPage();
    }

    private void BuildPage()
    {
        page.Dock = DockStyle.Fill;
        page.AutoScroll = true;
        page.BackColor = PageColor;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);
        BuildContent();

        toast.Size = new Size(440, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);

        pageBuilt = true;
        UpdateBioCount();
        LayoutPage();
    }

    private void UpdateBioCount()
    {
        bioCount.Text = $"{bio.Text.Length}/240";
        bioCount.ForeColor = bio.Text.Length >= bio.MaxLength ? Color.FromArgb(196, 61, 73) : Muted;
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Profile   /   Account Settings", 8, Muted, true);
        crumb.Location = new Point(22, 12);
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Navigate(Route.Profile);
        page.Controls.Add(crumb);
        AddPageLabel("settingsEyebrow", "ACCOUNT & PREFERENCES", 8, Blue, true);
        AddPageLabel("settingsHeading", "Edit Profile / Account Settings", 24, Navy, true);
        AddPageLabel("settingsSubtitle", "Keep your information current so neighbors know how to work with you.", 9, Muted, false);
        var user = AppSession.CurrentUser;
        if (!string.IsNullOrEmpty(newPassword.Text))
        {
            var changed = Backend.ChangePassword(user.Id, currentPassword.Text, newPassword.Text);
            if (!changed.Success)
            {
                SelectSection("security");
                ShowToast(changed.Error);
                return;
            }
        }

        var saved = Backend.UpdateProfile(user.Id, firstName.Text, lastName.Text, email.Text, phone.Text, user.Barangay);
        if (!saved.Success)
        {
            ShowToast(saved.Error);
            return;
        }

        Db.Try(() => Backend.SetBio(user.Id, bio.Text.Trim()));
        var first = firstName.Text.Trim();
        var last = lastName.Text.Trim();
        AppSession.UpdateCurrentUser(user with
        {
            FirstName = first,
            LastName = last,
            FullName = $"{first} {last}".Trim(),
            Initials = ((first.Length > 0 ? first[0].ToString() : "") + (last.Length > 0 ? last[0].ToString() : "")).ToUpperInvariant(),
            Email = email.Text.Trim().ToLowerInvariant()
        });
        currentPassword.Clear();
        newPassword.Clear();
        confirmPassword.Clear();
        saveState.Text = "All changes saved";
        saveState.ForeColor = Color.FromArgb(19, 160, 113);
        saveState.BackColor = Color.FromArgb(234, 249, 244);
        ShowToast("Your account settings were saved.");
    }

    private void ConfirmDeactivation()
    {
        if (MessageBox.Show(this, "Deactivate your account? You will be signed out and will not be able to sign in until an admin restores it.", "Deactivate account", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        if (!Db.Try(() => Backend.DeactivateOwnAccount(AppSession.CurrentUser.Id)))
            return;
        NavigationService.Current.Logout();
    }

    private void SelectSection(string section)
    {
        activeSection = section;
        foreach (var (key, button) in sectionButtons)
        {
            var active = string.Equals(key, section, StringComparison.OrdinalIgnoreCase);
            button.BackColor = active ? Color.FromArgb(234, 244, 255) : Color.White;
            button.ForeColor = active ? Blue : Muted;
        }

        foreach (var (key, panel) in sections)
            panel.Visible = string.Equals(key, section, StringComparison.OrdinalIgnoreCase);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var contentWidth = width - 44;
        var labels = page.Controls.OfType<Label>().ToArray();
        if (labels.Length >= 3)
        {
            labels[0].Location = new Point(22, 12);
            labels[1].Location = new Point(22, 40);
            labels[2].Location = new Point(22, 56);
            if (labels.Length > 3) labels[3].Location = new Point(22, 104);
        }
        saveState.Location = new Point(width - saveState.Width - 22, 61);
        settingsNav.Bounds = new Rectangle(22, 138, 238, 360);
        sectionHost.Bounds = new Rectangle(278, 138, contentWidth - 256, 620);
        foreach (var section in sections.Values) section.Bounds = new Rectangle(0, 0, sectionHost.Width, sectionHost.Height);
        var cancel = page.Controls.Find("SettingsCancelButton", false).FirstOrDefault();
        var save = page.Controls.Find("SettingsSaveButton", false).FirstOrDefault();
        if (cancel is not null) cancel.Location = new Point(width - cancel.Width - (save?.Width ?? 0) - 34, 775);
        if (save is not null) save.Location = new Point(width - save.Width - 22, 775);
        page.AutoScrollMinSize = new Size(0, 830);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void AddPageLabel(string name, string text, float size, Color color, bool bold)
    {
        var label = MakeLabel(text, size, color, bold);
        label.Name = name;
        page.Controls.Add(label);
    }

    private void AddLabel(Control parent, string text, float size, Color color, bool bold, Point location, Size dimensions)
    {
        parent.Controls.Add(new Label { Text = text, Location = location, Size = dimensions, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent, AutoEllipsis = true });
    }

    private Label MakeLabel(string text, float size, Color color, bool bold) => new() { Text = text, AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent };

    private void RequestVerification()
    {
        var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == AppSession.CurrentUser.FullName);
        if (user is null)
        {
            ShowToast("No matching member record for this session.");
            return;
        }

        if (user.VerificationRequested)
        {
            ShowToast("Your verification request is already pending.");
            return;
        }

        using var dialog = new ReasonDialog("Request verification", "Add a note for the reviewer (optional).", ["Identity document ready", "Address proof ready", "Re-verification", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.RequestVerification(user, dialog.SelectedReason ?? "Verification requested.");
        UpdateVerificationStatus();
        ShowToast("Verification requested. An admin will review it.");
    }

    private void UpdateVerificationStatus()
    {
        var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == AppSession.CurrentUser.FullName);
        if (user is null)
        {
            verificationStatus.Text = "Not verified";
            verificationStatus.ForeColor = Muted;
            return;
        }

        if (user.VerificationRequested)
        {
            verificationStatus.Text = $"Verification requested {user.RequestedAt}";
            verificationStatus.ForeColor = Color.FromArgb(180, 130, 20);
        }
        else if (user.Verified)
        {
            verificationStatus.Text = "Verified account";
            verificationStatus.ForeColor = Color.FromArgb(20, 153, 111);
        }
        else
        {
            verificationStatus.Text = "Not verified yet";
            verificationStatus.ForeColor = Muted;
        }
    }

    public override void OnNavigatedTo() => UpdateVerificationStatus();

    private void ShowToast(string message)    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private Panel MakeCard() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

    private Button MakeButton(string text, string name, bool primary, int width)
    {
        var button = new Button { Text = text, Name = name, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private Button MakeLink(string text)
    {
        var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, TabStop = false };
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(0);
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
