namespace TaskTap
{
    public sealed class ManageRequestsForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(8, 38, 87);
        private static readonly Color SidebarNavy = Color.FromArgb(8, 35, 77);
        private static readonly Color Blue = Color.FromArgb(8, 117, 232);
        private static readonly Color Muted = Color.FromArgb(104, 121, 144);
        private static readonly Color Page = Color.FromArgb(244, 248, 252);
        private static readonly Color Line = Color.FromArgb(226, 234, 242);
        private readonly Action? returnToParent;
        private readonly Panel sidebar = new();
        private readonly Panel main = new();
        private readonly Panel topbar = new();
        private readonly Panel page = new();
        private readonly Panel contextCard = new();
        private readonly Panel requestsToolbar = new();
        private readonly Panel decisionPanel = new();
        private readonly FlowLayoutPanel requestList = new();
        private readonly TextBox requestSearch = new();
        private readonly ComboBox requestSort = new();
        private readonly Label requestCount = new();
        private readonly Label selectedContent = new();
        private readonly Button acceptButton = new();
        private Button confirmButton = null!;
        private readonly Button clearSelectionButton = new();
        private readonly Panel modal = new();
        private readonly Label toast = new();
        private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
        private readonly List<RequestRecord> requests = [];
        private readonly Dictionary<string, Button> filters = [];
        private readonly Dictionary<RequestRecord, Panel> cards = [];
        private string currentFilter = "all";
        private RequestRecord? selected;
        private bool pageBuilt;
        private bool accepted;

        private sealed class RequestRecord(string name, string initials, double rating, int reviews, int completed, string requestedAt, string message, string[] skills, string barangay, string memberSince, string state, DateTime date, Color avatarColor)
        {
            public string Name { get; } = name;
            public string Initials { get; } = initials;
            public double Rating { get; } = rating;
            public int Reviews { get; } = reviews;
            public int Completed { get; } = completed;
            public string RequestedAt { get; } = requestedAt;
            public string Message { get; } = message;
            public string[] Skills { get; } = skills;
            public string Barangay { get; } = barangay;
            public string MemberSince { get; } = memberSince;
            public DateTime Date { get; } = date;
            public Color AvatarColor { get; } = avatarColor;
            public bool Shortlisted { get; set; } = state == "shortlisted";
            public bool Declined { get; set; }
        }

        public ManageRequestsForm(Action? returnToParent = null)
        {
            this.returnToParent = returnToParent;
            SeedRequests();
            BuildPage();
        }

        private void SeedRequests()
        {
            requests.AddRange([
                new("Maria Santos", "MS", 4.9, 18, 24, "Today, 7:18 PM", "I can fix the faucet and check the connection underneath the sink. I have basic plumbing tools and can come tomorrow morning.", ["Basic plumbing", "Home repairs", "Reliable"], "176", "2025", "new", new DateTime(2026, 10, 6), Color.FromArgb(109, 131, 215)),
                new("Juan Dela Cruz", "JD", 4.8, 12, 19, "Yesterday, 9:42 PM", "I’ve handled small plumbing and maintenance jobs before. Happy to inspect the leak first and explain what needs to be fixed.", ["Maintenance", "Plumbing", "Home service"], "179", "2025", "shortlisted", new DateTime(2026, 10, 5, 21, 42, 0), Color.FromArgb(10, 154, 115)),
                new("Angela Reyes", "AR", 4.7, 9, 15, "Oct 5, 6:31 PM", "I can help with the repair and am available on Oct 8. I’ve done several home improvement tasks through TaskTap.", ["Home improvement", "Repairs", "On time"], "178", "2026", "new", new DateTime(2026, 10, 5, 18, 31, 0), Color.FromArgb(223, 129, 80))
            ]);
        }

        private void BuildPage()
        {
            Text = "TaskTap — Manage Requests";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1120, 720);
            ClientSize = new Size(1540, 900);
            BackColor = Page;
            Font = new Font("Segoe UI", 9F);
            main.Dock = DockStyle.Fill;
            main.BackColor = Page;
            Controls.Add(main);

