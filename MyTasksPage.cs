namespace TaskTap;

public sealed class MyTasksPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel scrollHost = new();
    private readonly Panel canvas = new();
    private readonly FlowLayoutPanel taskList = new();
    private readonly List<TaskRecord> tasks = [];
    private readonly Dictionary<string, Button> tabs = [];
    private readonly Dictionary<string, int> tabCounts = new() { ["posted"] = 6, ["requested"] = 2, ["accepted"] = 1, ["progress"] = 1, ["completed"] = 8 };
    private readonly TextBox search = new();
    private readonly ComboBox categoryFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label noticeTitle = new();
    private readonly Label noticeBody = new();
    private readonly Label noticeLink = new();
    private readonly Label resultCount = new();
    private readonly Label resultLabel = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private string currentTab = "posted";
    private bool compactView;
    private bool pageBuilt;

    public override Route RouteName => Route.MyTasks;

    private sealed record TaskRecord(string Title, string Status, string Tab, string Category, int Amount, string Barangay, string Date, string Description, string Meta, string Icon, Color IconBack, int Requests = 0);

    public MyTasksPage()
    {
        SeedTasks();
        BuildPage();
    }

    private void SeedTasks()
    {
        foreach (var request in DemoDataStore.RequestsSent.Where(request => request.Status is "pending" or "accepted"))
        {
            var completed = request.Status == "accepted" && request.TaskStatus == "Completed";
            var tab = request.Status == "pending" ? "requested" : completed ? "completed" : "accepted";
            var status = request.Status == "pending" ? "Pending request" : completed ? "Completed" : "Accepted";
            var meta = request.Status == "pending"
                ? $"Requested {request.Date:MMM d}  ·  Request pending"
                : completed ? "Completed" : "Request accepted";
            tasks.Add(new TaskRecord(request.Title, status, tab, request.Category, request.Amount, request.Barangay,
                request.Deadline, request.Description, meta, request.Icon, request.IconBack));
        }
    }

    private void BuildPage()
    {
        scrollHost.Dock = DockStyle.Fill;
        scrollHost.AutoScroll = true;
        scrollHost.BackColor = Page;
        scrollHost.Resize += (_, _) => LayoutPage();
        Controls.Add(scrollHost);
        canvas.BackColor = Page;
        scrollHost.Controls.Add(canvas);
        BuildContent();
        toast.Size = new Size(420, 40);
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
        SelectTab(currentTab);
    }

    private void BuildContent()
    {
        AddLabel("myTasksEyebrow", "TASK MANAGEMENT", 8, Blue, true);
        AddLabel("myTasksHeading", "My Tasks", 25, Navy, true);
        AddLabel("myTasksSubtitle", "Keep track of tasks you posted and tasks you’re helping with.", 9, Muted);
        var payment = MakeButton("₱  Payment History", "MyTasksPaymentHistoryButton", false, 132);
        payment.Click += (_, _) => Nav.Navigate(new PaymentHistoryPage());
        canvas.Controls.Add(payment);
        var find = MakeButton("Find Tasks", "MyTasksFindTasksButton", false, 92);
        find.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        canvas.Controls.Add(find);
        var post = MakeButton("＋ Post a Task", "MyTasksPostTaskButton", true, 110);
        post.Click += (_, _) => Nav.Navigate(Route.PostTask);
        canvas.Controls.Add(post);

        var receivedOpen = DemoDataStore.RequestsReceived.Count(request => !request.Declined && !request.Accepted);
        var completedCount = DemoDataStore.TaskOutcomes.Count(outcome => outcome.Status == "Completed") + tasks.Count(task => task.Tab == "completed");
        var earned = DemoDataStore.Payments.Where(payment => payment.Person == AppSession.CurrentUser.FullName && payment.Status == "Paid").Sum(payment => payment.Amount);
        var stats = new[] { ("▤", DemoDataStore.PostedTasks.Count.ToString(), "Posted tasks", $"{DemoDataStore.PostedTasks.Count} active", Color.FromArgb(232, 244, 255), Blue), ("◷", DemoDataStore.RequestsReceived.Count(request => !request.Declined).ToString(), "Requests", receivedOpen == 0 ? "Nothing needs action" : $"{receivedOpen} need action", Color.FromArgb(255, 241, 230), Color.FromArgb(219, 108, 32)), ("✓", completedCount.ToString(), "Completed", $"₱{earned:N0} earned", Color.FromArgb(232, 248, 241), Color.FromArgb(20, 153, 111)), ("★", "–", "Average rating", "No reviews yet", Color.FromArgb(241, 237, 255), Color.FromArgb(118, 88, 207)) };
        for (var index = 0; index < stats.Length; index++)
        {
            var stat = stats[index];
            var card = new Panel { Name = $"myTasksStat{index + 1}", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var icon = new Label { Text = stat.Item1, Size = new Size(40, 40), Location = new Point(12, 14), BackColor = stat.Item5, ForeColor = stat.Item6, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            card.Controls.Add(icon);
            var value = MakeLabel(stat.Item2, 15, Navy, true); value.Location = new Point(62, 11); card.Controls.Add(value);
            var name = MakeLabel(stat.Item3, 8, Muted); name.Location = new Point(62, 36); card.Controls.Add(name);
            var detail = MakeLabel(stat.Item4, 7, Muted); detail.Location = new Point(12, 61); card.Controls.Add(detail);
            canvas.Controls.Add(card);
        }

        var toolbar = new Panel { Name = "myTasksToolbar", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        canvas.Controls.Add(toolbar);
        var requestShortcut = MakeButton("View My Requests →", "MyTasksRequestsButton", false, 155);
        requestShortcut.Click += (_, _) => Nav.Navigate(new MyRequestsPage());
        toolbar.Controls.Add(requestShortcut);
        var tabTitles = new[] { ("posted", "Posted"), ("requested", "Requested"), ("accepted", "Accepted"), ("progress", "In Progress"), ("completed", "Completed") };
        foreach (var (key, title) in tabTitles)
        {
            var tab = MakeTab(key, title);
            tab.Click += (_, _) => SelectTab(key);
            tabs.Add(key, tab);
            toolbar.Controls.Add(tab);
        }
        search.Name = "myTasksSearch";
        search.PlaceholderText = "⌕  Search your tasks...";
        search.TextChanged += (_, _) => ApplyFilters();
        toolbar.Controls.Add(search);
        categoryFilter.Name = "myTasksCategoryFilter";
        categoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        categoryFilter.Items.Add("All categories");
        categoryFilter.Items.AddRange(DemoDataStore.ActiveCategoryNames());
        categoryFilter.SelectedIndex = 0;
        categoryFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        toolbar.Controls.Add(categoryFilter);
        sortFilter.Name = "myTasksSortFilter";
        sortFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        sortFilter.Items.AddRange(["Most recent", "Deadline soonest", "Highest amount", "Lowest amount"]);
        sortFilter.SelectedIndex = 0;
        sortFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        toolbar.Controls.Add(sortFilter);
        var listView = MakeButton("☷", "MyTasksListViewButton", true, 36);
        listView.Click += (_, _) => { compactView = false; ApplyFilters(); };
        toolbar.Controls.Add(listView);
        var compact = MakeButton("▦", "MyTasksCompactViewButton", false, 36);
        compact.Click += (_, _) => { compactView = true; ApplyFilters(); };
        toolbar.Controls.Add(compact);

        var notice = new Panel { Name = "myTasksNotice", BackColor = Color.FromArgb(249, 252, 255), BorderStyle = BorderStyle.FixedSingle };
        var info = MakeLabel("i", 10, Blue, true); info.Location = new Point(13, 13); notice.Controls.Add(info);
        noticeTitle.Name = "myTasksNoticeTitle"; noticeTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); noticeTitle.ForeColor = Navy; noticeTitle.Location = new Point(40, 9); notice.Controls.Add(noticeTitle);
        noticeBody.Name = "myTasksNoticeBody"; noticeBody.Font = new Font("Segoe UI", 7.5F); noticeBody.ForeColor = Muted; noticeBody.Location = new Point(40, 29); notice.Controls.Add(noticeBody);
        noticeLink.Name = "myTasksNoticeLink"; noticeLink.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold); noticeLink.ForeColor = Blue; noticeLink.Cursor = Cursors.Hand; noticeLink.Click += (_, _) => NoticeAction(); notice.Controls.Add(noticeLink);
        canvas.Controls.Add(notice);

        resultCount.Font = new Font("Segoe UI", 8F, FontStyle.Bold); resultCount.ForeColor = Navy; canvas.Controls.Add(resultCount);
        resultLabel.Font = new Font("Segoe UI", 7.5F); resultLabel.ForeColor = Muted; canvas.Controls.Add(resultLabel);
        var archive = MakeTextButton("View archived tasks →", "MyTasksArchivedButton");
        archive.Click += (_, _) => Nav.Navigate(new ArchivedTasksPage());
        canvas.Controls.Add(archive);
        var updated = MakeLabel("Updated just now", 7, Muted); updated.Name = "myTasksUpdatedLabel"; canvas.Controls.Add(updated);

        taskList.Name = "myTasksList";
        taskList.FlowDirection = FlowDirection.TopDown;
        taskList.WrapContents = false;
        taskList.AutoScroll = false;
        taskList.BackColor = Page;
        canvas.Controls.Add(taskList);
        var empty = new Panel { Name = "myTasksEmptyState", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
        var emptyTitle = MakeLabel("No tasks found", 14, Navy, true); emptyTitle.Location = new Point(20, 25); empty.Controls.Add(emptyTitle);
        var emptyBody = MakeLabel("Try another search, category, or task status.", 9, Muted); emptyBody.Location = new Point(20, 55); empty.Controls.Add(emptyBody);
        var clear = MakeButton("Clear filters", "MyTasksClearFiltersButton", false, 105); clear.Location = new Point(20, 83); clear.Click += (_, _) => ClearFilters(); empty.Controls.Add(clear);
        canvas.Controls.Add(empty);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || scrollHost.ClientSize.Width <= 0)
            return;
        var availableWidth = Math.Max(850, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
        var width = Math.Min(1450, availableWidth);
        canvas.Size = new Size(width, 860);
        canvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);
        const int left = 22;
        Position("myTasksEyebrow", left, 16);
        Position("myTasksHeading", left, 35);
        Position("myTasksSubtitle", left, 84);
        var post = Find<Button>("MyTasksPostTaskButton");
        var find = Find<Button>("MyTasksFindTasksButton");
        var payment = Find<Button>("MyTasksPaymentHistoryButton");
        if (post is not null) post.Location = new Point(width - 22 - post.Width, 16);
        if (find is not null) find.Location = new Point((post?.Left ?? width - 22) - (find.Width + 8), 16);
        if (payment is not null) payment.Location = new Point((find?.Left ?? width - 22) - (payment.Width + 8), 16);
        var gap = 12;
        var statWidth = (width - 44 - gap * 3) / 4;
        for (var index = 0; index < 4; index++)
            Find<Panel>($"myTasksStat{index + 1}")!.Bounds = new Rectangle(left + index * (statWidth + gap), 104, statWidth, 80);
        var toolbar = Find<Panel>("myTasksToolbar")!;
        toolbar.Bounds = new Rectangle(left, 190, width - 44, 110);
        var requestShortcut = toolbar.Controls.Find("MyTasksRequestsButton", false).FirstOrDefault() as Button;
        if (requestShortcut is not null)
            requestShortcut.Location = new Point(toolbar.Width - requestShortcut.Width - 12, 8);
        var tabX = 12;
        foreach (var (key, tab) in tabs)
        {
            tab.Location = new Point(tabX, 39);
            tabX += tab.Width + 2;
        }
        var controlsY = 73;
        search.Location = new Point(12, controlsY); search.Size = new Size(Math.Max(220, toolbar.Width - 420), 30);
        categoryFilter.Location = new Point(search.Right + 8, controlsY); categoryFilter.Size = new Size(130, 30);
        sortFilter.Location = new Point(categoryFilter.Right + 8, controlsY); sortFilter.Size = new Size(136, 30);
        var listViewButton = toolbar.Controls.Find("MyTasksListViewButton", false).FirstOrDefault() as Button;
        if (listViewButton is not null)
            listViewButton.Location = new Point(sortFilter.Right + 8, controlsY - 2);
        var compactViewButton = toolbar.Controls.Find("MyTasksCompactViewButton", false).FirstOrDefault() as Button;
        if (compactViewButton is not null)
            compactViewButton.Location = new Point(sortFilter.Right + 46, controlsY - 2);
        var notice = Find<Panel>("myTasksNotice")!;
        notice.Bounds = new Rectangle(left, 310, width - 44, 55);
        noticeBody.MaximumSize = new Size(notice.Width - 220, 28);
        noticeLink.Location = new Point(notice.Width - 170, 20);
        resultCount.Location = new Point(left + 2, 376);
        resultLabel.Location = new Point(left + 72, 377);
        Find<Button>("MyTasksArchivedButton")!.Location = new Point(width - 248, 371);
        Find<Label>("myTasksUpdatedLabel")!.Location = new Point(width - 112, 374);
        taskList.Location = new Point(left, 398);
        taskList.Size = new Size(width - 44, 445);
        foreach (Control card in taskList.Controls)
            LayoutCard(card, taskList.ClientSize.Width);
        var empty = Find<Panel>("myTasksEmptyState")!;
        empty.Bounds = new Rectangle(left, 398, width - 44, 180);
        canvas.Height = 860;
    }

    private Button MakeTab(string key, string text)
    {
        return new Button { Name = $"myTasksTab{key}", Text = $"{text}  {tabCounts[key]}", Size = new Size(text == "In Progress" ? 106 : 84, 31), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, UseVisualStyleBackColor = false }.Also(button => button.FlatAppearance.BorderSize = 0);
    }

    private void SelectTab(string key)
    {
        currentTab = key;
        foreach (var (tabKey, button) in tabs)
        {
            var active = tabKey == currentTab;
            button.ForeColor = active ? Blue : Muted;
            button.BackColor = active ? Color.FromArgb(239, 247, 255) : Color.White;
        }
        var copy = key switch
        {
            "posted" => ("Posted tasks", "These are tasks you created. Manage requests, edit details, or archive tasks when they are no longer active.", "Create another task →"),
            "requested" => ("Requested tasks", "Tasks where you have asked to help. Watch for the poster to accept or decline your request.", "Find more tasks →"),
            "accepted" => ("Accepted tasks", "Tasks where your request was accepted. Check the schedule and message the poster before starting.", "View task activity →"),
            "progress" => ("Tasks in progress", "Keep active work organized here. Confirm timing, scope, and completion details.", "View task activity →"),
            _ => ("Completed tasks", "Your finished task records and ratings are kept here for reference.", "View task activity →")
        };
        noticeTitle.Text = copy.Item1;
        noticeBody.Text = copy.Item2;
        noticeLink.Text = copy.Item3;
        noticeLink.Tag = currentTab;
        resultLabel.Text = $"in {copy.Item1}";
        ApplyFilters();
    }

    private IEnumerable<TaskRecord> PostedTaskRecords()
    {
        foreach (var posted in DemoDataStore.PostedTasks)
        {
            var requestCount = DemoDataStore.RequestsForTask(posted.Title);
            yield return new TaskRecord(
                posted.Title,
                "Active",
                "posted",
                posted.Category,
                posted.Price,
                posted.Barangay,
                posted.Deadline,
                posted.Description,
                requestCount > 0 ? $"{requestCount} request{(requestCount == 1 ? "" : "s")}" : "No requests yet",
                posted.Icon,
                posted.IconBack,
                requestCount);
        }
    }

    private IEnumerable<TaskRecord> OutcomeRecords() => DemoDataStore.TaskOutcomes
        .Where(outcome => outcome.Status == "Completed")
        .Select(outcome => new TaskRecord(
            outcome.Title,
            outcome.Status,
            "completed",
            outcome.Category,
            outcome.Amount,
            outcome.Barangay,
            outcome.Date.ToString("MMM d, yyyy"),
            outcome.Description,
            $"Completed {outcome.Date:MMM d, yyyy}",
            outcome.Icon,
            outcome.IconBack,
            0));

    private void RefreshTabCounts()
    {
        if (tabs.Count == 0)
            return;

        tabCounts["posted"] = DemoDataStore.PostedTasks.Count;
        tabCounts["requested"] = tasks.Count(task => task.Tab == "requested");
        tabCounts["accepted"] = tasks.Count(task => task.Tab == "accepted");
        tabCounts["progress"] = tasks.Count(task => task.Tab == "progress");
        tabCounts["completed"] = tasks.Count(task => task.Tab == "completed")
                                 + DemoDataStore.TaskOutcomes.Count(outcome => outcome.Status == "Completed");

        foreach (var (key, button) in tabs)
        {
            var title = key switch
            {
                "posted" => "Posted",
                "requested" => "Requested",
                "accepted" => "Accepted",
                "progress" => "In Progress",
                _ => "Completed"
            };
            button.Text = $"{title}  {tabCounts[key]}";
        }
    }

    private void ApplyFilters()
    {
        if (taskList.IsDisposed)
            return;
        RefreshTabCounts();
        var query = search.Text.Trim();
        var category = categoryFilter.SelectedItem?.ToString() ?? "All categories";
        var source = currentTab == "posted" ? PostedTaskRecords().Concat(tasks) : tasks.Concat(OutcomeRecords());
        var filtered = source.Where(task => task.Tab == currentTab)
            .Where(task => category == "All categories" || task.Category == category)
            .Where(task => string.IsNullOrEmpty(query) || $"{task.Title} {task.Category} Brgy. {task.Barangay} {task.Description}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        filtered = sortFilter.SelectedItem?.ToString() switch
        {
            "Deadline soonest" => filtered.OrderBy(task => ParseDate(task.Date)).ToList(),
            "Highest amount" => filtered.OrderByDescending(task => task.Amount).ToList(),
            "Lowest amount" => filtered.OrderBy(task => task.Amount).ToList(),
            _ => filtered
        };
        taskList.SuspendLayout();
        taskList.Controls.Clear();
        foreach (var task in filtered)
            taskList.Controls.Add(CreateTaskCard(task));
        taskList.ResumeLayout();
        resultCount.Text = $"{filtered.Count} task{(filtered.Count == 1 ? "" : "s")}";
        var empty = Find<Panel>("myTasksEmptyState");
        if (empty is not null)
            empty.Visible = filtered.Count == 0;
        if (canvas.Controls.Find("myTasksEyebrow", false).Length > 0)
            LayoutPage();
    }

    private Control CreateTaskCard(TaskRecord task)
    {
        var card = new Panel { Name = "myTaskCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = task };
        var icon = new Label { Name = "myTaskIcon", Text = task.Icon, BackColor = task.IconBack, ForeColor = Navy, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 23F), Location = new Point(12, 12), Size = new Size(62, 62) };
        card.Controls.Add(icon);
        var status = MakeLabel(task.Status.ToUpperInvariant(), 7, task.Status == "Active" ? Color.FromArgb(0, 143, 103) : Blue, true);
        status.Name = "myTaskStatus"; status.Location = new Point(88, 10); card.Controls.Add(status);
        var title = MakeLabel(task.Title, 11, Navy, true); title.Name = "myTaskTitle"; title.Location = new Point(88, 26); card.Controls.Add(title);
        var amount = MakeLabel($"₱{task.Amount:N0}", 10, Navy, true); amount.Name = "myTaskAmount"; amount.Anchor = AnchorStyles.Top | AnchorStyles.Right; card.Controls.Add(amount);
        var description = MakeLabel(task.Description, 8, Muted); description.Name = "myTaskDescription"; description.Location = new Point(88, 48); description.MaximumSize = new Size(950, 25); card.Controls.Add(description);
        var meta = MakeLabel($"{task.Category}  ·  Brgy. {task.Barangay}  ·  Due {task.Date}  ·  {task.Meta}", 7, Muted); meta.Name = "myTaskMeta"; meta.Location = new Point(88, 77); card.Controls.Add(meta);
        var actions = new FlowLayoutPanel { Name = "myTaskActions", Location = new Point(86, 101), Size = new Size(760, 29), WrapContents = false, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight };
        card.Controls.Add(actions);
        if (task.Tab == "posted")
        {
            var editButton = MakeButton("Edit Task", "EditOwnedTaskButton", true, 76);
            editButton.Click += (_, _) =>
            {
                DemoDataStore.EditingTask = DemoDataStore.PostedTasks.FirstOrDefault(t => t.Title == task.Title);
                Nav.Navigate(new EditTaskPage());
            };
            actions.Controls.Add(editButton);
            AddAction(actions, "View Details", false, () => OpenTask(task));
            AddAction(actions, task.Requests == 1 ? "View request  1" : task.Requests > 1 ? $"View requests  {task.Requests}" : task.Status == "Paused" ? "Resume requests" : "View requests", false, () =>
            {
                if (task.Requests > 0)
                {
                    DemoDataStore.ManagedTask = DemoDataStore.PostedTasks.FirstOrDefault(t => t.Title == task.Title);
                    Nav.Navigate(new ManageRequestsPage());
                }
                else
                    ShowToast(task.Status == "Paused" ? "Resume requests from Edit Task." : "No requests yet. Check back soon.");
            });
        }
        else
        {
            AddAction(actions, task.Tab == "requested" ? "View Task" : task.Tab == "completed" ? "View Details" : "Open Task", task.Tab != "completed", () => OpenTask(task));
            AddAction(actions, task.Tab == "requested" ? "View Request" : task.Tab == "completed" ? "View payment / review" : "Message poster", false, () =>
            {
                if (task.Tab == "completed") Nav.Navigate(Route.PaymentHistory);
                else if (task.Tab == "requested") Nav.Navigate(new MyRequestsPage());
                else Nav.Navigate(Route.Messages);
            });
        }
        var favorite = MakeButton("♡", "MyTaskSaveButton", false, 32);
        favorite.FlatAppearance.BorderSize = 0;
        favorite.BackColor = Color.Transparent;
        favorite.ForeColor = Color.FromArgb(130, 150, 172);
        favorite.Location = new Point(0, 10); favorite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        favorite.Text = DemoDataStore.IsSaved(task.Title) ? "♥" : "♡";
        favorite.Click += (_, _) =>
        {
            var savedTask = DemoDataStore.AvailableTasks.Concat(DemoDataStore.PostedTasks).Concat(DemoDataStore.ArchivedTasks)
                .FirstOrDefault(candidate => candidate.Title == task.Title);
            if (savedTask is null)
            {
                ShowToast("This task is no longer available to save.");
                return;
            }
            DemoDataStore.ToggleSaved(savedTask);
            favorite.Text = DemoDataStore.IsSaved(task.Title) ? "♥" : "♡";
            ShowToast(favorite.Text == "♥" ? "Task saved to your favorites." : "Task removed from your favorites.");
        };
        card.Controls.Add(favorite);
        LayoutCard(card, Math.Max(500, taskList.ClientSize.Width));
        return card;
    }

    private void LayoutCard(Control card, int width)
    {
        if (card is not Panel panel || panel.Tag is not TaskRecord)
            return;
        panel.Width = Math.Max(500, width - 4);
        panel.Height = compactView ? 106 : 139;
        var amount = panel.Controls.Find("myTaskAmount", false).FirstOrDefault();
        var favorite = panel.Controls.Find("MyTaskSaveButton", false).FirstOrDefault();
        if (amount is not null) amount.Location = new Point(panel.Width - 96, 12);
        if (favorite is not null) favorite.Location = new Point(panel.Width - 44, 10);
        var description = panel.Controls.Find("myTaskDescription", false).FirstOrDefault();
        if (description is not null) description.Visible = !compactView;
        var actions = panel.Controls.Find("myTaskActions", false).FirstOrDefault();
        if (actions is not null) actions.Location = new Point(86, compactView ? 68 : 101);
    }

    private void OpenTask(TaskRecord task)
    {
        var match = DemoDataStore.AvailableTasks.FirstOrDefault(t => t.Title == task.Title)
                    ?? DemoDataStore.PostedTasks.FirstOrDefault(t => t.Title == task.Title);
        if (match is not null)
            Nav.Navigate(new UserTaskDetailsPage(match.Title, match.Category, match.Price, match.Barangay, match.Days, match.Deadline, match.Icon, match.Description, match.IconBack, match.Id));
        else if (task.Tab == "requested")
            Nav.Navigate(new MyRequestsPage());
        else
            Nav.Navigate(Route.FindTasks);
    }

    private void AddAction(FlowLayoutPanel panel, string text, bool primary, Action action)
    {
        var button = MakeButton(text, "MyTaskActionButton", primary, Math.Max(74, text.Length * 6 + 18));
        button.Height = 26;
        button.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void NoticeAction()
    {
        var tab = noticeLink.Tag as string;
        if (tab is "posted" or "requested")
            Nav.Navigate(tab == "posted" ? Route.PostTask : Route.FindTasks);
        else
            ShowToast("Open the task to see its latest activity.");
    }

    private void ClearFilters()
    {
        search.Clear();
        categoryFilter.SelectedIndex = 0;
        sortFilter.SelectedIndex = 0;
    }

    private void AddLabel(string name, string text, float size, Color color, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold);
        label.Name = name;
        canvas.Controls.Add(label);
    }

    private void Position(string name, int x, int y)
    {
        var control = canvas.Controls.Find(name, false).FirstOrDefault();
        if (control is not null) control.Location = new Point(x, y);
    }

    private TControl? Find<TControl>(string name) where TControl : Control => canvas.Controls.Find(name, false).FirstOrDefault() as TControl;

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop(); toastTimer.Start();
    }

    private static DateTime ParseDate(string text) => DateTime.TryParse(text, out var date) ? date : DateTime.MaxValue;

    private static Button MakeButton(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 34), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button MakeTab(string text) => new() { Text = text, FlatStyle = FlatStyle.Flat };

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}

internal static class ControlExtensions
{
    public static T Also<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
