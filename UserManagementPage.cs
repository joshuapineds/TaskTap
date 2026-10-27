namespace TaskTap;

/// <summary>
/// User Management: search, filters, sorting, pagination, bulk suspend/verify and
/// row actions over the shared <see cref="DemoDataStore.Users"/> list.
/// </summary>
public sealed class UserManagementPage : AdminPage
{
    private const int PageSize = 8;
    private const int RowHeight = 88;

    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel bulkBar = new();
    private readonly Panel headerRow = new();
    private readonly Panel rowsHost = new();
    private readonly Panel pagination = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox roleFilter = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox barangayFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label resultLabel = new();
    private readonly Label selectedLabel = new();
    private readonly Label pageLabel = new();
    private readonly Button prevButton;
    private readonly Button nextButton;
    private readonly Button bulkSuspend;
    private readonly Button bulkVerify;
    private readonly HashSet<string> selectedIds = new();
    private readonly List<Panel> rowPanels = new();
    private int pageIndex;

    public override AdminRoute AdminRouteName => AdminRoute.Users;

    public UserManagementPage()
    {
        prevButton = AdminUi.MakeSecondaryButton("← Previous", "UsersPrevButton", 100);
        nextButton = AdminUi.MakeSecondaryButton("Next →", "UsersNextButton", 100);
        bulkSuspend = AdminUi.MakeSecondaryButton("Restrict selected", "UsersBulkSuspend", 140);
        bulkVerify = AdminUi.MakeSecondaryButton("Verify selected", "UsersBulkVerify", 130);
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("USER DIRECTORY", 8, AdminUi.Blue, true);
        eyebrow.Name = "usersEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("User Management", 26, AdminUi.Navy, true);
        title.Name = "usersTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Review accounts, verify members and keep the community safe.", 9, AdminUi.Muted);
        subtitle.Name = "usersSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var verification = AdminUi.MakeSecondaryButton("Verification queue", "usersVerificationQueue", 180);
        verification.Click += (_, _) => Nav.Navigate(AdminRoute.VerificationQueue);
        page.Controls.Add(verification);

        BuildToolbar();

        headerRow.BackColor = Color.FromArgb(240, 245, 250);
        headerRow.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(headerRow);
        var memberHeader = AdminUi.MakeLabel("Member", 8, AdminUi.Muted, true);
        memberHeader.Location = new Point(94, 9);
        headerRow.Controls.Add(memberHeader);
        var statusHeader = AdminUi.MakeLabel("Status", 8, AdminUi.Muted, true);
        statusHeader.Name = "usersStatusHeader";
        headerRow.Controls.Add(statusHeader);
        var actionsHeader = AdminUi.MakeLabel("Actions", 8, AdminUi.Muted, true);
        actionsHeader.Name = "usersActionsHeader";
        headerRow.Controls.Add(actionsHeader);

        rowsHost.BackColor = AdminUi.Page;
        rowsHost.AutoScroll = true;
        page.Controls.Add(rowsHost);

        BuildPagination();

        RefreshRows();
        LayoutPage();
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.White;
        toolbar.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(toolbar);

        searchBox.Name = "usersSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search name, email, role or barangay…";
        searchBox.TextChanged += (_, _) => { pageIndex = 0; RefreshRows(); };
        toolbar.Controls.Add(searchBox);

        ConfigureFilter(roleFilter, "usersRoleFilter", ["All roles", "Standard User", "Task Doer", "Administrator"]);
        ConfigureFilter(statusFilter, "usersStatusFilter", ["All statuses", "Active", "Suspended", "Unverified", "Deactivated"]);
        ConfigureFilter(barangayFilter, "usersBarangayFilter", ["All barangays", .. DemoDataStore.Barangays.OrderBy(b => b.Id).Select(b => b.Name)]);
        ConfigureFilter(sortFilter, "usersSortFilter", ["Newest first", "Most tasks", "Highest rating", "Name A–Z"]);

