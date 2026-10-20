namespace TaskTap;

public sealed class ArchivedTasksPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Green = Color.FromArgb(19, 160, 113);
    private static readonly Color Orange = Color.FromArgb(255, 138, 61);
    private static readonly Color Muted = Color.FromArgb(113, 130, 151);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(223, 230, 239);
    private readonly Panel page = new();
    private readonly Panel statsPanel = new();
    private readonly Panel tabsPanel = new();
    private readonly Panel toolbar = new();
    private readonly FlowLayoutPanel taskList = new();
    private readonly Panel emptyPanel = new();
    private readonly Panel notePanel = new();
    private readonly TextBox search = new();
    private readonly ComboBox categoryFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label totalCount = new();
    private readonly Label completedCount = new();
    private readonly Label cancelledCount = new();
    private readonly Label valueTotal = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private readonly List<ArchivedTask> tasks = [];
    private readonly Dictionary<string, Button> tabButtons = [];
    private string activeFilter = "all";
    private bool pageBuilt;

    public override Route RouteName => Route.ArchivedTasks;

    private sealed record ArchivedTask(string Id, string Title, string Category, int Amount, string Location, DateTime ArchivedDate, string CompletedDate, string Status, string Reason, string Description, string? Rating, string Icon, Color IconBack, Color IconColor, bool HasPayment, bool HasReview);

    public ArchivedTasksPage()
    {
        SeedTasks();
        BuildPage();
    }

    private void SeedTasks()
    {
        foreach (var task in DemoDataStore.ArchivedTasks)
        {
            var outcome = DemoDataStore.TaskOutcomes.FirstOrDefault(item => item.Title == task.Title);
            var status = outcome?.Status.ToLowerInvariant() ?? "completed";
            var date = outcome?.Date ?? DateTime.Today;
            var hasPayment = DemoDataStore.Payments.Any(payment => payment.Task == task.Title);
            tasks.Add(new ArchivedTask(
                task.Id.ToString(), task.Title, task.Category, task.Price, task.Barangay, date,
                date.ToString("MMM d, yyyy"), status, outcome?.Reason ?? "Task moved to history.",
                task.Description, null, task.Icon, task.IconBack, task.IconBack, hasPayment, false));
        }
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
        pageBuilt = true;
        RenderTasks();
        UpdateCounts();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Tasks   /   Archived Tasks", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Task history", 8, Blue, true, new Point(22, 43)));
        page.Controls.Add(MakeLabel("My Archived Tasks", 23, Navy, true, new Point(22, 59)));
        page.Controls.Add(MakeLabel("Keep completed, cancelled, and expired tasks out of your active workspace while preserving their history.", 9, Muted, false, new Point(22, 104), new Size(720, 24)));
        var back = MakeButton("Back to My Tasks", true, 155);
        back.Name = "ArchivedBackButton";
        back.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(back);
        BuildStats();
        BuildTabs();
        BuildToolbar();
        taskList.FlowDirection = FlowDirection.TopDown;
        taskList.WrapContents = false;
        taskList.AutoScroll = false;
        taskList.BackColor = PageColor;
        page.Controls.Add(taskList);
        BuildEmptyState();
        BuildArchiveNote();
    }

    private void BuildStats()
    {
        var data = new[]
        {
            ("▤", totalCount, "Total archived", Blue, Color.FromArgb(234, 243, 255)),
            ("✓", completedCount, "Completed", Green, Color.FromArgb(234, 247, 242)),
            ("↗", cancelledCount, "Cancelled", Orange, Color.FromArgb(255, 243, 232)),
            ("₱", valueTotal, "Recorded task value", Color.FromArgb(104, 119, 138), Color.FromArgb(240, 243, 247))
        };
        foreach (var item in data)
        {
            var card = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var icon = MakeLabel(item.Item1, 12, item.Item4, true, new Point(16, 14));
            icon.AutoSize = false;
            icon.Size = new Size(34, 34);
            icon.TextAlign = ContentAlignment.MiddleCenter;
            icon.BackColor = item.Item5;
            card.Controls.Add(icon);
            item.Item2.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            item.Item2.ForeColor = Navy;
            item.Item2.Location = new Point(16, 52);
            item.Item2.AutoSize = true;
            card.Controls.Add(item.Item2);
            card.Controls.Add(MakeLabel(item.Item3, 8, Muted, true, new Point(16, 87)));
            statsPanel.Controls.Add(card);
        }
        page.Controls.Add(statsPanel);
    }

    private void BuildTabs()
    {
        tabsPanel.BackColor = Color.White;
        tabsPanel.BorderStyle = BorderStyle.FixedSingle;
        var tabData = new[] { ("all", "All"), ("completed", "Completed"), ("cancelled", "Cancelled"), ("expired", "Expired") };
        foreach (var (filter, text) in tabData)
        {
            var button = MakeButton(text, filter == activeFilter, 140);
            button.Name = "ArchiveTab" + filter;
            button.Click += (_, _) => { activeFilter = filter; UpdateTabs(); RenderTasks(); };
            tabButtons.Add(filter, button);
            tabsPanel.Controls.Add(button);
        }
        page.Controls.Add(tabsPanel);
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.Transparent;
        search.PlaceholderText = "Search archived tasks...";
        search.TextChanged += (_, _) => RenderTasks();
        toolbar.Controls.Add(search);
        ConfigureCombo(categoryFilter, ["All categories", "Cleaning", "Shopping", "Repairs", "Tutoring", "Delivery", "Pet Care"]);
        ConfigureCombo(sortFilter, ["Archived recently", "Archived oldest", "Highest amount", "Lowest amount"]);
        categoryFilter.SelectedIndexChanged += (_, _) => RenderTasks();
        sortFilter.SelectedIndexChanged += (_, _) => RenderTasks();
        toolbar.Controls.Add(categoryFilter);
        toolbar.Controls.Add(sortFilter);
        page.Controls.Add(toolbar);
    }

    private void BuildEmptyState()
    {
        emptyPanel.BackColor = Color.White;
        emptyPanel.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("▤", 19, Blue, true, new Point(0, 24));
        icon.AutoSize = false;
        icon.Size = new Size(48, 48);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.Location = new Point(0, 24);
        emptyPanel.Controls.Add(icon);
        emptyPanel.Controls.Add(MakeLabel("No archived tasks found", 13, Navy, true, new Point(0, 82)));
        emptyPanel.Controls.Add(MakeLabel("Try a different search, category, or archive status.", 8, Muted, false, new Point(0, 111)));
        var clear = MakeButton("Clear filters", false, 110);
        clear.Name = "ArchivedClearFiltersButton";
        clear.Click += (_, _) => ClearFilters();
        emptyPanel.Controls.Add(clear);
        page.Controls.Add(emptyPanel);
    }

    private void BuildArchiveNote()
    {
        notePanel.BackColor = Color.White;
        notePanel.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("i", 10, Blue, true, new Point(16, 15));
        icon.AutoSize = false;
        icon.Size = new Size(38, 38);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(234, 243, 255);
        notePanel.Controls.Add(icon);
        notePanel.Controls.Add(MakeLabel("Archived tasks stay private", 10, Navy, true, new Point(68, 13)));
        notePanel.Controls.Add(MakeLabel("Archiving removes a task from active lists; it does not publish it or expose private details. Completed task records can still be used for payment and review history.", 8, Muted, false, new Point(68, 39), new Size(700, 45)));
        page.Controls.Add(notePanel);
    }

    private List<ArchivedTask> AllArchived()
    {
        var merged = tasks.Concat(DemoDataStore.ArchivedTasks.Select(ToArchivedTask)).ToList();
        foreach (var outcome in DemoDataStore.TaskOutcomes)
        {
            merged.RemoveAll(task => task.Title == outcome.Title);
            merged.Insert(0, ToArchivedTask(outcome));
        }
        return merged;
    }

    private static ArchivedTask ToArchivedTask(DemoTaskOutcome outcome) => new(
        outcome.Title.ToLowerInvariant().Replace(" ", "-"),
        outcome.Title,
        outcome.Category,
        outcome.Amount,
        outcome.Barangay,
        outcome.Date,
        outcome.Date.ToString("MMM d, yyyy"),
        outcome.Status == "Completed" ? "completed" : "cancelled",
        string.IsNullOrEmpty(outcome.Reason) ? outcome.Status : outcome.Reason,
        outcome.Description,
        null,
        outcome.Icon,
        outcome.IconBack,
        outcome.IconBack,
        false,
        false);

    private static ArchivedTask ToArchivedTask(DemoTask task) => new(
        task.Title.ToLowerInvariant().Replace(" ", "-"),
        task.Title,
        task.Category,
        task.Price,
        task.Barangay,
        DateTime.Today,
        "",
        "cancelled",
        "Archived by you",
        task.Description,
        null,
        task.Icon,
        task.IconBack,
        task.IconBack,
        false,
        false);

    private void RenderTasks()
    {
        if (!pageBuilt && taskList.IsDisposed) return;
        taskList.SuspendLayout();
        taskList.Controls.Clear();
        var query = search.Text.Trim();
        var category = categoryFilter.SelectedIndex <= 0 ? "all" : categoryFilter.SelectedItem?.ToString() ?? "all";
        var filtered = AllArchived().Where(task =>
            (activeFilter == "all" || task.Status == activeFilter) &&
            (category == "all" || task.Category == category) &&
            (query.Length == 0 || $"{task.Title} {task.Category} {task.Location} {task.Reason} {task.Description}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        filtered = sortFilter.SelectedIndex switch
        {
            1 => filtered.OrderBy(task => task.ArchivedDate).ToList(),
            2 => filtered.OrderByDescending(task => task.Amount).ToList(),
            3 => filtered.OrderBy(task => task.Amount).ToList(),
            _ => filtered.OrderByDescending(task => task.ArchivedDate).ToList()
        };
        foreach (var task in filtered) taskList.Controls.Add(BuildTaskCard(task));
        taskList.Height = filtered.Count == 0 ? 0 : filtered.Sum(task => 154) + (filtered.Count - 1) * 12;
        emptyPanel.Visible = filtered.Count == 0;
        taskList.Visible = filtered.Count > 0;
        taskList.ResumeLayout();
        UpdateCounts();
        if (pageBuilt) LayoutPage();
    }

    private Panel BuildTaskCard(ArchivedTask task)
    {
        var card = new Panel { Name = "ArchiveCard" + task.Id, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Height = 154, Margin = new Padding(0, 0, 0, 12), Tag = task };
        var icon = MakeLabel(task.Icon, 19, task.IconColor, true, new Point(16, 16));
        icon.AutoSize = false;
        icon.Size = new Size(58, 58);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = task.IconBack;
        card.Controls.Add(icon);
        var statusText = task.Status switch { "completed" => "Completed", "cancelled" => "Cancelled", _ => "Expired" };
        var statusColor = task.Status switch { "completed" => Green, "cancelled" => Color.FromArgb(198, 77, 69), _ => Color.FromArgb(104, 119, 138) };
        card.Controls.Add(MakeLabel(statusText.ToUpperInvariant(), 7, statusColor, true, new Point(88, 13)));
        card.Controls.Add(MakeLabel(task.Title, 11, Navy, true, new Point(88, 31)));
        card.Controls.Add(MakeLabel($"₱{task.Amount:N0}", 12, Navy, true, new Point(0, 18)));
        card.Controls.Add(MakeLabel(task.Description, 8, Muted, false, new Point(88, 57), new Size(720, 32)));
        card.Controls.Add(MakeLabel($"⚒ {task.Category}     ⌖ Brgy. {task.Location}     ▣ {task.Reason}{(task.Rating is null ? "" : $"     ★ {task.Rating} rating")}", 7, Muted, false, new Point(88, 94), new Size(780, 20)));

        var buttons = new FlowLayoutPanel { Name = "archiveActions", Location = new Point(88, 117), Size = new Size(800, 30), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
        AddCardAction(buttons, "View details", () => Nav.Navigate(Route.FindTasks), false);
        if (task.Status == "completed")
        {
            if (task.HasPayment) AddCardAction(buttons, "Payment record", () => Nav.Navigate(Route.PaymentHistory), true);
            if (task.HasReview) AddCardAction(buttons, "Review", () => Nav.Navigate(new ReviewsPage()), true);
            AddCardAction(buttons, "Open dispute", () => OpenDispute(task), false);
        }
        AddCardAction(buttons, "Restore", () => RestoreTask(task), true);
        AddCardAction(buttons, "Remove", () => RemoveTask(task), false, true);
        card.Controls.Add(buttons);
        var archivedDate = MakeLabel($"Archived\r\n{task.ArchivedDate:MMM d, yyyy}", 7, Muted, false, Point.Empty);
        archivedDate.Name = "archiveDateLabel";
        archivedDate.TextAlign = ContentAlignment.TopRight;
        card.Controls.Add(archivedDate);
        return card;
    }

    private void AddCardAction(FlowLayoutPanel panel, string text, Action action, bool link, bool danger = false)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 27, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = danger ? Color.FromArgb(184, 84, 78) : link ? Blue : Navy, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 12, 0) };
        button.FlatAppearance.BorderColor = Line;
        button.FlatAppearance.BorderSize = link ? 0 : 1;
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void OpenDispute(ArchivedTask task)
    {
        using var dialog = new ReasonDialog(
            "Open dispute",
            $"What went wrong with \"{task.Title}\"?",
            ["Work incomplete", "No-show", "Poor quality", "Refund requested", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        var doer = DemoDataStore.RequestsReceived
            .Where(applicant => applicant.TaskTitle == task.Title && !applicant.Declined)
            .Select(applicant => applicant.Name)
            .FirstOrDefault() ?? "TaskTap member";

        DemoDataStore.OpenDispute(new DemoDispute
        {
            Id = DemoDataStore.NextDisputeId(),
            TaskTitle = task.Title,
            PosterName = AppSession.CurrentUser.FullName,
            DoerName = doer,
            Amount = task.Amount,
            Reason = dialog.SelectedReason ?? "Dispute opened.",
            Status = "Open",
            OpenedAt = DateTime.Today.ToString("MMM d, yyyy")
        });

        ShowToast("Dispute opened. Track it under My Disputes.");
    }

    private void RestoreTask(ArchivedTask task)    {
        if (MessageBox.Show(this, $"Restore “{task.Title}” to My Tasks?", "Restore task?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var storeTask = DemoDataStore.ArchivedTasks.FirstOrDefault(t => t.Title == task.Title);
        if (storeTask is not null) DemoDataStore.RestoreTask(storeTask);
        else tasks.Remove(task);
        RenderTasks();
        ShowToast($"{task.Title} restored to My Tasks.");
    }

    private void RemoveTask(ArchivedTask task)
    {
        if (MessageBox.Show(this, $"Remove “{task.Title}” from your archive? This cannot be undone.", "Remove archived task?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var storeTask = DemoDataStore.ArchivedTasks.FirstOrDefault(t => t.Title == task.Title);
        if (storeTask is not null) DemoDataStore.RemoveArchivedTask(storeTask);
        else tasks.Remove(task);
        RenderTasks();
        ShowToast("Archived task removed.");
    }

    private void UpdateCounts()
    {
        var all = AllArchived();
        totalCount.Text = all.Count.ToString();
        completedCount.Text = all.Count(task => task.Status == "completed").ToString();
        cancelledCount.Text = all.Count(task => task.Status == "cancelled").ToString();
        valueTotal.Text = $"₱{all.Sum(task => task.Amount):N0}";
        foreach (var (key, button) in tabButtons)
        {
            var count = key == "all" ? all.Count : all.Count(task => task.Status == key);
            var label = key switch { "all" => "All", "completed" => "Completed", "cancelled" => "Cancelled", _ => "Expired" };
            button.Text = $"{label}  {count}";
        }
    }

    private void UpdateTabs()
    {
        foreach (var (key, button) in tabButtons)
        {
            var active = key == activeFilter;
            button.BackColor = active ? Color.FromArgb(237, 245, 255) : Color.White;
            button.ForeColor = active ? Blue : Muted;
        }
    }

    private void ClearFilters()
    {
        search.Clear();
        categoryFilter.SelectedIndex = 0;
        sortFilter.SelectedIndex = 0;
        activeFilter = "all";
        UpdateTabs();
        RenderTasks();
        ShowToast("Filters cleared.");
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        const int margin = 22;
        var contentWidth = width - margin * 2;
        var back = page.Controls.Find("ArchivedBackButton", false).FirstOrDefault();
        if (back is not null) back.Location = new Point(width - margin - back.Width, 58);
        var statGap = 14;
        var statWidth = (contentWidth - statGap * 3) / 4;
        statsPanel.Bounds = new Rectangle(margin, 132, contentWidth, 122);
        for (var i = 0; i < statsPanel.Controls.Count; i++) statsPanel.Controls[i].Bounds = new Rectangle(i * (statWidth + statGap), 0, statWidth, 122);
        tabsPanel.Bounds = new Rectangle(margin, 268, contentWidth, 54);
        var tabWidth = Math.Min(160, (contentWidth - 20) / 4);
        for (var i = 0; i < tabsPanel.Controls.Count; i++) tabsPanel.Controls[i].Bounds = new Rectangle(8 + i * (tabWidth + 4), 7, tabWidth, 38);
        toolbar.Bounds = new Rectangle(margin, 334, contentWidth, 44);
        search.SetBounds(0, 0, Math.Max(260, contentWidth - 500), 42);
        categoryFilter.SetBounds(search.Right + 10, 0, 210, 42);
        sortFilter.SetBounds(categoryFilter.Right + 10, 0, 250, 42);
        var y = 392;
        taskList.SetBounds(margin, y, contentWidth, taskList.Height);
        foreach (var card in taskList.Controls.OfType<Panel>())
        {
            card.Width = contentWidth;
            card.Controls.Find("archiveDateLabel", false).FirstOrDefault()?.SetBounds(contentWidth - 145, 16, 125, 35);
            var amountLabel = card.Controls.OfType<Label>().ElementAtOrDefault(3);
            amountLabel?.SetBounds(contentWidth - 125, 18, 100, 24);
            var desc = card.Controls.OfType<Label>().FirstOrDefault(label => label.Location == new Point(88, 57));
            if (desc is not null) desc.Size = new Size(Math.Max(240, contentWidth - 260), 34);
            var meta = card.Controls.OfType<Label>().FirstOrDefault(label => label.Location == new Point(88, 94));
            if (meta is not null) meta.Size = new Size(Math.Max(240, contentWidth - 260), 20);
            var actions = card.Controls.Find("archiveActions", false).FirstOrDefault();
            if (actions is not null) actions.Width = contentWidth - 180;
        }
        emptyPanel.SetBounds(margin, y, contentWidth, 220);
        foreach (var control in emptyPanel.Controls.OfType<Control>())
        {
            if (control is Button button) button.Location = new Point((contentWidth - button.Width) / 2, 151);
            else if (control.Location.Y == 24) control.Location = new Point((contentWidth - control.Width) / 2, 24);
            else control.Left = (contentWidth - control.Width) / 2;
        }
        var listBottom = taskList.Visible ? taskList.Bottom : emptyPanel.Bottom;
        notePanel.Bounds = new Rectangle(margin, listBottom + 18, contentWidth, 102);
        notePanel.Controls.OfType<Label>().Last().Size = new Size(contentWidth - 115, 48);
        page.AutoScrollMinSize = new Size(0, notePanel.Bottom + 24);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void ConfigureCombo(ComboBox combo, string[] values)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 8F);
        combo.Items.AddRange(values);
        combo.SelectedIndex = 0;
    }

    private void ShowToast(string message)
    {
        var tooltip = new ToolTip { IsBalloon = true, ToolTipTitle = "TaskTap" };
        tooltip.Show(message, this, Width / 2, Height - 100, 2200);
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold, Point location, Size? dimensions = null) => new()
    {
        Text = text,
        AutoSize = dimensions is null,
        Location = location,
        Size = dimensions ?? Size.Empty,
        ForeColor = color,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        BackColor = Color.Transparent,
        AutoEllipsis = dimensions is not null
    };

    private Button MakeButton(string text, bool primary, int width)
    {
        var button = new Button { Text = text, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static Button MakeLink(string text)
    {
        var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, TabStop = false };
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(5, 2, 5, 2);
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
