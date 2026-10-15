namespace TaskTap
{
    public sealed class MyTasksForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(8, 38, 87);
        private static readonly Color SidebarNavy = Color.FromArgb(8, 35, 77);
        private static readonly Color Blue = Color.FromArgb(8, 117, 232);
        private static readonly Color Muted = Color.FromArgb(113, 131, 155);
        private static readonly Color Page = Color.FromArgb(244, 248, 252);
        private static readonly Color Line = Color.FromArgb(225, 233, 241);
        private readonly Action? returnToParent;
        private readonly Panel sidebar = new();
        private readonly Panel main = new();
        private readonly Panel topbar = new();
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

        private sealed record TaskRecord(string Title, string Status, string Tab, string Category, int Amount, string Barangay, string Date, string Description, string Meta, string Icon, Color IconBack, int Requests = 0);

        public MyTasksForm(Action? returnToParent = null)
        {
            this.returnToParent = returnToParent;
            SeedTasks();
            BuildPage();
        }

        private void SeedTasks()
        {
            tasks.AddRange([
                new("Pet Care", "Pending request", "requested", "Pet Care", 400, "176", "Oct 12, 2026", "Requested to help look after a small dog for an afternoon while the owner is away.", "Requested Oct 5  ·  Request pending", "🐾", Color.FromArgb(255, 241, 224)),
                new("Grocery Shopping", "Accepted", "accepted", "Shopping", 450, "179", "Oct 9, 2026", "Pick up the provided grocery list and deliver the items to the task poster after shopping.", "Request accepted", "🛍", Color.FromArgb(231, 248, 241)),
                new("Apartment Cleaning", "In progress", "progress", "Cleaning", 550, "178", "Oct 11, 2026", "Cleaning support for a small apartment. The task was accepted and is scheduled for this weekend.", "Accepted by you", "🧽", Color.FromArgb(234, 245, 255)),
                new("Document Delivery", "Completed", "completed", "Delivery", 350, "176", "Sep 29, 2026", "Delivered documents to the requested address and confirmed receipt with the task poster.", "Completed Sep 29  ·  5.0 rating", "⌂", Color.FromArgb(237, 242, 248)),
                new("Weekly Market Run", "Completed", "completed", "Shopping", 400, "179", "Sep 22, 2026", "Completed a weekly market shopping run and handed over all items with the receipt.", "Completed Sep 22  ·  4.9 rating", "🛒", Color.FromArgb(231, 248, 241)),
                new("Fix a leaking faucet", "Active", "posted", "Repairs", 600, "176", "Oct 8, 2026", "Need help fixing a leaking kitchen faucet. Please bring basic plumbing tools and check the connection under the sink.", "3 requests", "🔧", Color.FromArgb(255, 240, 230), 3),
                new("House Cleaning", "Active", "posted", "Cleaning", 500, "176", "Oct 7, 2026", "Two-bedroom apartment cleaning before the weekend. Basic supplies are available on site.", "1 request", "🧹", Color.FromArgb(234, 247, 255), 1),
                new("Math Tutor (Grade 10)", "Paused", "posted", "Tutoring", 300, "178", "Oct 10, 2026", "Looking for a patient tutor for a Grade 10 math review session focused on algebra and problem solving.", "Requests paused", "∑", Color.FromArgb(241, 237, 255))
            ]);
        }

        private void BuildPage()
        {
            Text = "TaskTap — My Tasks";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1120, 720);
            ClientSize = new Size(1540, 900);
            BackColor = Page;
            Font = new Font("Segoe UI", 9F);
            main.Dock = DockStyle.Fill;
            main.BackColor = Page;
            Controls.Add(main);
            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = Page;
            scrollHost.Resize += (_, _) => LayoutPage();
            main.Controls.Add(scrollHost);
            BuildSidebar();
            main.Controls.Add(sidebar);
            canvas.BackColor = Page;
            scrollHost.Controls.Add(canvas);
            BuildContent();
            BuildTopbar();
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

        private void BuildSidebar()
        {
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 220;
            sidebar.BackColor = SidebarNavy;
            var brand = MakeLabel("◉ TaskTap", 18, Color.White, true);
            brand.Location = new Point(22, 20);
            sidebar.Controls.Add(brand);
            var items = new[] { "Dashboard", "Find Tasks", "Post Task", "My Tasks", "Messages", "Notifications", "Profile" };
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                var button = MakeNavButton(item, item == "My Tasks");
                button.Location = new Point(12, 76 + index * 43);
                button.Click += (_, _) => Navigate(item);
                sidebar.Controls.Add(button);
                if (item is "Messages" or "Notifications")
                {
                    var badge = MakeLabel(item == "Messages" ? "3" : "5", 8, Color.White, true);
                    badge.AutoSize = false;
                    badge.Size = new Size(20, 20);
                    badge.TextAlign = ContentAlignment.MiddleCenter;
                    badge.BackColor = Color.FromArgb(255, 132, 77);
                    badge.Location = new Point(178, 85 + index * 43);
                    sidebar.Controls.Add(badge);
                }
            }
            var account = MakeLabel("ACCOUNT", 8, Color.FromArgb(126, 159, 198), true);
            account.Location = new Point(22, 393);
            sidebar.Controls.Add(account);
            var logout = MakeNavButton("Log out", false);
            logout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            sidebar.Controls.Add(logout);
            logout.Click += (_, _) => Close();
            sidebar.Resize += (_, _) => logout.Location = new Point(12, sidebar.ClientSize.Height - logout.Height - 14);
        }

        private void BuildTopbar()
        {
            topbar.Dock = DockStyle.Top;
            topbar.Height = 58;
            topbar.BackColor = Color.White;
            topbar.Paint += (_, e) => { using var pen = new Pen(Line); e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1); };
            var topSearch = new TextBox { PlaceholderText = "⌕  Search tasks...", Location = new Point(22, 11), Size = new Size(310, 34) };
            topbar.Controls.Add(topSearch);
            var searchButton = MakeSmallHeaderButton("♧  •", "MyTasksNotificationButton", 44, 36);
            searchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchButton.Location = new Point(topbar.Width - 245, 10);
            searchButton.Click += (_, _) => ShowToast("Notifications are sample content in this frontend demo.");
            topbar.Controls.Add(searchButton);
            var avatar = new Label { Text = "JD", Location = new Point(topbar.Width - 190, 11), Size = new Size(34, 34), BackColor = Color.FromArgb(224, 240, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            topbar.Controls.Add(avatar);
            var user = MakeLabel("Joshua D. Pineda\r\nStandard User", 8, Navy, true);
            user.Location = new Point(topbar.Width - 148, 11);
            user.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topbar.Controls.Add(user);
            topbar.Resize += (_, _) => { searchButton.Location = new Point(topbar.ClientSize.Width - 245, 10); avatar.Location = new Point(topbar.ClientSize.Width - 190, 11); user.Location = new Point(topbar.ClientSize.Width - 148, 11); };
            main.Controls.Add(topbar);
        }

        private void BuildContent()
        {
            AddLabel("myTasksEyebrow", "TASK MANAGEMENT", 8, Blue, true);
            AddLabel("myTasksHeading", "My Tasks", 25, Navy, true);
            AddLabel("myTasksSubtitle", "Keep track of tasks you posted and tasks you’re helping with.", 9, Muted);
            var payment = MakeButton("₱  Payment History", "MyTasksPaymentHistoryButton", false, 132);
            payment.Click += (_, _) => ShowToast("Payment history is a prototype-only destination.");
            canvas.Controls.Add(payment);
            var find = MakeButton("Find Tasks", "MyTasksFindTasksButton", false, 92);
            find.Click += (_, _) => Navigate("Find Tasks");
            canvas.Controls.Add(find);
            var post = MakeButton("＋ Post a Task", "MyTasksPostTaskButton", true, 110);
            post.Click += (_, _) => Navigate("Post Task");
            canvas.Controls.Add(post);

            var stats = new[] { ("▤", "6", "Posted tasks", "2 active", Color.FromArgb(232, 244, 255), Blue), ("◷", "3", "Requests", "1 needs action", Color.FromArgb(255, 241, 230), Color.FromArgb(219, 108, 32)), ("✓", "8", "Completed", "₱4,250 earned", Color.FromArgb(232, 248, 241), Color.FromArgb(20, 153, 111)), ("★", "4.9", "Average rating", "16 reviews", Color.FromArgb(241, 237, 255), Color.FromArgb(118, 88, 207)) };
            for (var index = 0; index < stats.Length; index++)
            {
                var stat = stats[index];
                var card = new Panel { Name = $"myTasksStat{index + 1}", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                var icon = new Label { Text = stat.Item1, Size = new Size(40, 40), Location = new Point(12, 14), BackColor = stat.Item5, ForeColor = stat.Item6, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
                card.Controls.Add(icon);
                var value = MakeLabel(stat.Item2, 15, Navy, true); value.Location = new Point(62, 11); card.Controls.Add(value);
                    var name = MakeLabel(stat.Item3, 8, Muted); name.Location = new Point(62, 34); card.Controls.Add(name);
                    var detail = MakeLabel(stat.Item4, 7, Muted); detail.Location = new Point(62, 57); card.Controls.Add(detail);
                canvas.Controls.Add(card);
            }

            var toolbar = new Panel { Name = "myTasksToolbar", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            canvas.Controls.Add(toolbar);
            var requestShortcut = MakeButton("View My Requests →", "MyTasksRequestsButton", false, 155);
            requestShortcut.Click += (_, _) => SelectTab("requested");
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
            categoryFilter.Items.AddRange(["All categories", "Cleaning", "Tutoring", "Repairs", "Shopping", "Pet Care", "Delivery"]);
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
            archive.Click += (_, _) => ShowToast("Archived tasks are not included in this sample listing.");
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
            Position("myTasksHeading", left, 36);
            Position("myTasksSubtitle", left, 76);
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
            toolbar.Bounds = new Rectangle(left, 198, width - 44, 116);
            var requestShortcut = toolbar.Controls.Find("MyTasksRequestsButton", false).FirstOrDefault() as Button;
            if (requestShortcut is not null)
                requestShortcut.Location = new Point(toolbar.Width - requestShortcut.Width - 12, 8);
            var tabX = 12;
            foreach (var (key, tab) in tabs)
            {
                tab.Location = new Point(tabX, 40);
                tabX += tab.Width + 2;
            }
            var controlsY = 78;
            var buttonsWidth = 36 * 2 + 8;
            var filterWidth = 130 + 136 + 16;
            var searchWidth = Math.Max(180, toolbar.Width - filterWidth - buttonsWidth - 52);
            search.Location = new Point(12, controlsY); search.Size = new Size(searchWidth, 30);
            categoryFilter.Location = new Point(search.Right + 8, controlsY); categoryFilter.Size = new Size(130, 30);
            sortFilter.Location = new Point(categoryFilter.Right + 8, controlsY); sortFilter.Size = new Size(136, 30);
            var listViewButton = toolbar.Controls.Find("MyTasksListViewButton", false).FirstOrDefault() as Button;
            if (listViewButton is not null)
                listViewButton.Location = new Point(sortFilter.Right + 8, controlsY - 2);
            var compactViewButton = toolbar.Controls.Find("MyTasksCompactViewButton", false).FirstOrDefault() as Button;
            if (compactViewButton is not null)
                compactViewButton.Location = new Point(sortFilter.Right + 46, controlsY - 2);
            var notice = Find<Panel>("myTasksNotice")!;
            notice.Bounds = new Rectangle(left, 322, width - 44, 55);
            noticeBody.MaximumSize = new Size(notice.Width - 220, 28);
            noticeLink.Location = new Point(notice.Width - 170, 20);
            resultCount.Location = new Point(left + 2, 389);
            resultLabel.Location = new Point(left + 72, 390);
            Find<Button>("MyTasksArchivedButton")!.Location = new Point(width - 248, 384);
            Find<Label>("myTasksUpdatedLabel")!.Location = new Point(width - 112, 387);
            taskList.Location = new Point(left, 411);
            taskList.Size = new Size(width - 44, 430);
            foreach (Control card in taskList.Controls)
                LayoutCard(card, taskList.ClientSize.Width);
            var empty = Find<Panel>("myTasksEmptyState")!;
            empty.Bounds = new Rectangle(left, 411, width - 44, 180);
            canvas.Height = 880;
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

        private void ApplyFilters()
        {
            if (taskList.IsDisposed)
                return;
            var query = search.Text.Trim();
            var category = categoryFilter.SelectedItem?.ToString() ?? "All categories";
            var filtered = tasks.Where(task => task.Tab == currentTab)
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
            var description = MakeLabel(task.Description, 8, Muted); description.Name = "myTaskDescription"; description.Location = new Point(88, 52); description.MaximumSize = new Size(950, 32); card.Controls.Add(description);
            var meta = MakeLabel($"{task.Category}  ·  Brgy. {task.Barangay}  ·  Due {task.Date}  ·  {task.Meta}", 7, Muted); meta.Name = "myTaskMeta"; meta.Location = new Point(88, 87); card.Controls.Add(meta);
            var actions = new FlowLayoutPanel { Name = "myTaskActions", Location = new Point(86, 111), Size = new Size(760, 29), WrapContents = false, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight };
            card.Controls.Add(actions);
            if (task.Tab == "posted")
            {
                if (task.Title == "Fix a leaking faucet")
                {
                    var edit = MakeButton("Edit Task", "EditOwnedTaskButton", true, 76);
                    edit.Click += (_, _) => OpenEditTask();
                    actions.Controls.Add(edit);
                }
                else
                {
                    var edit = MakeButton("Edit Task", "EditTaskUnavailableButton", true, 76);
                    edit.Click += (_, _) => ShowToast("This sample task is display-only in the frontend demo.");
                    actions.Controls.Add(edit);
                }
                AddAction(actions, "View Details", false, () => ShowToast("Task details are sample content in this frontend demo."));
                AddAction(actions, task.Requests == 1 ? "View request  1" : task.Requests > 1 ? $"View requests  {task.Requests}" : task.Status == "Paused" ? "Resume requests" : "View requests", false, () =>
                {
                    if (task.Title == "Fix a leaking faucet" && task.Requests > 0)
                        OpenManageRequests();
                    else
                        ShowToast(task.Status == "Paused" ? "Resume requests from Edit Task." : "Request management is sample content in this frontend demo.");
                });
            }
            else
            {
                AddAction(actions, task.Tab == "requested" ? "View Task" : task.Tab == "completed" ? "View Details" : "Open Task", task.Tab != "completed", () => ShowToast("Task details are sample content in this frontend demo."));
                AddAction(actions, task.Tab == "requested" ? "View Request" : task.Tab == "completed" ? "View payment / review" : "Message poster", false, () => ShowToast("This action is sample content in the frontend demo."));
            }
            var favorite = MakeButton("♡", "MyTaskSaveButton", false, 32);
            favorite.Location = new Point(0, 10); favorite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            favorite.Click += (_, _) => { favorite.Text = favorite.Text == "♡" ? "♥" : "♡"; ShowToast(favorite.Text == "♥" ? "Task saved to your favorites." : "Task removed from your favorites."); };
            card.Controls.Add(favorite);
            LayoutCard(card, Math.Max(500, taskList.ClientSize.Width));
            return card;
        }

        private void LayoutCard(Control card, int width)
        {
            if (card is not Panel panel || panel.Tag is not TaskRecord)
                return;
            panel.Width = Math.Max(260, width - taskList.Padding.Horizontal - panel.Margin.Horizontal - 4);
            panel.Height = compactView ? 108 : 154;
            var amount = panel.Controls.Find("myTaskAmount", false).FirstOrDefault();
            var favorite = panel.Controls.Find("MyTaskSaveButton", false).FirstOrDefault();
            if (amount is not null) amount.Location = new Point(panel.Width - 84, 12);
            if (favorite is not null) favorite.Location = new Point(panel.Width - 44, 10);
            var description = panel.Controls.Find("myTaskDescription", false).FirstOrDefault();
            if (description is not null)
            {
                description.Visible = !compactView;
                description.MaximumSize = new Size(Math.Max(140, panel.Width - 260), 32);
            }
            var title = panel.Controls.Find("myTaskTitle", false).FirstOrDefault();
            if (title is not null)
                title.MaximumSize = new Size(Math.Max(140, panel.Width - 180), 24);
            var meta = panel.Controls.Find("myTaskMeta", false).FirstOrDefault();
            if (meta is not null)
            {
                meta.Location = new Point(88, compactView ? 52 : 88);
                meta.MaximumSize = new Size(Math.Max(140, panel.Width - 260), 18);
            }
            var actions = panel.Controls.Find("myTaskActions", false).FirstOrDefault();
            if (actions is not null) actions.Location = new Point(86, compactView ? 72 : 116);
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
                Navigate(tab == "posted" ? "Post Task" : "Find Tasks");
            else
                ShowToast("Task activity is sample content in this frontend demo.");
        }

        private void ClearFilters()
        {
            search.Clear();
            categoryFilter.SelectedIndex = 0;
            sortFilter.SelectedIndex = 0;
        }

        private void Navigate(string destination)
        {
            if (destination == "My Tasks")
                return;
            if (destination == "Dashboard")
            {
                var dashboard = new UserDashboardForm();
                dashboard.FormClosed += (_, _) => Close();
                Hide(); dashboard.Show(); return;
            }
            if (destination == "Find Tasks")
            {
                var find = new FindTasksForm();
                find.FormClosed += (_, _) => Close();
                Hide(); find.Show(); return;
            }
            if (destination == "Post Task")
            {
                Hide();
                var post = new PostTaskForm(() => { if (!IsDisposed) { Show(); Activate(); } });
                post.Show(); return;
            }
            ShowToast($"{destination} is not implemented yet in the native User section.");
        }

        private void OpenEditTask()
        {
            Hide();
            var edit = new EditTaskForm(() => { if (!IsDisposed) { Show(); Activate(); } });
            edit.Show();
        }

        private void OpenManageRequests()
        {
            Hide();
            var manageRequests = new ManageRequestsForm(() => { if (!IsDisposed) { Show(); Activate(); } });
            manageRequests.Show();
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

        private static Button MakeNavButton(string text, bool active)
        {
            var glyph = text switch { "Dashboard" => "⌂", "Find Tasks" => "⌕", "Post Task" => "＋", "My Tasks" => "▤", "Messages" => "▱", "Notifications" => "♧", "Profile" => "◉", _ => "↪" };
            var button = new Button { Text = $"{glyph}   {text}", Size = new Size(196, 39), FlatStyle = FlatStyle.Flat, BackColor = active ? Color.FromArgb(31, 63, 108) : Color.Transparent, ForeColor = Color.FromArgb(222, 234, 250), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

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

        private static Button MakeSmallHeaderButton(string text, string name, int width, int height)
        {
            var button = new Button { Name = name, Text = text, Size = new Size(width, height), FlatStyle = FlatStyle.Flat, BackColor = Page, ForeColor = Muted, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button MakeTab(string text) => new() { Text = text, FlatStyle = FlatStyle.Flat };

        private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            returnToParent?.Invoke();
            base.OnFormClosed(e);
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
}
