namespace TaskTap;

/// <summary>
/// Task Management: browse every listing (available, posted, archived), filter and
/// sort, inspect a task, feature/pause/archive/restore/delete, and act on selections.
/// </summary>
public sealed class TaskManagementPage : AdminPage
{
    private sealed record TaskEntry(DemoTask Task, string Status);

    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Panel detailCard = new();
    private readonly Panel detailBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox categoryFilter = new();
    private readonly ComboBox barangayFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label selectedLabel = new();
    private readonly Label countLabel = new();
    private readonly HashSet<DemoTask> selected = new();
    private TaskEntry? selectedEntry;
    private int _lastListWidth = -1;
    private int _lastDetailWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Tasks;

    public TaskManagementPage()
    {
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("MARKETPLACE OPERATIONS", 8, AdminUi.Blue, true);
        eyebrow.Name = "tasksEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Task Management", 26, AdminUi.Navy, true);
        title.Name = "tasksTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Moderate every listing across the marketplace, posted and archived queues.", 9, AdminUi.Muted);
        subtitle.Name = "tasksSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        BuildToolbar();
        BuildListCard();
        BuildDetailCard();

        RefreshList();
        LayoutPage();
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.White;
        toolbar.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(toolbar);

        searchBox.Name = "tasksSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search title, category, barangay…";
        searchBox.TextChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(searchBox);

        ConfigureFilter(statusFilter, "tasksStatusFilter", ["All statuses", "Available", "Posted", "Archived", "Featured", "Paused", "Reported"]);
        ConfigureFilter(categoryFilter, "tasksCategoryFilter", ["All categories", .. DemoDataStore.Categories.OrderBy(c => c.SortOrder).Select(c => c.Name)]);
        ConfigureFilter(barangayFilter, "tasksBarangayFilter", ["All barangays", .. DemoDataStore.Barangays.OrderBy(b => b.Id).Select(b => b.Name)]);
        ConfigureFilter(sortFilter, "tasksSortFilter", ["Deadline soonest", "Price high → low", "Price low → high", "Title A–Z"]);

        var reset = AdminUi.MakeLinkButton("Reset filters", "tasksResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            statusFilter.SelectedIndex = 0;
            categoryFilter.SelectedIndex = 0;
            barangayFilter.SelectedIndex = 0;
            sortFilter.SelectedIndex = 0;
        };
        toolbar.Controls.Add(reset);
    }

    private void ConfigureFilter(ComboBox combo, string name, string[] items)
    {
        combo.Name = name;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 9F);
        combo.FlatStyle = FlatStyle.Flat;
        combo.Items.AddRange(items);
        combo.SelectedIndex = 0;
        combo.SelectedIndexChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(combo);
    }

    private void BuildListCard()
    {
        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var title = AdminUi.MakeLabel("Tasks", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        listCard.Controls.Add(title);

        countLabel.AutoSize = true;
        countLabel.ForeColor = AdminUi.Muted;
        countLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        countLabel.Location = new Point(74, 16);
        listCard.Controls.Add(countLabel);

        selectedLabel.AutoSize = true;
        selectedLabel.ForeColor = AdminUi.Muted;
        selectedLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        selectedLabel.Location = new Point(18, 42);
        listCard.Controls.Add(selectedLabel);

        var archive = AdminUi.MakeSecondaryButton("Archive", "tasksBulkArchive", 90);
        archive.Size = new Size(90, 28);
        archive.Location = new Point(150, 38);
        archive.Click += (_, _) => BulkArchive();
        listCard.Controls.Add(archive);

        var restore = AdminUi.MakeSecondaryButton("Restore", "tasksBulkRestore", 90);
        restore.Size = new Size(90, 28);
        restore.Location = new Point(248, 38);
        restore.Click += (_, _) => BulkRestore();
        listCard.Controls.Add(restore);

        var delete = AdminUi.MakeSecondaryButton("Delete", "tasksBulkDelete", 90);
        delete.Size = new Size(90, 28);
        delete.ForeColor = AdminUi.Bad;
        delete.Location = new Point(346, 38);
        delete.Click += (_, _) => BulkDelete();
        listCard.Controls.Add(delete);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);
    }

    private void BuildDetailCard()
    {
        detailCard.BackColor = Color.White;
        detailCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(detailCard);

        var title = AdminUi.MakeLabel("Task detail", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        detailCard.Controls.Add(title);

        detailBody.BackColor = Color.White;
        detailCard.Controls.Add(detailBody);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "tasksEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "tasksTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "tasksSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        toolbar.Bounds = new Rectangle(left, 130, contentWidth, 72);
        searchBox.Bounds = new Rectangle(12, 18, 220, 32);
        statusFilter.Bounds = new Rectangle(244, 18, 120, 32);
        categoryFilter.Bounds = new Rectangle(376, 18, 140, 32);
        barangayFilter.Bounds = new Rectangle(528, 18, 140, 32);
        sortFilter.Bounds = new Rectangle(680, 18, 150, 32);
        var reset = AdminUi.Find<Button>(toolbar, "tasksResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - 12, 24);

        var rightWidth = Math.Max(340, Math.Min(430, contentWidth * 34 / 100));
        var mainWidth = contentWidth - rightWidth - 18;

        listCard.Bounds = new Rectangle(left, 216, mainWidth, 586);
        listBody.Bounds = new Rectangle(14, 76, listCard.Width - 28, listCard.Height - 88);

        detailCard.Bounds = new Rectangle(left + mainWidth + 18, 216, rightWidth, 586);
        detailBody.Bounds = new Rectangle(14, 46, detailCard.Width - 28, detailCard.Height - 58);

        page.AutoScrollMinSize = new Size(0, 816);

        if (listBody.Width != _lastListWidth)
        {
            _lastListWidth = listBody.Width;
            RefreshList();
        }

        if (detailBody.Width != _lastDetailWidth)
        {
            _lastDetailWidth = detailBody.Width;
            BuildDetail();
        }
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        var query = searchBox.Text.Trim();
        var status = statusFilter.SelectedItem?.ToString() ?? "All statuses";
        var category = categoryFilter.SelectedItem?.ToString() ?? "All categories";
        var barangay = barangayFilter.SelectedItem?.ToString() ?? "All barangays";
        var sort = sortFilter.SelectedItem?.ToString() ?? "Deadline soonest";

        var reportedTitles = DemoDataStore.Reports
            .Where(report => report.TargetType == "Task" && report.Status is "Open" or "Investigating")
            .Select(report => report.TargetLabel)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entries = new List<TaskEntry>();
        entries.AddRange(DemoDataStore.AvailableTasks.Select(task => new TaskEntry(task, "Available")));
        entries.AddRange(DemoDataStore.PostedTasks.Select(task => new TaskEntry(task, "Posted")));
        entries.AddRange(DemoDataStore.ArchivedTasks.Select(task => new TaskEntry(task, "Archived")));

        var filtered = entries
            .Where(entry => status switch
            {
                "Available" => entry.Status == "Available",
                "Posted" => entry.Status == "Posted",
                "Archived" => entry.Status == "Archived",
                "Featured" => DemoDataStore.FeaturedTasks.Contains(entry.Task),
                "Paused" => DemoDataStore.PausedTasks.Contains(entry.Task),
                "Reported" => reportedTitles.Contains(entry.Task.Title),
                _ => true
            })
            .Where(entry => category == "All categories" || entry.Task.Category == category)
            .Where(entry => barangay == "All barangays" || $"Brgy. {entry.Task.Barangay}" == barangay)
            .Where(entry => string.IsNullOrEmpty(query) ||
                            $"{entry.Task.Title} {entry.Task.Category} {entry.Task.Barangay} {entry.Task.Description}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered = sort switch
        {
            "Price high → low" => filtered.OrderByDescending(entry => entry.Task.Price).ToList(),
            "Price low → high" => filtered.OrderBy(entry => entry.Task.Price).ToList(),
            "Title A–Z" => filtered.OrderBy(entry => entry.Task.Title).ToList(),
            _ => filtered.OrderBy(entry => ParseDeadline(entry.Task.Deadline)).ToList()
        };

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var entry in filtered)
        {
            var row = BuildRow(entry, width);
            row.SetBounds(0, y, width, 76);
            listBody.Controls.Add(row);
            y += 82;
        }

        if (filtered.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No tasks match these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();

        countLabel.Text = $"{filtered.Count} task{(filtered.Count == 1 ? "" : "s")}";
        selected.RemoveWhere(task => !entries.Any(entry => ReferenceEquals(entry.Task, task)));
        UpdateBulkBar();

        if (selectedEntry is not null && !filtered.Any(entry => ReferenceEquals(entry.Task, selectedEntry.Task)))
            selectedEntry = null;
        BuildDetail();
    }

    private Panel BuildRow(TaskEntry entry, int width)
    {
        var task = entry.Task;
        var row = new Panel
        {
            BackColor = ReferenceEquals(selectedEntry?.Task, task) ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(entry);

        var check = new CheckBox
        {
            Location = new Point(12, 30),
            Size = new Size(18, 18),
            Checked = selected.Contains(task),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        check.CheckedChanged += (_, _) =>
        {
            if (check.Checked)
                selected.Add(task);
            else
                selected.Remove(task);
            UpdateBulkBar();
        };
        row.Controls.Add(check);

        var icon = new Label
        {
            Text = task.Icon,
            Size = new Size(44, 44),
            Location = new Point(40, 16),
            BackColor = task.IconBack,
            ForeColor = AdminUi.Navy,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Emoji", 16F)
        };
        row.Controls.Add(icon);

        var title = AdminUi.MakeLabel(task.Title, 10, AdminUi.Navy, true);
        title.Location = new Point(94, 12);
        row.Controls.Add(title);

        var meta = AdminUi.MakeLabel($"{task.Category} · Brgy. {task.Barangay} · {task.Deadline} · ₱{task.Price:N0}", 8, AdminUi.Muted);
        meta.Name = "taskRowMeta";
        meta.MaximumSize = new Size(Math.Max(240, width - 440), 18);
        meta.AutoEllipsis = true;
        meta.Location = new Point(94, 32);
        row.Controls.Add(meta);

        var pills = new FlowLayoutPanel
        {
            Name = "taskRowPills",
            Size = new Size(320, 26),
            WrapContents = false,
            BackColor = Color.Transparent
        };
        pills.Controls.Add(StatusPill(entry.Status));
        if (DemoDataStore.FeaturedTasks.Contains(task))
            pills.Controls.Add(TagPill("Featured", Color.FromArgb(255, 244, 230), AdminUi.Warn));
        if (DemoDataStore.PausedTasks.Contains(task))
            pills.Controls.Add(TagPill("Paused", Color.FromArgb(255, 247, 224), Color.FromArgb(180, 130, 20)));
        row.Controls.Add(pills);

        var actions = new FlowLayoutPanel
        {
            Name = "taskRowActions",
            Size = new Size(320, 26),
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var view = AdminUi.MakeLinkButton("View", $"taskView{task.Title}");
        view.Margin = new Padding(0, 0, 10, 0);
        view.Click += (_, _) => Select(entry);
        actions.Controls.Add(view);

        var archive = AdminUi.MakeLinkButton(entry.Status == "Archived" ? "Restore" : "Archive", $"taskArchive{task.Title}");
        archive.Margin = new Padding(0, 0, 10, 0);
        archive.Click += (_, _) =>
        {
            if (entry.Status == "Archived")
                DemoDataStore.RestoreAdminTask(task);
            else
                DemoDataStore.ArchiveAdminTask(task);
            RefreshList();
        };
        actions.Controls.Add(archive);

        var delete = AdminUi.MakeLinkButton("Delete", $"taskDelete{task.Title}");
        delete.ForeColor = AdminUi.Bad;
        delete.Click += (_, _) => DeleteTask(task);
        actions.Controls.Add(delete);

        row.Controls.Add(actions);
        return row;
    }

    private static Label StatusPill(string status) => AdminUi.MakePill(
        status,
        status switch
        {
            "Available" => Color.FromArgb(236, 244, 255),
            "Posted" => Color.FromArgb(232, 248, 241),
            _ => Color.FromArgb(240, 243, 247)
        },
        status switch
        {
            "Available" => AdminUi.Blue,
            "Posted" => AdminUi.Good,
            _ => AdminUi.Muted
        });

    private static Label TagPill(string text, Color back, Color fore)
    {
        var pill = AdminUi.MakePill(text, back, fore);
        pill.Margin = new Padding(8, 0, 0, 0);
        return pill;
    }

    private void Select(TaskEntry entry)
    {
        selectedEntry = entry;
        RefreshList();
    }

    private void BuildDetail()
    {
        if (detailBody.Width <= 0)
            return;

        detailBody.SuspendLayout();
        detailBody.Controls.Clear();

        if (selectedEntry is null)
        {
            var empty = AdminUi.MakeLabel("Select a task to inspect it.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            detailBody.Controls.Add(empty);
            detailBody.ResumeLayout();
            return;
        }

        var entry = selectedEntry;
        var task = entry.Task;
        var width = detailBody.ClientSize.Width;

        var icon = new Label
        {
            Text = task.Icon,
            Size = new Size(56, 56),
            Location = new Point(0, 0),
            BackColor = task.IconBack,
            ForeColor = AdminUi.Navy,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Emoji", 20F)
        };
        detailBody.Controls.Add(icon);

        var title = AdminUi.MakeLabel(task.Title, 12, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 76), 0);
        title.Location = new Point(72, 2);
        detailBody.Controls.Add(title);

        var meta = AdminUi.MakeLabel($"{task.Category} · Brgy. {task.Barangay}", 8, AdminUi.Muted);
        meta.Location = new Point(72, 30);
        detailBody.Controls.Add(meta);

        var price = AdminUi.MakeLabel($"₱{task.Price:N0}", 14, AdminUi.Navy, true);
        price.Location = new Point(72, 48);
        detailBody.Controls.Add(price);

        var pillRow = new FlowLayoutPanel
        {
            Location = new Point(0, 76),
            Size = new Size(width, 28),
            WrapContents = false,
            BackColor = Color.Transparent
        };
        pillRow.Controls.Add(StatusPill(entry.Status));
        if (DemoDataStore.FeaturedTasks.Contains(task))
            pillRow.Controls.Add(TagPill("Featured", Color.FromArgb(255, 244, 230), AdminUi.Warn));
        if (DemoDataStore.PausedTasks.Contains(task))
            pillRow.Controls.Add(TagPill("Paused", Color.FromArgb(255, 247, 224), Color.FromArgb(180, 130, 20)));
        detailBody.Controls.Add(pillRow);

        var description = AdminUi.MakeWrappedLabel(task.Description, 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        description.Location = new Point(0, 112);
        detailBody.Controls.Add(description);

        var deadline = AdminUi.MakeLabel($"Deadline: {task.Deadline}", 8, AdminUi.Muted, true);
        deadline.Location = new Point(0, 168);
        detailBody.Controls.Add(deadline);

        var feature = AdminUi.MakeSecondaryButton(DemoDataStore.FeaturedTasks.Contains(task) ? "Unfeature" : "Feature", "taskDetailFeature", 160);
        feature.Location = new Point(0, 200);
        feature.Click += (_, _) => { DemoDataStore.ToggleFeatured(task); RefreshList(); };
        detailBody.Controls.Add(feature);

        var pause = AdminUi.MakeSecondaryButton(DemoDataStore.PausedTasks.Contains(task) ? "Resume" : "Pause", "taskDetailPause", 160);
        pause.Location = new Point(172, 200);
        pause.Click += (_, _) => { DemoDataStore.TogglePaused(task); RefreshList(); };
        detailBody.Controls.Add(pause);

        var archive = AdminUi.MakeSecondaryButton(entry.Status == "Archived" ? "Restore task" : "Archive task", "taskDetailArchive", 160);
        archive.Location = new Point(0, 248);
        archive.Click += (_, _) =>
        {
            if (entry.Status == "Archived")
                DemoDataStore.RestoreAdminTask(task);
            else
                DemoDataStore.ArchiveAdminTask(task);
            RefreshList();
        };
        detailBody.Controls.Add(archive);

        var delete = AdminUi.MakeSecondaryButton("Delete task", "taskDetailDelete", 160);
        delete.ForeColor = AdminUi.Bad;
        delete.Location = new Point(172, 248);
        delete.Click += (_, _) => DeleteTask(task);
        detailBody.Controls.Add(delete);

        var open = AdminUi.MakeLinkButton("View in user app →", "taskDetailOpenApp");
        open.Location = new Point(0, 300);
        open.Click += (_, _) => Toast("Members open this task from Find Tasks in the TaskTap app.");
        detailBody.Controls.Add(open);

        detailBody.ResumeLayout();
    }

    private void DeleteTask(DemoTask task)
    {
        var answer = MessageBox.Show(
            this,
            $"Delete \"{task.Title}\"?\n\nThis permanently removes the listing and its requests.",
            "Delete task",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        DemoDataStore.DeleteTask(task);
        selected.Remove(task);
        selectedEntry = null;
        RefreshList();
    }

    private void BulkArchive()
    {
        foreach (var task in selected.Where(task => !DemoDataStore.ArchivedTasks.Contains(task)).ToList())
            DemoDataStore.ArchiveAdminTask(task);
        selected.Clear();
        RefreshList();
    }

    private void BulkRestore()
    {
        foreach (var task in selected.Where(task => DemoDataStore.ArchivedTasks.Contains(task)).ToList())
            DemoDataStore.RestoreAdminTask(task);
        selected.Clear();
        RefreshList();
    }

    private void BulkDelete()
    {
        if (selected.Count == 0)
            return;

        var answer = MessageBox.Show(
            this,
            $"Delete {selected.Count} selected task{(selected.Count == 1 ? "" : "s")}?",
            "Delete tasks",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        foreach (var task in selected.ToList())
            DemoDataStore.DeleteTask(task);
        selected.Clear();
        selectedEntry = null;
        RefreshList();
    }

    private void UpdateBulkBar()
    {
        selectedLabel.Text = selected.Count == 0 ? "No tasks selected" : $"{selected.Count} selected";
    }

    private static DateTime ParseDeadline(string deadline) =>
        DateTime.TryParse(deadline, out var date) ? date : DateTime.MaxValue;

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
