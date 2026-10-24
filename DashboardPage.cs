namespace TaskTap;

public sealed class DashboardPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private static readonly Color Green = Color.FromArgb(32, 175, 132);
    private static readonly Color Orange = Color.FromArgb(240, 132, 58);
    private static readonly Color Amber = Color.FromArgb(180, 130, 20);

    private readonly Panel scrollHost = new();
    private readonly Panel page = new();
    private readonly Label greetingLabel = new();
    private readonly Label statusLabel = new();
    private readonly Button hiringTab = new();
    private readonly Button workingTab = new();
    private readonly FlowLayoutPanel actionStrip = new();
    private readonly Panel statsRow = new();
    private readonly Panel taskCard = new();
    private readonly Label taskTitle = new();
    private readonly FlowLayoutPanel taskList = new();
    private readonly Panel activityCard = new();
    private readonly FlowLayoutPanel activityList = new();
    private readonly Panel tipCard = new();
    private string mode = "hiring";
    private bool built;

    public override Route RouteName => Route.Dashboard;

    public DashboardPage()
    {
        BuildPage();
        DemoDataStore.DataChanged += OnDataChanged;
    }

    private string Me => AppSession.CurrentUser.FullName;

    private DemoUser? MeRecord => DemoDataStore.Users.FirstOrDefault(user => user.FullName == Me);

    private void BuildPage()
    {
        scrollHost.Dock = DockStyle.Fill;
        scrollHost.AutoScroll = true;
        scrollHost.BackColor = Page;
        scrollHost.Resize += (_, _) => LayoutPage();
        Controls.Add(scrollHost);

        page.BackColor = Page;
        scrollHost.Controls.Add(page);

        BuildHeader();
        BuildBody();

        built = true;
        RefreshDashboard();
    }

    private void BuildHeader()
    {
        var eyebrow = MakeLabel("YOUR WORKSPACE", 9, Blue, true);
        eyebrow.Name = "dashboardEyebrow";
        page.Controls.Add(eyebrow);

        greetingLabel.Name = "dashboardTitle";
        greetingLabel.AutoSize = true;
        greetingLabel.ForeColor = Navy;
        greetingLabel.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
        page.Controls.Add(greetingLabel);

        statusLabel.Name = "dashboardSubtitle";
        statusLabel.AutoSize = true;
        statusLabel.ForeColor = Muted;
        statusLabel.Font = new Font("Segoe UI", 10F);
        page.Controls.Add(statusLabel);

        var post = MakeAction("＋  Post a Task", "DashboardPostTaskButton", true, 146);
        post.Click += (_, _) => Nav.Navigate(Route.PostTask);
        page.Controls.Add(post);

        ConfigureToggle(hiringTab, "Hiring", "DashboardModeHiring");
        ConfigureToggle(workingTab, "Working", "DashboardModeWorking");
        hiringTab.Click += (_, _) => SetMode("hiring");
        workingTab.Click += (_, _) => SetMode("working");
        page.Controls.Add(hiringTab);
        page.Controls.Add(workingTab);

        actionStrip.Name = "dashboardActionStrip";
        actionStrip.BackColor = Color.Transparent;
        actionStrip.WrapContents = false;
        actionStrip.AutoScroll = false;
        actionStrip.Margin = new Padding(0);
        page.Controls.Add(actionStrip);
    }

    private void BuildBody()
    {
        statsRow.BackColor = Color.Transparent;
        page.Controls.Add(statsRow);

        taskCard.Name = "DashboardTasksCard";
        ConfigureCard(taskCard);
        page.Controls.Add(taskCard);

        taskTitle.AutoSize = true;
        taskTitle.ForeColor = Navy;
        taskTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        taskTitle.Location = new Point(18, 15);
        taskCard.Controls.Add(taskTitle);

        taskList.Name = "dashboardTaskList";
        taskList.FlowDirection = FlowDirection.TopDown;
        taskList.WrapContents = false;
        taskList.AutoScroll = true;
        taskList.BackColor = Color.White;
        taskCard.Controls.Add(taskList);

        activityCard.Name = "DashboardActivityCard";
        ConfigureCard(activityCard);
        page.Controls.Add(activityCard);

        var activityTitle = MakeLabel("Recent Activity", 12, Navy, true);
        activityTitle.Location = new Point(18, 15);
        activityCard.Controls.Add(activityTitle);

        activityList.Name = "dashboardActivityList";
        activityList.FlowDirection = FlowDirection.TopDown;
        activityList.WrapContents = false;
        activityList.AutoScroll = true;
        activityList.BackColor = Color.White;
        activityCard.Controls.Add(activityList);

        tipCard.Name = "dashboardTipCard";
        tipCard.BackColor = Color.White;
        tipCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(tipCard);
    }

    private void SetMode(string next)
    {
        mode = next;
        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        if (!built)
            return;

        var hour = DateTime.Now.Hour;
        greetingLabel.Text = $"{(hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening")}, {AppSession.CurrentUser.FirstName}!";

        var attention = CountAttention();
        statusLabel.Text = attention == 0
            ? $"{DateTime.Today:dddd, MMMM d} · You're all caught up."
            : $"{DateTime.Today:dddd, MMMM d} · {attention} thing{(attention == 1 ? "" : "s")} need{(attention == 1 ? "s" : "")} your attention.";

        hiringTab.BackColor = mode == "hiring" ? Blue : Color.White;
        hiringTab.ForeColor = mode == "hiring" ? Color.White : Navy;
        workingTab.BackColor = mode == "working" ? Blue : Color.White;
        workingTab.ForeColor = mode == "working" ? Color.White : Navy;

        RefreshActionStrip();
        RefreshStats();
        RefreshTasks();
        RefreshActivity();
        RefreshTip();
        LayoutPage();
    }

    private int CountAttention()
    {
        var count = 0;
        if (DemoDataStore.RequestsReceived.Any(applicant => !applicant.Declined)) count++;
        if (DemoDataStore.PostedTasks.Any(task => task.Days <= 2)) count++;
        if (DemoDataStore.Disputes.Any(dispute => IsMine(dispute) && dispute.Status == "Open")) count++;
        return count;
    }

    private bool IsMine(DemoDispute dispute) =>
        dispute.PosterName == Me || dispute.DoerName == Me;

    private void RefreshActionStrip()
    {
        actionStrip.SuspendLayout();
        actionStrip.Controls.Clear();

        var chips = new List<Control>();
        var requests = DemoDataStore.RequestsReceived.Count(applicant => !applicant.Declined);
        if (requests > 0)
            chips.Add(MakeChip($"✓  {requests} request{(requests == 1 ? "" : "s")} to review", "DashboardChipRequests", Color.FromArgb(232, 248, 241), Green, () =>
            {
                DemoDataStore.ManagedTask = DemoDataStore.PostedTasks.FirstOrDefault(task => DemoDataStore.RequestsReceived.Any(request => request.TaskId == task.Id));
                Nav.Navigate(new ManageRequestsPage());
            }));

        if (DemoDataStore.UnreadNotifications > 0)
            chips.Add(MakeChip($"◔  {DemoDataStore.UnreadNotifications} unread notification{(DemoDataStore.UnreadNotifications == 1 ? "" : "s")}", "DashboardChipNotifications", Color.FromArgb(238, 245, 252), Navy, () => Nav.Navigate(Route.Notifications)));

        var dueSoon = DemoDataStore.PostedTasks.Count(task => task.Days <= 2);
        if (dueSoon > 0)
            chips.Add(MakeChip($"◷  {dueSoon} due soon", "DashboardChipDue", Color.FromArgb(255, 247, 224), Amber, () => Nav.Navigate(Route.MyTasks)));

        var disputes = DemoDataStore.Disputes.Count(dispute => IsMine(dispute) && dispute.Status == "Open");
        if (disputes > 0)
            chips.Add(MakeChip($"⚑  {disputes} open dispute{(disputes == 1 ? "" : "s")}", "DashboardChipDisputes", Color.FromArgb(255, 240, 230), Color.FromArgb(185, 55, 67), () => Nav.Navigate(Route.MyDisputes)));

        var saved = DemoDataStore.SavedTasks.Count;
        if (saved > 0)
            chips.Add(MakeChip($"♡  {saved} saved task{(saved == 1 ? "" : "s")}", "DashboardChipSaved", Color.FromArgb(245, 240, 255), Color.FromArgb(110, 80, 190), () => Nav.Navigate(Route.SavedTasks)));

        if (chips.Count == 0)
            chips.Add(MakeChip("✓  You're all caught up", "DashboardChipClear", Color.FromArgb(232, 248, 241), Green, () => Nav.Navigate(Route.FindTasks)));

        foreach (var chip in chips.Take(4))
            actionStrip.Controls.Add(chip);

        actionStrip.ResumeLayout();
    }

    private void RefreshStats()
    {
        statsRow.SuspendLayout();
        statsRow.Controls.Clear();

        var cards = new List<Panel>();
        if (mode == "hiring")
        {
            cards.Add(MakeStatCard("▤", DemoDataStore.PostedTasks.Count.ToString(), "Open posts", Color.FromArgb(232, 243, 255), Blue, () => Nav.Navigate(Route.MyTasks)));
            cards.Add(MakeStatCard("✓", DemoDataStore.RequestsReceived.Count(applicant => !applicant.Declined).ToString(), "Requests to review", Color.FromArgb(230, 248, 241), Green, () => Nav.Navigate(new ManageRequestsPage())));
            cards.Add(MakeStatCard("★", $"{(MeRecord?.Rating ?? 0):0.0}", "Your rating", Color.FromArgb(255, 241, 231), Orange, () => Nav.Navigate(Route.Reviews)));
        }
        else
        {
            cards.Add(MakeStatCard("↗", DemoDataStore.RequestsSent.Count.ToString(), "Requests sent", Color.FromArgb(232, 243, 255), Blue, () => Nav.Navigate(Route.MyRequests)));
            var earned = DemoDataStore.Payments.Where(payment => payment.Person == Me && payment.Status == "Paid").Sum(payment => payment.Amount);
            cards.Add(MakeStatCard("₱", $"₱{earned:N0}", "Earnings", Color.FromArgb(230, 248, 241), Green, () => Nav.Navigate(Route.Earnings)));
            cards.Add(MakeStatCard("♡", DemoDataStore.SavedTasks.Count.ToString(), "Saved tasks", Color.FromArgb(255, 241, 231), Orange, () => Nav.Navigate(Route.SavedTasks)));
        }

        foreach (var card in cards)
            statsRow.Controls.Add(card);

        statsRow.ResumeLayout();
    }

    private void RefreshTasks()
    {
        taskList.SuspendLayout();
        taskList.Controls.Clear();

        if (mode == "hiring")
        {
            taskTitle.Text = "Your open posts";
            var posts = DemoDataStore.PostedTasks.Take(3).ToList();
            if (posts.Count == 0)
                AddEmptyRow(taskList, "No open posts yet. Post a task to get help from neighbors.");
            foreach (var task in posts)
                AddPostRow(task);
        }
        else
        {
            taskTitle.Text = "Tasks for You";
            var barangay = AppSession.CurrentUser.Barangay.Replace("Brgy. ", "").Trim();
            var tasks = DemoDataStore.AvailableTasks
                .OrderByDescending(task => task.Barangay == barangay)
                .ThenBy(task => task.Days)
                .Take(3)
                .ToList();
            if (tasks.Count == 0)
                AddEmptyRow(taskList, "No tasks available right now. Check back soon.");
            foreach (var task in tasks)
                AddBrowseRow(task);
        }

        taskList.ResumeLayout();
    }

    private void AddPostRow(DemoTask task)
    {
        var row = NewRow();
        AddIcon(row, task);
        AddRowTitle(row, task.Title);

        var requests = DemoDataStore.RequestsForTask(task.Title);
        var meta = MakeLabel($"₱{task.Price:N0} · {task.Category} · Brgy. {task.Barangay} · {(requests == 0 ? "No requests yet" : $"{requests} request{(requests == 1 ? "" : "s")}")}", 8, Muted);
        meta.Location = new Point(58, 32);
        meta.Size = new Size(320, 30);
        row.Controls.Add(meta);

        var inspect = MakeAction(requests > 0 ? "View requests" : "Manage", "DashboardOpenRequests", false, 104);
        inspect.Size = new Size(104, 32);
        inspect.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        inspect.Location = new Point(0, 22);
        inspect.Click += (_, _) => Nav.Navigate(new ManageRequestsPage());
        row.Controls.Add(inspect);

        taskList.Controls.Add(row);
        LayoutRow(row, inspect, 104);
    }

    private void AddBrowseRow(DemoTask task)
    {
        var row = NewRow();
        AddIcon(row, task);
        AddRowTitle(row, task.Title);

        var meta = MakeLabel($"₱{task.Price:N0} · {task.Category} · Brgy. {task.Barangay}\nDeadline: {task.Deadline}", 8, Muted);
        meta.Location = new Point(58, 32);
        meta.Size = new Size(320, 38);
        row.Controls.Add(meta);

        var view = MakeAction("View Details", "DashboardOpenTask", false, 96);
        view.Size = new Size(96, 32);
        view.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        view.Location = new Point(0, 22);
        view.Click += (_, _) => Nav.Navigate(new UserTaskDetailsPage(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack, task.Id));
        row.Controls.Add(view);

        taskList.Controls.Add(row);
        LayoutRow(row, view, 96);
    }

    private Panel NewRow() => new()
    {
        BackColor = Color.White,
        Margin = new Padding(0),
        Size = new Size(600, 81)
    };

    private static void AddIcon(Panel row, DemoTask task)
    {
        var icon = new Label
        {
            Text = task.Icon,
            Location = new Point(0, 13),
            Size = new Size(46, 46),
            BackColor = task.IconBack,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Emoji", 19F)
        };
        row.Controls.Add(icon);
    }

    private static void AddRowTitle(Panel row, string title)
    {
        var label = MakeLabel(title, 11, Navy, true);
        label.Location = new Point(58, 10);
        row.Controls.Add(label);
    }

    private void LayoutRow(Panel row, Control action, int actionWidth)
    {
        row.Width = Math.Max(320, taskList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        action.Location = new Point(row.Width - action.Width - 12, 22);
        var price = row.Controls.OfType<Label>().FirstOrDefault(label => label.Location.Y == 32);
        if (price is not null)
            price.Width = Math.Max(120, row.Width - actionWidth - 90);
        var divider = new Panel { BackColor = Line, Location = new Point(0, 79), Size = new Size(row.Width, 1) };
        if (!row.Controls.OfType<Panel>().Any())
            row.Controls.Add(divider);
    }

    private void RefreshActivity()
    {
        activityList.SuspendLayout();
        activityList.Controls.Clear();

        var events = new List<(DateTime When, string Icon, string Title, string Detail, Color IconBack, Color IconColor, Action Open)>();

        foreach (var applicant in DemoDataStore.RequestsReceived.Where(applicant => !applicant.Declined).OrderByDescending(applicant => applicant.Date).Take(2))
            events.Add((applicant.Date, "✓", "New task request", $"{applicant.Name} · {applicant.TaskTitle}", Color.FromArgb(230, 248, 241), Green, () => Nav.Navigate(new ManageRequestsPage())));

        foreach (var payment in DemoDataStore.Payments.Where(payment => payment.Person == Me).OrderByDescending(payment => payment.Date).Take(2))
            events.Add((payment.Date, "₱", "Payment recorded", $"₱{payment.Amount:N0} · {payment.Task}", Color.FromArgb(255, 241, 231), Orange, () => Nav.Navigate(Route.Earnings)));

        foreach (var outcome in DemoDataStore.TaskOutcomes.OrderByDescending(outcome => outcome.Date).Take(1))
            events.Add((outcome.Date, "✓", outcome.Status == "Completed" ? "Task completed" : "Task cancelled", outcome.Title, Color.FromArgb(230, 248, 241), Green, () => Nav.Navigate(Route.ArchivedTasks)));

        foreach (var announcement in DemoDataStore.Announcements.Where(announcement => announcement.Active).Take(1))
            events.Add((DateTime.Today, "✉", "Announcement", announcement.Title, Color.FromArgb(236, 244, 255), Blue, () => Nav.Navigate(Route.Notifications)));

        var recent = events.OrderByDescending(item => item.When).Take(4).ToList();
        if (recent.Count == 0)
            AddEmptyRow(activityList, "No recent activity yet.");

        foreach (var item in recent)
            AddActivityRow(item.Icon, item.Title, item.Detail, When(item.When), item.IconBack, item.IconColor, item.Open);

        activityList.ResumeLayout();
    }

    private static string When(DateTime date) =>
        date.Date == DateTime.Today ? "Today" : date.Year < 2000 ? "Recently" : date.ToString("MMM d");

    private void AddActivityRow(string icon, string title, string detail, string when, Color iconBack, Color iconColor, Action open)
    {
        var item = new Panel { Size = new Size(300, 60), BackColor = Color.White, Margin = new Padding(0), Cursor = Cursors.Hand };
        var badge = new Label
        {
            Text = icon,
            Location = new Point(0, 6),
            Size = new Size(30, 30),
            BackColor = iconBack,
            ForeColor = iconColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        var heading = MakeLabel(title, 9, Navy, true);
        heading.Location = new Point(40, 4);
        var description = MakeLabel(detail, 8, Muted);
        description.Location = new Point(40, 23);
        var stamp = MakeLabel(when, 8, Muted);
        stamp.Location = new Point(40, 39);
        item.Controls.Add(badge);
        item.Controls.Add(heading);
        item.Controls.Add(description);
        item.Controls.Add(stamp);
        item.Controls.Add(new Panel { BackColor = Line, Location = new Point(0, 58), Size = new Size(300, 1) });
        item.Click += (_, _) => open();
        badge.Click += (_, _) => open();
        heading.Click += (_, _) => open();
        description.Click += (_, _) => open();
        stamp.Click += (_, _) => open();
        activityList.Controls.Add(item);
    }

    private void RefreshTip()
    {
        tipCard.SuspendLayout();
        tipCard.Controls.Clear();

        var hiring = mode == "hiring";
        var icon = new Label
        {
            Text = hiring ? "✓" : "★",
            Location = new Point(16, 16),
            Size = new Size(36, 36),
            BackColor = hiring ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 241, 231),
            ForeColor = hiring ? Green : Orange,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold)
        };
        tipCard.Controls.Add(icon);

        var title = MakeLabel(hiring ? "Review requests quickly" : "Keep your profile current", 10.5F, Navy, true);
        title.Location = new Point(62, 16);
        tipCard.Controls.Add(title);

        var body = MakeLabel(
            hiring
                ? "Applicants with a verified profile and a short message are usually the best fit. Accept one to lock the task."
                : "A complete profile and a verified account help posters choose you. Add your skills and request verification.",
            8,
            Muted);
        body.MaximumSize = new Size(240, 46);
        body.Location = new Point(62, 40);
        tipCard.Controls.Add(body);

        var action = MakeAction(hiring ? "Review requests" : "Open account settings", "DashboardTipAction", false, 150);
        action.Size = new Size(150, 32);
        action.Location = new Point(62, 92);
        action.Click += (_, _) =>
        {
            if (hiring)
                Nav.Navigate(new ManageRequestsPage());
            else
                Nav.Navigate(Route.AccountSettings);
        };
        tipCard.Controls.Add(action);

        tipCard.ResumeLayout();
    }

    private void LayoutPage()
    {
        if (!built || scrollHost.ClientSize.Width <= 0)
            return;

        var width = Math.Max(820, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
        page.Size = new Size(width, 760);
        page.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);

        var left = 24;
        var right = width - 24;
        var contentWidth = width - 48;

        Position("dashboardEyebrow", left, 24);
        greetingLabel.Location = new Point(left, 42);
        statusLabel.Location = new Point(left, 98);
        actionStrip.Location = new Point(left, 134);
        actionStrip.Size = new Size(contentWidth, 40);

        var post = Find<Button>("DashboardPostTaskButton");
        if (post is not null)
            post.Location = new Point(right - post.Width, 46);

        var toggleWidth = 88;
        var toggleRight = right - (post?.Width ?? 146) - 12;
        workingTab.Size = new Size(toggleWidth, 34);
        hiringTab.Size = new Size(toggleWidth, 34);
        workingTab.Location = new Point(toggleRight - toggleWidth, 52);
        hiringTab.Location = new Point(toggleRight - toggleWidth * 2 - 4, 52);

        statsRow.Location = new Point(left, 186);
        statsRow.Size = new Size(contentWidth, 74);
        var gap = 14;
        var statWidth = (contentWidth - gap * 2) / 3;
        for (var index = 0; index < statsRow.Controls.Count; index++)
        {
            statsRow.Controls[index].Location = new Point(index * (statWidth + gap), 0);
            statsRow.Controls[index].Size = new Size(statWidth, 74);
        }

        var rightWidth = Math.Max(300, Math.Min(360, (contentWidth - 16) / 3));
        var taskWidth = contentWidth - rightWidth - 16;
        var contentTop = 276;

        taskCard.Location = new Point(left, contentTop);
        taskCard.Size = new Size(taskWidth, 440);
        taskList.Location = new Point(16, 48);
        taskList.Size = new Size(taskWidth - 32, 378);
        foreach (Control row in taskList.Controls)
        {
            row.Width = Math.Max(320, taskList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            var action = row.Controls.OfType<Button>().FirstOrDefault();
            if (action is not null)
                action.Location = new Point(row.Width - action.Width - 12, 22);
            var meta = row.Controls.OfType<Label>().FirstOrDefault(label => label.Location.Y == 32);
            if (meta is not null)
                meta.Width = Math.Max(120, row.Width - (action?.Width ?? 96) - 90);
            var divider = row.Controls.OfType<Panel>().LastOrDefault();
            if (divider is not null)
                divider.Width = row.Width;
        }

        activityCard.Location = new Point(left + taskWidth + 16, contentTop);
        activityCard.Size = new Size(rightWidth, 286);
        activityList.Location = new Point(16, 48);
        activityList.Size = new Size(rightWidth - 32, 224);
        foreach (Control item in activityList.Controls)
        {
            item.Width = activityList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
        }

        tipCard.Location = new Point(left + taskWidth + 16, contentTop + 302);
        tipCard.Size = new Size(rightWidth, 138);

        page.Height = contentTop + 460;
    }

    private void AddEmptyRow(FlowLayoutPanel host, string text)
    {
        var label = MakeLabel(text, 9, Muted);
        label.MaximumSize = new Size(320, 40);
        label.Margin = new Padding(0, 12, 0, 0);
        host.Controls.Add(label);
    }

    private Panel MakeStatCard(string icon, string value, string caption, Color iconBack, Color iconColor, Action open)
    {
        var card = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
        var iconLabel = new Label
        {
            Text = icon,
            Location = new Point(16, 17),
            Size = new Size(40, 40),
            BackColor = iconBack,
            ForeColor = iconColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold)
        };
        var valueLabel = MakeLabel(value, 21, Navy, true);
        valueLabel.Location = new Point(64, 14);
        var captionLabel = MakeLabel(caption, 9, Muted);
        captionLabel.Location = new Point(65, 52);

        card.Controls.Add(iconLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(captionLabel);
        card.Click += (_, _) => open();
        iconLabel.Click += (_, _) => open();
        valueLabel.Click += (_, _) => open();
        captionLabel.Click += (_, _) => open();
        return card;
    }

    private Button MakeChip(string text, string name, Color back, Color fore, Action open)
    {
        var chip = new Button
        {
            Name = name,
            Text = text,
            AutoSize = true,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            Padding = new Padding(12, 0, 12, 0),
            Margin = new Padding(0, 0, 8, 0)
        };
        chip.FlatAppearance.BorderSize = 0;
        chip.Click += (_, _) => open();
        return chip;
    }

    private static void ConfigureToggle(Button button, string text, string name)
    {
        button.Name = name;
        button.Text = text;
        button.Size = new Size(88, 34);
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(8, 38, 87);
        button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderColor = Color.FromArgb(221, 231, 242);
        button.FlatAppearance.BorderSize = 1;
    }

    private void Position(string name, int x, int y)
    {
        var control = page.Controls.Find(name, false).FirstOrDefault();
        if (control is not null)
            control.Location = new Point(x, y);
    }

    private static void ConfigureCard(Panel card)
    {
        card.BackColor = Color.White;
        card.BorderStyle = BorderStyle.FixedSingle;
    }

    private T? Find<T>(string name) where T : Control =>
        page.Controls.Find(name, false).OfType<T>().FirstOrDefault();

    private static Button MakeAction(string text, string name, bool primary, int width)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(width, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Blue : Color.White,
            ForeColor = primary ? Color.White : Navy,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(221, 231, 242);
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
    };

    private void OnDataChanged()
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
        {
            BeginInvoke(OnDataChanged);
            return;
        }

        RefreshDashboard();
    }

    public override void OnNavigatedTo() => RefreshDashboard();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
