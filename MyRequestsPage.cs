using RequestRecord = TaskTap.DemoRequest;

namespace TaskTap;

public sealed class MyRequestsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(104, 121, 144);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(223, 230, 239);
    private readonly Panel page = new();
    private readonly FlowLayoutPanel requestGrid = new();
    private readonly TextBox search = new();
    private readonly ComboBox sort = new();
    private readonly Label resultCount = new();
    private readonly Panel emptyState = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private readonly List<RequestRecord> requests = DemoDataStore.RequestsSent;
    private readonly Dictionary<RequestRecord, Panel> cards = [];
    private readonly Dictionary<string, Button> tabs = [];
    private string activeStatus = "all";
    private RequestRecord? withdrawTarget;
    private readonly Form withdrawDialog = new();
    private readonly Label withdrawCopy = new();
    private readonly Label statTotal = new();
    private readonly Label statPending = new();
    private readonly Label statAccepted = new();
    private readonly Label statCompleted = new();
    private bool pageBuilt;

    public override Route RouteName => Route.MyRequests;

    public MyRequestsPage()
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
        BuildWithdrawDialog();
        toast.Name = "myRequestsToast";
        toast.Size = new Size(430, 40);
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

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Tasks   /   My Requests", 8, Muted, true); crumb.Name = "myRequestsCrumb"; crumb.Cursor = Cursors.Hand; crumb.Click += (_, _) => Nav.Back(); page.Controls.Add(crumb);
        AddPageLabel("myRequestsEyebrow", "TASK-DOER ACTIVITY", 8, Blue, true);
        AddPageLabel("myRequestsHeading", "My Requests", 26, Navy, true);
        AddPageLabel("myRequestsSubtitle", "Track the tasks you’ve asked to perform and what happens next.", 10, Muted);
        var find = MakeButton("⌕  Find more tasks", "MyRequestsFindTasksButton", true, 145); find.Click += (_, _) => Nav.Navigate(Route.FindTasks); page.Controls.Add(find);

        var stats = new[] { ("↗", "4", "Total requests", Color.FromArgb(234, 243, 255), Blue), ("◷", "2", "Pending", Color.FromArgb(255, 241, 232), Color.FromArgb(221, 117, 32)), ("✓", "1", "Accepted", Color.FromArgb(233, 248, 242), Color.FromArgb(13, 155, 107)), ("★", "18", "Tasks completed", Color.FromArgb(237, 240, 247), Navy) };
        for (var index = 0; index < stats.Length; index++)
        {
            var stat = stats[index]; var card = new Panel { Name = $"myRequestsStat{index}", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var icon = new Label { Text = stat.Item1, Size = new Size(40, 40), Location = new Point(14, 15), BackColor = stat.Item4, ForeColor = stat.Item5, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14F, FontStyle.Bold) }; card.Controls.Add(icon);
            var value = MakeLabel(stat.Item2, 19, Navy, true); value.Location = new Point(64, 12); card.Controls.Add(value);
            var caption = MakeLabel(stat.Item3, 8, Muted); caption.Location = new Point(64, 38); card.Controls.Add(caption);
            page.Controls.Add(card);
        }

        var toolbar = new Panel { Name = "myRequestsToolbar", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle }; page.Controls.Add(toolbar);
        var tabLabels = new[] { ("all", "All", 4), ("pending", "Pending", 2), ("accepted", "Accepted", 1), ("declined", "Declined", 1) };
        foreach (var (key, text, count) in tabLabels)
        {
            var tab = new Button { Name = $"MyRequestsTab{key}", Text = $"{text}  {count}", Size = new Size(104, 36), FlatStyle = FlatStyle.Flat, BackColor = key == "all" ? Color.FromArgb(234, 243, 255) : Color.White, ForeColor = key == "all" ? Blue : Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            tab.FlatAppearance.BorderSize = 0; tab.Click += (_, _) => SelectTab(key); tabs.Add(key, tab); toolbar.Controls.Add(tab);
        }
        search.Name = "myRequestsSearch"; search.PlaceholderText = "⌕  Search your requests..."; search.TextChanged += (_, _) => ApplyRequests(); toolbar.Controls.Add(search);
        sort.Name = "myRequestsSort"; sort.DropDownStyle = ComboBoxStyle.DropDownList; sort.Items.AddRange(["Most recent", "Deadline soonest", "Highest amount"]); sort.SelectedIndex = 0; sort.SelectedIndexChanged += (_, _) => ApplyRequests(); toolbar.Controls.Add(sort);
        resultCount.Name = "myRequestsResultCount"; resultCount.Font = new Font("Segoe UI", 8F, FontStyle.Bold); resultCount.ForeColor = Navy; page.Controls.Add(resultCount);
        requestGrid.Name = "myRequestsGrid"; requestGrid.FlowDirection = FlowDirection.LeftToRight; requestGrid.WrapContents = true; requestGrid.AutoScroll = false; requestGrid.BackColor = PageColor; page.Controls.Add(requestGrid);
        foreach (var request in requests) { var card = CreateRequestCard(request); cards.Add(request, card); requestGrid.Controls.Add(card); }
        emptyState.Name = "myRequestsEmptyState"; emptyState.BackColor = Color.White; emptyState.BorderStyle = BorderStyle.FixedSingle; emptyState.Visible = false;
        emptyState.Controls.Add(MakeLabel("No requests found", 14, Navy, true).At(18, 20));
        emptyState.Controls.Add(MakeLabel("Try another search or request status.", 9, Muted).At(18, 50));
        var clear = MakeButton("Clear filters", "MyRequestsClearFilters", false, 110); clear.Location = new Point(18, 82); clear.Click += (_, _) => ClearFilters(); emptyState.Controls.Add(clear); page.Controls.Add(emptyState);
        var tip = new Panel { Name = "myRequestsTip", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var tipIcon = new Label { Text = "i", Size = new Size(30, 30), Location = new Point(12, 10), BackColor = Color.FromArgb(234, 243, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11F, FontStyle.Bold) }; tip.Controls.Add(tipIcon);
        tip.Controls.Add(MakeLabel("Keep your requests organized", 9, Navy, true).At(52, 8));
        tip.Controls.Add(MakeLabel("Pending requests can be withdrawn. If accepted, contact the poster to confirm details and timing.", 7.5F, Muted).At(52, 27));
        page.Controls.Add(tip);
    }

    private Panel CreateRequestCard(RequestRecord request)
    {
        var card = new Panel { Name = "MyRequestCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = request };
        var icon = new Label { Text = request.Icon, Size = new Size(45, 45), Location = new Point(14, 14), BackColor = request.IconBack, ForeColor = Navy, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 18F) }; card.Controls.Add(icon);
        var stateText = request.Status switch { "pending" => "PENDING", "accepted" => "ACCEPTED", _ => "DECLINED" };
        var stateColor = request.Status switch { "pending" => Color.FromArgb(173, 104, 0), "accepted" => Color.FromArgb(7, 131, 90), _ => Muted };
        var stateBack = request.Status switch { "pending" => Color.FromArgb(255, 244, 223), "accepted" => Color.FromArgb(232, 248, 241), _ => Color.FromArgb(241, 243, 246) };
        var badge = new Label { Name = "MyRequestStatus", Text = stateText, AutoSize = true, Padding = new Padding(7, 4, 7, 4), BackColor = stateBack, ForeColor = stateColor, Font = new Font("Segoe UI", 6.5F, FontStyle.Bold), Location = new Point(70, 10) }; card.Controls.Add(badge);
        var title = MakeLabel(request.Title, 11, Navy, true); title.Name = "MyRequestTitle"; title.Location = new Point(70, 31); card.Controls.Add(title);
        var category = MakeLabel($"{request.Category} · Brgy. {request.Barangay}", 7.5F, Muted); category.Location = new Point(70, 52); card.Controls.Add(category);
        var amount = MakeLabel($"₱{request.Amount:N0}", 14, Navy, true); amount.Name = "MyRequestAmount"; card.Controls.Add(amount);
        card.Controls.Add(MakeLabel(request.Description, 8, Muted).At(14, 76));
        var metaText = request.Status == "pending" ? $"◷ Due {request.Deadline}   ·   ↗ Requested {DateTime.Parse(request.RequestDate):MMM d}" : request.Status == "accepted" ? $"◷ Due {request.Deadline}   ·   ✓ Accepted {DateTime.Parse(request.RequestDate):MMM d}" : $"◷ {request.Deadline}   ·   × Declined Oct 1";
        card.Controls.Add(MakeLabel(metaText, 7.5F, Muted).At(14, 101));
        if (request.Status == "pending")
        {
            var message = new Panel { Name = "MyRequestMessage", BackColor = Color.FromArgb(247, 249, 252) };
            message.Controls.Add(MakeLabel("Your request", 7.5F, Navy, true).At(10, 8));
            var copy = MakeLabel($"“{request.RequestMessage}”", 7.5F, Muted); copy.Name = "MyRequestMessageCopy"; copy.MaximumSize = new Size(480, 38); copy.Location = new Point(10, 25); message.Controls.Add(copy); card.Controls.Add(message);
        }
        else if (request.Status == "accepted")
        {
            var accepted = new Panel { Name = "MyRequestAcceptedNote", BackColor = Color.FromArgb(237, 249, 244) };
            accepted.Controls.Add(MakeLabel("✓", 10, Color.FromArgb(13, 155, 107), true).At(10, 12));
            accepted.Controls.Add(MakeLabel("You’re selected for this task", 8, Navy, true).At(30, 8));
            accepted.Controls.Add(MakeLabel("Message the poster to confirm the schedule and task details.", 7.5F, Muted).At(30, 25)); card.Controls.Add(accepted);
        }
        else
        {
            var declined = new Panel { Name = "MyRequestDeclinedNote", BackColor = Color.FromArgb(247, 249, 252) };
            declined.Controls.Add(MakeLabel("Request not selected", 8, Navy, true).At(10, 8));
            declined.Controls.Add(MakeLabel("The poster chose another task-doer. Browse for similar tasks.", 7.5F, Muted).At(10, 25)); card.Controls.Add(declined);
        }
        var actions = new FlowLayoutPanel { Name = "MyRequestActions", FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
        var view = MakeButton("View task", $"MyRequestView{request.Title.Replace(" ", "")}", request.Status == "accepted", 82); view.Height = 30; view.Click += (_, _) => OpenRequestTask(request); actions.Controls.Add(view);
        if (request.Status == "pending")
        {
            var withdraw = MakeTextButton("Withdraw request", "MyRequestWithdrawButton"); withdraw.Name = "MyRequestWithdrawButton"; withdraw.Tag = request; withdraw.Click += (_, _) => BeginWithdraw(request); actions.Controls.Add(withdraw);
        }
        else if (request.Status == "accepted")
            actions.Controls.Add(MakeButton("Message poster", "MyRequestMessagePosterButton", true, 108).Also(button => button.Click += (_, _) => Nav.Navigate(Route.Messages)));
        else
            actions.Controls.Add(MakeTextButton("Find similar tasks", "MyRequestSimilarTasksButton").Also(button => button.Click += (_, _) => Nav.Navigate(Route.FindTasks)));
        card.Controls.Add(actions);
        return card;
    }

    private void ApplyRequests()
    {
        if (!pageBuilt) return;
        var query = search.Text.Trim();
        var filtered = requests.Where(request => !request.Withdrawn)
            .Where(request => activeStatus == "all" || request.Status == activeStatus)
            .Where(request => string.IsNullOrEmpty(query) || $"{request.Title} {request.Category} Brgy. {request.Barangay} {request.Description} {request.RequestMessage}".Contains(query, StringComparison.OrdinalIgnoreCase));
        filtered = sort.SelectedItem?.ToString() switch
        {
            "Deadline soonest" => filtered.OrderBy(request => request.Status == "declined" ? DateTime.MaxValue : DateTime.TryParse(request.Deadline, out var date) ? date : DateTime.MaxValue),
            "Highest amount" => filtered.OrderByDescending(request => request.Amount),
            _ => filtered.OrderByDescending(request => request.Date)
        };
        var visible = filtered.ToList();
        requestGrid.SuspendLayout(); requestGrid.Controls.Clear();
        foreach (var request in visible) requestGrid.Controls.Add(cards[request]);
        requestGrid.ResumeLayout();
        resultCount.Text = $"{visible.Count} request{(visible.Count == 1 ? "" : "s")}";
        emptyState.Visible = visible.Count == 0;
        UpdateCounts();
        if (pageBuilt) LayoutPage();
    }

    private void UpdateCounts()
    {
        var active = requests.Where(request => !request.Withdrawn).ToList();
        statTotal.Text = active.Count.ToString();
        statPending.Text = active.Count(request => request.Status == "pending").ToString();
        statAccepted.Text = active.Count(request => request.Status == "accepted").ToString();
        foreach (var (key, tab) in tabs)
        {
            var count = key == "all" ? active.Count : active.Count(request => request.Status == key);
            tab.Text = $"{(key == "all" ? "All" : char.ToUpperInvariant(key[0]) + key[1..])}  {count}";
        }
    }

    private void SelectTab(string status)
    {
        activeStatus = status;
        foreach (var (key, tab) in tabs)
        {
            var active = key == status; tab.BackColor = active ? Color.FromArgb(234, 243, 255) : Color.White; tab.ForeColor = active ? Blue : Muted;
        }
        ApplyRequests();
    }

    private void BeginWithdraw(RequestRecord request)
    {
        withdrawTarget = request;
        withdrawCopy.Text = $"Withdraw your request for “{request.Title}”? You can browse and request other tasks later. The poster will be told you withdrew.";
        withdrawDialog.ShowDialog(this);
    }

    private void BuildWithdrawDialog()
    {
        withdrawDialog.Text = "Withdraw request?";
        withdrawDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
        withdrawDialog.StartPosition = FormStartPosition.CenterParent;
        withdrawDialog.ClientSize = new Size(470, 205);
        withdrawDialog.MinimizeBox = false;
        withdrawDialog.MaximizeBox = false;
        withdrawDialog.ShowInTaskbar = false;
        withdrawDialog.BackColor = Color.White;
        var title = MakeLabel("Withdraw this request?", 15, Navy, true); title.Location = new Point(24, 22); withdrawDialog.Controls.Add(title);
        withdrawCopy.Location = new Point(24, 62); withdrawCopy.MaximumSize = new Size(420, 66); withdrawCopy.Size = new Size(420, 66); withdrawCopy.Font = new Font("Segoe UI", 9F); withdrawCopy.ForeColor = Muted; withdrawDialog.Controls.Add(withdrawCopy);
        var cancel = MakeButton("Keep request", "MyRequestsCancelWithdraw", false, 110); cancel.Location = new Point(220, 142); cancel.Click += (_, _) => withdrawDialog.DialogResult = DialogResult.Cancel; withdrawDialog.Controls.Add(cancel);
        var confirm = MakeButton("Withdraw", "MyRequestsConfirmWithdraw", true, 100); confirm.Location = new Point(340, 142); confirm.Click += (_, _) => withdrawDialog.DialogResult = DialogResult.OK; withdrawDialog.Controls.Add(confirm);
        withdrawDialog.FormClosing += (_, e) =>
        {
            if (withdrawDialog.DialogResult == DialogResult.OK && withdrawTarget is not null)
            {
                var target = withdrawTarget;
                if (!DemoDataStore.WithdrawRequest(target))
                {
                    withdrawDialog.DialogResult = DialogResult.Cancel;
                    return;
                }
                withdrawTarget = null;
                ApplyRequests();
                ShowToast("Request withdrawn.");
            }
            else withdrawTarget = null;
        };
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var left = 22;
        var contentWidth = width - 44;
        Position("myRequestsCrumb", left, 12);
        Position("myRequestsEyebrow", left, 40);
        Position("myRequestsHeading", left, 58);
        Position("myRequestsSubtitle", left, 106);
        var find = page.Controls.Find("MyRequestsFindTasksButton", false).FirstOrDefault();
        if (find is not null) find.Location = new Point(width - 22 - find.Width, 68);
        var gap = 12;
        var statWidth = (contentWidth - gap * 3) / 4;
        for (var index = 0; index < 4; index++)
        {
            var card = page.Controls.Find($"myRequestsStat{index}", false).FirstOrDefault();
            if (card is not null) card.Bounds = new Rectangle(left + index * (statWidth + gap), 127, statWidth, 75);
        }
        var toolbar = page.Controls.Find("myRequestsToolbar", false).FirstOrDefault();
        if (toolbar is not null)
        {
            toolbar.Bounds = new Rectangle(left, 216, contentWidth, 112);
            var x = 10;
            foreach (var tab in tabs.Values) { tab.Location = new Point(x, 8); x += tab.Width + 2; }
            search.Location = new Point(12, 60); search.Size = new Size(Math.Max(180, toolbar.Width - 250), 34);
            sort.Location = new Point(toolbar.Width - 220, 60); sort.Size = new Size(208, 34);
        }
        resultCount.Location = new Point(left + 2, 340);
        var cols = width >= 1050 ? 2 : 1;
        var gapX = 14;
        var cardWidth = (contentWidth - gapX * (cols - 1)) / cols;
        var rows = (int)Math.Ceiling(requestGrid.Controls.Count / (double)cols);
        var cardHeight = 260;
        var gridHeight = rows * cardHeight + Math.Max(0, rows - 1) * 14;
        requestGrid.Location = new Point(left, 363);
        requestGrid.Size = new Size(contentWidth, gridHeight);
        foreach (Control control in requestGrid.Controls)
        {
            control.Size = new Size(cardWidth, cardHeight);
            LayoutRequestCard(control);
        }
        emptyState.Bounds = new Rectangle(left, 363, contentWidth, 160);
        var tip = page.Controls.Find("myRequestsTip", false).FirstOrDefault();
        if (tip is not null)
        {
            tip.Bounds = new Rectangle(left, 363 + Math.Max(gridHeight, 160) + 14, contentWidth, 56);
            page.AutoScrollMinSize = new Size(0, tip.Bottom + 20);
        }
    }

    private void LayoutRequestCard(Control control)
    {
        if (control is not Panel card) return;
        var width = card.Width;
        var amount = card.Controls.Find("MyRequestAmount", false).FirstOrDefault();
        if (amount is not null) amount.Location = new Point(width - amount.Width - 14, 33);
        foreach (var panel in card.Controls.OfType<Panel>())
        {
            if (panel.Name == "MyRequestMessage") panel.Bounds = new Rectangle(14, 127, width - 28, 66);
            else if (panel.Name is "MyRequestAcceptedNote" or "MyRequestDeclinedNote") panel.Bounds = new Rectangle(14, 127, width - 28, 52);
        }
        var actions = card.Controls.Find("MyRequestActions", false).FirstOrDefault();
        if (actions is not null) actions.Location = new Point(12, card.Controls.OfType<Panel>().Any(panel => panel.Name == "MyRequestMessage" || panel.Name is "MyRequestAcceptedNote" or "MyRequestDeclinedNote") ? 202 : 132);
        var desc = card.Controls.OfType<Label>().FirstOrDefault(label => label.Location == new Point(14, 76));
        if (desc is not null) desc.MaximumSize = new Size(width - 28, 30);
    }

    private void ClearFilters()
    {
        search.Clear(); sort.SelectedIndex = 0; SelectTab("all");
    }

    private void OpenRequestTask(RequestRecord request)
    {
        var task = DemoDataStore.AvailableTasks.FirstOrDefault(t => t.Title == request.Title)
                   ?? DemoDataStore.PostedTasks.FirstOrDefault(t => t.Title == request.Title);
        if (task is not null)
            Nav.Navigate(new UserTaskDetailsPage(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack, task.Id));
        else
            Nav.Navigate(Route.FindTasks);
    }

    private void ShowToast(string message)
    {
        toast.Text = message; toast.Visible = true; toast.BringToFront(); toastTimer.Stop(); toastTimer.Start();
    }

    private void Position(string name, int x, int y)
    {
        var control = page.Controls.Find(name, false).FirstOrDefault(); if (control is not null) control.Location = new Point(x, y);
    }

    private static Button MakeButton(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line; button.FlatAppearance.BorderSize = primary ? 0 : 1; return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false }; button.FlatAppearance.BorderSize = 0; return button;
    }

    private void AddPageLabel(string name, string text, float size, Color color, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold); label.Name = name; page.Controls.Add(label);
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            toastTimer.Dispose();
            withdrawDialog.Dispose();
        }
        base.Dispose(disposing);
    }
}
