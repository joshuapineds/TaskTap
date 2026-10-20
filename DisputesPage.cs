namespace TaskTap;

/// <summary>
/// Disputes: resolve, refund or dismiss poster/doer disputes and jump to the people
/// and payments involved.
/// </summary>
public sealed class DisputesPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Panel detailCard = new();
    private readonly Panel detailBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private DemoDispute? selectedDispute;
    private int _lastListWidth = -1;
    private int _lastDetailWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Disputes;

    public DisputesPage()
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

        var crumb = AdminUi.MakeLinkButton("← Reports", "disputesCrumb");
        crumb.Click += (_, _) => Nav.Navigate(AdminRoute.Reports);
        page.Controls.Add(crumb);

        var eyebrow = AdminUi.MakeLabel("TRUST & SAFETY", 8, AdminUi.Blue, true);
        eyebrow.Name = "disputesEyebrow";
        eyebrow.Location = new Point(32, 48);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Disputes", 26, AdminUi.Navy, true);
        title.Name = "disputesTitle";
        title.Location = new Point(32, 66);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Settle task disagreements with refunds, resolutions or dismissal.", 9, AdminUi.Muted);
        subtitle.Name = "disputesSubtitle";
        subtitle.Location = new Point(32, 118);
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

        searchBox.Name = "disputesSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search dispute id, task or person…";
        searchBox.TextChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(searchBox);

        statusFilter.Name = "disputesStatusFilter";
        statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilter.Font = new Font("Segoe UI", 9F);
        statusFilter.FlatStyle = FlatStyle.Flat;
        statusFilter.Items.AddRange(["All statuses", "Open", "Resolved", "Dismissed"]);
        statusFilter.SelectedIndex = 0;
        statusFilter.SelectedIndexChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(statusFilter);

        var reset = AdminUi.MakeLinkButton("Reset filters", "disputesResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            statusFilter.SelectedIndex = 0;
        };
        toolbar.Controls.Add(reset);
    }

    private void BuildListCard()
    {
        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var title = AdminUi.MakeLabel("Disputes", 11, AdminUi.Navy, true);
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

        var title = AdminUi.MakeLabel("Dispute detail", 11, AdminUi.Navy, true);
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

        var crumb = AdminUi.Find<Button>(page, "disputesCrumb");
        if (crumb is not null) crumb.Location = new Point(left, 16);
        var eyebrow = AdminUi.Find<Label>(page, "disputesEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 46);
        var title = AdminUi.Find<Label>(page, "disputesTitle");
        if (title is not null) title.Location = new Point(left, 64);
        var subtitle = AdminUi.Find<Label>(page, "disputesSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 116);

        toolbar.Bounds = new Rectangle(left, 152, contentWidth, 68);
        searchBox.Bounds = new Rectangle(12, 18, 280, 32);
        statusFilter.Bounds = new Rectangle(304, 18, 140, 32);
        var reset = AdminUi.Find<Button>(toolbar, "disputesResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - 12, 24);

        var rightWidth = Math.Max(340, Math.Min(430, contentWidth * 34 / 100));
        var mainWidth = contentWidth - rightWidth - 18;

        listCard.Bounds = new Rectangle(left, 234, mainWidth, 540);
        listBody.Bounds = new Rectangle(14, 44, listCard.Width - 28, listCard.Height - 56);

        detailCard.Bounds = new Rectangle(left + mainWidth + 18, 234, rightWidth, 540);
        detailBody.Bounds = new Rectangle(14, 44, detailCard.Width - 28, detailCard.Height - 56);

        page.AutoScrollMinSize = new Size(0, 794);

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

        var disputes = DemoDataStore.Disputes
            .Where(dispute => status == "All statuses" || dispute.Status == status)
            .Where(dispute => string.IsNullOrEmpty(query) ||
                              $"{dispute.Id} {dispute.TaskTitle} {dispute.PosterName} {dispute.DoerName} {dispute.Reason}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(dispute => dispute.Status == "Open" ? 0 : 1)
            .ThenByDescending(dispute => dispute.Id)
            .ToList();

        if (selectedDispute is not null && !disputes.Contains(selectedDispute))
            selectedDispute = null;
        selectedDispute ??= disputes.FirstOrDefault();

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var dispute in disputes)
        {
            var row = BuildRow(dispute, width);
            row.SetBounds(0, y, width, 84);
            listBody.Controls.Add(row);
            y += 90;
        }

        if (disputes.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No disputes match these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
        BuildDetail();
    }

    private Panel BuildRow(DemoDispute dispute, int width)
    {
        var selected = ReferenceEquals(selectedDispute, dispute);
        var row = new Panel
        {
            BackColor = selected ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(dispute);

        var title = AdminUi.MakeLabel($"{dispute.Id} · {dispute.TaskTitle}", 10, AdminUi.Navy, true);
        title.Name = "disputeRowTitle";
        title.MaximumSize = new Size(Math.Max(200, width - 360), 18);
        title.AutoEllipsis = true;
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var reason = AdminUi.MakeLabel(dispute.Reason, 8, AdminUi.Muted);
        reason.Location = new Point(14, 34);
        row.Controls.Add(reason);

        var meta = AdminUi.MakeLabel($"{dispute.PosterName} vs {dispute.DoerName} · ₱{dispute.Amount:N0} · {dispute.OpenedAt}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(14, 54);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(dispute.Status, StatusBack(dispute.Status), StatusFore(dispute.Status));
        status.Location = new Point(width - 300, 14);
        row.Controls.Add(status);

        var review = AdminUi.MakeSecondaryButton(selected ? "Reviewing" : "Review", $"disputeReview{dispute.Id}", 94);
        review.Size = new Size(94, 30);
        review.Location = new Point(width - 110, 40);
        review.Click += (_, _) => Select(dispute);
        row.Controls.Add(review);

        return row;
    }

    private void Select(DemoDispute dispute)
    {
        selectedDispute = dispute;
        RefreshList();
    }

    private void BuildDetail()
    {
        if (detailBody.Width <= 0)
            return;

        detailBody.SuspendLayout();
        detailBody.Controls.Clear();

        if (selectedDispute is null)
        {
            var empty = AdminUi.MakeLabel("Select a dispute to review it.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            detailBody.Controls.Add(empty);
            detailBody.ResumeLayout();
            return;
        }

        var dispute = selectedDispute;
        var width = detailBody.ClientSize.Width;

        var title = AdminUi.MakeLabel($"{dispute.Id} · {dispute.TaskTitle}", 12, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 8), 0);
        title.Location = new Point(0, 0);
        detailBody.Controls.Add(title);

        var meta = AdminUi.MakeWrappedLabel($"{dispute.PosterName} vs {dispute.DoerName}\n₱{dispute.Amount:N0} · {dispute.OpenedAt}", 8, AdminUi.Muted, Math.Max(200, width - 8));
        meta.Location = new Point(0, 32);
        detailBody.Controls.Add(meta);

        var status = AdminUi.MakePill(dispute.Status, StatusBack(dispute.Status), StatusFore(dispute.Status));
        status.Location = new Point(0, 80);
        detailBody.Controls.Add(status);

        var reason = AdminUi.MakeWrappedLabel($"Reason: {dispute.Reason}", 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        reason.Location = new Point(0, 116);
        detailBody.Controls.Add(reason);

        var resolution = AdminUi.MakeWrappedLabel($"Resolution: {dispute.Resolution}", 8, AdminUi.Muted, Math.Max(200, width - 8));
        resolution.Location = new Point(0, 152);
        detailBody.Controls.Add(resolution);

        var openPoster = AdminUi.MakeLinkButton("Open poster →", "disputeOpenPoster");
        openPoster.Location = new Point(0, 188);
        openPoster.Click += (_, _) => OpenPerson(dispute.PosterName);
        detailBody.Controls.Add(openPoster);

        var openDoer = AdminUi.MakeLinkButton("Open doer →", "disputeOpenDoer");
        openDoer.Location = new Point(130, 188);
        openDoer.Click += (_, _) => OpenPerson(dispute.DoerName);
        detailBody.Controls.Add(openDoer);

        var openPayment = AdminUi.MakeLinkButton("Payment records →", "disputeOpenPayment");
        openPayment.Location = new Point(0, 218);
        openPayment.Click += (_, _) => Nav.Navigate(AdminRoute.Payments);
        detailBody.Controls.Add(openPayment);

        var resolve = AdminUi.MakePrimaryButton("Resolve", "disputeResolve", 150);
        resolve.Location = new Point(0, 252);
        resolve.Click += (_, _) => Resolve(dispute);
        detailBody.Controls.Add(resolve);

        var refund = AdminUi.MakeSecondaryButton("Refund", "disputeRefund", 130);
        refund.ForeColor = AdminUi.Warn;
        refund.Location = new Point(162, 252);
        refund.Click += (_, _) => { DemoDataStore.RefundDispute(dispute); RefreshList(); };
        detailBody.Controls.Add(refund);

        var dismiss = AdminUi.MakeSecondaryButton("Dismiss", "disputeDismiss", 130);
        dismiss.Location = new Point(0, 298);
        dismiss.Click += (_, _) => { DemoDataStore.DismissDispute(dispute); RefreshList(); };
        detailBody.Controls.Add(dismiss);

        detailBody.ResumeLayout();
    }

    private void Resolve(DemoDispute dispute)
    {
        using var dialog = new ReasonDialog(
            "Resolve dispute",
            $"How was {dispute.Id} resolved?",
            ["Work completed", "Partial refund", "Full refund", "No action needed", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.ResolveDispute(dispute, dialog.SelectedReason ?? "Resolved.");
        RefreshList();
    }

    private void OpenPerson(string name)
    {
        var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == name);
        if (user is null)
        {
            Toast($"No matching account for {name}.");
            return;
        }
        Nav.Navigate(new UserDetailPage(user));
    }

    private static Color StatusBack(string status) => status switch
    {
        "Open" => Color.FromArgb(255, 244, 230),
        "Resolved" => Color.FromArgb(232, 248, 241),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color StatusFore(string status) => status switch
    {
        "Open" => AdminUi.Warn,
        "Resolved" => AdminUi.Good,
        _ => AdminUi.Muted
    };

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
