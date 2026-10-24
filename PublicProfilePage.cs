namespace TaskTap;

public sealed class PublicProfilePage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel page = new();
    private readonly Panel hero = new();
    private readonly Panel trustStrip = new();
    private readonly Panel leftColumn = new();
    private readonly Panel rightColumn = new();
    private readonly Panel aboutCard = new();
    private readonly Panel skillsCard = new();
    private readonly Panel reviewsCard = new();
    private readonly Panel historyCard = new();
    private readonly Panel actionCard = new();
    private readonly Panel trustCard = new();
    private readonly Panel preferredCard = new();
    private readonly Panel safetyCard = new();
    private readonly Panel modalBackdrop = new();
    private readonly Panel modal = new();
    private readonly Panel modalBody = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2500 };
    private bool pageBuilt;
    private readonly ProfileSubject subject;

    private sealed record ProfileSubject(
        string FullName,
        string FirstName,
        string Initials,
        string Barangay,
        string MemberSince,
        string Subtitle,
        double Rating,
        int ReviewCount,
        int CompletedTasks,
        bool Verified,
        Color AvatarColor);

    public override Route RouteName => Route.PublicProfile;

    public PublicProfilePage() : this(null)
    {
    }

    public PublicProfilePage(DemoUser? user)
    {
        subject = user is null
            ? new ProfileSubject(
                AppSession.CurrentUser.FullName,
                AppSession.CurrentUser.FirstName,
                AppSession.CurrentUser.Initials,
                AppSession.CurrentUser.Barangay,
                AppSession.CurrentUser.MemberSince,
                "Reliable home helper, errand runner, and community task doer",
                0,
                0,
                0,
                ProfileLinks.Find(AppSession.CurrentUser.FullName)?.Verified ?? false,
                Blue)
            : new ProfileSubject(
                user.FullName,
                user.FirstName,
                user.Initials,
                user.Barangay,
                string.IsNullOrWhiteSpace(user.Joined) ? "2025" : user.Joined,
                string.IsNullOrWhiteSpace(user.Role) ? "TaskTap member" : user.Role,
                user.Rating,
                user.Reviews,
                user.CompletedTasks,
                user.Verified,
                user.AvatarColor == Color.Empty ? Blue : user.AvatarColor);
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
        BuildModal();
        toast.Size = new Size(440, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => { toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18); LayoutModal(); };
        pageBuilt = true;
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Back   /   Public Profile", 8, Muted, true);
        crumb.Name = "publicProfileCrumb";
        crumb.Location = new Point(22, 12);
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);

        hero.Name = "publicProfileHero";
        ConfigureCard(hero);
        page.Controls.Add(hero);
        var avatar = new Label { Name = "publicProfileAvatar", Text = subject.Initials, Size = new Size(92, 92), BackColor = subject.AvatarColor, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 26F, FontStyle.Bold) };
        hero.Controls.Add(avatar);
        AddLabel(hero, "TASKTAP MEMBER", 8, Blue, true, new Point(0, 0), new Size(240, 18));
        AddLabel(hero, subject.FullName, 22, Navy, true, new Point(0, 0), new Size(400, 34));
        var verified = MakeLabel(subject.Verified ? "✓ Verified" : "Unverified", 8, subject.Verified ? Color.FromArgb(8, 122, 89) : Muted, true);
        verified.Name = "publicProfileVerified";
        hero.Controls.Add(verified);
        AddLabel(hero, subject.Subtitle, 9, Muted, false, new Point(0, 0), new Size(500, 22));
        AddLabel(hero, $"● Member since {subject.MemberSince}     ⌖ {subject.Barangay}     Last active {subject.LastActive}", 8, Muted, false, new Point(0, 0), new Size(480, 22));
        var share = MakeButton("↗ Share profile", "PublicProfileShareButton", false, 138);
        share.Click += (_, _) => OpenShareModal();
        hero.Controls.Add(share);

        trustStrip.Name = "publicProfileTrustStrip";
        ConfigureCard(trustStrip);
        page.Controls.Add(trustStrip);
        var metrics = new[]
        {
            ($"{subject.Rating:0.0}", "★★★★★", $"{subject.ReviewCount} reviews"),
            (subject.CompletedTasks.ToString(), "", "tasks completed"),
            (subject.CompletedTasks == 0 ? "—" : "Recorded", "", "completion rate"),
            ("—", "", "typical response"),
            ("—", "", "repeat clients")
        };
        for (var i = 0; i < metrics.Length; i++)
        {
            var metricPanel = new Panel { BackColor = Color.Transparent };
            trustStrip.Controls.Add(metricPanel);
            AddLabel(metricPanel, metrics[i].Item1, 17, Navy, true, new Point(0, 0), new Size(160, 28));
            if (!string.IsNullOrEmpty(metrics[i].Item2)) AddLabel(metricPanel, metrics[i].Item2, 9, Color.FromArgb(255, 155, 69), true, new Point(0, 29), new Size(160, 20));
            AddLabel(metricPanel, metrics[i].Item3, 8, Muted, false, new Point(0, 51), new Size(160, 20));
        }

        leftColumn.Name = "publicProfileLeftColumn";
        rightColumn.Name = "publicProfileRightColumn";
        page.Controls.Add(leftColumn);
        page.Controls.Add(rightColumn);
        BuildAbout();
        BuildSkills();
        BuildReviews();
        BuildHistory();
        BuildActionCard();
        BuildTrustCard();
        BuildPreferredCard();
        BuildSafetyCard();
    }

    private void BuildAbout()
    {
        ConfigureCard(aboutCard);
        leftColumn.Controls.Add(aboutCard);
        AddSectionHeading(aboutCard, "ABOUT", $"About {subject.FirstName}");
        AddLabel(aboutCard, "This member has not added a bio yet.", 9, Muted, false, new Point(20, 62), new Size(600, 92));
    }

    private void BuildSkills()
    {
        ConfigureCard(skillsCard);
        leftColumn.Controls.Add(skillsCard);
        AddSectionHeading(skillsCard, "SKILLS & SERVICES", $"What {subject.FirstName} can help with");
        var entries = Array.Empty<(string, string)>();
        for (var i = 0; i < entries.Length; i++)
        {
            var x = 20 + (i % 2) * 300;
            var y = 68 + (i / 2) * 64;
            var check = new Label { Text = "✓", Location = new Point(x, y + 3), Size = new Size(30, 30), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(8, 122, 89), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            skillsCard.Controls.Add(check);
            AddLabel(skillsCard, entries[i].Item1, 8.5F, Navy, true, new Point(x + 40, y), new Size(235, 20));
            AddLabel(skillsCard, entries[i].Item2, 7.5F, Muted, false, new Point(x + 40, y + 21), new Size(235, 28));
        }
    }

    private void BuildReviews()
    {
        ConfigureCard(reviewsCard);
        leftColumn.Controls.Add(reviewsCard);
        AddSectionHeading(reviewsCard, "REPUTATION", "Reviews from the community");
        var viewAll = MakeLink("View all");
        viewAll.Location = new Point(0, 25);
        viewAll.Click += (_, _) => ShowToast("No reviews yet.");
        reviewsCard.Controls.Add(viewAll);
        AddLabel(reviewsCard, $"{subject.Rating:0.0}", 26, Navy, true, new Point(22, 64), new Size(110, 36));
        AddLabel(reviewsCard, "★★★★★", 13, Color.FromArgb(255, 155, 69), true, new Point(22, 102), new Size(125, 22));
        AddLabel(reviewsCard, $"{subject.ReviewCount} reviews", 8, Muted, false, new Point(22, 127), new Size(130, 20));
        AddRatingBar(reviewsCard, "5", 0, "0", 69);
        AddRatingBar(reviewsCard, "4", 0, "0", 94);
        AddRatingBar(reviewsCard, "3", 0, "0", 119);
    }

    private void BuildHistory()
    {
        ConfigureCard(historyCard);
        leftColumn.Controls.Add(historyCard);
        AddSectionHeading(historyCard, "RECENT ACTIVITY", "Completed task history");
        var find = MakeLink("Find tasks");
        find.Location = new Point(0, 25);
        find.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        historyCard.Controls.Add(find);
        AddHistoryRow(historyCard, "Grocery Shopping", "Brgy. 179 · Completed Oct 3, 2026", "₱450", 65);
        AddHistoryRow(historyCard, "Apartment Cleaning", "Brgy. 178 · Completed Sep 18, 2026", "₱550", 119);
        AddHistoryRow(historyCard, "Pet Care", "Brgy. 176 · Completed Aug 29, 2026", "₱400", 173);
    }

    private void BuildActionCard()
    {
        ConfigureCard(actionCard);
        rightColumn.Controls.Add(actionCard);
        var icon = new Label { Text = "✓", Location = new Point(18, 17), Size = new Size(38, 38), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(8, 122, 89), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14F, FontStyle.Bold) };
        actionCard.Controls.Add(icon);
        AddLabel(actionCard, $"Considering {subject.FirstName} for a task?", 11, Navy, true, new Point(18, 64), new Size(260, 28));
        AddLabel(actionCard, "Post a task and they can apply with their CV. Contact details are shared only after you accept a request.", 8, Muted, false, new Point(18, 96), new Size(260, 52));
        var postTask = MakeButton("Post a task", "PublicProfilePostTaskButton", true, 260);
        postTask.Location = new Point(18, 155);
        postTask.Click += (_, _) => Nav.Navigate(Route.PostTask);
        actionCard.Controls.Add(postTask);
        var find = MakeButton("Find tasks", "PublicProfileFindTasksButton", false, 260);
        find.Location = new Point(18, 199);
        find.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        actionCard.Controls.Add(find);
    }

    private void BuildTrustCard()
    {
        ConfigureCard(trustCard);
        rightColumn.Controls.Add(trustCard);
        AddLabel(trustCard, "Trust signals", 12, Navy, true, new Point(18, 16), new Size(260, 24));
        AddTrustRow(trustCard, "✓", subject.Verified ? "Identity verified" : "Not verified", "Account verification status", 52);
        AddTrustRow(trustCard, "★", $"{subject.Rating:0.0} / 5.0 rating", $"Based on {subject.ReviewCount} community reviews", 102);
        AddTrustRow(trustCard, "↻", $"{subject.CompletedTasks} completed tasks", "Strong recent activity", 152);
        AddTrustRow(trustCard, "⚡", "Fast responses", "Typically replies within an hour", 202);
    }

    private void BuildPreferredCard()
    {
        ConfigureCard(preferredCard);
        rightColumn.Controls.Add(preferredCard);
        AddLabel(preferredCard, "Preferred work", 12, Navy, true, new Point(18, 16), new Size(260, 24));
        var tags = new[] { "Cleaning", "Errands", "Pet care", "Shopping", "Home help" };
        var x = 18;
        var y = 56;
        foreach (var text in tags)
        {
            var tag = new Label { Text = text, AutoSize = true, BackColor = Color.FromArgb(237, 246, 255), ForeColor = Color.FromArgb(7, 95, 190), Font = new Font("Segoe UI", 8F, FontStyle.Bold), Padding = new Padding(9, 5, 9, 5) };
            if (x + tag.PreferredWidth > 292) { x = 18; y += 34; }
            tag.Location = new Point(x, y);
            preferredCard.Controls.Add(tag);
            x += tag.PreferredWidth + 7;
        }
    }

    private void BuildSafetyCard()
    {
        safetyCard.BackColor = Color.FromArgb(242, 248, 255);
        safetyCard.BorderStyle = BorderStyle.FixedSingle;
        rightColumn.Controls.Add(safetyCard);
        var icon = new Label { Text = "!", Location = new Point(18, 16), Size = new Size(34, 34), BackColor = Color.FromArgb(220, 238, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F, FontStyle.Bold) };
        safetyCard.Controls.Add(icon);
        AddLabel(safetyCard, "Stay safe on TaskTap", 11, Navy, true, new Point(18, 58), new Size(270, 24));
        AddLabel(safetyCard, "Keep conversations and payments within TaskTap. Never share passwords, OTPs, card numbers, or unnecessary personal information.", 8, Muted, false, new Point(18, 86), new Size(270, 70));
        var report = MakeLink("Report profile");
        report.ForeColor = Color.FromArgb(189, 64, 64);
        report.Location = new Point(18, 158);
        report.Click += (_, _) => OpenReportModal();
        safetyCard.Controls.Add(report);
    }

    private void BuildModal()
    {
        modalBackdrop.Name = "publicProfileModalBackdrop";
        modalBackdrop.BackColor = Color.FromArgb(140, 8, 25, 46);
        modalBackdrop.Visible = false;
        modalBackdrop.Click += (_, _) => CloseModal();
        Controls.Add(modalBackdrop);
        modal.Name = "publicProfileModal";
        modal.BackColor = Color.White;
        modal.BorderStyle = BorderStyle.FixedSingle;
        modalBackdrop.Controls.Add(modal);
        var close = MakeButton("×", "PublicProfileModalClose", false, 36);
        close.Location = new Point(0, 10);
        close.Click += (_, _) => CloseModal();
        modal.Controls.Add(close);
        modalBody.Name = "publicProfileModalBody";
        modal.Controls.Add(modalBody);
    }

    private void OpenShareModal()
    {
        modalBody.Controls.Clear();
        AddLabel(modalBody, $"Share {subject.FirstName}'s profile", 15, Navy, true, new Point(22, 18), new Size(400, 30));
        AddLabel(modalBody, "Share this public profile with someone looking for a trusted local task doer.", 8, Muted, false, new Point(22, 56), new Size(400, 42));
        var shareBox = new TextBox { Text = $"TaskTap public profile · {subject.FullName}", ReadOnly = true, Bounds = new Rectangle(22, 110, 400, 36), Font = new Font("Segoe UI", 9F) };
        modalBody.Controls.Add(shareBox);
        var close = MakeButton("Close", "PublicProfileShareCancel", false, 96);
        close.Location = new Point(220, 170);
        close.Click += (_, _) => CloseModal();
        modalBody.Controls.Add(close);
        var copy = MakeButton("Copy profile link", "PublicProfileShareCopy", true, 142);
        copy.Location = new Point(326, 170);
        copy.Click += (_, _) =>
        {
            var link = $"https://tasktap.local/profile/{Uri.EscapeDataString(subject.FullName.Replace(" ", "-"))}";
            Clipboard.SetText(link);
            CloseModal();
            ShowToast("Profile link copied to your clipboard.");
        };
        modalBody.Controls.Add(copy);
        ShowModal(470, 250);
    }

    private void OpenReportModal()
    {
        modalBody.Controls.Clear();
        AddLabel(modalBody, "Report profile", 15, Navy, true, new Point(22, 18), new Size(400, 30));
        AddLabel(modalBody, "Choose a reason. The TaskTap team will review your report.", 8, Muted, false, new Point(22, 55), new Size(400, 36));
        AddLabel(modalBody, "Reason", 8, Navy, true, new Point(22, 102), new Size(350, 18));
        var reason = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(22, 124, 400, 30) };
        reason.Items.AddRange(["Suspicious or misleading profile", "Harassment or unsafe behavior", "Spam or inappropriate content", "Other"]);
        reason.SelectedIndex = 0;
        modalBody.Controls.Add(reason);
        AddLabel(modalBody, "Additional details", 8, Navy, true, new Point(22, 169), new Size(350, 18));
        var note = new TextBox { Multiline = true, Bounds = new Rectangle(22, 191, 400, 70), Font = new Font("Segoe UI", 9F), PlaceholderText = "Optional details..." };
        modalBody.Controls.Add(note);
        var cancel = MakeButton("Cancel", "PublicProfileReportCancel", false, 96);
        cancel.Location = new Point(220, 278);
        cancel.Click += (_, _) => CloseModal();
        modalBody.Controls.Add(cancel);
        var submit = MakeButton("Submit report", "PublicProfileReportSubmit", true, 142);
        submit.Location = new Point(326, 278);
        submit.Click += (_, _) => { DemoDataStore.SubmitReport("User", subject.FullName, reason.SelectedItem?.ToString() ?? "Other", note.Text.Trim()); CloseModal(); ShowToast("Report submitted. Thank you."); };
        modalBody.Controls.Add(submit);
        ShowModal(470, 360);
    }

    private void ShowModal(int width, int height)
    {
        modal.Size = new Size(width, height);
        modalBody.Bounds = new Rectangle(0, 0, width, height);
        modalBackdrop.Visible = true;
        modalBackdrop.BringToFront();
        LayoutModal();
    }

    private void CloseModal()
    {
        modalBackdrop.Visible = false;
        modalBody.Controls.Clear();
    }

    private void LayoutModal()
    {
        modalBackdrop.Bounds = ClientRectangle;
        modal.Location = new Point(Math.Max(0, (ClientSize.Width - modal.Width) / 2), Math.Max(0, (ClientSize.Height - modal.Height) / 2));
        var close = modal.Controls.Find("PublicProfileModalClose", false).FirstOrDefault();
        if (close is not null) close.Location = new Point(modal.Width - close.Width - 10, 8);
    }

    private void AddSectionHeading(Panel panel, string eyebrow, string title)
    {
        AddLabel(panel, eyebrow, 7.5F, Blue, true, new Point(20, 15), new Size(330, 18));
        AddLabel(panel, title, 13, Navy, true, new Point(20, 34), new Size(480, 26));
    }

    private void AddRatingBar(Panel parent, string rating, int percent, string count, int y)
    {
        AddLabel(parent, rating, 8, Muted, false, new Point(183, y), new Size(16, 18));
        var track = new Panel { Location = new Point(203, y + 5), Size = new Size(190, 7), BackColor = Color.FromArgb(232, 238, 245) };
        track.Controls.Add(new Panel { Dock = DockStyle.Left, Width = (int)(190 * percent / 100d), BackColor = Blue });
        parent.Controls.Add(track);
        AddLabel(parent, count, 7.5F, Muted, false, new Point(400, y), new Size(30, 18));
    }

    private void AddReview(Panel parent, string initials, string name, string date, string text, string task, int y, bool fourStars)
    {
        parent.Controls.Add(new Panel { Location = new Point(20, y - 7), Size = new Size(610, 1), BackColor = Line });
        var avatar = new Label { Text = initials, Location = new Point(20, y + 2), Size = new Size(38, 38), BackColor = Color.FromArgb(233, 243, 255), ForeColor = Color.FromArgb(7, 95, 190), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
        parent.Controls.Add(avatar);
        AddLabel(parent, name, 8.5F, Navy, true, new Point(70, y), new Size(120, 18));
        AddLabel(parent, fourStars ? "★★★★☆" : "★★★★★", 8, Color.FromArgb(255, 155, 69), true, new Point(195, y), new Size(80, 18));
        AddLabel(parent, date, 7, Muted, false, new Point(290, y), new Size(130, 18));
        AddLabel(parent, text, 8, Muted, false, new Point(70, y + 22), new Size(525, 42));
        AddLabel(parent, task, 7.5F, Blue, true, new Point(70, y + 66), new Size(340, 18));
    }

    private void AddHistoryRow(Panel parent, string title, string details, string amount, int y)
    {
        parent.Controls.Add(new Panel { Location = new Point(20, y - 6), Size = new Size(610, 1), BackColor = Line });
        var dot = new Label { Text = "✓", Location = new Point(20, y + 3), Size = new Size(28, 28), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(8, 122, 89), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
        parent.Controls.Add(dot);
        AddLabel(parent, title, 8.5F, Navy, true, new Point(60, y), new Size(360, 20));
        AddLabel(parent, details, 7.5F, Muted, false, new Point(60, y + 21), new Size(390, 18));
        AddLabel(parent, amount, 8, Navy, true, new Point(520, y + 8), new Size(80, 20));
    }

    private void AddTrustRow(Panel parent, string iconText, string title, string subtitle, int y)
    {
        var icon = new Label { Text = iconText, Location = new Point(18, y), Size = new Size(30, 30), BackColor = Color.FromArgb(237, 246, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        parent.Controls.Add(icon);
        AddLabel(parent, title, 8, Navy, true, new Point(58, y), new Size(225, 18));
        AddLabel(parent, subtitle, 7, Muted, false, new Point(58, y + 17), new Size(230, 20));
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var contentWidth = width - 44;
        var crumb = page.Controls.Find("publicProfileCrumb", false).FirstOrDefault();
        if (crumb is not null) crumb.Location = new Point(22, 12);
        hero.Bounds = new Rectangle(22, 42, contentWidth, 150);
        trustStrip.Bounds = new Rectangle(22, 208, contentWidth, 96);
        var leftWidth = Math.Max(450, (contentWidth - 22) * 66 / 100);
        var rightWidth = contentWidth - leftWidth - 22;
        leftColumn.Bounds = new Rectangle(22, 314, leftWidth, 1080);
        rightColumn.Bounds = new Rectangle(22 + leftWidth + 22, 314, rightWidth, 1010);
        var metricWidth = (trustStrip.Width - 20) / Math.Max(1, trustStrip.Controls.Count);
        for (var i = 0; i < trustStrip.Controls.Count; i++)
            trustStrip.Controls[i].Bounds = new Rectangle(14 + i * metricWidth, 12, metricWidth - 6, 72);
        var avatar = hero.Controls.Find("publicProfileAvatar", false).FirstOrDefault();
        if (avatar is not null) avatar.Location = new Point(24, 28);
        var labels = hero.Controls.OfType<Label>().Where(label => label.Name.Length == 0).ToArray();
        if (labels.Length >= 4)
        {
            labels[0].Location = new Point(138, 18);
            labels[1].Location = new Point(138, 39);
            labels[2].Location = new Point(138, 78);
            labels[3].Location = new Point(138, 110);
        }
        var verified = hero.Controls.Find("publicProfileVerified", false).FirstOrDefault();
        if (verified is not null)
        {
            var nameWidth = labels.Length >= 2 ? labels[1].PreferredWidth : 260;
            verified.Location = new Point(138 + nameWidth + 14, 52);
            verified.BringToFront();
        }
        Control? message = null;
        var share = hero.Controls.Find("PublicProfileShareButton", false).FirstOrDefault();
        if (message is not null) message.Location = new Point(contentWidth - 260, 45);
        if (share is not null) share.Location = new Point(contentWidth - 145, 45);
        aboutCard.Bounds = new Rectangle(0, 0, leftWidth, 185);
        skillsCard.Bounds = new Rectangle(0, 201, leftWidth, 270);
        reviewsCard.Bounds = new Rectangle(0, 487, leftWidth, 520);
        historyCard.Bounds = new Rectangle(0, 1023, leftWidth, 245);
        actionCard.Bounds = new Rectangle(0, 0, rightWidth, 258);
        trustCard.Bounds = new Rectangle(0, 274, rightWidth, 260);
        preferredCard.Bounds = new Rectangle(0, 550, rightWidth, 142);
        safetyCard.Bounds = new Rectangle(0, 708, rightWidth, 194);
        page.AutoScrollMinSize = new Size(0, 314 + Math.Max(leftColumn.Height, rightColumn.Height) + 30);
        LayoutModal();
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private Label MakeLabel(string text, float size, Color color, bool bold) => new() { Text = text, AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent };

    private void AddLabel(Control parent, string text, float size, Color color, bool bold, Point location, Size dimensions)
    {
        parent.Controls.Add(new Label { Text = text, Location = location, Size = dimensions, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent, AutoEllipsis = true });
    }

    private void ConfigureCard(Panel panel)
    {
        panel.BackColor = Color.White;
        panel.BorderStyle = BorderStyle.FixedSingle;
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