        var reset = AdminUi.MakeLinkButton("Reset filters", "usersResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            roleFilter.SelectedIndex = 0;
            statusFilter.SelectedIndex = 0;
            barangayFilter.SelectedIndex = 0;
            sortFilter.SelectedIndex = 0;
        };
        toolbar.Controls.Add(reset);

        bulkBar.BackColor = Color.FromArgb(247, 251, 255);
        toolbar.Controls.Add(bulkBar);

        selectedLabel.Text = "No members selected";
        selectedLabel.AutoSize = true;
        selectedLabel.ForeColor = AdminUi.Muted;
        selectedLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        bulkBar.Controls.Add(selectedLabel);

        bulkSuspend.Enabled = false;
        bulkSuspend.Size = new Size(140, 28);
        bulkSuspend.Click += (_, _) => BulkAction(suspend: true);
        bulkBar.Controls.Add(bulkSuspend);

        bulkVerify.Enabled = false;
        bulkVerify.Size = new Size(130, 28);
        bulkVerify.Click += (_, _) => BulkAction(suspend: false);
        bulkBar.Controls.Add(bulkVerify);

        resultLabel.AutoSize = true;
        resultLabel.ForeColor = AdminUi.Muted;
        resultLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        bulkBar.Controls.Add(resultLabel);
    }

    private void ConfigureFilter(ComboBox combo, string name, string[] items)
    {
        combo.Name = name;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 9F);
        combo.FlatStyle = FlatStyle.Flat;
        combo.Items.AddRange(items);
        combo.SelectedIndex = 0;
        combo.SelectedIndexChanged += (_, _) => { pageIndex = 0; RefreshRows(); };
        toolbar.Controls.Add(combo);
    }

    private void BuildPagination()
    {
        page.Controls.Add(pagination);
        pagination.BackColor = Color.Transparent;

        prevButton.Click += (_, _) => { if (pageIndex > 0) { pageIndex--; RefreshRows(); } };
        pagination.Controls.Add(prevButton);

        nextButton.Click += (_, _) => { pageIndex++; RefreshRows(); };
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

        var eyebrow = AdminUi.Find<Label>(page, "usersEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "usersTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "usersSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var verification = AdminUi.Find<Button>(page, "usersVerificationQueue");
        if (verification is not null) verification.Location = new Point(left + contentWidth - 180, 40);

        toolbar.Bounds = new Rectangle(left, 130, contentWidth, 100);
        const int inner = 12;
        var rowOne = 12;
        searchBox.Bounds = new Rectangle(inner, rowOne, 260, 34);
        roleFilter.Bounds = new Rectangle(inner + 272, rowOne, 130, 34);
        statusFilter.Bounds = new Rectangle(inner + 412, rowOne, 130, 34);
        barangayFilter.Bounds = new Rectangle(inner + 552, rowOne, 150, 34);
        sortFilter.Bounds = new Rectangle(inner + 712, rowOne, 170, 34);
        var reset = AdminUi.Find<Button>(toolbar, "usersResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - inner, rowOne + 6);

        bulkBar.Bounds = new Rectangle(inner, 58, toolbar.Width - inner * 2, 32);
        selectedLabel.Location = new Point(10, 7);
        bulkSuspend.Location = new Point(160, 2);
        bulkVerify.Location = new Point(308, 2);
        resultLabel.Location = new Point(Math.Max(440, bulkBar.Width - resultLabel.Width - 10), 7);

        headerRow.Bounds = new Rectangle(left, 242, contentWidth, 34);
        var statusHeader = AdminUi.Find<Label>(headerRow, "usersStatusHeader");
        if (statusHeader is not null) statusHeader.Location = new Point(headerRow.Width - 370, 9);
        var actionsHeader = AdminUi.Find<Label>(headerRow, "usersActionsHeader");
        if (actionsHeader is not null) actionsHeader.Location = new Point(headerRow.Width - 260, 9);

        var rowsHeight = Math.Max(RowHeight, rowPanels.Count * RowHeight);
        rowsHost.Bounds = new Rectangle(left, 280, contentWidth, rowsHeight);
        LayoutRows();

        pagination.Bounds = new Rectangle(left, rowsHost.Bottom + 8, contentWidth, 36);
        pageLabel.Location = new Point(contentWidth - pageLabel.Width - 220, 10);
        prevButton.Location = new Point(contentWidth - 210, 2);
        nextButton.Location = new Point(contentWidth - 104, 2);

        page.AutoScrollMinSize = new Size(0, pagination.Bottom + 40);
    }

    private void LayoutRows()
    {
        var width = Math.Max(600, rowsHost.ClientSize.Width - 2);
        for (var index = 0; index < rowPanels.Count; index++)
        {
            var row = rowPanels[index];
            row.Bounds = new Rectangle(0, index * RowHeight, width, RowHeight - 4);

            var status = AdminUi.Find<Label>(row, "userStatus");
            if (status is not null) status.Location = new Point(width - 370, 16);

            var actions = AdminUi.Find<FlowLayoutPanel>(row, "userActions");
            if (actions is not null) actions.Location = new Point(width - 372, 44);

            var meta = AdminUi.Find<Label>(row, "userMeta");
            if (meta is not null) meta.MaximumSize = new Size(Math.Max(240, width - 480), 20);
        }
    }

    private void RefreshRows()
    {
        var query = searchBox.Text.Trim();
        var role = roleFilter.SelectedItem?.ToString() ?? "All roles";
        var status = statusFilter.SelectedItem?.ToString() ?? "All statuses";
        var barangay = barangayFilter.SelectedItem?.ToString() ?? "All barangays";
        var sort = sortFilter.SelectedItem?.ToString() ?? "Newest first";

        var filtered = DemoDataStore.Users
            .Where(user => role == "All roles" || user.Role == role)
            .Where(user => status == "All statuses" || user.Status == status)
            .Where(user => barangay == "All barangays" || $"Brgy. {user.Barangay}" == barangay)
            .Where(user => string.IsNullOrEmpty(query) ||
                           $"{user.FullName} {user.Email} {user.Role} {user.Barangay}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered = sort switch
        {
            "Most tasks" => filtered.OrderByDescending(user => user.CompletedTasks + user.PostedTasks).ToList(),
            "Highest rating" => filtered.OrderByDescending(user => user.Rating).ToList(),
            "Name A–Z" => filtered.OrderBy(user => user.FullName).ToList(),
            _ => filtered.OrderByDescending(user => user.Id).ToList()
        };

        var total = filtered.Count;
        var pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        pageIndex = Math.Clamp(pageIndex, 0, pageCount - 1);
        var visible = filtered.Skip(pageIndex * PageSize).Take(PageSize).ToList();

        rowsHost.SuspendLayout();
        rowsHost.Controls.Clear();
        rowPanels.Clear();
        foreach (var user in visible)
        {
            var row = BuildRow(user);
            rowPanels.Add(row);
            rowsHost.Controls.Add(row);
        }
        rowsHost.ResumeLayout();

        var first = total == 0 ? 0 : pageIndex * PageSize + 1;
        var last = Math.Min(total, (pageIndex + 1) * PageSize);
        pageLabel.Text = $"Page {pageIndex + 1} of {pageCount} · showing {first}–{last} of {total}";
        resultLabel.Text = $"{total} member{(total == 1 ? "" : "s")}";
        prevButton.Enabled = pageIndex > 0;
        nextButton.Enabled = pageIndex < pageCount - 1;

        UpdateBulkBar();
        LayoutRows();
    }

    private Panel BuildRow(DemoUser user)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = user };

        var check = new CheckBox
        {
            Name = "userRowCheck",
            Location = new Point(14, 34),
            Size = new Size(18, 18),
            Checked = selectedIds.Contains(user.Id),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        check.CheckedChanged += (_, _) =>
        {
            if (check.Checked)
                selectedIds.Add(user.Id);
            else
                selectedIds.Remove(user.Id);
            UpdateBulkBar();
        };
        row.Controls.Add(check);

        var avatar = new Label
        {
            Text = user.Initials,
            Size = new Size(40, 40),
            Location = new Point(44, 22),
            BackColor = user.AvatarColor,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        row.Controls.Add(avatar);

        var name = AdminUi.MakeLabel(user.FullName, 10, AdminUi.Navy, true);
        name.Location = new Point(94, 14);
        row.Controls.Add(name);

        var email = AdminUi.MakeLabel(user.Email, 8, AdminUi.Muted);
        email.Location = new Point(94, 34);
        row.Controls.Add(email);

        var meta = AdminUi.MakeLabel(
            $"{user.Role} · Brgy. {user.Barangay} · joined {user.Joined} · {user.CompletedTasks + user.PostedTasks} tasks · {user.Rating:0.0} ★",
            7.5F,
            AdminUi.Muted);
        meta.Name = "userMeta";
        meta.Location = new Point(94, 54);
        meta.AutoEllipsis = true;
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(user.Status, StatusBack(user.Status), StatusFore(user.Status));
        status.Name = "userStatus";
        row.Controls.Add(status);

        var actions = new FlowLayoutPanel
        {
            Name = "userActions",
            Size = new Size(340, 30),
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };
        var view = AdminUi.MakeLinkButton("View", $"userView{user.Id}");
        view.Margin = new Padding(0, 0, 10, 0);
        view.Click += (_, _) => Nav.Navigate(new UserDetailPage(user));
        actions.Controls.Add(view);

        var toggle = AdminUi.MakeLinkButton(user.Status == "Suspended" ? "Restore access" : "Restrict", $"userToggle{user.Id}");
        toggle.Margin = new Padding(0, 0, 10, 0);
        toggle.ForeColor = user.Status == "Suspended" ? AdminUi.Good : AdminUi.Warn;
        toggle.Click += (_, _) =>
        {
            if (user.Status == "Suspended")
                DemoDataStore.ActivateUser(user);
            else
                DemoDataStore.SuspendUser(user);
            RefreshRows();
        };
        actions.Controls.Add(toggle);

        if (!user.Verified)
        {
            var verify = AdminUi.MakeLinkButton("Verify", $"userVerify{user.Id}");
            verify.Margin = new Padding(0, 0, 10, 0);
            verify.Click += (_, _) =>
            {
                DemoDataStore.VerifyUser(user);
                RefreshRows();
            };
            actions.Controls.Add(verify);
        }

        row.Controls.Add(actions);
        return row;
    }

    private void BulkAction(bool suspend)
    {
        var targets = DemoDataStore.Users.Where(user => selectedIds.Contains(user.Id)).ToList();
        foreach (var user in targets)
        {
            if (suspend)
                DemoDataStore.SuspendUser(user);
            else
                DemoDataStore.VerifyUser(user);
        }

        selectedIds.Clear();
        RefreshRows();
    }

    private void UpdateBulkBar()
    {
        var count = selectedIds.Count;
        selectedLabel.Text = count == 0 ? "No members selected" : $"{count} selected";
        bulkSuspend.Enabled = count > 0;
        bulkVerify.Enabled = count > 0;
    }

    private static Color StatusBack(string status) => status switch
    {
        "Active" => Color.FromArgb(232, 248, 241),
        "Suspended" => Color.FromArgb(253, 236, 238),
        "Unverified" => Color.FromArgb(255, 244, 230),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color StatusFore(string status) => status switch
    {
        "Active" => AdminUi.Good,
        "Suspended" => AdminUi.Bad,
        "Unverified" => AdminUi.Warn,
        _ => AdminUi.Muted
    };

    public override void OnNavigatedTo() => RefreshRows();
}
