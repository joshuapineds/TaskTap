namespace TaskTap;

/// <summary>
/// Report Management: filter and triage moderation reports, then resolve, dismiss,
/// escalate, suspend the target user or archive the reported task.
/// </summary>
public sealed class ReportManagementPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Panel detailCard = new();
    private readonly Panel detailBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox severityFilter = new();
    private readonly ComboBox targetFilter = new();
    private DemoReport? selectedReport;
    private int _lastListWidth = -1;
    private int _lastDetailWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Reports;

    public ReportManagementPage()
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

        var eyebrow = AdminUi.MakeLabel("TRUST & SAFETY", 8, AdminUi.Blue, true);
        eyebrow.Name = "reportsEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Report Management", 26, AdminUi.Navy, true);
        title.Name = "reportsTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Triage community reports and keep the marketplace trustworthy.", 9, AdminUi.Muted);
        subtitle.Name = "reportsSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var disputes = AdminUi.MakeSecondaryButton("Disputes →", "reportsDisputesButton", 130);
        disputes.Click += (_, _) => Nav.Navigate(AdminRoute.Disputes);
        page.Controls.Add(disputes);

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

        searchBox.Name = "reportsSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search report id, target, reporter…";
        searchBox.TextChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(searchBox);

        ConfigureFilter(statusFilter, "reportsStatusFilter", ["All statuses", "Open", "Investigating", "Resolved", "Dismissed"]);
        ConfigureFilter(severityFilter, "reportsSeverityFilter", ["All severities", "High", "Medium", "Low"]);
        ConfigureFilter(targetFilter, "reportsTargetFilter", ["All targets", "Task", "User", "Message", "Review"]);

        var reset = AdminUi.MakeLinkButton("Reset filters", "reportsResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            statusFilter.SelectedIndex = 0;
            severityFilter.SelectedIndex = 0;
            targetFilter.SelectedIndex = 0;
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

        var title = AdminUi.MakeLabel("Reports", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        listCard.Controls.Add(title);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);
    }

    private void BuildDetailCard()
    {
        detailCard.BackColor = Color.White;
        detailCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(detailCard);

        var title = AdminUi.MakeLabel("Report detail", 11, AdminUi.Navy, true);
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

        var eyebrow = AdminUi.Find<Label>(page, "reportsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "reportsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "reportsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var disputes = AdminUi.Find<Button>(page, "reportsDisputesButton");
        if (disputes is not null) disputes.Location = new Point(left + contentWidth - 130, 40);

        toolbar.Bounds = new Rectangle(left, 130, contentWidth, 68);
        searchBox.Bounds = new Rectangle(12, 18, 240, 32);
        statusFilter.Bounds = new Rectangle(264, 18, 140, 32);
        severityFilter.Bounds = new Rectangle(416, 18, 140, 32);
        targetFilter.Bounds = new Rectangle(568, 18, 150, 32);
        var reset = AdminUi.Find<Button>(toolbar, "reportsResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - 12, 24);

        var rightWidth = Math.Max(340, Math.Min(430, contentWidth * 34 / 100));
        var mainWidth = contentWidth - rightWidth - 18;

        listCard.Bounds = new Rectangle(left, 212, mainWidth, 566);
        listBody.Bounds = new Rectangle(14, 44, listCard.Width - 28, listCard.Height - 56);

        detailCard.Bounds = new Rectangle(left + mainWidth + 18, 212, rightWidth, 566);
        detailBody.Bounds = new Rectangle(14, 44, detailCard.Width - 28, detailCard.Height - 56);

        page.AutoScrollMinSize = new Size(0, 796);

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
        var severity = severityFilter.SelectedItem?.ToString() ?? "All severities";
        var target = targetFilter.SelectedItem?.ToString() ?? "All targets";

        var reports = DemoDataStore.Reports
            .Where(report => status == "All statuses" || report.Status == status)
            .Where(report => severity == "All severities" || report.Severity == severity)
            .Where(report => target == "All targets" || report.TargetType == target)
            .Where(report => string.IsNullOrEmpty(query) ||
                             $"{report.Id} {report.TargetLabel} {report.ReporterName} {report.Reason}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(report => report.Status == "Open" ? 0 : report.Status == "Investigating" ? 1 : 2)
            .ThenByDescending(report => report.Id)
            .ToList();

        if (selectedReport is not null && !reports.Contains(selectedReport))
            selectedReport = null;
        selectedReport ??= reports.FirstOrDefault();

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var report in reports)
        {
            var row = BuildRow(report, width);
            row.SetBounds(0, y, width, 84);
            listBody.Controls.Add(row);
            y += 90;
        }

        if (reports.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No reports match these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
        BuildDetail();
    }

    private Panel BuildRow(DemoReport report, int width)
    {
        var selected = ReferenceEquals(selectedReport, report);
        var row = new Panel
        {
            BackColor = selected ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(report);

        var title = AdminUi.MakeLabel($"{report.Id} · {report.Reason}", 10, AdminUi.Navy, true);
        title.Name = "reportRowTitle";
        title.MaximumSize = new Size(Math.Max(200, width - 360), 18);
        title.AutoEllipsis = true;
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var target = AdminUi.MakeLabel($"{report.TargetType} · {report.TargetLabel}", 8, AdminUi.Muted);
        target.Location = new Point(14, 34);
        row.Controls.Add(target);

        var meta = AdminUi.MakeLabel($"Reported by {report.ReporterName} · {report.CreatedAt}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(14, 54);
        row.Controls.Add(meta);

        var severity = AdminUi.MakePill(report.Severity, SeverityBack(report.Severity), SeverityFore(report.Severity));
        severity.Location = new Point(width - 300, 14);
        row.Controls.Add(severity);

        var status = AdminUi.MakePill(report.Status, StatusBack(report.Status), StatusFore(report.Status));
        status.Location = new Point(width - 300, 44);
        row.Controls.Add(status);

        var review = AdminUi.MakeSecondaryButton(selected ? "Reviewing" : "Review", $"reportReview{report.Id}", 94);
        review.Size = new Size(94, 30);
        review.Location = new Point(width - 110, 27);
        review.Click += (_, _) => Select(report);
        row.Controls.Add(review);

        return row;
    }

    private void Select(DemoReport report)
    {
        selectedReport = report;
        RefreshList();
    }

    private void BuildDetail()
    {
        if (detailBody.Width <= 0)
            return;

        detailBody.SuspendLayout();
        detailBody.Controls.Clear();

        if (selectedReport is null)
        {
            var empty = AdminUi.MakeLabel("Select a report to review it.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            detailBody.Controls.Add(empty);
            detailBody.ResumeLayout();
            return;
        }

        var report = selectedReport;
        var width = detailBody.ClientSize.Width;

        var title = AdminUi.MakeLabel($"{report.Id} · {report.Reason}", 12, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 8), 0);
        title.Location = new Point(0, 0);
        detailBody.Controls.Add(title);

        var meta = AdminUi.MakeWrappedLabel($"{report.TargetType} · {report.TargetLabel}\nReported by {report.ReporterName} · {report.CreatedAt}", 8, AdminUi.Muted, Math.Max(200, width - 8));
        meta.Location = new Point(0, 30);
        detailBody.Controls.Add(meta);

        var pillRow = new FlowLayoutPanel
        {
            Location = new Point(0, 76),
            Size = new Size(width, 28),
            WrapContents = false,
            BackColor = Color.Transparent
        };
        pillRow.Controls.Add(AdminUi.MakePill(report.Severity, SeverityBack(report.Severity), SeverityFore(report.Severity)));
        var statusPill = AdminUi.MakePill(report.Status, StatusBack(report.Status), StatusFore(report.Status));
        statusPill.Margin = new Padding(8, 0, 0, 0);
        pillRow.Controls.Add(statusPill);
        detailBody.Controls.Add(pillRow);

        var description = AdminUi.MakeWrappedLabel(report.Description, 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        description.Location = new Point(0, 112);
        detailBody.Controls.Add(description);

        var resolutionLabel = AdminUi.MakeWrappedLabel($"Resolution: {report.Resolution}", 8, AdminUi.Muted, Math.Max(200, width - 8));
        resolutionLabel.Location = new Point(0, 176);
        detailBody.Controls.Add(resolutionLabel);

        var openTarget = AdminUi.MakeLinkButton("Open target →", "reportOpenTarget");
        openTarget.Location = new Point(0, 208);
        openTarget.Click += (_, _) => OpenTarget(report);
        detailBody.Controls.Add(openTarget);

        var resolve = AdminUi.MakePrimaryButton("Resolve", "reportResolve", 150);
        resolve.Location = new Point(0, 240);
        resolve.Click += (_, _) => Resolve(report);
        detailBody.Controls.Add(resolve);

        var dismiss = AdminUi.MakeSecondaryButton("Dismiss", "reportDismiss", 130);
        dismiss.Location = new Point(162, 240);
        dismiss.Click += (_, _) => { DemoDataStore.DismissReport(report); RefreshList(); };
        detailBody.Controls.Add(dismiss);

        var escalate = AdminUi.MakeSecondaryButton("Escalate", "reportEscalate", 130);
        escalate.Location = new Point(0, 286);
        escalate.Click += (_, _) => { DemoDataStore.EscalateReport(report); RefreshList(); };
        detailBody.Controls.Add(escalate);

        if (report.TargetType == "User")
        {
            var suspend = AdminUi.MakeSecondaryButton("Suspend user", "reportSuspendTarget", 150);
            suspend.ForeColor = AdminUi.Bad;
            suspend.Location = new Point(162, 286);
            suspend.Click += (_, _) =>
            {
                var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == report.TargetLabel);
                if (user is null)
                {
                    Toast("No matching user account for this report.");
                    return;
                }
                DemoDataStore.SuspendUser(user);
                RefreshList();
            };
            detailBody.Controls.Add(suspend);
        }
        else if (report.TargetType == "Task")
        {
            var archive = AdminUi.MakeSecondaryButton("Archive task", "reportArchiveTarget", 150);
            archive.Location = new Point(162, 286);
            archive.Click += (_, _) =>
            {
                var task = DemoDataStore.AvailableTasks.Concat(DemoDataStore.PostedTasks)
                    .FirstOrDefault(candidate => candidate.Title == report.TargetLabel);
                if (task is null)
                {
                    Toast("No matching active task for this report.");
                    return;
                }
                DemoDataStore.ArchiveAdminTask(task);
                RefreshList();
            };
            detailBody.Controls.Add(archive);
        }

        detailBody.ResumeLayout();
    }

    private void Resolve(DemoReport report)
    {
        using var dialog = new ReasonDialog(
            "Resolve report",
            $"How was {report.Id} resolved?",
            ["Warning issued", "Content removed", "User suspended", "No violation found", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.ResolveReport(report, dialog.SelectedReason ?? "Resolved.");
        RefreshList();
    }

    private void OpenTarget(DemoReport report)
    {
        if (report.TargetType == "User")
        {
            var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == report.TargetLabel);
            if (user is not null)
            {
                Nav.Navigate(new UserDetailPage(user));
                return;
            }
        }
        else if (report.TargetType == "Task")
        {
            Nav.Navigate(AdminRoute.Tasks);
            return;
        }

        Toast("The reported item has no dedicated admin page.");
    }

    private static Color SeverityBack(string severity) => severity switch
    {
        "High" => Color.FromArgb(253, 236, 238),
        "Medium" => Color.FromArgb(255, 244, 230),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color SeverityFore(string severity) => severity switch
    {
        "High" => AdminUi.Bad,
        "Medium" => AdminUi.Warn,
        _ => AdminUi.Muted
    };

    private static Color StatusBack(string status) => status switch
    {
        "Open" => Color.FromArgb(236, 244, 255),
        "Investigating" => Color.FromArgb(255, 244, 230),
        "Resolved" => Color.FromArgb(232, 248, 241),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color StatusFore(string status) => status switch
    {
        "Open" => AdminUi.Blue,
        "Investigating" => AdminUi.Warn,
        "Resolved" => AdminUi.Good,
        _ => AdminUi.Muted
    };

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
