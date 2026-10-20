using TaskTap.Data;

namespace TaskTap;

public sealed class ProfilePage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel page = new();
    private readonly Panel hero = new();
    private readonly Panel leftColumn = new();
    private readonly Panel rightColumn = new();
    private readonly Panel statsPanel = new();
    private readonly Panel aboutCard = new();
    private readonly Panel reviewsCard = new();
    private readonly Panel completenessCard = new();
    private readonly Panel activityCard = new();
    private readonly Panel accountCard = new();
    private readonly Panel safetyCard = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private bool pageBuilt;

    public override Route RouteName => Route.Profile;

    public ProfilePage()
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
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Dashboard   /   My Profile", 8, Muted, true);
        crumb.Name = "profileCrumb";
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);

        hero.Name = "profileHero";
        hero.BackColor = Color.White;
        hero.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(hero);
        var bigAvatar = new Label { Text = AppSession.CurrentUser.Initials, Name = "profileBigAvatar", Size = new Size(92, 92), BackColor = Blue, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 27F, FontStyle.Bold) };
        hero.Controls.Add(bigAvatar);
        AddLabel(hero, "TaskTap MEMBER", 8, Blue, true, new Point(0, 0), new Size(250, 20));
        AddLabel(hero, AppSession.CurrentUser.FullName, 21, Navy, true, new Point(0, 0), new Size(360, 35));
        AddLabel(hero, "TaskTap community member", 9, Muted, false, new Point(0, 0), new Size(410, 24));
        AddLabel(hero, $"● Member since {AppSession.CurrentUser.MemberSince}     ⌖ {AppSession.CurrentUser.Barangay}     {(ProfileLinks.Find(AppSession.CurrentUser.FullName)?.Verified == true ? "✓ Verified account" : "Not verified yet")}", 8, Color.FromArgb(8, 122, 89), true, new Point(0, 0), new Size(480, 24));
        var edit = MakeButton("Edit Profile", "ProfileEditButton", true, 125);
        edit.Click += (_, _) => OpenAccountSettings();
        hero.Controls.Add(edit);
        var publicProfile = MakeButton("View public profile", "ProfilePublicButton", false, 160);
        publicProfile.Click += (_, _) => OpenPublicProfile();
        hero.Controls.Add(publicProfile);

        leftColumn.Name = "profileLeftColumn";
        rightColumn.Name = "profileRightColumn";
        page.Controls.Add(leftColumn);
        page.Controls.Add(rightColumn);
        BuildStats();
        BuildAbout();
        BuildReviews();
        BuildCompleteness();
        BuildActivity();
        BuildAccount();
        BuildSafety();
    }

    private void BuildStats()
    {
        statsPanel.Name = "profileStats";
        statsPanel.BackColor = PageColor;
        leftColumn.Controls.Add(statsPanel);
        var completed = DemoDataStore.TaskOutcomes.Count(outcome => outcome.Status == "Completed") + DemoDataStore.RequestsSent.Count(request => request.Status == "accepted" && request.TaskStatus == "Completed");
        var me = ProfileLinks.Find(AppSession.CurrentUser.FullName);
        var stats = new[] { ("★", me is { Reviews: > 0 } ? $"{me.Rating:0.0}" : "–", me is { Reviews: > 0 } ? $"Rating · {me.Reviews} review{(me.Reviews == 1 ? "" : "s")}" : "No reviews yet", Color.FromArgb(8, 117, 232)), ("✓", completed.ToString(), "Completed tasks", Color.FromArgb(8, 122, 89)), ("▤", DemoDataStore.PostedTasks.Count.ToString(), "Active posts", Color.FromArgb(8, 117, 232)), ("◷", DemoDataStore.RequestsSent.Count.ToString(), "Requests sent", Color.FromArgb(8, 117, 232)) };
        foreach (var (icon, value, caption, color) in stats)
        {
            var card = MakeCard();
            statsPanel.Controls.Add(card);
            AddLabel(card, icon, 16, color, true, new Point(16, 13), new Size(30, 25));
            AddLabel(card, value, 21, Navy, true, new Point(16, 40), new Size(110, 30));
            AddLabel(card, caption, 8, Muted, false, new Point(16, 80), new Size(145, 20));
        }
    }

    private void BuildAbout()
    {
        aboutCard.Name = "profileAboutCard";
        ConfigureCard(aboutCard);
        leftColumn.Controls.Add(aboutCard);
        AddSectionHeading(aboutCard, "ABOUT", $"About {AppSession.CurrentUser.FirstName}", "Edit", OpenAccountSettings);
        AddLabel(aboutCard, string.IsNullOrWhiteSpace(Backend.GetPhoneAndBio(AppSession.CurrentUser.Id).Bio) ? "You have not added a bio yet. Add one in Account Settings." : Backend.GetPhoneAndBio(AppSession.CurrentUser.Id).Bio, 9, Muted, false, new Point(20, 70), new Size(650, 78));
        var skills = ProfileLinks.Find(AppSession.CurrentUser.FullName)?.Skills ?? [];
        var x = 20;
        var y = 157;
        foreach (var skill in skills)
        {
            var tag = new Label { Text = skill, AutoSize = true, BackColor = Color.FromArgb(237, 246, 255), ForeColor = Color.FromArgb(7, 95, 190), Font = new Font("Segoe UI", 8F, FontStyle.Bold), Padding = new Padding(10, 6, 10, 6) };
            tag.Location = new Point(x, y);
            aboutCard.Controls.Add(tag);
            x += tag.PreferredWidth + 8;
            if (x > 540) { x = 20; y += 34; tag.Location = new Point(x, y); x += tag.PreferredWidth + 8; }
        }
    }

    private void BuildReviews()
    {
        var profile = ProfileLinks.Find(AppSession.CurrentUser.FullName);
        reviewsCard.Name = "profileReviewsCard";
        ConfigureCard(reviewsCard);
        leftColumn.Controls.Add(reviewsCard);
        AddSectionHeading(reviewsCard, "REPUTATION", "Recent reviews", "View all", OpenReviews);
        AddLabel(reviewsCard, profile is { Reviews: > 0 } ? $"{profile.Reviews} review{(profile.Reviews == 1 ? "" : "s")} received" : "No reviews yet", 11, Muted, true, new Point(22, 66), new Size(300, 26));
        AddLabel(reviewsCard, profile is { Reviews: > 0 } ? $"Average rating {profile.Rating:0.0} / 5.0" : "Reviews from people you work with will appear here.", 8, Muted, false, new Point(22, 96), new Size(420, 20));
    }

    private void BuildCompleteness()
    {
        var completedFields = new[] { AppSession.CurrentUser.FirstName, AppSession.CurrentUser.LastName, AppSession.CurrentUser.Email, AppSession.CurrentUser.Barangay, Backend.GetPhoneAndBio(AppSession.CurrentUser.Id).Bio }
            .Count(value => !string.IsNullOrWhiteSpace(value));
        var completeness = completedFields * 20;
        completenessCard.Name = "profileCompletenessCard";
        ConfigureCard(completenessCard);
        rightColumn.Controls.Add(completenessCard);
        AddLabel(completenessCard, "Profile completeness", 11, Navy, true, new Point(18, 17), new Size(195, 24));
        AddLabel(completenessCard, $"{completeness}%", 11, Blue, true, new Point(240, 17), new Size(50, 24));
        var track = new Panel { BackColor = Color.FromArgb(232, 238, 245), Location = new Point(18, 51), Size = new Size(267, 8) };
        var fill = new Panel { BackColor = Blue, Size = new Size(267 * completeness / 100, 8), Dock = DockStyle.Left };
        track.Controls.Add(fill);
        completenessCard.Controls.Add(track);
        AddLabel(completenessCard, "Revisit your setup to update your basics, skills, and task preferences.", 8, Muted, false, new Point(18, 72), new Size(267, 44));
        var setup = MakeLink("Review setup →");
        setup.Location = new Point(15, 116);
        setup.Click += (_, _) => Nav.Navigate(new AccountSettingsPage());
        completenessCard.Controls.Add(setup);
    }

    private void BuildActivity()
    {
        var profile = ProfileLinks.Find(AppSession.CurrentUser.FullName);
        var completed = DemoDataStore.TaskOutcomes.Count(outcome => outcome.Status == "Completed") + DemoDataStore.RequestsSent.Count(request => request.Status == "accepted" && request.TaskStatus == "Completed");
        activityCard.Name = "profileActivityCard";
        ConfigureCard(activityCard);
        rightColumn.Controls.Add(activityCard);
        AddLabel(activityCard, "Task activity", 12, Navy, true, new Point(18, 16), new Size(170, 25));
        var viewAll = MakeLink("View all");
        viewAll.Location = new Point(228, 17);
        viewAll.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        activityCard.Controls.Add(viewAll);
        AddActivityRow(activityCard, "✓", $"{completed} tasks completed", "Completed work recorded on TaskTap", Color.FromArgb(8, 122, 89), 52);
        AddActivityRow(activityCard, "＋", $"{DemoDataStore.PostedTasks.Count} tasks posted", "Active task posts", Blue, 113);
        AddActivityRow(activityCard, "★", $"{profile?.Reviews ?? 0} reviews received", $"Average rating {(profile?.Rating ?? 0):0.0} / 5.0", Color.FromArgb(230, 120, 32), 174);
    }

    private void BuildAccount()
    {
        accountCard.Name = "profileAccountCard";
        ConfigureCard(accountCard);
        rightColumn.Controls.Add(accountCard);
        AddLabel(accountCard, "Account", 12, Navy, true, new Point(18, 16), new Size(240, 25));
        AddAccountRow(accountCard, "⚙", "Account settings", "Password, contact and preferences", 48, OpenAccountSettings);
        AddAccountRow(accountCard, "₱", "Payment history", "View your recorded task payments", 104, OpenPaymentHistory);
        AddAccountRow(accountCard, "↗", "Earnings", "Payments recorded to you as a doer", 160, OpenEarnings);
        AddAccountRow(accountCard, "★", "Reviews", "Reviews received and given", 216, OpenReviews);
        AddAccountRow(accountCard, "▤", "Archived tasks", "Completed and closed task history", 272, OpenArchivedTasks);
        AddAccountRow(accountCard, "⚑", "My disputes", "Issues you opened and their status", 328, OpenDisputes);
        AddAccountRow(accountCard, "♡", "Saved tasks", "Tasks you saved for later", 384, OpenSaved);
        AddAccountRow(accountCard, "☎", "Support", "Contact the TaskTap team", 440, OpenSupport);
        AddAccountRow(accountCard, "?", "Help & policies", "Help, guidelines and legal", 496, OpenHelp);
    }

    private void BuildSafety()
    {
        safetyCard.Name = "profileSafetyCard";
        safetyCard.BackColor = Color.FromArgb(242, 248, 255);
        safetyCard.BorderStyle = BorderStyle.FixedSingle;
        rightColumn.Controls.Add(safetyCard);
        var icon = new Label { Text = "✓", Location = new Point(18, 16), Size = new Size(34, 34), BackColor = Color.FromArgb(223, 245, 235), ForeColor = Color.FromArgb(8, 122, 89), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F, FontStyle.Bold) };
        safetyCard.Controls.Add(icon);
        AddLabel(safetyCard, "Keep your profile safe", 11, Navy, true, new Point(18, 58), new Size(260, 24));
        AddLabel(safetyCard, "Only share information needed to complete a task. Never publish passwords, OTPs, or financial credentials.", 8, Muted, false, new Point(18, 87), new Size(268, 63));
    }

    private void AddSectionHeading(Panel card, string eyebrow, string title, string actionText, Action action)
    {
        AddLabel(card, eyebrow, 7.5F, Blue, true, new Point(20, 15), new Size(200, 18));
        AddLabel(card, title, 14, Navy, true, new Point(20, 32), new Size(280, 27));
        var link = MakeLink(actionText);
        link.Location = new Point(0, 23);
        link.Click += (_, _) => action();
        card.Controls.Add(link);
        card.Resize += (_, _) => link.Location = new Point(card.ClientSize.Width - link.Width - 18, 22);
    }

    private void AddRatingBar(Panel parent, string rating, int percentage, string count, int y)
    {
        AddLabel(parent, rating, 8, Muted, false, new Point(178, y), new Size(15, 18));
        var track = new Panel { Location = new Point(198, y + 4), Size = new Size(190, 7), BackColor = Color.FromArgb(232, 238, 245) };
        var bar = new Panel { Dock = DockStyle.Left, Width = (int)(190 * percentage / 100d), BackColor = Blue };
        track.Controls.Add(bar);
        parent.Controls.Add(track);
        AddLabel(parent, count, 7.5F, Muted, false, new Point(395, y), new Size(28, 18));
    }

    private void AddReview(Panel parent, string initials, string name, string date, string text, string task, int y)
    {
        var divider = new Panel { Location = new Point(20, y - 8), Size = new Size(450, 1), BackColor = Line };
        parent.Controls.Add(divider);
        var avatar = new Label { Text = initials, Location = new Point(20, y + 2), Size = new Size(38, 38), BackColor = Color.FromArgb(233, 243, 255), ForeColor = Color.FromArgb(7, 95, 190), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
        parent.Controls.Add(avatar);
        AddLabel(parent, name, 8.5F, Navy, true, new Point(70, y), new Size(140, 20));
        AddLabel(parent, "★★★★★", 8, Color.FromArgb(255, 155, 69), true, new Point(211, y), new Size(78, 20));
        AddLabel(parent, date, 7, Muted, false, new Point(300, y), new Size(120, 20));
        AddLabel(parent, text, 8, Muted, false, new Point(70, y + 23), new Size(415, 40));
        AddLabel(parent, task, 7.5F, Blue, true, new Point(70, y + 65), new Size(250, 18));
    }

    private void AddActivityRow(Panel parent, string iconText, string title, string details, Color color, int y)
    {
        var icon = new Label { Text = iconText, Location = new Point(18, y), Size = new Size(34, 34), BackColor = Color.FromArgb(239, 246, 252), ForeColor = color, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
        parent.Controls.Add(icon);
        AddLabel(parent, title, 8, Navy, true, new Point(64, y + 1), new Size(215, 19));
        AddLabel(parent, details, 7, Muted, false, new Point(64, y + 19), new Size(215, 29));
    }

    private void AddAccountRow(Panel parent, string iconText, string title, string subtitle, int y, Action action)
    {
        if (y > 48) parent.Controls.Add(new Panel { Location = new Point(18, y - 1), Size = new Size(267, 1), BackColor = Line });
        var button = new Button { Text = iconText, Location = new Point(18, y + 8), Size = new Size(34, 34), BackColor = Color.FromArgb(241, 246, 251), ForeColor = Blue, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        parent.Controls.Add(button);
        var titleLink = MakeLink(title);
        titleLink.Location = new Point(62, y + 6);
        titleLink.ForeColor = Navy;
        titleLink.Click += (_, _) => action();
        parent.Controls.Add(titleLink);
        AddLabel(parent, subtitle, 7, Muted, false, new Point(62, y + 26), new Size(220, 20));
        var arrow = MakeLabel("›", 16, Muted, false);
        arrow.Location = new Point(278, y + 10);
        parent.Controls.Add(arrow);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var left = 22;
        var contentWidth = width - 44;
        var crumb = page.Controls.Find("profileCrumb", false).FirstOrDefault();
        if (crumb is not null) crumb.Location = new Point(left, 12);
        hero.Bounds = new Rectangle(left, 42, contentWidth, 160);
        var leftWidth = Math.Max(450, (contentWidth - 20) * 64 / 100);
        var rightWidth = contentWidth - leftWidth - 20;
        leftColumn.Bounds = new Rectangle(left, 220, leftWidth, 900);
        rightColumn.Bounds = new Rectangle(left + leftWidth + 20, 220, rightWidth, 1200);
        statsPanel.Bounds = new Rectangle(0, 0, leftWidth, 114);
        var statWidth = (leftWidth - 42) / 4;
        for (var i = 0; i < statsPanel.Controls.Count; i++)
            statsPanel.Controls[i].Bounds = new Rectangle(i * (statWidth + 10), 0, statWidth, 108);
        aboutCard.Bounds = new Rectangle(0, 130, leftWidth, 224);
        reviewsCard.Bounds = new Rectangle(0, 370, leftWidth, 470);
        completenessCard.Bounds = new Rectangle(0, 0, rightWidth, 155);
        activityCard.Bounds = new Rectangle(0, 171, rightWidth, 238);
        accountCard.Bounds = new Rectangle(0, 425, rightWidth, 568);
        safetyCard.Bounds = new Rectangle(0, 1009, rightWidth, 172);
        var avatar = hero.Controls.Find("profileBigAvatar", false).FirstOrDefault();
        if (avatar is not null) avatar.Location = new Point(24, 32);
        var heroLabels = hero.Controls.OfType<Label>().Where(label => label.Name.Length == 0).ToArray();
        if (heroLabels.Length >= 4)
        {
            heroLabels[0].Location = new Point(138, 22);
            heroLabels[1].Location = new Point(138, 44);
            heroLabels[2].Location = new Point(138, 81);
            heroLabels[3].Location = new Point(138, 113);
        }
        var edit = hero.Controls.Find("ProfileEditButton", false).FirstOrDefault();
        var publicProfile = hero.Controls.Find("ProfilePublicButton", false).FirstOrDefault();
        if (edit is not null) edit.Location = new Point(contentWidth - 310, 54);
        if (publicProfile is not null) publicProfile.Location = new Point(contentWidth - 175, 54);
        page.AutoScrollMinSize = new Size(0, 220 + safetyCard.Bottom + 28);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void OpenAccountSettings() => Nav.Navigate(new AccountSettingsPage());

    private void OpenPublicProfile() => Nav.Navigate(new PublicProfilePage());

    private void OpenPaymentHistory() => Nav.Navigate(new PaymentHistoryPage());

    private void OpenArchivedTasks() => Nav.Navigate(new ArchivedTasksPage());

    private void OpenHelp() => Nav.Navigate(Route.HelpFaq);

    private void OpenSaved() => Nav.Navigate(Route.SavedTasks);

    private void OpenDisputes() => Nav.Navigate(Route.MyDisputes);

    private void OpenEarnings() => Nav.Navigate(Route.Earnings);

    private void OpenSupport() => Nav.Navigate(Route.Support);

    private void OpenReviews() => Nav.Navigate(new ReviewsPage());

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private Panel MakeCard() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

    private void ConfigureCard(Panel card)
    {
        card.BackColor = Color.White;
        card.BorderStyle = BorderStyle.FixedSingle;
    }

    private Label MakeLabel(string text, float size, Color color, bool bold) => new() { Text = text, AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent };

    private void AddLabel(Control parent, string text, float size, Color color, bool bold, Point location, Size dimensions)
    {
        parent.Controls.Add(new Label { Text = text, Location = location, Size = dimensions, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent, AutoEllipsis = true });
    }

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
