using TaskTap.Data;

namespace TaskTap;

/// <summary>
/// Settings: platform configuration, the signed-in admin profile, demo-data reset and
/// shortcuts to Roles &amp; Permissions.
/// </summary>
public sealed class AdminSettingsPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel platformCard = new();
    private readonly Panel profileCard = new();
    private readonly Panel demoCard = new();
    private readonly Panel rolesCard = new();
    private readonly Panel aboutCard = new();
    private readonly Panel saveBar = new();
    private readonly TextBox platformNameBox = new();
    private readonly TextBox supportEmailBox = new();
    private readonly ComboBox defaultBarangayCombo = new();
    private readonly NumericUpDown autoApproveBox = new();
    private readonly NumericUpDown sessionTimeoutBox = new();
    private readonly CheckBox requireVerificationCheck = new();
    private readonly CheckBox maintenanceCheck = new();
    private readonly TextBox adminNameBox = new();
    private readonly TextBox adminEmailBox = new();
    private readonly Label feedbackLabel = new();
    private readonly Button saveButton;
    private readonly Button cancelButton;

    public override AdminRoute AdminRouteName => AdminRoute.Settings;

    public AdminSettingsPage()
    {
        saveButton = AdminUi.MakePrimaryButton("Save changes", "settingsSaveButton", 150);
        cancelButton = AdminUi.MakeSecondaryButton("Cancel", "settingsCancelButton", 140);
        BuildContent();
        LoadSettings();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("CONFIGURATION", 8, AdminUi.Blue, true);
        eyebrow.Name = "settingsEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Settings", 26, AdminUi.Navy, true);
        title.Name = "settingsTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Platform defaults, the admin profile and database controls.", 9, AdminUi.Muted);
        subtitle.Name = "settingsSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        BuildPlatformCard();
        BuildProfileCard();
        BuildDemoCard();
        BuildRolesCard();
        BuildAboutCard();
        BuildSaveBar();

        LayoutPage();
    }

    private void BuildPlatformCard()
    {
        platformCard.BackColor = Color.White;
        platformCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(platformCard);

        var title = AdminUi.MakeLabel("Platform settings", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        platformCard.Controls.Add(title);

        AddField(platformCard, "Platform name", platformNameBox, 18, 50);
        AddField(platformCard, "Support email", supportEmailBox, 18, 118);

        var barangayLabel = FieldLabel("Default barangay", 18, 186);
        platformCard.Controls.Add(barangayLabel);
        defaultBarangayCombo.Name = "settingsBarangayCombo";
        defaultBarangayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        defaultBarangayCombo.Font = new Font("Segoe UI", 9F);
        defaultBarangayCombo.FlatStyle = FlatStyle.Flat;
        defaultBarangayCombo.Items.AddRange(DemoDataStore.ActiveBarangayNames());
        platformCard.Controls.Add(defaultBarangayCombo);

        AddNumber(platformCard, "Auto-approve threshold", autoApproveBox, 350, 50);
        AddNumber(platformCard, "Session timeout (minutes)", sessionTimeoutBox, 350, 118);

        requireVerificationCheck.Name = "settingsRequireVerification";
        requireVerificationCheck.Text = "Require identity verification";
        requireVerificationCheck.AutoSize = true;
        requireVerificationCheck.ForeColor = AdminUi.Navy;
        requireVerificationCheck.Font = new Font("Segoe UI", 9F);
        requireVerificationCheck.Location = new Point(350, 190);
        platformCard.Controls.Add(requireVerificationCheck);

        maintenanceCheck.Name = "settingsMaintenance";
        maintenanceCheck.Text = "Maintenance mode";
        maintenanceCheck.AutoSize = true;
        maintenanceCheck.ForeColor = AdminUi.Navy;
        maintenanceCheck.Font = new Font("Segoe UI", 9F);
        maintenanceCheck.Location = new Point(350, 218);
        platformCard.Controls.Add(maintenanceCheck);
    }

    private void BuildProfileCard()
    {
        profileCard.BackColor = Color.White;
        profileCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(profileCard);

        var title = AdminUi.MakeLabel("Admin profile", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        profileCard.Controls.Add(title);

        AddField(profileCard, "Admin name", adminNameBox, 18, 50);
        AddField(profileCard, "Admin email", adminEmailBox, 18, 118);

        var password = AdminUi.MakeSecondaryButton("Change password", "settingsChangePassword", 170);
        password.Location = new Point(350, 70);
        password.Click += (_, _) =>
        {
            if (AppSession.CurrentAdmin is { } admin && PasswordDialog.ShowFor(this, admin.Id))
                Toast("Password changed.");
        };
        profileCard.Controls.Add(password);

        var note = AdminUi.MakeWrappedLabel("Signed in as Administrator. Your password is stored securely (salted hash).", 8, AdminUi.Muted, 330);
        note.Location = new Point(350, 118);
        profileCard.Controls.Add(note);
    }

    private void BuildDemoCard()
    {
        demoCard.BackColor = Color.White;
        demoCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(demoCard);

        var title = AdminUi.MakeLabel("Database", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        demoCard.Controls.Add(title);

        var note = AdminUi.MakeWrappedLabel($"Connected to the MySQL database \"{Db.DatabaseName}\". Every change is saved automatically.", 8, AdminUi.Muted, 320);
        note.Name = "settingsDemoNote";
        note.Location = new Point(18, 42);
        demoCard.Controls.Add(note);

        var reset = AdminUi.MakeSecondaryButton("Reload data", "settingsResetButton", 180);
        reset.Location = new Point(18, 104);
        reset.Click += (_, _) => ReloadData();
        demoCard.Controls.Add(reset);

        var warning = AdminUi.MakeWrappedLabel("Reloads every list from MySQL.", 7.5F, AdminUi.Muted, 320);
        warning.Location = new Point(18, 152);
        demoCard.Controls.Add(warning);
    }

    private void BuildRolesCard()
    {
        rolesCard.BackColor = Color.White;
        rolesCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(rolesCard);

        var title = AdminUi.MakeLabel("Access control", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        rolesCard.Controls.Add(title);

        var note = AdminUi.MakeWrappedLabel("Manage administrator accounts and the role permission matrix.", 8, AdminUi.Muted, 320);
        note.Location = new Point(18, 42);
        rolesCard.Controls.Add(note);

        var open = AdminUi.MakePrimaryButton("Roles & Permissions", "settingsRolesButton", 200);
        open.Location = new Point(18, 100);
        open.Click += (_, _) => Nav.Navigate(AdminRoute.RolesPermissions);
        rolesCard.Controls.Add(open);
    }

    private void BuildAboutCard()
    {
        aboutCard.BackColor = Color.White;
        aboutCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(aboutCard);

        var title = AdminUi.MakeLabel("About", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        aboutCard.Controls.Add(title);

        var lines = new[]
        {
            "TaskTap admin portal",
            "Data stored in MySQL",
            "Changes are saved immediately"
        };
        for (var index = 0; index < lines.Length; index++)
        {
            var line = AdminUi.MakeLabel(lines[index], 8, AdminUi.Muted);
            line.Location = new Point(18, 44 + index * 22);
            aboutCard.Controls.Add(line);
        }
    }

    private void BuildSaveBar()
    {
        saveBar.BackColor = Color.Transparent;
        page.Controls.Add(saveBar);

        feedbackLabel.AutoSize = true;
        feedbackLabel.ForeColor = AdminUi.Good;
        feedbackLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        feedbackLabel.Location = new Point(0, 12);
        saveBar.Controls.Add(feedbackLabel);

        saveButton.Click += (_, _) => SaveSettings();
        saveBar.Controls.Add(saveButton);

        cancelButton.Click += (_, _) =>
        {
            LoadSettings();
            feedbackLabel.Text = string.Empty;
            Toast("Changes discarded.");
        };
        saveBar.Controls.Add(cancelButton);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "settingsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "settingsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "settingsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        var leftWidth = (int)(contentWidth * 0.58);
        var rightWidth = contentWidth - leftWidth - 18;

        platformCard.Bounds = new Rectangle(left, 130, leftWidth, 320);
        platformNameBox.Bounds = new Rectangle(18, 70, 300, 30);
        supportEmailBox.Bounds = new Rectangle(18, 138, 300, 30);
        defaultBarangayCombo.Bounds = new Rectangle(18, 206, 300, 30);
        autoApproveBox.Bounds = new Rectangle(350, 70, 160, 30);
        sessionTimeoutBox.Bounds = new Rectangle(350, 138, 160, 30);

        profileCard.Bounds = new Rectangle(left, 466, leftWidth, 250);
        adminNameBox.Bounds = new Rectangle(18, 70, 300, 30);
        adminEmailBox.Bounds = new Rectangle(18, 138, 300, 30);

        demoCard.Bounds = new Rectangle(left + leftWidth + 18, 130, rightWidth, 200);
        var demoNote = AdminUi.Find<Label>(demoCard, "settingsDemoNote");
        if (demoNote is not null) demoNote.MaximumSize = new Size(Math.Max(200, rightWidth - 36), 0);

        rolesCard.Bounds = new Rectangle(left + leftWidth + 18, 346, rightWidth, 160);
        aboutCard.Bounds = new Rectangle(left + leftWidth + 18, 522, rightWidth, 140);

        saveBar.Bounds = new Rectangle(left, 746, contentWidth, 44);
        saveButton.Location = new Point(contentWidth - 150, 0);
        cancelButton.Location = new Point(contentWidth - 300, 0);

        page.AutoScrollMinSize = new Size(0, 830);
    }

    private void LoadSettings()
    {
        var settings = DemoDataStore.Settings;
        platformNameBox.Text = settings.PlatformName;
        supportEmailBox.Text = settings.SupportEmail;
        var barangayIndex = defaultBarangayCombo.Items.IndexOf(settings.DefaultBarangay);
        defaultBarangayCombo.SelectedIndex = barangayIndex >= 0 ? barangayIndex : 0;
        autoApproveBox.Value = Math.Clamp(settings.AutoApproveThreshold, 0, 999);
        sessionTimeoutBox.Value = Math.Clamp(settings.SessionTimeoutMinutes, 0, 999);
        requireVerificationCheck.Checked = settings.RequireVerification;
        maintenanceCheck.Checked = settings.MaintenanceMode;
        adminNameBox.Text = AppSession.CurrentAdmin?.FullName ?? "";
        adminEmailBox.Text = AppSession.CurrentAdmin?.Email ?? "";
        feedbackLabel.Text = string.Empty;
    }

    private void SaveSettings()
    {
        if (string.IsNullOrWhiteSpace(platformNameBox.Text) || string.IsNullOrWhiteSpace(supportEmailBox.Text))
        {
            feedbackLabel.ForeColor = AdminUi.Bad;
            feedbackLabel.Text = "Platform name and support email are required.";
            return;
        }

        DemoDataStore.UpdateSettings(new AdminSettings
        {
            PlatformName = platformNameBox.Text.Trim(),
            SupportEmail = supportEmailBox.Text.Trim(),
            DefaultBarangay = defaultBarangayCombo.SelectedItem?.ToString() ?? "Brgy. 176",
            AutoApproveThreshold = (int)autoApproveBox.Value,
            SessionTimeoutMinutes = (int)sessionTimeoutBox.Value,
            RequireVerification = requireVerificationCheck.Checked,
            MaintenanceMode = maintenanceCheck.Checked
        });

        feedbackLabel.ForeColor = AdminUi.Good;
        feedbackLabel.Text = "Settings saved.";
        Toast("Settings saved.");
    }

    private void ReloadData()
    {
        if (AppSession.CurrentAdmin is { } admin)
            DemoDataStore.LoadForAdmin(admin);
        LoadSettings();
        feedbackLabel.ForeColor = AdminUi.Good;
        feedbackLabel.Text = "Data reloaded from MySQL.";
        Toast("Data reloaded.");
    }

    private static Label FieldLabel(string text, int left, int top)
    {
        var label = AdminUi.MakeLabel(text, 8, AdminUi.Muted, true);
        label.Location = new Point(left, top);
        return label;
    }

    private void AddField(Control parent, string label, TextBox box, int left, int top)
    {
        parent.Controls.Add(FieldLabel(label, left, top));
        box.Name = $"settings{label.Replace(" ", string.Empty)}";
        box.BorderStyle = BorderStyle.FixedSingle;
        box.Font = new Font("Segoe UI", 10F);
        box.Location = new Point(left, top + 20);
        box.Size = new Size(300, 30);
        parent.Controls.Add(box);
    }

    private void AddNumber(Control parent, string label, NumericUpDown box, int left, int top)
    {
        parent.Controls.Add(FieldLabel(label, left, top));
        box.Name = $"settings{label.Replace(" ", string.Empty)}";
        box.Font = new Font("Segoe UI", 9F);
        box.Minimum = 0;
        box.Maximum = 999;
        box.Location = new Point(left, top + 20);
        box.Size = new Size(160, 30);
        parent.Controls.Add(box);
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => LoadSettings();
}
