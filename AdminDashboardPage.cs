namespace TaskTap;

/// <summary>
/// Admin home: KPIs, a needs-attention queue, recent activity, newest members,
/// quick actions and two painted mini charts. Everything is computed from the
/// shared <see cref="DemoDataStore"/> and refreshes on its DataChanged event.
/// </summary>
public sealed class AdminDashboardPage : AdminPage
{
    private static readonly Color[] ChartPalette =
    [
        Color.FromArgb(8, 117, 232),
        Color.FromArgb(20, 153, 111),
        Color.FromArgb(219, 108, 32),
        Color.FromArgb(123, 97, 209),
        Color.FromArgb(229, 83, 110),
        Color.FromArgb(255, 176, 102),
        Color.FromArgb(8, 35, 87),
        Color.FromArgb(110, 128, 150)
    ];

    private readonly Panel page = new();
    private readonly List<Panel> statCards = new();
    private readonly List<Label> statValues = new();
    private readonly List<Label> statCaptions = new();

    private Panel? attentionCard;
    private Panel? attentionBody;
    private Panel? activityCard;
    private Panel? activityBody;
    private Panel? quickCard;
    private Panel? signupsCard;
    private Panel? signupsBody;
    private Panel? taskChartCard;
    private Panel? paymentChartCard;
    private AdminBarChart? taskChart;
    private AdminBarChart? paymentChart;
    private int _attentionWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Dashboard;

    public AdminDashboardPage()
    {
        BuildContent();
        DemoDataStore.DataChanged += OnDataChanged;
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("ADMIN OVERVIEW", 8, AdminUi.Blue, true);
        eyebrow.Name = "adminDashEyebrow";
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Admin dashboard", 28, AdminUi.Navy, true);
        title.Name = "adminDashTitle";
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Operational overview for the TaskTap community.", 9, AdminUi.Muted);
        subtitle.Name = "adminDashSubtitle";
        page.Controls.Add(subtitle);

        AddStatCard("Users", "Registered accounts", 0);
        AddStatCard("Active tasks", "Posted + available", 1);
        AddStatCard("Open reports", "Open or investigating", 2);
        AddStatCard("Payments", "Recorded in the latest month", 3);
        AddStatCard("Verifications", "Waiting for approval", 4);

        BuildAttentionCard();
        BuildActivityCard();
        BuildQuickCard();
        BuildSignupsCard();
        BuildCharts();

        RefreshAll();
        LayoutPage();
    }

    private void AddStatCard(string heading, string caption, int index)
    {
        var card = AdminUi.MakeCard();
        card.Name = $"adminStat{index + 1}";

        var headingLabel = AdminUi.MakeLabel(heading, 8, AdminUi.Muted, true);
        headingLabel.Location = new Point(14, 12);
        card.Controls.Add(headingLabel);

        var valueLabel = AdminUi.MakeLabel("0", 20, AdminUi.Navy, true);
        valueLabel.Name = $"adminStatValue{index + 1}";
        valueLabel.Location = new Point(14, 32);
        card.Controls.Add(valueLabel);
        statValues.Add(valueLabel);

        var captionLabel = AdminUi.MakeLabel(caption, 7.5F, AdminUi.Muted);
        captionLabel.Location = new Point(14, 68);
        card.Controls.Add(captionLabel);
        statCaptions.Add(captionLabel);

        page.Controls.Add(card);
        statCards.Add(card);
    }

