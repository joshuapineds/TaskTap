namespace TaskTap;

/// <summary>
/// Activity Log: the audit trail written by every admin mutation, with search,
/// category/actor filters, sorting and pagination.
/// </summary>
public sealed class ActivityLogPage : AdminPage
{
    private const int PageSize = 12;

    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel tableCard = new();
    private readonly Panel headerRow = new();
    private readonly Panel tableBody = new();
    private readonly Panel pagination = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox categoryFilter = new();
    private readonly ComboBox actorFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label pageLabel = new();
    private readonly Button prevButton;
    private readonly Button nextButton;
    private readonly Label timeHeader = new();
    private readonly Label activityHeader = new();
    private readonly Label actorHeader = new();
    private readonly Label categoryHeader = new();
    private int pageIndex;
    private int _lastWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.ActivityLog;

    public ActivityLogPage()
    {
        prevButton = AdminUi.MakeSecondaryButton("← Previous", "activityPrevButton", 100);
        nextButton = AdminUi.MakeSecondaryButton("Next →", "activityNextButton", 100);
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("AUDIT", 8, AdminUi.Blue, true);
        eyebrow.Name = "activityEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Activity Log", 26, AdminUi.Navy, true);
        title.Name = "activityTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Every admin mutation is recorded here for accountability.", 9, AdminUi.Muted);
        subtitle.Name = "activitySubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        BuildToolbar();
        BuildTable();
        BuildPagination();

        RefreshList();
        LayoutPage();
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.White;
        toolbar.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(toolbar);

        searchBox.Name = "activitySearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search actor, action, target or detail…";
        searchBox.TextChanged += (_, _) => { pageIndex = 0; RefreshList(); };
        toolbar.Controls.Add(searchBox);

        ConfigureFilter(categoryFilter, "activityCategoryFilter", ["All categories", "User", "Task", "Payment", "Report", "Dispute", "Category", "Barangay", "Roles", "Settings"]);
        ConfigureFilter(actorFilter, "activityActorFilter", ["All actors", "TaskTap Admin", "System"]);
        ConfigureFilter(sortFilter, "activitySortFilter", ["Newest first", "Oldest first"]);

        var reset = AdminUi.MakeLinkButton("Reset filters", "activityResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            categoryFilter.SelectedIndex = 0;
            actorFilter.SelectedIndex = 0;
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
        combo.SelectedIndexChanged += (_, _) => { pageIndex = 0; RefreshList(); };
        toolbar.Controls.Add(combo);
    }

    private void BuildTable()
    {
        tableCard.BackColor = Color.White;
        tableCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(tableCard);

        headerRow.BackColor = Color.FromArgb(240, 245, 250);
        headerRow.BorderStyle = BorderStyle.FixedSingle;
        tableCard.Controls.Add(headerRow);

        timeHeader.Text = "Time";
        timeHeader.AutoSize = true;
        timeHeader.ForeColor = AdminUi.Muted;
        timeHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        timeHeader.Location = new Point(12, 7);
        headerRow.Controls.Add(timeHeader);

        activityHeader.Text = "Activity";
        activityHeader.AutoSize = true;
        activityHeader.ForeColor = AdminUi.Muted;
        activityHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        activityHeader.Location = new Point(120, 7);
        headerRow.Controls.Add(activityHeader);

        actorHeader.Text = "Actor";
        actorHeader.AutoSize = true;
        actorHeader.ForeColor = AdminUi.Muted;
        actorHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        headerRow.Controls.Add(actorHeader);

        categoryHeader.Text = "Category";
        categoryHeader.AutoSize = true;
        categoryHeader.ForeColor = AdminUi.Muted;
        categoryHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        headerRow.Controls.Add(categoryHeader);

        tableBody.BackColor = Color.White;
        tableBody.AutoScroll = true;
        tableCard.Controls.Add(tableBody);
    }

    private void BuildPagination()
    {
        page.Controls.Add(pagination);
        pagination.BackColor = Color.Transparent;

        prevButton.Click += (_, _) => { if (pageIndex > 0) { pageIndex--; RefreshList(); } };
        pagination.Controls.Add(prevButton);

        nextButton.Click += (_, _) => { pageIndex++; RefreshList(); };
        pagination.Controls.Add(nextButton);

        pageLabel.AutoSize = true;
        pageLabel.ForeColor = AdminUi.Muted;
        pageLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        pagination.Controls.Add(pageLabel);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "activityEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "activityTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "activitySubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        toolbar.Bounds = new Rectangle(left, 130, contentWidth, 68);
        searchBox.Bounds = new Rectangle(12, 18, 240, 32);
        categoryFilter.Bounds = new Rectangle(264, 18, 150, 32);
        actorFilter.Bounds = new Rectangle(426, 18, 160, 32);
        sortFilter.Bounds = new Rectangle(598, 18, 150, 32);
        var reset = AdminUi.Find<Button>(toolbar, "activityResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - 12, 24);

        tableCard.Bounds = new Rectangle(left, 212, contentWidth, 640);
        headerRow.Bounds = new Rectangle(14, 14, tableCard.Width - 28, 30);
        tableBody.Bounds = new Rectangle(14, 48, tableCard.Width - 28, tableCard.Height - 60);

        var width = tableCard.Width - 28;
        actorHeader.Location = new Point(width - 430, 7);
        categoryHeader.Location = new Point(width - 150, 7);

        pagination.Bounds = new Rectangle(left, tableCard.Bottom + 8, contentWidth, 36);
        pageLabel.Location = new Point(contentWidth - pageLabel.Width - 220, 10);
        prevButton.Location = new Point(contentWidth - 210, 2);
        nextButton.Location = new Point(contentWidth - 104, 2);

        page.AutoScrollMinSize = new Size(0, pagination.Bottom + 40);

        if (tableBody.Width != _lastWidth)
        {
            _lastWidth = tableBody.Width;
            RefreshList();
        }
        else
        {
            LayoutRows();
        }
    }

    private void RefreshList()
    {
        if (tableBody.Width <= 0)
            return;

        var query = searchBox.Text.Trim();
        var category = categoryFilter.SelectedItem?.ToString() ?? "All categories";
        var actor = actorFilter.SelectedItem?.ToString() ?? "All actors";
        var sort = sortFilter.SelectedItem?.ToString() ?? "Newest first";

        var filtered = DemoDataStore.ActivityLog
            .Where(item => category == "All categories" || item.Category == category)
            .Where(item => actor == "All actors" || item.Actor == actor)
            .Where(item => string.IsNullOrEmpty(query) ||
                           $"{item.Actor} {item.Action} {item.Target} {item.Detail}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered = sort == "Oldest first"
            ? filtered.OrderBy(item => item.Timestamp).ToList()
            : filtered.OrderByDescending(item => item.Timestamp).ToList();

        var pageCount = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        pageIndex = Math.Clamp(pageIndex, 0, pageCount - 1);
        var visible = filtered.Skip(pageIndex * PageSize).Take(PageSize).ToList();

        tableBody.SuspendLayout();
        tableBody.Controls.Clear();

        var width = Math.Max(420, tableBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var item in visible)
        {
            var row = BuildRow(item, width);
            row.SetBounds(0, y, width, 64);
            tableBody.Controls.Add(row);
            y += 70;
        }

        if (visible.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No activity matches these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            tableBody.Controls.Add(empty);
        }

        tableBody.ResumeLayout();

        var first = filtered.Count == 0 ? 0 : pageIndex * PageSize + 1;
        var last = Math.Min(filtered.Count, (pageIndex + 1) * PageSize);
        pageLabel.Text = $"Page {pageIndex + 1} of {pageCount} · showing {first}–{last} of {filtered.Count}";
        prevButton.Enabled = pageIndex > 0;
        nextButton.Enabled = pageIndex < pageCount - 1;

        LayoutRows();
    }

    private void LayoutRows()
    {
        var width = Math.Max(420, tableBody.ClientSize.Width - 2);
        foreach (var row in tableBody.Controls.OfType<Panel>())
        {
            var action = AdminUi.Find<Label>(row, "activityRowAction");
            if (action is not null) action.MaximumSize = new Size(Math.Max(200, width - 560), 18);
            var detail = AdminUi.Find<Label>(row, "activityRowDetail");
            if (detail is not null) detail.MaximumSize = new Size(Math.Max(200, width - 560), 18);
        }
    }

    private Panel BuildRow(DemoActivity item, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var time = AdminUi.MakeLabel(item.Timestamp.ToString("MMM d, h:mm tt"), 8, AdminUi.Muted);
        time.Location = new Point(12, 12);
        row.Controls.Add(time);

        var action = AdminUi.MakeLabel(item.Action, 9.5F, AdminUi.Navy, true);
        action.Name = "activityRowAction";
        action.AutoEllipsis = true;
        action.MaximumSize = new Size(Math.Max(200, width - 560), 18);
        action.Location = new Point(120, 10);
        row.Controls.Add(action);

        var detail = AdminUi.MakeLabel($"{item.Target} · {item.Detail}", 8, AdminUi.Muted);
        detail.Name = "activityRowDetail";
        detail.AutoEllipsis = true;
        detail.MaximumSize = new Size(Math.Max(200, width - 560), 18);
        detail.Location = new Point(120, 30);
        row.Controls.Add(detail);

        var actor = AdminUi.MakeLabel(item.Actor, 8, AdminUi.Navy, true);
        actor.Location = new Point(width - 430, 12);
        row.Controls.Add(actor);

        var category = AdminUi.MakePill(item.Category, Color.FromArgb(240, 245, 250), AdminUi.Navy);
        category.Location = new Point(width - 140, 16);
        row.Controls.Add(category);

        return row;
    }

    public override void OnNavigatedTo() => RefreshList();
}
