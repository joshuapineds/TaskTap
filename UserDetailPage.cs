using TaskTap.Data;

namespace TaskTap;

/// <summary>
/// User Detail: profile header, admin actions and tabbed context (overview, tasks,
/// payments, reviews, activity, reports) for one member of the shared repository.
/// </summary>
public sealed class UserDetailPage : AdminPage
{
    private readonly DemoUser user;
    private readonly Panel page = new();
    private readonly Panel headerCard = new();
    private readonly Panel tabsBar = new();
    private readonly Panel bodyCard = new();
    private readonly Panel bodyPanel = new();
    private readonly Dictionary<string, Button> tabButtons = new();
    private readonly Label nameLabel;
    private readonly Label roleLabel;
    private readonly Label contactLabel;
    private readonly Label metaLabel;
    private readonly Label statusPill;
    private readonly FlowLayoutPanel actions;
    private string activeTab = "overview";
    private int _lastBodyWidth = -1;

    public UserDetailPage(DemoUser user)
    {
        this.user = user;

        nameLabel = AdminUi.MakeLabel(user.FullName, 20, AdminUi.Navy, true);
        roleLabel = AdminUi.MakeLabel(user.Role, 9, AdminUi.Muted, true);
        contactLabel = AdminUi.MakeLabel($"{user.Email}   ·   {user.Phone}", 8.5F, AdminUi.Muted);
        metaLabel = AdminUi.MakeLabel($"Brgy. {user.Barangay}   ·   joined {user.Joined}   ·   {(user.Verified ? "Verified account" : "Not verified")}", 8.5F, AdminUi.Muted);
        statusPill = AdminUi.MakePill(user.Status, Color.FromArgb(240, 245, 250), AdminUi.Navy);
        actions = new FlowLayoutPanel
        {
            Name = "userDetailActions",
            Size = new Size(600, 40),
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        BuildContent();
    }

    public override AdminRoute AdminRouteName => AdminRoute.UserDetail;

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var crumb = AdminUi.MakeLinkButton("← User Management", "userDetailCrumb");
        crumb.Click += (_, _) => Nav.Navigate(AdminRoute.Users);
        page.Controls.Add(crumb);

        BuildHeader();
        BuildTabs();

        bodyCard.BackColor = Color.White;
        bodyCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(bodyCard);

        bodyPanel.BackColor = Color.White;
        bodyCard.Controls.Add(bodyPanel);

        BuildTab();
        LayoutPage();
    }

    private void BuildHeader()
    {
        headerCard.BackColor = Color.White;
        headerCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(headerCard);

        var avatar = new Label
        {
            Text = user.Initials,
            Size = new Size(64, 64),
            Location = new Point(20, 20),
            BackColor = user.AvatarColor,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };
        headerCard.Controls.Add(avatar);

        nameLabel.Location = new Point(100, 14);
        headerCard.Controls.Add(nameLabel);

        statusPill.Location = new Point(100, 46);
        headerCard.Controls.Add(statusPill);

        roleLabel.Location = new Point(100 + statusPill.Width + 12, 48);
        headerCard.Controls.Add(roleLabel);

        contactLabel.Location = new Point(100, 78);
        headerCard.Controls.Add(contactLabel);

        metaLabel.Location = new Point(100, 100);
        headerCard.Controls.Add(metaLabel);

        AddAction("Suspend / Activate", "userDetailToggle", user.Status == "Suspended" ? AdminUi.Good : AdminUi.Warn, () =>
        {
            if (user.Status == "Suspended")
                DemoDataStore.ActivateUser(user);
            else
                DemoDataStore.SuspendUser(user);
            ClosePage("User status updated.");
        });

        AddAction("Verify", "userDetailVerify", AdminUi.Blue, () =>
        {
            DemoDataStore.VerifyUser(user);
            ClosePage("User verified.");
        });

        AddAction("Reset password", "userDetailReset", AdminUi.Navy, () => Toast($"A password reset link would be sent to {user.Email}."));
        AddAction("Message", "userDetailMessage", AdminUi.Navy, () => Toast("Admins cannot message members directly. Use a support ticket reply instead."));
        AddAction("Delete", "userDetailDelete", AdminUi.Bad, DeleteUser);

        headerCard.Controls.Add(actions);
    }

    private void AddAction(string text, string name, Color color, Action click)
    {
        var button = AdminUi.MakeSecondaryButton(text, name, 110);
        button.ForeColor = color;
        button.Margin = new Padding(0, 0, 8, 0);
        button.Click += (_, _) => click();
        actions.Controls.Add(button);
    }

