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
    private readonly TextBox bio = new() { Text = Backend.GetPhoneAndBio(AppSession.CurrentUser.Id).Bio, Multiline = true, MaxLength = 240, ScrollBars = ScrollBars.Vertical };
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
    private bool initialized;
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

        initialized = true;
        pageBuilt = true;
        UpdateBioCount();
        LayoutPage();
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
        saveState.Text = "All changes saved";
        saveState.AutoSize = true;
        saveState.ForeColor = Color.FromArgb(19, 160, 113);
        saveState.BackColor = Color.FromArgb(234, 249, 244);
        saveState.Padding = new Padding(12, 8, 12, 8);
        saveState.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        page.Controls.Add(saveState);

        settingsNav.Name = "settingsNavigation";
        settingsNav.BackColor = Color.White;
        settingsNav.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(settingsNav);
        var nav = new[]
        {
            ("profile", "Profile information", "About you & skills"),
            ("contact", "Contact & location", "Reach you safely"),
            ("security", "Password & security", "Protect your account"),
            ("notifications", "Notifications", "Choose what you receive"),
            ("privacy", "Privacy & account", "Visibility & controls")
        };
        for (var i = 0; i < nav.Length; i++)
        {
            var (key, title, subtitle) = nav[i];
            var button = new Button { Text = title + "\r\n" + subtitle, Tag = key, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 5, 0), Size = new Size(210, 62), Location = new Point(8, 8 + i * 68), FlatStyle = FlatStyle.Flat, BackColor = key == activeSection ? Color.FromArgb(237, 245, 255) : Color.White, ForeColor = key == activeSection ? Blue : Color.FromArgb(80, 98, 123), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => SelectSection((string)button.Tag!);
            sectionButtons.Add(key, button);
            settingsNav.Controls.Add(button);
        }
        sectionHost.Name = "settingsSectionHost";
        page.Controls.Add(sectionHost);

        BuildProfileSection();
        BuildContactSection();
        BuildSecuritySection();
        BuildNotificationSection();
        BuildPrivacySection();
        ShowSection(activeSection);

        var cancel = MakeButton("Cancel", "SettingsCancelButton", false, 110);
        cancel.Click += (_, _) => Nav.Navigate(Route.Profile);
        page.Controls.Add(cancel);
        var save = MakeButton("Save Changes", "SettingsSaveButton", true, 140);
        save.Click += (_, _) => SaveChanges();
        page.Controls.Add(save);
    }

    private void BuildProfileSection()
    {
        var panel = CreateSection("profile");
        AddSectionTitle(panel, "PROFILE INFORMATION", "Tell the community about you", "These details appear on your TaskTap profile and can help task posters choose the right person.");
        var avatarArea = MakeCard();
        avatarArea.Bounds = new Rectangle(24, 100, 610, 94);
        panel.Controls.Add(avatarArea);
        var avatar = new Label { Text = AppSession.CurrentUser.Initials, Location = new Point(14, 12), Size = new Size(70, 70), BackColor = Color.FromArgb(220, 236, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 21F, FontStyle.Bold) };
        avatarArea.Controls.Add(avatar);
        AddLabel(avatarArea, "Profile photo", 9, Navy, true, new Point(100, 12), new Size(220, 20));
        AddLabel(avatarArea, "Use a clear photo so neighbors can recognize you. Photo upload is coming soon.", 8, Muted, false, new Point(100, 35), new Size(440, 20));
        var photo = MakeButton("Change photo", "SettingsPhotoButton", false, 112);
        photo.Location = new Point(100, 59);
        photo.Click += (_, _) => ShowToast("Profile photos are not available yet.");
        avatarArea.Controls.Add(photo);

        var verify = MakeButton("Request verification", "SettingsRequestVerificationButton", false, 170);
        verify.Location = new Point(220, 59);
        verify.Click += (_, _) => RequestVerification();
        avatarArea.Controls.Add(verify);

        verificationStatus.AutoSize = false;
        verificationStatus.Size = new Size(210, 36);
        verificationStatus.Location = new Point(400, 59);
        verificationStatus.ForeColor = Muted;
        verificationStatus.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        avatarArea.Controls.Add(verificationStatus);
        UpdateVerificationStatus();
        AddFieldLabel(panel, "First name", firstName, 24, 215, 286, 54);
        AddFieldLabel(panel, "Last name", lastName, 338, 215, 286, 54);
        AddFieldLabel(panel, "About you", bio, 24, 285, 600, 125);
        bioCount.Location = new Point(560, 393);
        bioCount.Size = new Size(64, 18);
        bioCount.TextAlign = ContentAlignment.MiddleRight;
        bioCount.ForeColor = Muted;
        bioCount.Font = new Font("Segoe UI", 7.5F);
        panel.Controls.Add(bioCount);
        AddLabel(panel, "Skills & services", 11, Navy, true, new Point(24, 430), new Size(260, 23));
        AddLabel(panel, "Add services you are comfortable offering.", 8, Muted, false, new Point(24, 454), new Size(360, 20));
        skillsPanel.Location = new Point(24, 482);
        skillsPanel.Size = new Size(600, 118);
        skillsPanel.WrapContents = true;
        skillsPanel.FlowDirection = FlowDirection.LeftToRight;
        skillsPanel.BackColor = Color.White;
        panel.Controls.Add(skillsPanel);
        RenderSkills();
    }

    private void BuildContactSection()
    {
        var panel = CreateSection("contact");
        AddSectionTitle(panel, "CONTACT & LOCATION", "Keep coordination simple", "TaskTap uses contact details for account communication. Your exact location is not displayed to other users.");
        AddFieldLabel(panel, "Email address", email, 24, 115, 286, 54);
        AddFieldLabel(panel, "Mobile number", phone, 338, 115, 286, 54);
        barangay.DropDownStyle = ComboBoxStyle.DropDownList;
        barangay.Items.AddRange(["Brgy. 176", "Brgy. 178", "Brgy. 179", "Brgy. 180"]);
        barangay.SelectedIndex = 0;
        AddFieldLabel(panel, "Barangay", barangay, 24, 210, 286, 54);
        contactMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        contactMethod.Items.AddRange(["Mobile phone", "Email"]);
        contactMethod.SelectedIndex = 0;
        AddFieldLabel(panel, "Preferred contact method", contactMethod, 338, 210, 286, 54);
        AddInfoBox(panel, "⌖", "Approximate location only", "TaskTap does not expose a precise home address or continuous GPS location. Share an exact meeting point only when necessary and appropriate for a task.", 302);
    }

    private void BuildSecuritySection()
    {
        var panel = CreateSection("security");
        AddSectionTitle(panel, "PASSWORD & SECURITY", "Protect your account", "Use a strong password and keep verification information private.");
        AddPasswordField(panel, "Current password", currentPassword, 24, 112, 600, "Enter current password");
        AddPasswordField(panel, "New password", newPassword, 24, 205, 286, "At least 8 characters");
        AddPasswordField(panel, "Confirm new password", confirmPassword, 338, 205, 286, "Repeat new password");
        AddLabel(panel, "Use uppercase, lowercase, number, and symbol.", 7.5F, Muted, false, new Point(24, 272), new Size(286, 18));
        var security = MakeCard();
        security.Bounds = new Rectangle(24, 310, 600, 115);
        panel.Controls.Add(security);
        AddLabel(security, "✓", 12, Color.FromArgb(19, 160, 113), true, new Point(18, 18), new Size(28, 24));
        AddLabel(security, "Verified account", 9, Navy, true, new Point(54, 18), new Size(210, 20));
        AddLabel(security, "Request verification so other members can trust your profile.", 8, Muted, false, new Point(54, 41), new Size(255, 48));
        AddLabel(security, "⌁", 12, Blue, true, new Point(326, 18), new Size(28, 24));
        AddLabel(security, "Active sessions", 9, Navy, true, new Point(362, 18), new Size(195, 20));
        AddLabel(security, "Signed in on this device", 8, Muted, false, new Point(362, 41), new Size(220, 20));
        var signOut = MakeLink("Sign out other sessions");
        signOut.Location = new Point(362, 67);
        signOut.Click += (_, _) => ShowToast("You are only signed in on this device.");
        security.Controls.Add(signOut);
        var danger = MakeCard();
        danger.BackColor = Color.FromArgb(255, 248, 248);
        danger.Bounds = new Rectangle(24, 445, 600, 112);
        panel.Controls.Add(danger);
        AddLabel(danger, "DANGER ZONE", 7.5F, Color.FromArgb(189, 64, 64), true, new Point(15, 10), new Size(180, 18));
        AddLabel(danger, "Deactivate account", 10, Navy, true, new Point(15, 31), new Size(230, 22));
        AddLabel(danger, "Hide your account temporarily. An admin can restore it later.", 8, Muted, false, new Point(15, 55), new Size(390, 36));
        var deactivate = MakeButton("Deactivate", "SettingsDeactivateButton", false, 105);
        deactivate.ForeColor = Color.FromArgb(189, 64, 64);
        deactivate.Location = new Point(472, 38);
        deactivate.Click += (_, _) => ConfirmDeactivation();
        danger.Controls.Add(deactivate);
    }

    private void BuildNotificationSection()
    {
        var panel = CreateSection("notifications");
        AddSectionTitle(panel, "NOTIFICATIONS", "Choose what reaches you", "Control updates without missing important task activity.");
        var card = MakeCard();
        card.Bounds = new Rectangle(24, 112, 600, 120);
        panel.Controls.Add(card);
        AddLabel(card, "Manage notifications", 10, Navy, true, new Point(16, 12), new Size(300, 22));
        AddLabel(card, "Choose which kinds of updates you want (requests, payments, reviews, reminders) on the Notifications page. Your choices are saved to your account.", 8, Muted, false, new Point(16, 40), new Size(560, 38));
        var open = MakeLink("Open notification preferences →");
        open.Location = new Point(16, 88);
        open.Click += (_, _) => Nav.Navigate(Route.Notifications);
        card.Controls.Add(open);
    }

    private void BuildPrivacySection()
    {
        var panel = CreateSection("privacy");
        AddSectionTitle(panel, "PRIVACY & ACCOUNT", "Control your visibility", "Choose what other TaskTap members can discover about your account.");
        AddInfoBox(panel, "◉", "What other members see", "Your name, barangay, rating, reviews and completed tasks. Your phone and email are shared only with the person whose request you accept (or who accepts yours).", 112);
        AddInfoBox(panel, "◎", "Your location", "Tasks show an approximate area on the map, never an exact pin or address.", 225);
        AddInfoBox(panel, "✓", "What you should never share", "Passwords, OTPs, bank login credentials, full card numbers, or sensitive identity documents in profile fields, tasks, or your CV.", 338);
    }

    private Panel CreateSection(string key)
    {
        var panel = new Panel { Name = "settingsSection_" + key, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
        sections.Add(key, panel);
        sectionHost.Controls.Add(panel);
        return panel;
    }

    private void AddSectionTitle(Panel panel, string eyebrow, string title, string description)
    {
        AddLabel(panel, eyebrow, 7.5F, Blue, true, new Point(24, 18), new Size(400, 18));
        AddLabel(panel, title, 15, Navy, true, new Point(24, 39), new Size(560, 28));
        AddLabel(panel, description, 8, Muted, false, new Point(24, 70), new Size(600, 35));
    }

    private void AddFieldLabel(Panel parent, string labelText, Control input, int x, int y, int width, int height)
    {
        AddLabel(parent, labelText, 8, Color.FromArgb(52, 70, 94), true, new Point(x, y), new Size(width, 18));
        input.Bounds = new Rectangle(x, y + 23, width, height - 22);
        input.Font = new Font("Segoe UI", 9F);
        if (input is TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.TextChanged += (_, _) => { if (ReferenceEquals(textBox, bio)) UpdateBioCount(); MarkDirty(); };
        }
        if (input is ComboBox combo)
            combo.SelectedIndexChanged += (_, _) => MarkDirty();
        parent.Controls.Add(input);
    }

    private void AddPasswordField(Panel parent, string labelText, TextBox input, int x, int y, int width, string placeholder)
    {
        AddLabel(parent, labelText, 8, Color.FromArgb(52, 70, 94), true, new Point(x, y), new Size(width, 18));
        input.PlaceholderText = placeholder;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Font = new Font("Segoe UI", 9F);
        input.Bounds = new Rectangle(x, y + 24, width, 34);
        input.TextChanged += (_, _) => MarkDirty();
        parent.Controls.Add(input);
        PasswordReveal.Attach(input);
    }

    private void AddInfoBox(Panel parent, string icon, string title, string message, int y)
    {
        var box = MakeCard();
        box.BackColor = Color.FromArgb(239, 247, 255);
        box.Bounds = new Rectangle(24, y, 600, 116);
        parent.Controls.Add(box);
        var iconLabel = new Label { Text = icon, Location = new Point(14, 16), Size = new Size(30, 30), BackColor = Color.FromArgb(220, 238, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        box.Controls.Add(iconLabel);
        AddLabel(box, title, 9, Navy, true, new Point(56, 14), new Size(510, 23));
        AddLabel(box, message, 8, Muted, false, new Point(56, 39), new Size(520, 64));
    }

    private void AddPreferenceRow(Panel parent, string title, string subtitle, bool isChecked, int y)
    {
        var row = MakeCard();
        row.Bounds = new Rectangle(24, y, 600, 56);
        parent.Controls.Add(row);
        AddLabel(row, title, 8.5F, Color.FromArgb(48, 67, 92), true, new Point(14, 8), new Size(440, 18));
        AddLabel(row, subtitle, 7.5F, Muted, false, new Point(14, 28), new Size(480, 20));
        var toggle = new CheckBox { Checked = isChecked, Appearance = Appearance.Button, Text = isChecked ? "ON" : "OFF", TextAlign = ContentAlignment.MiddleCenter, FlatStyle = FlatStyle.Flat, BackColor = isChecked ? Blue : Color.FromArgb(214, 222, 232), ForeColor = Color.White, Font = new Font("Segoe UI", 7F, FontStyle.Bold), Size = new Size(44, 24), Location = new Point(530, 16), Cursor = Cursors.Hand };
        toggle.FlatAppearance.BorderSize = 0;
        toggle.CheckedChanged += (_, _) => { toggle.Text = toggle.Checked ? "ON" : "OFF"; toggle.BackColor = toggle.Checked ? Blue : Color.FromArgb(170, 184, 200); MarkDirty(); };
        row.Controls.Add(toggle);
    }

    private void RenderSkills()
    {
        skillsPanel.Controls.Clear();
        foreach (var skill in skills.ToArray())
        {
            var tag = new Button { Text = skill + "  ×", AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(246, 249, 252), ForeColor = Color.FromArgb(56, 80, 109), Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
            tag.FlatAppearance.BorderColor = Line;
            tag.Click += (_, _) => { skills.Remove(skill); RenderSkills(); MarkDirty(); };
            skillsPanel.Controls.Add(tag);
        }
        var add = new Button { Text = "＋ Add skill", AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
        add.FlatAppearance.BorderColor = Line;
        add.Click += (_, _) => AddSkill();
        skillsPanel.Controls.Add(add);
    }

    private void AddSkill()
    {
        using var dialog = new Form { Text = "Add a skill or service", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false, ClientSize = new Size(360, 130), BackColor = Color.White };
        var input = new TextBox { Location = new Point(20, 25), Size = new Size(320, 30), Font = new Font("Segoe UI", 9F) };
        var add = MakeButton("Add skill", "AddSkillConfirm", true, 105);
        add.Location = new Point(235, 75);
        add.DialogResult = DialogResult.OK;
        dialog.Controls.Add(input);
        dialog.Controls.Add(add);
        dialog.AcceptButton = add;
        if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(input.Text)) return;
        var skill = input.Text.Trim();
        if (skills.Contains(skill, StringComparer.OrdinalIgnoreCase)) { ShowToast("That skill is already listed."); return; }
        skills.Add(skill);
        RenderSkills();
        MarkDirty();
    }

    private void SelectSection(string key)
    {
        activeSection = key;
        foreach (var (name, button) in sectionButtons)
        {
            var active = name == key;
            button.BackColor = active ? Color.FromArgb(237, 245, 255) : Color.White;
            button.ForeColor = active ? Blue : Color.FromArgb(80, 98, 123);
        }
        ShowSection(key);
    }

    private void ShowSection(string key)
    {
        foreach (var (name, section) in sections) section.Visible = name == key;
    }

    private void MarkDirty()
    {
        if (!initialized) return;
        saveState.Text = "Unsaved changes";
        saveState.ForeColor = Color.FromArgb(161, 91, 0);
        saveState.BackColor = Color.FromArgb(255, 247, 232);
    }

    private void UpdateBioCount()
    {
        if (bio.Text.Length > 240) bio.Text = bio.Text[..240];
        bioCount.Text = $"{bio.Text.Length}/240";
    }

    private void SaveChanges()
    {
        if (string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(lastName.Text) || string.IsNullOrWhiteSpace(email.Text))
        {
            SelectSection(string.IsNullOrWhiteSpace(email.Text) ? "contact" : "profile");
            ShowToast("Please complete the required profile fields.");
            return;
        }
        try { _ = new System.Net.Mail.MailAddress(email.Text.Trim()); }
        catch (FormatException) { SelectSection("contact"); ShowToast("Please enter a valid email address."); return; }
        if (!string.IsNullOrEmpty(newPassword.Text) && newPassword.Text != confirmPassword.Text)
        {
            SelectSection("security");
            ShowToast("New passwords do not match.");
            return;
        }
        if (!string.IsNullOrEmpty(newPassword.Text) && newPassword.Text.Length < 8)
        {
            SelectSection("security");
            ShowToast("New password must be at least 8 characters.");
            return;
        }
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