    private void BuildAttentionCard()
    {
        attentionCard = AdminUi.MakeCard();
        attentionCard.Name = "adminAttentionCard";
        page.Controls.Add(attentionCard);

        var title = AdminUi.MakeLabel("Needs attention", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        attentionCard.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Items that may need a decision today.", 8, AdminUi.Muted);
        subtitle.Location = new Point(18, 33);
        attentionCard.Controls.Add(subtitle);

        attentionBody = new Panel { BackColor = Color.White };
        attentionCard.Controls.Add(attentionBody);
    }

    private void BuildActivityCard()
    {
        activityCard = AdminUi.MakeCard();
        activityCard.Name = "adminActivityCard";
        page.Controls.Add(activityCard);

        var title = AdminUi.MakeLabel("Recent activity", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        activityCard.Controls.Add(title);

        activityBody = new Panel { BackColor = Color.White };
        activityCard.Controls.Add(activityBody);
    }

    private void BuildQuickCard()
    {
        quickCard = AdminUi.MakeCard();
        quickCard.Name = "adminQuickCard";
        page.Controls.Add(quickCard);

        var title = AdminUi.MakeLabel("Quick actions", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        quickCard.Controls.Add(title);

        var actions = new (string Text, AdminRoute Route)[]
        {
            ("Manage users", AdminRoute.Users),
            ("Review reports", AdminRoute.Reports),
            ("Task management", AdminRoute.Tasks),
            ("Categories", AdminRoute.Categories),
            ("Settings", AdminRoute.Settings)
        };

        var y = 46;
        foreach (var (text, route) in actions)
        {
            var button = AdminUi.MakeSecondaryButton(text, $"adminQuick{route}", 200);
            button.Location = new Point(18, y);
            var captured = route;
            button.Click += (_, _) => Nav.Navigate(captured);
            quickCard.Controls.Add(button);
            y += 46;
        }
    }

    private void BuildSignupsCard()
    {
        signupsCard = AdminUi.MakeCard();
        signupsCard.Name = "adminSignupsCard";
        page.Controls.Add(signupsCard);

        var title = AdminUi.MakeLabel("Newest members", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        signupsCard.Controls.Add(title);

        var viewAll = AdminUi.MakeLinkButton("View all →", "adminSignupsViewAll");
        viewAll.Click += (_, _) => Nav.Navigate(AdminRoute.Users);
        signupsCard.Controls.Add(viewAll);

        signupsBody = new Panel { BackColor = Color.White };
        signupsCard.Controls.Add(signupsBody);
    }

    private void BuildCharts()
    {
        taskChartCard = AdminUi.MakeCard();
        taskChartCard.Name = "adminTaskChartCard";
        page.Controls.Add(taskChartCard);

        var taskTitle = AdminUi.MakeLabel("Tasks by category", 11, AdminUi.Navy, true);
        taskTitle.Location = new Point(18, 14);
        taskChartCard.Controls.Add(taskTitle);

        taskChart = new AdminBarChart { Name = "adminTaskChart" };
        taskChartCard.Controls.Add(taskChart);

        paymentChartCard = AdminUi.MakeCard();
        paymentChartCard.Name = "adminPaymentChartCard";
        page.Controls.Add(paymentChartCard);

        var paymentTitle = AdminUi.MakeLabel("Payments by status", 11, AdminUi.Navy, true);
        paymentTitle.Location = new Point(18, 14);
        paymentChartCard.Controls.Add(paymentTitle);

        paymentChart = new AdminBarChart { Name = "adminPaymentChart" };
        paymentChartCard.Controls.Add(paymentChart);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "adminDashEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "adminDashTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "adminDashSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 96);

        const int gap = 14;
        var cardWidth = (contentWidth - gap * 4) / 5;
        for (var index = 0; index < statCards.Count; index++)
        {
            statCards[index].Location = new Point(left + index * (cardWidth + gap), 132);
            statCards[index].Size = new Size(cardWidth, 108);
        }

        var rightWidth = Math.Max(310, Math.Min(380, contentWidth / 3));
        var mainWidth = contentWidth - rightWidth - 18;

        if (attentionCard is not null)
        {
            attentionCard.Location = new Point(left, 258);
            attentionCard.Size = new Size(mainWidth, 262);
        }

        if (activityCard is not null)
        {
            activityCard.Location = new Point(left, 534);
            activityCard.Size = new Size(mainWidth, 300);
        }

        if (taskChartCard is not null)
        {
            taskChartCard.Location = new Point(left, 850);
            taskChartCard.Size = new Size(mainWidth, 210);
        }

        if (quickCard is not null)
        {
            quickCard.Location = new Point(left + mainWidth + 18, 258);
            quickCard.Size = new Size(rightWidth, 300);
        }

        if (signupsCard is not null)
        {
            signupsCard.Location = new Point(left + mainWidth + 18, 574);
            signupsCard.Size = new Size(rightWidth, 246);
        }

        if (paymentChartCard is not null)
        {
            paymentChartCard.Location = new Point(left + mainWidth + 18, 850);
            paymentChartCard.Size = new Size(rightWidth, 210);
        }

        if (attentionBody is not null)
            attentionBody.Bounds = new Rectangle(14, 56, (attentionCard?.Width ?? 0) - 28, (attentionCard?.Height ?? 0) - 66);
        if (activityBody is not null)
            activityBody.Bounds = new Rectangle(14, 42, (activityCard?.Width ?? 0) - 28, (activityCard?.Height ?? 0) - 52);
        if (signupsBody is not null)
            signupsBody.Bounds = new Rectangle(14, 42, (signupsCard?.Width ?? 0) - 28, (signupsCard?.Height ?? 0) - 52);
        if (taskChart is not null)
            taskChart.Bounds = new Rectangle(18, 44, (taskChartCard?.Width ?? 0) - 36, (taskChartCard?.Height ?? 0) - 56);
        if (paymentChart is not null)
            paymentChart.Bounds = new Rectangle(18, 44, (paymentChartCard?.Width ?? 0) - 36, (paymentChartCard?.Height ?? 0) - 56);

        if (signupsCard is not null)
        {
            var viewAll = AdminUi.Find<Button>(signupsCard, "adminSignupsViewAll");
            if (viewAll is not null)
                viewAll.Location = new Point(signupsCard.Width - viewAll.Width - 16, 14);
        }

        if (attentionBody is not null && attentionBody.Width != _attentionWidth)
        {
            _attentionWidth = attentionBody.Width;
            RefreshAttention();
        }

        page.AutoScrollMinSize = new Size(0, 1090);
    }

    private void RefreshAll()
    {
        RefreshStats();
        RefreshAttention();
        RefreshActivity();
        RefreshSignups();
        RefreshCharts();
    }

    private void RefreshStats()
    {
        if (statValues.Count < 5)
            return;

        var activeTasks = DemoDataStore.PostedTasks.Count + DemoDataStore.AvailableTasks.Count;
        var openReports = DemoDataStore.Reports.Count(report => report.Status is "Open" or "Investigating");
        var latest = DemoDataStore.Payments.OrderByDescending(payment => payment.Date).FirstOrDefault();
        var month = latest?.Date.Month ?? DateTime.Today.Month;
        var year = latest?.Date.Year ?? DateTime.Today.Year;
        var monthTotal = DemoDataStore.Payments
            .Where(payment => payment.Date.Month == month && payment.Date.Year == year)
            .Sum(payment => payment.Amount);

        statValues[0].Text = DemoDataStore.Users.Count.ToString();
        statValues[1].Text = activeTasks.ToString();
        statValues[2].Text = openReports.ToString();
        statValues[3].Text = $"₱{monthTotal:N0}";
        statValues[4].Text = DemoDataStore.Users.Count(user => user.IsPendingVerification).ToString();

        if (statCaptions.Count >= 4)
            statCaptions[3].Text = $"Recorded in {new DateTime(year, month, 1):MMM yyyy}";
    }

    private void RefreshAttention()
    {
        if (attentionBody is null)
            return;

        attentionBody.SuspendLayout();
        attentionBody.Controls.Clear();

        var rows = new (string Label, int Count, string Detail, AdminRoute Route, Color Color)[]
        {
            ("Pending verification", DemoDataStore.Users.Count(user => user.IsPendingVerification), "Waiting for identity approval", AdminRoute.VerificationQueue, AdminUi.Warn),
            ("Open reports", DemoDataStore.Reports.Count(report => report.Status == "Open"), "Not yet triaged", AdminRoute.Reports, AdminUi.Bad),
            ("Investigating", DemoDataStore.Reports.Count(report => report.Status == "Investigating"), "Reports under review", AdminRoute.Reports, AdminUi.Accent),
            ("Open disputes", DemoDataStore.Disputes.Count(dispute => dispute.Status == "Open"), "Awaiting resolution", AdminRoute.Disputes, AdminUi.Warn),
            ("Suspended users", DemoDataStore.Users.Count(user => user.Status == "Suspended"), "Accounts currently suspended", AdminRoute.Users, AdminUi.Muted)
        };

        var y = 0;
        foreach (var row in rows)
        {
            var dot = new Panel { BackColor = row.Color, Size = new Size(8, 8), Location = new Point(2, y + 7) };
            attentionBody.Controls.Add(dot);

            var label = AdminUi.MakeLabel(row.Label, 9, AdminUi.Navy, true);
            label.Location = new Point(18, y);
            attentionBody.Controls.Add(label);

            var detail = AdminUi.MakeLabel(row.Detail, 8, AdminUi.Muted);
            detail.Location = new Point(18, y + 19);
            attentionBody.Controls.Add(detail);

            var count = AdminUi.MakePill(row.Count.ToString(), Color.FromArgb(244, 248, 252), AdminUi.Navy);
            count.Location = new Point(Math.Max(18, attentionBody.Width - 128), y + 3);
            attentionBody.Controls.Add(count);

            var view = AdminUi.MakeLinkButton("View →", $"adminAttention{y}");
            view.Location = new Point(Math.Max(18, attentionBody.Width - 62), y + 3);
            var captured = row.Route;
            view.Click += (_, _) => Nav.Navigate(captured);
            attentionBody.Controls.Add(view);

            y += 38;
        }

        attentionBody.ResumeLayout();
    }

    private void RefreshActivity()
    {
        if (activityBody is null)
            return;

        activityBody.SuspendLayout();
        activityBody.Controls.Clear();

        var y = 0;
        foreach (var item in DemoDataStore.ActivityLog.OrderByDescending(activity => activity.Timestamp).Take(5))
        {
            var action = AdminUi.MakeLabel(item.Action, 9, AdminUi.Navy, true);
            action.Location = new Point(2, y);
            activityBody.Controls.Add(action);

            var detail = AdminUi.MakeLabel($"{item.Target} · {item.Actor} · {item.Timestamp:MMM d, h:mm tt}", 8, AdminUi.Muted);
            detail.Location = new Point(2, y + 18);
            activityBody.Controls.Add(detail);

            y += 42;
        }

        activityBody.ResumeLayout();
    }

    private void RefreshSignups()
    {
        if (signupsBody is null)
            return;

        signupsBody.SuspendLayout();
        signupsBody.Controls.Clear();

        var y = 0;
        foreach (var user in DemoDataStore.Users
                     .Where(user => !user.IsAdministrator)
                     .OrderByDescending(user => user.Id)
                     .Take(5))
        {
            var avatar = new Label
            {
                Text = user.Initials,
                Size = new Size(28, 28),
                Location = new Point(2, y),
                BackColor = Color.FromArgb(236, 243, 251),
                ForeColor = AdminUi.Blue,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            signupsBody.Controls.Add(avatar);

            var name = AdminUi.MakeLabel(user.FullName, 9, AdminUi.Navy, true);
            name.Location = new Point(38, y + 1);
            signupsBody.Controls.Add(name);

            var meta = AdminUi.MakeLabel($"{user.Role} · Brgy. {user.Barangay} · joined {user.Joined}", 7.5F, AdminUi.Muted);
            meta.Location = new Point(38, y + 17);
            signupsBody.Controls.Add(meta);

            y += 38;
        }

        signupsBody.ResumeLayout();
    }

    private void RefreshCharts()
    {
        if (taskChart is not null)
        {
            var items = new List<(string Label, int Value, Color Color)>();
            foreach (var category in DemoDataStore.Categories.OrderBy(category => category.SortOrder))
            {
                var count = DemoDataStore.PostedTasks.Count(task => task.Category == category.Name)
                          + DemoDataStore.AvailableTasks.Count(task => task.Category == category.Name);
                if (count > 0)
                    items.Add((category.Name, count, ChartPalette[items.Count % ChartPalette.Length]));
            }
            taskChart.SetItems(items);
        }

        if (paymentChart is not null)
        {
            var items = new List<(string Label, int Value, Color Color)>();
            foreach (var status in new[] { "Paid", "Pending", "Void", "Refunded" })
            {
                var count = DemoDataStore.Payments.Count(payment => payment.Status == status);
                if (count > 0)
                    items.Add((status, count, status switch
                    {
                        "Paid" => AdminUi.Good,
                        "Pending" => AdminUi.Warn,
                        _ => AdminUi.Bad
                    }));
            }
            paymentChart.SetItems(items);
        }
    }

    private void OnDataChanged()
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
        {
            BeginInvoke(OnDataChanged);
            return;
        }

        RefreshAll();
    }

    public override void OnNavigatedTo() => RefreshAll();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