    private void BuildTabs()
    {
        tabsBar.BackColor = Color.Transparent;
        page.Controls.Add(tabsBar);

        var tabs = new (string Key, string Label)[]
        {
            ("overview", "Overview"),
            ("tasks", "Tasks"),
            ("payments", "Payments"),
            ("reviews", "Reviews"),
            ("activity", "Activity"),
            ("reports", "Reports")
        };

        var x = 0;
        foreach (var (key, label) in tabs)
        {
            var button = new Button
            {
                Text = label,
                Size = new Size(104, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AdminUi.Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Location = new Point(x, 0)
            };
            button.FlatAppearance.BorderColor = AdminUi.Line;
            button.FlatAppearance.BorderSize = 1;
            var captured = key;
            button.Click += (_, _) => SetActiveTab(captured);
            tabButtons[key] = button;
            tabsBar.Controls.Add(button);
            x += 110;
        }
    }

    private void SetActiveTab(string key)
    {
        activeTab = key;
        foreach (var (tabKey, button) in tabButtons)
        {
            var active = tabKey == key;
            button.BackColor = active ? AdminUi.Blue : Color.White;
            button.ForeColor = active ? Color.White : AdminUi.Muted;
        }

        BuildTab();
    }

    private void BuildTab()
    {
        bodyPanel.SuspendLayout();
        bodyPanel.Controls.Clear();
        var width = Math.Max(300, bodyPanel.ClientSize.Width);

        switch (activeTab)
        {
            case "tasks":
                BuildTasks(width);
                break;
            case "payments":
                BuildPayments(width);
                break;
            case "reviews":
                BuildReviews(width);
                break;
            case "activity":
                BuildActivity(width);
                break;
            case "reports":
                BuildReports(width);
                break;
            default:
                BuildOverview(width);
                break;
        }

        bodyPanel.ResumeLayout();
    }

    private void BuildOverview(int width)
    {
        var gap = 12;
        var cardWidth = (width - gap * 3) / 4;
        var stats = new (string Value, string Caption)[]
        {
            (user.CompletedTasks.ToString(), "Completed tasks"),
            (user.PostedTasks.ToString(), "Posted tasks"),
            ($"{user.Rating:0.0}", "Average rating"),
            (user.Reviews.ToString(), "Reviews")
        };

        for (var index = 0; index < stats.Length; index++)
        {
            var card = AdminUi.MakeCard();
            card.SetBounds(index * (cardWidth + gap), 0, cardWidth, 88);
            var value = AdminUi.MakeLabel(stats[index].Value, 20, AdminUi.Navy, true);
            value.Location = new Point(14, 12);
            card.Controls.Add(value);
            var caption = AdminUi.MakeLabel(stats[index].Caption, 8, AdminUi.Muted);
            caption.Location = new Point(14, 46);
            card.Controls.Add(caption);
            bodyPanel.Controls.Add(card);
        }

        var skillsTitle = AdminUi.MakeLabel("Skills & services", 10, AdminUi.Navy, true);
        skillsTitle.Location = new Point(0, 108);
        bodyPanel.Controls.Add(skillsTitle);

        var skillsFlow = new FlowLayoutPanel
        {
            Location = new Point(0, 130),
            Size = new Size(width, 40),
            WrapContents = false,
            BackColor = Color.Transparent
        };
        var skills = user.Skills.Length == 0 ? ["No skills listed"] : user.Skills;
        foreach (var skill in skills)
        {
            var pill = AdminUi.MakePill(skill, Color.FromArgb(240, 245, 250), AdminUi.Navy);
            pill.Margin = new Padding(0, 0, 8, 0);
            skillsFlow.Controls.Add(pill);
        }
        bodyPanel.Controls.Add(skillsFlow);

        var accountTitle = AdminUi.MakeLabel("Account details", 10, AdminUi.Navy, true);
        accountTitle.Location = new Point(0, 184);
        bodyPanel.Controls.Add(accountTitle);

        var lines = new (string Label, string Value)[]
        {
            ("Email", user.Email),
            ("Phone", user.Phone),
            ("Role", user.Role),
            ("Barangay", $"Brgy. {user.Barangay}"),
            ("Joined", user.Joined),
            ("Status", user.Status),
            ("Verification", user.Verified ? "Verified" : "Not verified")
        };

        var y = 210;
        foreach (var (label, value) in lines)
        {
            var key = AdminUi.MakeLabel(label, 8, AdminUi.Muted, true);
            key.Location = new Point(0, y);
            bodyPanel.Controls.Add(key);
            var val = AdminUi.MakeLabel(value, 9, AdminUi.Navy);
            val.Location = new Point(140, y - 1);
            bodyPanel.Controls.Add(val);
            y += 26;
        }
    }

    private void BuildTasks(int width)
    {
        var userId = Backend.ParseUserKey(user.Id);
        var tasks = DemoDataStore.AvailableTasks
            .Concat(DemoDataStore.ArchivedTasks)
            .Where(task => task.PosterId == userId)
            .GroupBy(task => task.Id)
            .Select(group => group.First())
            .ToList();

        if (tasks.Count == 0)
        {
            Empty("No tasks posted by this account.");
            return;
        }

        var note = AdminUi.MakeLabel("Task activity shown from the TaskTap marketplace.", 8, AdminUi.Muted);
        note.Location = new Point(0, 0);
        bodyPanel.Controls.Add(note);

        var y = 26;
        foreach (var task in tasks)
        {
            var row = AdminUi.MakeCard();
            row.SetBounds(0, y, width, 74);

            var icon = new Label
            {
                Text = task.Icon,
                Size = new Size(44, 44),
                Location = new Point(14, 14),
                BackColor = task.IconBack,
                ForeColor = AdminUi.Navy,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 16F)
            };
            row.Controls.Add(icon);

            var title = AdminUi.MakeLabel(task.Title, 10, AdminUi.Navy, true);
            title.Location = new Point(72, 12);
            row.Controls.Add(title);

            var meta = AdminUi.MakeLabel($"{task.Category} · ₱{task.Price:N0} · Brgy. {task.Barangay} · {task.Deadline}", 8, AdminUi.Muted);
            meta.Location = new Point(72, 34);
            row.Controls.Add(meta);

            var statusText = DemoDataStore.ArchivedTasks.Any(candidate => candidate.Id == task.Id) ? "Closed" : "Active";
            var status = AdminUi.MakePill(statusText,
                statusText == "Active" ? Color.FromArgb(232, 248, 241) : Color.FromArgb(240, 245, 250),
                statusText == "Active" ? AdminUi.Good : AdminUi.Muted);
            status.Location = new Point(width - 86, 24);
            row.Controls.Add(status);

            bodyPanel.Controls.Add(row);
            y += 82;
        }
    }

