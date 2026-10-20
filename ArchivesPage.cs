namespace TaskTap;

/// <summary>
/// Archives: soft-deleted tasks, archived members and resolved/dismissed reports with
/// restore, reopen and permanent delete (single and bulk).
/// </summary>
public sealed class ArchivesPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel tabsBar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly TextBox searchBox = new();
    private readonly Label selectedLabel = new();
    private readonly Button bulkPrimary;
    private readonly Button bulkDelete;
    private readonly Dictionary<string, Button> tabButtons = new();
    private readonly HashSet<object> selected = new();
    private string activeTab = "tasks";
    private int _lastListWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Archives;

    public ArchivesPage()
    {
        bulkPrimary = AdminUi.MakeSecondaryButton("Restore", "archivesBulkPrimary", 120);
        bulkDelete = AdminUi.MakeSecondaryButton("Delete", "archivesBulkDelete", 110);
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = AdminUi.MakeLabel("RECORDS", 8, AdminUi.Blue, true);
        eyebrow.Name = "archivesEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Archives", 26, AdminUi.Navy, true);
        title.Name = "archivesTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Restore or permanently remove items that left the active queues.", 9, AdminUi.Muted);
        subtitle.Name = "archivesSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        BuildTabs();

        var searchLabel = AdminUi.MakeLabel("Search", 8, AdminUi.Muted, true);
        searchLabel.Location = new Point(32, 186);
        page.Controls.Add(searchLabel);
        searchBox.Name = "archivesSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search the archive…";
        searchBox.TextChanged += (_, _) => RefreshList();
        page.Controls.Add(searchBox);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var header = AdminUi.MakeLabel("Archived items", 11, AdminUi.Navy, true);
        header.Location = new Point(18, 14);
        listCard.Controls.Add(header);

        selectedLabel.AutoSize = true;
        selectedLabel.ForeColor = AdminUi.Muted;
        selectedLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        selectedLabel.Location = new Point(18, 42);
        listCard.Controls.Add(selectedLabel);

        bulkPrimary.Size = new Size(120, 28);
        bulkPrimary.Location = new Point(150, 38);
        bulkPrimary.Click += (_, _) => BulkPrimary();
        listCard.Controls.Add(bulkPrimary);

        bulkDelete.Size = new Size(110, 28);
        bulkDelete.Location = new Point(278, 38);
        bulkDelete.ForeColor = AdminUi.Bad;
        bulkDelete.Click += (_, _) => BulkDelete();
        listCard.Controls.Add(bulkDelete);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        SetTab("tasks");
        LayoutPage();
    }

    private void BuildTabs()
    {
        tabsBar.BackColor = Color.Transparent;
        page.Controls.Add(tabsBar);

        var tabs = new (string Key, string Label)[]
        {
            ("tasks", "Tasks"),
            ("users", "Users"),
            ("reports", "Resolved reports")
        };

        var x = 0;
        foreach (var (key, label) in tabs)
        {
            var button = new Button
            {
                Text = label,
                Size = new Size(key == "reports" ? 150 : 104, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AdminUi.Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Location = new Point(x, 0)
            };
            button.FlatAppearance.BorderColor = AdminUi.Line;
            button.FlatAppearance.BorderSize = 1;
            var captured = key;
            button.Click += (_, _) => SetTab(captured);
            tabButtons[key] = button;
            tabsBar.Controls.Add(button);
            x += button.Width + 6;
        }
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "archivesEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "archivesTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "archivesSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        tabsBar.Bounds = new Rectangle(left, 130, contentWidth, 38);
        searchBox.Bounds = new Rectangle(left, 204, 320, 32);

        listCard.Bounds = new Rectangle(left, 252, contentWidth, 520);
        listBody.Bounds = new Rectangle(14, 76, listCard.Width - 28, listCard.Height - 88);

        page.AutoScrollMinSize = new Size(0, 812);

        if (listBody.Width != _lastListWidth)
        {
            _lastListWidth = listBody.Width;
            RefreshList();
        }
    }

    private void SetTab(string key)
    {
        activeTab = key;
        foreach (var (tabKey, button) in tabButtons)
        {
            var active = tabKey == key;
            button.BackColor = active ? AdminUi.Blue : Color.White;
            button.ForeColor = active ? Color.White : AdminUi.Muted;
        }

        bulkPrimary.Text = key == "reports" ? "Reopen" : "Restore";
        selected.Clear();
        RefreshList();
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        var query = searchBox.Text.Trim();
        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;

        switch (activeTab)
        {
            case "users":
                foreach (var user in DemoDataStore.Users
                             .Where(user => user.Status == "Deactivated")
                             .Where(user => Matches(query, $"{user.FullName} {user.Email} {user.Barangay}"))
                             .OrderBy(user => user.FullName))
                {
                    var row = BuildUserRow(user, width);
                    row.SetBounds(0, y, width, 76);
                    listBody.Controls.Add(row);
                    y += 82;
                }
                break;
            case "reports":
                foreach (var report in DemoDataStore.Reports
                             .Where(report => report.Status is "Resolved" or "Dismissed")
                             .Where(report => Matches(query, $"{report.Id} {report.TargetLabel} {report.Reason} {report.ReporterName}"))
                             .OrderByDescending(report => report.Id))
                {
                    var row = BuildReportRow(report, width);
                    row.SetBounds(0, y, width, 76);
                    listBody.Controls.Add(row);
                    y += 82;
                }
                break;
            default:
                foreach (var task in DemoDataStore.ArchivedTasks
                             .Where(task => Matches(query, $"{task.Title} {task.Category} {task.Barangay}"))
                             .OrderBy(task => task.Title))
                {
                    var row = BuildTaskRow(task, width);
                    row.SetBounds(0, y, width, 76);
                    listBody.Controls.Add(row);
                    y += 82;
                }
                break;
        }

        if (listBody.Controls.Count == 0)
        {
            var empty = AdminUi.MakeLabel("Nothing in the archive for this view.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
        UpdateBulkBar();
    }

    private Panel BuildTaskRow(DemoTask task, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = task };
        row.Controls.Add(RowCheck(task, row));

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
        meta.Location = new Point(94, 34);
        row.Controls.Add(meta);

        AddRowActions(row, width, "Restore", () => { DemoDataStore.RestoreAdminTask(task); RefreshList(); }, () => DeleteTask(task));
        return row;
    }

    private Panel BuildUserRow(DemoUser user, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = user };
        row.Controls.Add(RowCheck(user, row));

        var avatar = new Label
        {
            Text = user.Initials,
            Size = new Size(36, 36),
            Location = new Point(40, 18),
            BackColor = user.AvatarColor,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        row.Controls.Add(avatar);

        var name = AdminUi.MakeLabel(user.FullName, 10, AdminUi.Navy, true);
        name.Location = new Point(90, 12);
        row.Controls.Add(name);

        var email = AdminUi.MakeLabel(user.Email, 8, AdminUi.Muted);
        email.Location = new Point(90, 32);
        row.Controls.Add(email);

        var meta = AdminUi.MakeLabel($"{user.Role} · Brgy. {user.Barangay} · joined {user.Joined}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(90, 52);
        row.Controls.Add(meta);

        AddRowActions(row, width, "Restore", () => { DemoDataStore.RestoreUser(user); RefreshList(); }, () => DeleteUser(user));
        return row;
    }

    private Panel BuildReportRow(DemoReport report, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = report };
        row.Controls.Add(RowCheck(report, row));

        var title = AdminUi.MakeLabel($"{report.Id} · {report.Reason}", 10, AdminUi.Navy, true);
        title.Location = new Point(40, 12);
        row.Controls.Add(title);

        var target = AdminUi.MakeLabel($"{report.TargetType} · {report.TargetLabel}", 8, AdminUi.Muted);
        target.Location = new Point(40, 34);
        row.Controls.Add(target);

        var meta = AdminUi.MakeLabel($"Reported by {report.ReporterName} · {report.CreatedAt} · {report.Status}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(40, 54);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(report.Status, report.Status == "Resolved" ? Color.FromArgb(232, 248, 241) : Color.FromArgb(240, 243, 247), report.Status == "Resolved" ? AdminUi.Good : AdminUi.Muted);
        status.Location = new Point(width - 300, 22);
        row.Controls.Add(status);

        AddRowActions(row, width, "Reopen", () => { DemoDataStore.EscalateReport(report); RefreshList(); }, () => DeleteReport(report));
        return row;
    }

    private CheckBox RowCheck(object item, Control row)
    {
        var check = new CheckBox
        {
            Location = new Point(12, 28),
            Size = new Size(18, 18),
            Checked = selected.Contains(item),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        check.CheckedChanged += (_, _) =>
        {
            if (check.Checked)
                selected.Add(item);
            else
                selected.Remove(item);
            UpdateBulkBar();
        };
        return check;
    }

    private void AddRowActions(Control row, int width, string primaryText, Action primary, Action delete)
    {
        var actions = new FlowLayoutPanel
        {
            Size = new Size(180, 28),
            WrapContents = false,
            BackColor = Color.Transparent,
            Location = new Point(width - 196, 24)
        };

        var primaryButton = AdminUi.MakeLinkButton(primaryText, $"archivePrimary{row.GetHashCode()}");
        primaryButton.Margin = new Padding(0, 0, 12, 0);
        primaryButton.Click += (_, _) => primary();
        actions.Controls.Add(primaryButton);

        var deleteButton = AdminUi.MakeLinkButton("Delete", $"archiveDelete{row.GetHashCode()}");
        deleteButton.ForeColor = AdminUi.Bad;
        deleteButton.Click += (_, _) => delete();
        actions.Controls.Add(deleteButton);

        row.Controls.Add(actions);
    }

    private void DeleteTask(DemoTask task)
    {
        if (!Confirm($"Delete \"{task.Title}\" permanently?"))
            return;
        DemoDataStore.DeleteTask(task);
        selected.Remove(task);
        RefreshList();
    }

    private void DeleteUser(DemoUser user)
    {
        if (!Confirm($"Delete {user.FullName} permanently?"))
            return;
        DemoDataStore.DeleteUser(user);
        selected.Remove(user);
        RefreshList();
    }

    private void DeleteReport(DemoReport report)
    {
        if (!Confirm($"Delete report {report.Id} permanently?"))
            return;
        DemoDataStore.DeleteReport(report);
        selected.Remove(report);
        RefreshList();
    }

    private bool Confirm(string message) =>
        MessageBox.Show(this, message, "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

    private void BulkPrimary()
    {
        if (selected.Count == 0)
            return;

        foreach (var item in selected.ToList())
        {
            switch (item)
            {
                case DemoTask task:
                    DemoDataStore.RestoreAdminTask(task);
                    break;
                case DemoUser user:
                    DemoDataStore.RestoreUser(user);
                    break;
                case DemoReport report:
                    DemoDataStore.EscalateReport(report);
                    break;
            }
        }

        selected.Clear();
        RefreshList();
    }

    private void BulkDelete()
    {
        if (selected.Count == 0)
            return;
        if (!Confirm($"Delete {selected.Count} selected item{(selected.Count == 1 ? "" : "s")} permanently?"))
            return;

        foreach (var item in selected.ToList())
        {
            switch (item)
            {
                case DemoTask task:
                    DemoDataStore.DeleteTask(task);
                    break;
                case DemoUser user:
                    DemoDataStore.DeleteUser(user);
                    break;
                case DemoReport report:
                    DemoDataStore.DeleteReport(report);
                    break;
            }
        }

        selected.Clear();
        RefreshList();
    }

    private void UpdateBulkBar()
    {
        selectedLabel.Text = selected.Count == 0 ? "No items selected" : $"{selected.Count} selected";
        bulkPrimary.Enabled = selected.Count > 0;
        bulkDelete.Enabled = selected.Count > 0;
    }

    private static bool Matches(string query, string text) =>
        string.IsNullOrEmpty(query) || text.Contains(query, StringComparison.OrdinalIgnoreCase);

    public override void OnNavigatedTo() => RefreshList();
}