            BuildSidebar();
            page.Dock = DockStyle.Fill;
            page.BackColor = Page;
            page.Padding = new Padding(22, 14, 22, 18);
            page.AutoScroll = true;
            main.Controls.Add(page);
            BuildTopbar();
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
            ApplyRequests();
            LayoutPage();
        }

        private void BuildSidebar()
        {
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 198;
            sidebar.BackColor = SidebarNavy;
            var brand = MakeLabel("◉ TaskTap", 17, Color.White, true);
            brand.Location = new Point(20, 18);
            sidebar.Controls.Add(brand);
            var items = new[] { "Dashboard", "Find Tasks", "Post Task", "My Tasks", "Messages", "Notifications", "Profile" };
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                var button = MakeNavButton(item, item == "My Tasks");
                button.Location = new Point(10, 66 + index * 42);
                button.Click += (_, _) => Navigate(item);
                sidebar.Controls.Add(button);
                if (item is "Messages" or "Notifications" or "My Tasks")
                {
                    var badge = MakeLabel(item == "Messages" ? "3" : item == "Notifications" ? "5" : "4", 7, Color.White, true);
                    badge.AutoSize = false;
                    badge.Size = new Size(18, 18);
                    badge.TextAlign = ContentAlignment.MiddleCenter;
                    badge.BackColor = Color.FromArgb(255, 132, 77);
                    badge.Location = new Point(166, 76 + index * 42);
                    sidebar.Controls.Add(badge);
                }
            }
            var account = MakeLabel("ACCOUNT", 7, Color.FromArgb(126, 159, 198), true);
            account.Location = new Point(20, 362);
            sidebar.Controls.Add(account);
            var logout = MakeNavButton("Log out", false);
            logout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            logout.Location = new Point(10, 0);
            logout.Click += (_, _) => Close();
            sidebar.Controls.Add(logout);
            sidebar.Resize += (_, _) => logout.Location = new Point(10, sidebar.ClientSize.Height - logout.Height - 12);
            Controls.Add(sidebar);
        }

        private void BuildTopbar()
        {
            topbar.Dock = DockStyle.Top;
            topbar.Height = 58;
            topbar.BackColor = Color.White;
            topbar.Paint += (_, e) => { using var pen = new Pen(Line); e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1); };
            topbar.Controls.Add(new TextBox { PlaceholderText = "⌕  Search tasks...", Location = new Point(20, 11), Size = new Size(300, 34) });
            var notification = new Button { Text = "♧  •", Size = new Size(42, 36), Location = new Point(0, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = Page, ForeColor = Muted, UseVisualStyleBackColor = false };
            notification.FlatAppearance.BorderSize = 0;
            notification.Click += (_, _) => ShowToast("Notifications are sample content in this frontend demo.");
            topbar.Controls.Add(notification);
            var avatar = new Label { Text = "JD", Size = new Size(32, 32), Location = new Point(0, 11), Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(224, 240, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            topbar.Controls.Add(avatar);
            var user = MakeLabel("Joshua D. Pineda\r\nStandard User", 7.5F, Navy, true);
            user.Location = new Point(0, 12);
            user.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topbar.Controls.Add(user);
            topbar.Resize += (_, _) => { notification.Left = topbar.ClientSize.Width - 235; avatar.Left = topbar.ClientSize.Width - 185; user.Left = topbar.ClientSize.Width - 145; };
            main.Controls.Add(topbar);
            topbar.BringToFront();
        }

        private void BuildContent()
        {
            var crumb = MakeLabel("← My Tasks   /   Manage Requests", 8, Muted, true);
            crumb.Location = new Point(0, 0);
            crumb.Cursor = Cursors.Hand;
            crumb.Click += (_, _) => Close();
            page.Controls.Add(crumb);
            page.Controls.Add(MakeLabel("TASK MANAGEMENT", 7, Blue, true).At(0, 26));
            page.Controls.Add(MakeLabel("Manage Requests", 24, Navy, true).At(0, 42));
            page.Controls.Add(MakeLabel("Review people who want to perform your task and choose the best fit.", 9, Muted).At(0, 77));
            var viewTask = MakeButton("View Task", "ManageRequestsViewTaskButton", false, 84);
            viewTask.Location = new Point(0, 46);
            viewTask.Click += (_, _) => ShowToast("Task details are sample content in this frontend demo.");
            page.Controls.Add(viewTask);
            var editTask = MakeButton("Edit Task", "ManageRequestsEditTaskButton", true, 88);
            editTask.Location = new Point(0, 46);
            editTask.Click += (_, _) => OpenEditTask();
            page.Controls.Add(editTask);

            BuildTaskSummary();
            BuildRequestToolbar();
            BuildDecisionPanel();
            BuildRequestList();
            BuildSafetyStrip();
            BuildDecisionModal();
        }

        private void BuildTaskSummary()
        {
            contextCard.Name = "manageRequestsTaskSummary";
            contextCard.BackColor = Color.White;
            contextCard.BorderStyle = BorderStyle.FixedSingle;
            var icon = new Label { Text = "🔧", Size = new Size(66, 66), Location = new Point(12, 12), BackColor = Color.FromArgb(255, 240, 230), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 24F) };
            contextCard.Controls.Add(icon);
            var status = MakeLabel("ACTIVE  ·  3 REQUESTS", 7, Color.FromArgb(10, 141, 102), true); status.Location = new Point(90, 10); contextCard.Controls.Add(status);
            var title = MakeLabel("Fix a leaking faucet", 13, Navy, true); title.Location = new Point(90, 26); contextCard.Controls.Add(title);
            var description = MakeLabel("Need help fixing a leaking kitchen faucet. Please bring basic plumbing tools and check the connection under the sink.", 8, Muted); description.Location = new Point(90, 48); description.MaximumSize = new Size(800, 28); contextCard.Controls.Add(description);
            var meta = MakeLabel("⚒ Repairs    ·    ⌖ Brgy. 176    ·    ◷ Due Oct 8, 2026", 7, Muted); meta.Location = new Point(90, 80); contextCard.Controls.Add(meta);
            var amount = MakeLabel("₱600", 10, Navy, true); amount.Name = "ManageRequestsTaskAmount"; contextCard.Controls.Add(amount);
            var separator = new Panel { BackColor = Line, Width = 1 }; contextCard.Controls.Add(separator);
            var count = MakeLabel("3", 17, Navy, true); count.Name = "ManageRequestsSummaryCount"; contextCard.Controls.Add(count);
            var received = MakeLabel("requests received", 7, Muted); contextCard.Controls.Add(received);
            var progressBack = new Panel { BackColor = Color.FromArgb(237, 242, 247), Height = 4 }; contextCard.Controls.Add(progressBack);
            var progress = new Panel { BackColor = Blue, Height = 4, Width = 72 }; progressBack.Controls.Add(progress);
            contextCard.Controls.Add(MakeLabel("Choose one performer", 6.5F, Muted));
            page.Controls.Add(contextCard);
        }

        private void BuildRequestToolbar()
        {
            requestsToolbar.Name = "manageRequestsToolbar";
            requestsToolbar.BackColor = Color.White;
            requestsToolbar.BorderStyle = BorderStyle.FixedSingle;
            var countLine = MakeLabel("3 requests", 8, Navy, true); countLine.Name = "ManageRequestsCountLabel"; countLine.Location = new Point(12, 10); requestsToolbar.Controls.Add(countLine);
            requestsToolbar.Controls.Add(MakeLabel("· newest first", 7, Muted).At(86, 11));
            requestSort.Name = "ManageRequestsSort";
            requestSort.DropDownStyle = ComboBoxStyle.DropDownList;
            requestSort.Items.AddRange(["Recommended", "Highest rating", "Most recent"]);
            requestSort.SelectedIndex = 0;
            requestSort.SelectedIndexChanged += (_, _) => ApplyRequests();
            requestsToolbar.Controls.Add(requestSort);
            requestSearch.Name = "ManageRequestsSearch";
            requestSearch.PlaceholderText = "⌕  Search applicants...";
            requestSearch.TextChanged += (_, _) => ApplyRequests();
            requestsToolbar.Controls.Add(requestSearch);
            foreach (var (key, label, count) in new[] { ("all", "All", "3"), ("new", "New", "2"), ("shortlisted", "Shortlisted", "1") })
            {
                var filter = new Button { Name = $"ManageRequestsFilter{key}", Text = $"{label}  {count}", Size = new Size(key == "shortlisted" ? 105 : 68, 30), FlatStyle = FlatStyle.Flat, BackColor = key == "all" ? Color.FromArgb(237, 245, 253) : Color.White, ForeColor = key == "all" ? Blue : Muted, Font = new Font("Segoe UI", 7F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
                filter.FlatAppearance.BorderColor = Line;
                filter.Click += (_, _) => SelectFilter(key);
                filters.Add(key, filter);
                requestsToolbar.Controls.Add(filter);
            }
            page.Controls.Add(requestsToolbar);
        }

        private void BuildDecisionPanel()
        {
            decisionPanel.Name = "manageRequestsDecisionPanel";
            decisionPanel.BackColor = Color.White;
            decisionPanel.BorderStyle = BorderStyle.FixedSingle;
            var eyebrow = MakeLabel("SELECTION", 7, Blue, true); eyebrow.Location = new Point(12, 12); decisionPanel.Controls.Add(eyebrow);
            var title = MakeLabel("Choose a performer", 11, Navy, true); title.Location = new Point(12, 29); decisionPanel.Controls.Add(title);
            var count = new Label { Name = "ManageRequestsSelectedCount", Text = "1", Size = new Size(24, 24), Location = new Point(0, 10), BackColor = Color.FromArgb(234, 244, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) }; decisionPanel.Controls.Add(count);
            var body = MakeLabel("Review profiles and messages before accepting a request. Once accepted, no other applicant can be selected for this task.", 7, Muted); body.Name = "ManageRequestsPanelCopy"; body.Location = new Point(12, 52); body.MaximumSize = new Size(245, 55); decisionPanel.Controls.Add(body);
            var selectedBox = new Panel { Name = "ManageRequestsSelectedBox", BackColor = Color.FromArgb(251, 253, 255), BorderStyle = BorderStyle.FixedSingle };
            var emptyIcon = new Label { Text = "◎", Size = new Size(34, 34), BackColor = Color.FromArgb(237, 244, 251), ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F, FontStyle.Bold) }; emptyIcon.Name = "ManageRequestsSelectionIcon"; selectedBox.Controls.Add(emptyIcon);
            var emptyTitle = MakeLabel("No one selected yet", 8, Navy, true); emptyTitle.Name = "ManageRequestsSelectionTitle"; selectedBox.Controls.Add(emptyTitle);
            var emptyText = MakeLabel("Select an applicant card to review them here.", 7, Muted); emptyText.Name = "ManageRequestsSelectionText"; emptyText.MaximumSize = new Size(220, 35); selectedBox.Controls.Add(emptyText);
            selectedContent.Text = "";
            selectedBox.Controls.Add(selectedContent);
            clearSelectionButton.Text = "Clear selection";
            clearSelectionButton.Name = "ManageRequestsClearSelection";
            clearSelectionButton.FlatStyle = FlatStyle.Flat;
            clearSelectionButton.ForeColor = Blue;
            clearSelectionButton.BackColor = Color.Transparent;
            clearSelectionButton.FlatAppearance.BorderSize = 0;
            clearSelectionButton.Visible = false;
            clearSelectionButton.Click += (_, _) => SetSelected(null);
            selectedBox.Controls.Add(clearSelectionButton);
            decisionPanel.Controls.Add(selectedBox);
            var note = new Panel { Name = "ManageRequestsSafetyNote", BackColor = Color.FromArgb(247, 251, 255), BorderStyle = BorderStyle.FixedSingle };
            var info = MakeLabel("i", 8, Blue, true); info.Location = new Point(8, 8); note.Controls.Add(info);
            var noteTitle = MakeLabel("Keep communication on TaskTap", 7, Navy, true); noteTitle.Location = new Point(27, 7); note.Controls.Add(noteTitle);
            var noteText = MakeLabel("Confirm scope and timing. Share exact addresses only when necessary.", 6.5F, Muted); noteText.Location = new Point(27, 24); noteText.MaximumSize = new Size(205, 28); note.Controls.Add(noteText);
            decisionPanel.Controls.Add(note);
            acceptButton.Name = "ManageRequestsAcceptButton";
            acceptButton.Text = "Accept selected request";
            acceptButton.Size = new Size(260, 36);
            acceptButton.Enabled = false;
            acceptButton.FlatStyle = FlatStyle.Flat;
            acceptButton.BackColor = Blue;
            acceptButton.ForeColor = Color.White;
            acceptButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            acceptButton.FlatAppearance.BorderSize = 0;
            acceptButton.Click += (_, _) => ShowDecisionModal();
            decisionPanel.Controls.Add(acceptButton);
            var compare = MakeButton("Compare shortlisted", "ManageRequestsCompareButton", false, 260);
            compare.Click += (_, _) => { var countShortlisted = requests.Count(request => request.Shortlisted && !request.Declined); ShowToast(countShortlisted == 0 ? "Shortlist applicants to compare them." : $"Comparing {countShortlisted} shortlisted applicant{(countShortlisted == 1 ? "" : "s")}…"); };
            decisionPanel.Controls.Add(compare);
            page.Controls.Add(decisionPanel);
        }

        private void BuildRequestList()
        {
            requestCount.Name = "ManageRequestsVisibleCount";
            requestCount.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            requestCount.ForeColor = Navy;
            page.Controls.Add(requestCount);
            requestList.Name = "ManageRequestsList";
            requestList.FlowDirection = FlowDirection.TopDown;
            requestList.WrapContents = false;
            requestList.AutoScroll = false;
            requestList.BackColor = Page;
            foreach (var request in requests)
            {
                var card = CreateRequestCard(request);
                cards.Add(request, card);
                requestList.Controls.Add(card);
            }
            page.Controls.Add(requestList);
            var empty = new Panel { Name = "ManageRequestsEmptyState", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
            empty.Controls.Add(MakeLabel("No matching requests", 13, Navy, true).At(16, 18));
            empty.Controls.Add(MakeLabel("Try another name, filter, or sort option.", 8, Muted).At(16, 45));
            var clear = MakeButton("Clear filters", "ManageRequestsClearFilters", false, 104);
            clear.Location = new Point(16, 72); clear.Click += (_, _) => ClearFilters(); empty.Controls.Add(clear);
            page.Controls.Add(empty);
        }

        private Panel CreateRequestCard(RequestRecord request)
        {
            var card = new Panel { Name = "RequestCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = request, Cursor = Cursors.Hand };
            card.Click += (_, _) => SetSelected(request);
            var avatar = new Label { Name = "RequestApplicantAvatar", Text = request.Initials, Size = new Size(48, 48), Location = new Point(12, 12), BackColor = request.AvatarColor, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            avatar.Click += (_, _) => SetSelected(request); card.Controls.Add(avatar);
            var state = MakeLabel(request.Shortlisted ? "SHORTLISTED" : "NEW REQUEST", 6.5F, request.Shortlisted ? Color.FromArgb(180, 106, 27) : Blue, true); state.Name = "RequestState"; state.Location = new Point(74, 9); state.Click += (_, _) => SetSelected(request); card.Controls.Add(state);
            var name = MakeLabel(request.Name, 11, Navy, true); name.Name = "RequestApplicantName"; name.Location = new Point(74, 23); name.Click += (_, _) => SetSelected(request); card.Controls.Add(name);
            var rating = MakeLabel($"{request.Rating:F1}  ★   {request.Reviews} reviews · {request.Completed} completed", 7, Muted); rating.Location = new Point(74, 43); rating.Click += (_, _) => SetSelected(request); card.Controls.Add(rating);
            var when = MakeLabel(request.RequestedAt, 6.5F, Muted); when.Name = "RequestReceivedAt"; when.Anchor = AnchorStyles.Top | AnchorStyles.Right; card.Controls.Add(when);
            var message = MakeLabel($"“{request.Message}”", 7.5F, Muted); message.Name = "RequestMessage"; message.Location = new Point(74, 63); message.MaximumSize = new Size(700, 34); message.Click += (_, _) => SetSelected(request); card.Controls.Add(message);
            var skillY = 101;
            var skillX = 74;
            foreach (var skill in request.Skills)
            {
                var pill = new Label { Text = skill, AutoSize = true, Padding = new Padding(7, 4, 7, 4), BackColor = Color.FromArgb(243, 246, 249), ForeColor = Muted, Font = new Font("Segoe UI", 6.5F), Location = new Point(skillX, skillY) };
                card.Controls.Add(pill);
                skillX += pill.PreferredWidth + 5;
            }
            var meta = MakeLabel($"✓ Verified phone   ·   ⌖ Brgy. {request.Barangay   }   ·   Member since {request.MemberSince}", 6.5F, Muted); meta.Name = "RequestMeta"; meta.Location = new Point(74, 129); card.Controls.Add(meta);
            var actions = new FlowLayoutPanel { Name = "RequestActions", Location = new Point(72, 152), Size = new Size(650, 32), WrapContents = false, BackColor = Color.Transparent };
            var shortlist = MakeButton(request.Shortlisted ? "Shortlisted" : "Shortlist", "RequestShortlistButton", true, request.Shortlisted ? 90 : 78);
            shortlist.Height = 27;
            shortlist.Click += (_, _) => ToggleShortlist(request);
            actions.Controls.Add(shortlist);
            AddRequestAction(actions, "Message", () => ShowToast($"Messaging is not connected in this frontend demo (applicant: {request.Name})."));
            AddRequestAction(actions, "View profile", () => ShowToast($"Profile for {request.Name} is sample content in this frontend demo."));
            AddRequestAction(actions, "Decline", () => Decline(request));
            card.Controls.Add(actions);
            return card;
        }

        private void BuildSafetyStrip()
        {
            var strip = new Panel { Name = "ManageRequestsSafetyStrip", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var icon = new Label { Text = "✓", Size = new Size(24, 24), Location = new Point(12, 10), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(10, 141, 102), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }; strip.Controls.Add(icon);
            strip.Controls.Add(MakeLabel("Community safety", 7, Navy, true).At(44, 8));
            strip.Controls.Add(MakeLabel("Review ratings, task history, and each applicant’s message before choosing. Decisions are demo-only and not saved.", 6.5F, Muted).At(44, 23));
            var safety = MakeTextButton("Safety guidelines →", "ManageRequestsSafetyButton"); safety.Click += (_, _) => ShowToast("Review ratings and keep task conversations on TaskTap."); strip.Controls.Add(safety);
            page.Controls.Add(strip);
        }

        private void BuildDecisionModal()
        {
            modal.Name = "ManageRequestsDecisionModal";
            modal.Visible = false;
            modal.BackColor = Color.FromArgb(245, 248, 252);
            modal.BorderStyle = BorderStyle.FixedSingle;
            modal.Size = new Size(450, 240);
            modal.Anchor = AnchorStyles.None;
            var icon = new Label { Text = "✓", Size = new Size(42, 42), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(10, 141, 102), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 17F, FontStyle.Bold), Location = new Point(204, 18) }; modal.Controls.Add(icon);
            var title = MakeLabel("Accept this request?", 14, Navy, true); title.Location = new Point(145, 68); modal.Controls.Add(title);
            var summary = MakeLabel("Select an applicant to review this decision.", 8, Muted); summary.Name = "ManageRequestsDecisionSummary"; summary.Location = new Point(52, 103); summary.MaximumSize = new Size(340, 45); modal.Controls.Add(summary);
            var cancel = MakeButton("Cancel", "ManageRequestsCancelDecision", false, 110); cancel.Location = new Point(103, 174); cancel.Click += (_, _) => modal.Visible = false; modal.Controls.Add(cancel);
            confirmButton = MakeButton("Confirm acceptance", "ManageRequestsConfirmDecision", true, 150); confirmButton.Location = new Point(221, 174); confirmButton.Click += (_, _) => ConfirmAcceptance(); modal.Controls.Add(confirmButton);
            page.Controls.Add(modal);
            modal.BringToFront();
        }

        private void LayoutPage()
        {
            if (!pageBuilt || page.ClientSize.Width <= 0)
                return;
            var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            var right = width - page.Padding.Right;
            var left = page.Padding.Left;
            var actionX = right;
            var edit = page.Controls.Find("ManageRequestsEditTaskButton", false).FirstOrDefault();
            var view = page.Controls.Find("ManageRequestsViewTaskButton", false).FirstOrDefault();
            if (edit is not null) { edit.Location = new Point(actionX - edit.Width, 46); actionX -= edit.Width + 8; }
            if (view is not null) view.Location = new Point(actionX - view.Width, 46);
            contextCard.Bounds = new Rectangle(left, 105, width - left - page.Padding.Right, 102);
            var summaryAmount = contextCard.Controls.Find("ManageRequestsTaskAmount", false).FirstOrDefault();
            var separator = contextCard.Controls.OfType<Panel>().FirstOrDefault(panel => panel.Width == 1);
            var progressLabels = contextCard.Controls.OfType<Label>().Where(label => label.Name != "ManageRequestsTaskAmount").ToArray();
            if (summaryAmount is not null) summaryAmount.Location = new Point(contextCard.Width - 250, 76);
            if (separator is not null) separator.Bounds = new Rectangle(contextCard.Width - 224, 14, 1, 74);
            var summaryCount = contextCard.Controls.Find("ManageRequestsSummaryCount", false).FirstOrDefault();
            if (summaryCount is not null) summaryCount.Location = new Point(contextCard.Width - 208, 12);
            var received = progressLabels.FirstOrDefault(label => label.Text == "requests received");
            if (received is not null) received.Location = new Point(contextCard.Width - 208, 38);
            var progress = contextCard.Controls.OfType<Panel>().Where(panel => panel.Height == 4).ToList();
            if (progress.Count > 0) progress[0].Bounds = new Rectangle(contextCard.Width - 208, 57, 110, 4);
            if (progress.Count > 1) progress[1].Bounds = new Rectangle(0, 0, 72, 4);
            var choose = progressLabels.FirstOrDefault(label => label.Text == "Choose one performer");
            if (choose is not null) choose.Location = new Point(contextCard.Width - 208, 67);

            const int gap = 14;
            var decisionWidth = 276;
            var requestsWidth = width - left - page.Padding.Right - decisionWidth - gap;
            requestsToolbar.Bounds = new Rectangle(left, 219, requestsWidth, 96);
            var sort = requestSort;
            sort.Location = new Point(requestsToolbar.Width - sort.Width - 12, 8);
            var count = requestsToolbar.Controls.Find("ManageRequestsCountLabel", false).FirstOrDefault();
            if (count is not null) count.Location = new Point(12, 10);
            var sortRowBottom = 40;
            requestSearch.Location = new Point(12, sortRowBottom + 7);
            requestSearch.Size = new Size(Math.Max(180, requestsToolbar.Width - 236), 30);
            var filterX = requestSearch.Right + 8;
            foreach (var key in new[] { "all", "new", "shortlisted" })
            {
                if (filters.TryGetValue(key, out var filter))
                {
                    filter.Location = new Point(filterX, sortRowBottom + 7);
                    filterX += filter.Width + 5;
                }
            }
            requestCount.Location = new Point(left + 2, 326);
            var requestHeight = Math.Max(210, requests.Count(request => !request.Declined &&
                (currentFilter == "all" || currentFilter == "new" && !request.Shortlisted || currentFilter == "shortlisted" && request.Shortlisted) &&
                (string.IsNullOrEmpty(requestSearch.Text.Trim()) || $"{request.Name} {request.Message} {string.Join(" ", request.Skills)} {request.Barangay} {request.Rating}".Contains(requestSearch.Text.Trim(), StringComparison.OrdinalIgnoreCase))) * 195);
            requestList.Bounds = new Rectangle(left, 348, requestsWidth, requestHeight);
            foreach (Control card in requestList.Controls)
                LayoutRequestCard(card, requestsWidth);
            var empty = page.Controls.Find("ManageRequestsEmptyState", false).FirstOrDefault();
            if (empty is not null) empty.Bounds = new Rectangle(left, 348, requestsWidth, 160);

            decisionPanel.Bounds = new Rectangle(left + requestsWidth + gap, 219, decisionWidth, 456);
            LayoutDecisionPanel(decisionWidth);
            var safety = page.Controls.Find("ManageRequestsSafetyStrip", false).FirstOrDefault();
            if (safety is not null)
            {
                var safetyTop = 348 + requestHeight + 12;
                safety.Bounds = new Rectangle(left, safetyTop, width - left - page.Padding.Right, 48);
                var safetyButton = safety.Controls.Find("ManageRequestsSafetyButton", false).FirstOrDefault();
                if (safetyButton is not null) safetyButton.Location = new Point(safety.Width - safetyButton.Width - 10, 8);
                page.AutoScrollMinSize = new Size(0, safetyTop + 62);
            }
            modal.Location = new Point(Math.Max(0, (page.ClientSize.Width - modal.Width) / 2), Math.Max(0, (page.ClientSize.Height - modal.Height) / 2));
        }

        private void LayoutDecisionPanel(int width)
        {
            var selection = decisionPanel.Controls.Find("ManageRequestsSelectedBox", false).FirstOrDefault();
            if (selection is not null)
            {
                selection.Bounds = new Rectangle(12, 128, width - 24, 124);
                var icon = selection.Controls.Find("ManageRequestsSelectionIcon", false).FirstOrDefault();
                var title = selection.Controls.Find("ManageRequestsSelectionTitle", false).FirstOrDefault();
                var text = selection.Controls.Find("ManageRequestsSelectionText", false).FirstOrDefault();
                if (icon is not null) icon.Location = new Point((selection.Width - icon.Width) / 2, 14);
                if (title is not null) title.Location = new Point((selection.Width - title.Width) / 2, 54);
                if (text is not null) text.Location = new Point(12, 74);
                selectedContent.Location = new Point(12, 11);
                clearSelectionButton.Location = new Point(12, 91);
            }
            var note = decisionPanel.Controls.Find("ManageRequestsSafetyNote", false).FirstOrDefault();
            if (note is not null) note.Bounds = new Rectangle(12, 264, width - 24, 56);
            acceptButton.Location = new Point(12, 330);
            acceptButton.Width = width - 24;
            var compare = decisionPanel.Controls.Find("ManageRequestsCompareButton", false).FirstOrDefault();
            if (compare is not null) { compare.Location = new Point(12, 374); compare.Width = width - 24; }
        }

        private void LayoutRequestCard(Control control, int width)
        {
            if (control is not Panel card || card.Tag is not RequestRecord request)
                return;
            card.Width = width - 4;
            card.Height = 190;
            var time = card.Controls.Find("RequestReceivedAt", false).FirstOrDefault();
            if (time is not null) time.Location = new Point(card.Width - time.Width - 12, 12);
            var message = card.Controls.Find("RequestMessage", false).FirstOrDefault();
            if (message is not null) message.MaximumSize = new Size(Math.Max(250, card.Width - 115), 34);
            var meta = card.Controls.Find("RequestMeta", false).FirstOrDefault();
            if (meta is not null) meta.Text = $"✓ Verified phone   ·   ⌖ Brgy. {request.Barangay}   ·   Member since {request.MemberSince}";
            var actions = card.Controls.Find("RequestActions", false).FirstOrDefault();
            if (actions is not null) actions.Location = new Point(72, 153);
            card.BackColor = ReferenceEquals(selected, request) ? Color.FromArgb(240, 248, 255) : Color.White;
        }

        private void ApplyRequests()
        {
            if (requestList.IsDisposed)
                return;
            var query = requestSearch.Text.Trim();
            IEnumerable<RequestRecord> filtered = requests.Where(request => !request.Declined)
                .Where(request => currentFilter == "all" || currentFilter == "new" && !request.Shortlisted || currentFilter == "shortlisted" && request.Shortlisted)
                .Where(request => string.IsNullOrEmpty(query) || $"{request.Name} {request.Message} {string.Join(" ", request.Skills)} {request.Barangay} {request.Rating}".Contains(query, StringComparison.OrdinalIgnoreCase));
            filtered = requestSort.SelectedItem?.ToString() switch
            {
                "Highest rating" => filtered.OrderByDescending(request => request.Rating),
                "Most recent" => filtered.OrderByDescending(request => request.Date),
                _ => filtered.OrderByDescending(request => request.Shortlisted).ThenByDescending(request => request.Date)
            };
            var visible = filtered.ToHashSet();
            requestList.SuspendLayout();
            foreach (var (request, card) in cards)
            {
                card.Visible = visible.Contains(request);
                card.Parent = null;
            }
            foreach (var request in filtered)
                requestList.Controls.Add(cards[request]);
            requestList.ResumeLayout();
            requestCount.Text = $"{visible.Count} request{(visible.Count == 1 ? "" : "s")}";
            var countLabel = requestsToolbar.Controls.Find("ManageRequestsCountLabel", false).FirstOrDefault();
            if (countLabel is not null) countLabel.Text = requestCount.Text;
            var empty = page.Controls.Find("ManageRequestsEmptyState", false).FirstOrDefault();
            if (empty is not null) empty.Visible = visible.Count == 0;
            if (pageBuilt) LayoutPage();
        }

        private void SelectFilter(string filter)
        {
            currentFilter = filter;
            foreach (var (key, button) in filters)
            {
                var active = key == filter;
                button.BackColor = active ? Color.FromArgb(237, 245, 253) : Color.White;
                button.ForeColor = active ? Blue : Muted;
            }
            ApplyRequests();
        }

        private void ToggleShortlist(RequestRecord request)
        {
            request.Shortlisted = !request.Shortlisted;
            if (ReferenceEquals(selected, request)) UpdateSelectionPanel();
            ApplyRequests();
            ShowToast(request.Shortlisted ? $"{request.Name} shortlisted for this demo session." : $"{request.Name} removed from shortlist.");
        }

        private void SetSelected(RequestRecord? request)
        {
            if (accepted)
            {
                ShowToast("A performer has already been selected for this demo session.");
                return;
            }
            selected = request;
            foreach (var (record, card) in cards)
                LayoutRequestCard(card, requestList.ClientSize.Width > 0 ? requestList.ClientSize.Width : 500);
            UpdateSelectionPanel();
        }

        private void UpdateSelectionPanel()
        {
            var selectedBox = decisionPanel.Controls.Find("ManageRequestsSelectedBox", false).FirstOrDefault();
            if (selectedBox is null) return;
            foreach (var name in new[] { "ManageRequestsSelectionIcon", "ManageRequestsSelectionTitle", "ManageRequestsSelectionText" })
            {
                var control = selectedBox.Controls.Find(name, false).FirstOrDefault();
                if (control is not null) control.Visible = selected is null;
            }
            selectedContent.Visible = selected is not null;
            clearSelectionButton.Visible = selected is not null;
            selectedContent.Text = selected is null ? string.Empty : $"{selected.Initials}   {selected.Name}\r\n{selected.Rating:F1} ★ · ready to start\r\n\r\nYou can still message them before confirming.";
            selectedContent.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            selectedContent.ForeColor = Navy;
            selectedContent.MaximumSize = new Size(selectedBox.Width - 24, 76);
            acceptButton.Enabled = selected is not null && !accepted;
            acceptButton.Text = accepted ? "Performer selected" : "Accept selected request";
        }

        private void ShowDecisionModal()
        {
            if (selected is null) return;
            var summary = modal.Controls.Find("ManageRequestsDecisionSummary", false).FirstOrDefault();
            if (summary is not null) summary.Text = $"{selected.Name}\r\n{selected.Rating:F1} ★  ·  {selected.Reviews} reviews  ·  {selected.Completed} completed\r\n\r\nThis is a demo-only action; nothing will be saved.";
            modal.Visible = true;
            modal.BringToFront();
        }

        private void ConfirmAcceptance()
        {
            if (selected is null) return;
            accepted = true;
            modal.Visible = false;
            var acceptedName = selected.Name;
            UpdateSelectionPanel();
            foreach (var request in requests.Where(request => !ReferenceEquals(request, selected)))
            {
                var card = cards[request];
                foreach (Control control in card.Controls)
                    if (control is Button button) button.Enabled = false;
            }
            ShowToast($"{acceptedName} selected for this demo session only; the choice is not saved.");
        }

        private void Decline(RequestRecord request)
        {
            if (MessageBox.Show($"Remove {request.Name} from this request list for the current demo session? This action is not saved.", "Decline request", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            request.Declined = true;
            if (ReferenceEquals(selected, request)) SetSelected(null);
            ApplyRequests();
            ShowToast("Request declined for this demo session only.");
        }

        private void ClearFilters()
        {
            requestSearch.Clear();
            SelectFilter("all");
            requestSort.SelectedIndex = 0;
        }

        private void OpenEditTask()
        {
            Hide();
            var editor = new EditTaskForm(() => { if (!IsDisposed) { Show(); Activate(); } });
            editor.Show();
        }

        private void Navigate(string destination)
        {
            if (destination == "My Tasks") return;
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

        private void AddRequestAction(FlowLayoutPanel panel, string text, Action action)
        {
            var button = MakeTextButton(text, "RequestActionButton");
            button.Height = 28;
            button.Click += (_, _) => action();
            panel.Controls.Add(button);
        }

        private void ShowToast(string message)
        {
            toast.Text = message;
            toast.Visible = true;
            toast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private TControl? Find<TControl>(string name) where TControl : Control => page.Controls.Find(name, false).FirstOrDefault() as TControl;

        private static Button MakeNavButton(string text, bool active)
        {
            var glyph = text switch { "Dashboard" => "⌂", "Find Tasks" => "⌕", "Post Task" => "＋", "My Tasks" => "▤", "Messages" => "▱", "Notifications" => "♧", "Profile" => "◉", _ => "↪" };
            var button = new Button { Text = $"{glyph}   {text}", Size = new Size(178, 38), FlatStyle = FlatStyle.Flat, BackColor = active ? Color.FromArgb(31, 63, 108) : Color.Transparent, ForeColor = Color.FromArgb(222, 234, 250), Font = new Font("Segoe UI", 8F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
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

        private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            returnToParent?.Invoke();
            base.OnFormClosed(e);
        }
    }

    internal static class ManageRequestsControlExtensions
    {
        public static T At<T>(this T control, int x, int y) where T : Control
        {
            control.Location = new Point(x, y);
            return control;
        }
    }
}