    private void BuildPayments(int width)
    {
        var payments = DemoDataStore.Payments.Where(payment => payment.Person == user.FullName).ToList();
        if (payments.Count == 0)
        {
            Empty("No recorded payments for this account.");
            return;
        }

        var y = 0;
        foreach (var payment in payments)
        {
            var row = AdminUi.MakeCard();
            row.SetBounds(0, y, width, 68);

            var title = AdminUi.MakeLabel(payment.Task, 10, AdminUi.Navy, true);
            title.Location = new Point(14, 12);
            row.Controls.Add(title);

            var meta = AdminUi.MakeLabel($"{payment.Reference} · {payment.Method} · {payment.Date:MMM d, yyyy}", 8, AdminUi.Muted);
            meta.Location = new Point(14, 34);
            row.Controls.Add(meta);

            var amount = AdminUi.MakeLabel($"₱{payment.Amount:N0}", 11, AdminUi.Navy, true);
            amount.Location = new Point(width - 160, 12);
            row.Controls.Add(amount);

            var back = payment.Status == "Paid" ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 244, 230);
            var fore = payment.Status == "Paid" ? AdminUi.Good : AdminUi.Warn;
            var status = AdminUi.MakePill(payment.Status, back, fore);
            status.Location = new Point(width - 92, 34);
            row.Controls.Add(status);

            bodyPanel.Controls.Add(row);
            y += 76;
        }
    }

    private void BuildReviews(int width)
    {
        var userId = Backend.ParseUserKey(user.Id);
        var reviews = userId == 0 ? [] : Backend.LoadReviews(userId, true);

        var summary = AdminUi.MakeCard();
        summary.SetBounds(0, 0, width, 92);

        var rating = AdminUi.MakeLabel($"{user.Rating:0.0}", 26, AdminUi.Navy, true);
        rating.Location = new Point(18, 14);
        summary.Controls.Add(rating);

        var stars = AdminUi.MakeLabel("★★★★★", 12, Color.FromArgb(255, 176, 102), true);
        stars.Location = new Point(110, 18);
        summary.Controls.Add(stars);

        var caption = AdminUi.MakeLabel($"{user.Reviews} reviews received", 8, AdminUi.Muted);
        caption.Location = new Point(110, 44);
        summary.Controls.Add(caption);

        var note = AdminUi.MakeLabel(reviews.Count == 0 ? "No review details are available." : "Most recent reviews", 8, AdminUi.Muted);
        note.Location = new Point(110, 62);
        summary.Controls.Add(note);

        bodyPanel.Controls.Add(summary);

        var y = 104;
        foreach (var review in reviews)
        {
            var card = AdminUi.MakeCard();
            card.SetBounds(0, y, width, 92);

            var headline = AdminUi.MakeLabel(review.Title.Length == 0 ? review.Task : review.Title, 10, AdminUi.Navy, true);
            headline.Location = new Point(18, 14);
            card.Controls.Add(headline);

            var starLabel = AdminUi.MakeLabel(new string('★', review.Rating), 9, Color.FromArgb(255, 176, 102), true);
            starLabel.Location = new Point(width - 110, 14);
            card.Controls.Add(starLabel);

            var text = AdminUi.MakeWrappedLabel($"{review.OtherName} · {review.Date:MMM d, yyyy}\n{review.Body}", 8.5F, AdminUi.Muted, Math.Max(300, width - 200));
            text.Location = new Point(18, 38);
            card.Controls.Add(text);

            bodyPanel.Controls.Add(card);
            y += 100;
        }
    }

    private void BuildActivity(int width)
    {
        var items = DemoDataStore.ActivityLog
            .Where(activity => activity.Target == user.FullName || activity.Actor == user.FullName)
            .OrderByDescending(activity => activity.Timestamp)
            .ToList();

        if (items.Count == 0)
        {
            Empty("No activity recorded for this account yet.");
            return;
        }

        var y = 0;
        foreach (var item in items)
        {
            var row = AdminUi.MakeCard();
            row.SetBounds(0, y, width, 64);

            var action = AdminUi.MakeLabel(item.Action, 9.5F, AdminUi.Navy, true);
            action.Location = new Point(16, 12);
            row.Controls.Add(action);

            var detail = AdminUi.MakeLabel($"{item.Target} · {item.Timestamp:MMM d, yyyy h:mm tt}", 8, AdminUi.Muted);
            detail.Location = new Point(16, 34);
            row.Controls.Add(detail);

            var category = AdminUi.MakePill(item.Category, Color.FromArgb(240, 245, 250), AdminUi.Navy);
            category.Location = new Point(width - 116, 18);
            row.Controls.Add(category);

            bodyPanel.Controls.Add(row);
            y += 72;
        }
    }

    private void BuildReports(int width)
    {
        var reports = DemoDataStore.Reports
            .Where(report => report.TargetLabel == user.FullName || report.ReporterName == user.FullName)
            .ToList();

        if (reports.Count == 0)
        {
            Empty("No reports involve this account.");
            return;
        }

        var y = 0;
        foreach (var report in reports)
        {
            var row = AdminUi.MakeCard();
            row.SetBounds(0, y, width, 76);

            var title = AdminUi.MakeLabel($"{report.Reason} · {report.TargetType}", 10, AdminUi.Navy, true);
            title.Location = new Point(16, 12);
            row.Controls.Add(title);

            var meta = AdminUi.MakeLabel($"{report.Id} · reported by {report.ReporterName} · {report.CreatedAt}", 8, AdminUi.Muted);
            meta.Location = new Point(16, 34);
            row.Controls.Add(meta);

            var severity = AdminUi.MakePill(report.Severity, Color.FromArgb(255, 244, 230), AdminUi.Warn);
            severity.Location = new Point(width - 100, 12);
            row.Controls.Add(severity);

            var status = AdminUi.MakePill(report.Status, Color.FromArgb(240, 245, 250), AdminUi.Navy);
            status.Location = new Point(width - 100, 42);
            row.Controls.Add(status);

            bodyPanel.Controls.Add(row);
            y += 84;
        }
    }

    private void Empty(string message)
    {
        var label = AdminUi.MakeLabel(message, 9, AdminUi.Muted);
        label.Location = new Point(0, 0);
        bodyPanel.Controls.Add(label);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var crumb = AdminUi.Find<Button>(page, "userDetailCrumb");
        if (crumb is not null) crumb.Location = new Point(left, 18);

        headerCard.Bounds = new Rectangle(left, 48, contentWidth, 136);

        nameLabel.MaximumSize = new Size(Math.Max(240, contentWidth - 720), 0);
        actions.Location = new Point(headerCard.Width - 608, 16);

        tabsBar.Bounds = new Rectangle(left, 200, contentWidth, 38);

        bodyCard.Bounds = new Rectangle(left, 252, contentWidth, 540);
        bodyPanel.Bounds = new Rectangle(18, 18, bodyCard.Width - 36, bodyCard.Height - 36);

        page.AutoScrollMinSize = new Size(0, bodyCard.Bottom + 40);
        if (bodyPanel.Width != _lastBodyWidth)
        {
            _lastBodyWidth = bodyPanel.Width;
            BuildTab();
        }
    }

    private void ClosePage(string message)
    {
        Toast(message);
        if (!Nav.Back())
            Nav.Navigate(AdminRoute.Users);
    }

    private void DeleteUser()
    {
        var answer = MessageBox.Show(
            this,
            $"Delete {user.FullName}?\n\nThis permanently removes the account and everything it posted.",
            "Delete user",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        DemoDataStore.DeleteUser(user);
        if (!Nav.Back())
            Nav.Navigate(AdminRoute.Users);
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);
}
