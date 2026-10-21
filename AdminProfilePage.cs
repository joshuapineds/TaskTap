using TaskTap.Data;

namespace TaskTap;

/// <summary>Admin Profile: the signed-in administrator's own details and recent activity.</summary>
public sealed class AdminProfilePage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel profileCard = new();
    private readonly Panel activityCard = new();
    private readonly Label nameLabel = new();
    private readonly Label emailLabel = new();
    private readonly TextBox nameBox = new();
    private readonly TextBox emailBox = new();
    private readonly Label feedback = new();
    private int _lastWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.AdminProfile;

    public AdminProfilePage()
    {
        BuildContent();
        LoadProfile();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("MY ACCOUNT", 8, AdminUi.Blue, true);
        eyebrow.Name = "adminProfileEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Admin Profile", 26, AdminUi.Navy, true);
        title.Name = "adminProfileTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Your administrator account and recent activity.", 9, AdminUi.Muted);
        subtitle.Name = "adminProfileSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        profileCard.BackColor = Color.White;
        profileCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(profileCard);

        var avatar = new Label
        {
            Text = AppSession.CurrentAdmin?.Initials ?? "TA",
            Size = new Size(64, 64),
            Location = new Point(18, 18),
            BackColor = Color.FromArgb(255, 243, 232),
            ForeColor = AdminUi.Warn,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };
        profileCard.Controls.Add(avatar);

        nameLabel.Text = "Name";
        nameLabel.AutoSize = true;
        nameLabel.ForeColor = AdminUi.Muted;
        nameLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        nameLabel.Location = new Point(100, 16);
        profileCard.Controls.Add(nameLabel);

        nameBox.Name = "adminProfileNameBox";
        nameBox.BorderStyle = BorderStyle.FixedSingle;
        nameBox.Font = new Font("Segoe UI", 10F);
        nameBox.Location = new Point(100, 36);
        nameBox.Size = new Size(340, 30);
        profileCard.Controls.Add(nameBox);

        emailLabel.Text = "Email";
        emailLabel.AutoSize = true;
        emailLabel.ForeColor = AdminUi.Muted;
        emailLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        emailLabel.Location = new Point(100, 78);
        profileCard.Controls.Add(emailLabel);

        emailBox.Name = "adminProfileEmailBox";
        emailBox.BorderStyle = BorderStyle.FixedSingle;
        emailBox.Font = new Font("Segoe UI", 10F);
        emailBox.Location = new Point(100, 98);
        emailBox.Size = new Size(340, 30);
        profileCard.Controls.Add(emailBox);

        var save = AdminUi.MakePrimaryButton("Save changes", "adminProfileSaveButton", 160);
        save.Location = new Point(100, 144);
        save.Click += (_, _) => SaveProfile();
        profileCard.Controls.Add(save);

        var password = AdminUi.MakeSecondaryButton("Change password", "adminProfilePasswordButton", 170);
        password.Location = new Point(270, 144);
        password.Click += (_, _) =>
        {
            if (AppSession.CurrentAdmin is { } admin && PasswordDialog.ShowFor(this, admin.Id))
                Toast("Password changed.");
        };
        profileCard.Controls.Add(password);

        feedback.AutoSize = true;
        feedback.ForeColor = AdminUi.Good;
        feedback.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        feedback.Location = new Point(100, 194);
        profileCard.Controls.Add(feedback);

        activityCard.BackColor = Color.White;
        activityCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(activityCard);

        var activityTitle = AdminUi.MakeLabel("Recent admin activity", 11, AdminUi.Navy, true);
        activityTitle.Location = new Point(18, 14);
        activityCard.Controls.Add(activityTitle);

        var y = 48;
        foreach (var item in DemoDataStore.ActivityLog.OrderByDescending(activity => activity.Timestamp).Take(6))
        {
            var action = AdminUi.MakeLabel(item.Action, 9.5F, AdminUi.Navy, true);
            action.Location = new Point(18, y);
            activityCard.Controls.Add(action);

            var detail = AdminUi.MakeLabel($"{item.Target} · {item.Timestamp:MMM d, h:mm tt}", 8, AdminUi.Muted);
            detail.Location = new Point(18, y + 18);
            activityCard.Controls.Add(detail);

            y += 44;
        }
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        if (contentWidth == _lastWidth)
            return;
        _lastWidth = contentWidth;

        const int left = 32;
        var eyebrow = AdminUi.Find<Label>(page, "adminProfileEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "adminProfileTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "adminProfileSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        profileCard.Bounds = new Rectangle(left, 132, contentWidth, 246);
        activityCard.Bounds = new Rectangle(left, 394, contentWidth, 330);
        page.AutoScrollMinSize = new Size(0, 760);
    }

    private void LoadProfile()
    {
        nameBox.Text = AppSession.CurrentAdmin?.FullName ?? "";
        emailBox.Text = AppSession.CurrentAdmin?.Email ?? "";
        feedback.Text = string.Empty;
    }

    private void SaveProfile()
    {
        var name = nameBox.Text.Trim();
        var email = emailBox.Text.Trim();
        if (name.Length == 0 || email.Length == 0)
        {
            feedback.ForeColor = AdminUi.Bad;
            feedback.Text = "Enter a name and email.";
            return;
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var first = parts.Length > 0 ? parts[0] : name;
        var last = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "";
        var initials = ((first.Length > 0 ? first[0].ToString() : "") + (last.Length > 0 ? last[0].ToString() : "")).ToUpperInvariant();

        var current = AppSession.CurrentAdmin;
        if (current is null)
            return;
        var saved = Backend.UpdateProfile(current.Id, first, last, email, null, current.Barangay);
        if (!saved.Success)
        {
            feedback.ForeColor = AdminUi.Bad;
            feedback.Text = saved.Error;
            return;
        }

        AppSession.UpdateCurrentAdmin(current with { FullName = name, FirstName = first, LastName = last, Initials = initials, Email = email.ToLowerInvariant() });

        DemoDataStore.LogActivity(name, "Updated admin profile", name, "Admin", "Admin profile details saved.");
        feedback.ForeColor = AdminUi.Good;
        feedback.Text = "Profile saved.";
        Toast("Admin profile saved.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => LoadProfile();
}
