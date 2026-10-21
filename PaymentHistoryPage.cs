namespace TaskTap;

public sealed class PaymentHistoryPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Green = Color.FromArgb(19, 160, 113);
    private static readonly Color Muted = Color.FromArgb(102, 117, 138);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(228, 234, 242);
    private readonly Panel page = new();
    private readonly Panel stats = new();
    private readonly Panel filters = new();
    private readonly Panel historyList = new();
    private readonly Panel breakdown = new();
    private readonly Panel safety = new();
    private readonly Panel quickCard = new();
    private readonly TextBox search = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox methodFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Label resultCount = new();
    private readonly Label emptyState = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private readonly Dictionary<PaymentRecordDetails, Panel> rowPanels = [];
    private bool pageBuilt;

    public override Route RouteName => Route.PaymentHistory;

    public PaymentHistoryPage()
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
        ApplyFilters();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Tasks   /   Payment History", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Your payment records", 8, Blue, true, new Point(22, 40)));
        page.Controls.Add(MakeLabel("Payment History", 23, Navy, true, new Point(22, 56)));
        page.Controls.Add(MakeLabel("Review recorded payments from completed TaskTap work.", 9, Muted, false, new Point(22, 104)));
        page.Controls.Add(PaymentsTabs.Build(true));
        var myTasks = MakeButton("My Tasks", false, 100);
        myTasks.Name = "PaymentHistoryMyTasksButton";
        myTasks.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(myTasks);
        var recordPayment = MakeButton("＋ Record Payment", true, 150);
        recordPayment.Name = "PaymentHistoryRecordButton";
        recordPayment.Click += (_, _) => Nav.Navigate(new RecordPaymentPage());
        page.Controls.Add(recordPayment);

        BuildStats();
        BuildFilters();
        BuildBreakdown();
        BuildSafetyCards();
        emptyState.Text = "₱\r\nNo payments found\r\nTry another search or clear your filters.";
        emptyState.Name = "paymentHistoryEmptyState";
        emptyState.TextAlign = ContentAlignment.MiddleCenter;
        emptyState.ForeColor = Muted;
        emptyState.BackColor = Color.White;
        emptyState.BorderStyle = BorderStyle.FixedSingle;
        emptyState.Visible = false;
        page.Controls.Add(emptyState);
        foreach (var payment in DemoDataStore.Payments)
        {
            var row = BuildPaymentRow(payment);
            rowPanels.Add(payment, row);
            historyList.Controls.Add(row);
        }
    }

    private void BuildStats()
    {
        var data = new[]
        {
            ("₱", "Total recorded", "₱3,250", "Across 6 records", Blue),
            ("✓", "Paid", "₱2,850", "5 completed payments", Green),
            ("◷", "Pending", "₱400", "1 payment awaiting completion", Color.FromArgb(255, 138, 61)),
            ("↗", "Average payment", "₱542", "Per recorded task", Color.FromArgb(123, 97, 209))
        };
        foreach (var item in data)
        {
            var card = MakeCard();
            var icon = MakeLabel(item.Item1, 14, item.Item5, true, new Point(14, 16));
            icon.AutoSize = false;
            icon.Size = new Size(40, 40);
            icon.TextAlign = ContentAlignment.MiddleCenter;
            icon.BackColor = Color.FromArgb(234, 243, 255);
            card.Controls.Add(icon);
            card.Controls.Add(MakeLabel(item.Item2, 8, Muted, false, new Point(64, 12)));
            card.Controls.Add(MakeLabel(item.Item3, 18, Navy, true, new Point(64, 28)));
            card.Controls.Add(MakeLabel(item.Item4, 7, Muted, false, new Point(64, 62)));
            stats.Controls.Add(card);
        }
        page.Controls.Add(stats);
    }

    private void BuildFilters()
    {
        filters.BackColor = Color.White;
        filters.BorderStyle = BorderStyle.FixedSingle;
        search.PlaceholderText = "Search task or reference...";
        search.TextChanged += (_, _) => ApplyFilters();
        filters.Controls.Add(search);
        ConfigureFilter(statusFilter, ["All statuses", "Paid", "Pending", "Partially paid"]);
        ConfigureFilter(methodFilter, ["All methods", "Cash", "GCash", "Maya", "Bank transfer"]);
        ConfigureFilter(sortFilter, ["Most recent", "Highest amount", "Lowest amount", "Oldest"]);
        statusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        methodFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        sortFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        filters.Controls.Add(statusFilter);
        filters.Controls.Add(methodFilter);
        filters.Controls.Add(sortFilter);
        resultCount.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        resultCount.ForeColor = Navy;
        filters.Controls.Add(resultCount);
        var clear = MakeLink("Clear filters");
        clear.Click += (_, _) => ClearFilters();
        filters.Controls.Add(clear);
        page.Controls.Add(filters);

        historyList.BackColor = PageColor;
        page.Controls.Add(historyList);
    }

    private void ConfigureFilter(ComboBox combo, string[] items)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 8F);
        combo.Items.AddRange(items);
        combo.SelectedIndex = 0;
    }

    private void BuildBreakdown()
    {
        breakdown.BackColor = Color.White;
        breakdown.BorderStyle = BorderStyle.FixedSingle;
        breakdown.Controls.Add(MakeLabel("Payment breakdown", 12, Navy, true, new Point(16, 16)));
        AddBreakdownRow("Cash", "₱1,550", 48, 52);
        AddBreakdownRow("GCash", "₱450", 14, 112);
        AddBreakdownRow("Maya", "₱300", 9, 172);
        AddBreakdownRow("Bank transfer", "₱950", 29, 232);
        page.Controls.Add(breakdown);
    }

    private void AddBreakdownRow(string name, string amount, int percent, int y)
    {
        var label = MakeLabel(name, 8, Muted, false, new Point(16, y));
        label.Name = $"breakdownName{y}";
        var value = MakeLabel(amount, 8, Navy, true, new Point(0, y));
        value.Name = $"breakdownValue{y}";
        value.AutoSize = false;
        value.Size = new Size(70, 16);
        value.TextAlign = ContentAlignment.MiddleRight;
        breakdown.Controls.Add(label);
        breakdown.Controls.Add(value);
        breakdown.Controls.Add(new Panel { Name = $"paymentBar{y}", BackColor = Color.FromArgb(237, 241, 246), Location = new Point(16, y + 22), Size = new Size(240, 6), Tag = percent });
    }

    private void BuildSafetyCards()
    {
        safety.BackColor = Color.FromArgb(240, 248, 255);
        safety.BorderStyle = BorderStyle.FixedSingle;
        safety.Controls.Add(MakeLabel("✓", 13, Blue, true, new Point(16, 14)));
        safety.Controls.Add(MakeLabel("Keep payment records safe", 10, Navy, true, new Point(16, 46)));
        safety.Controls.Add(MakeLabel("Use references to keep your task history organized. Never store or share OTPs, PINs, passwords, or full banking credentials in task notes.", 8, Muted, false, new Point(16, 72), new Size(260, 94)));
        page.Controls.Add(safety);

        quickCard.BackColor = Color.White;
        quickCard.BorderStyle = BorderStyle.FixedSingle;
        quickCard.Controls.Add(MakeLabel("Need to update a payment?", 10, Navy, true, new Point(16, 16)));
        quickCard.Controls.Add(MakeLabel("Open a payment record to edit its status, method, date, or notes.", 8, Muted, false, new Point(16, 44), new Size(260, 50)));
        var open = MakeButton("Open latest record", false, 145);
        open.Location = new Point(16, 104);
        open.Click += (_, _) => Nav.Navigate(new PaymentRecordPage(DemoDataStore.Payments.FirstOrDefault() ?? new PaymentRecordDetails("", "", "Cash", "Pending", 0, DateTime.Today, "", "", "", "")));
        quickCard.Controls.Add(open);
        page.Controls.Add(quickCard);
    }

    private Panel BuildPaymentRow(PaymentRecordDetails payment)
    {
        var panel = MakeCard();
        panel.Height = 96;
        var iconBack = payment.Method switch
        {
            "GCash" => Color.FromArgb(232, 248, 246),
            "Maya" => Color.FromArgb(237, 248, 233),
            "Bank transfer" => Color.FromArgb(241, 239, 255),
            _ => Color.FromArgb(255, 242, 233)
        };
        var icon = MakeLabel("₱", 14, Blue, true, new Point(14, 22));
        icon.AutoSize = false;
        icon.Size = new Size(46, 46);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = iconBack;
        panel.Controls.Add(icon);
        panel.Controls.Add(MakeLabel(payment.Task, 10, Navy, true, new Point(72, 12)));
        var status = MakeLabel(payment.Status, 7, payment.Status == "Paid" ? Color.FromArgb(8, 123, 91) : Color.FromArgb(185, 92, 16), true, new Point(0, 12));
        status.Name = "paymentStatus";
        status.AutoSize = false;
        status.TextAlign = ContentAlignment.MiddleCenter;
        status.Size = new Size(payment.Status == "Paid" ? 48 : 60, 21);
        status.BackColor = payment.Status == "Paid" ? Color.FromArgb(230, 247, 240) : Color.FromArgb(255, 241, 223);
        panel.Controls.Add(status);
        var meta = MakeLabel($"{payment.Reference}   ·   {payment.Method}   ·   {payment.Date:MMM d, yyyy}   ·   {payment.Category} · Brgy. {payment.Barangay}", 7.5F, Muted, false, new Point(72, 39));
        meta.Name = "paymentMeta";
        panel.Controls.Add(meta);
        panel.Controls.Add(MakeLabel($"{payment.Person} · {payment.Description}", 7, Color.FromArgb(138, 151, 169), false, new Point(72, 64)));
        var amount = MakeLabel($"₱{payment.Amount:N0}", 13, Navy, true, new Point(0, 19));
        amount.Name = "paymentAmount";
        amount.TextAlign = ContentAlignment.MiddleRight;
        panel.Controls.Add(amount);
        var view = MakeLink(payment.Status == "Pending" ? "Update →" : "View record →");
        view.Name = "paymentViewButton";
        view.Click += (_, _) => Nav.Navigate(new PaymentRecordPage(payment));
        panel.Controls.Add(view);
        panel.Tag = payment;
        return panel;
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var left = 22;
        var contentWidth = width - left * 2;
        var myTasks = page.Controls.Find("PaymentHistoryMyTasksButton", false).FirstOrDefault();
        var record = page.Controls.Find("PaymentHistoryRecordButton", false).FirstOrDefault();
        var tabs = page.Controls.Find("paymentsTabs", false).FirstOrDefault();
        if (tabs is not null) tabs.Location = new Point(width - left - tabs.Width, 10);
        if (record is not null) record.Location = new Point(width - left - record.Width, 52);
        if (myTasks is not null) myTasks.Location = new Point((record?.Left ?? width - 170) - myTasks.Width - 8, 52);

        var statGap = 14;
        var statWidth = (contentWidth - statGap * 3) / 4;
        stats.Bounds = new Rectangle(left, 132, contentWidth, 80);
        for (var i = 0; i < stats.Controls.Count; i++)
            stats.Controls[i].Bounds = new Rectangle(i * (statWidth + statGap), 0, statWidth, 80);

        filters.Bounds = new Rectangle(left, 224, contentWidth, 108);
        search.Bounds = new Rectangle(12, 12, Math.Max(180, contentWidth * 36 / 100), 38);
        statusFilter.Bounds = new Rectangle(search.Right + 10, 12, 150, 38);
        methodFilter.Bounds = new Rectangle(statusFilter.Right + 8, 12, 155, 38);
        sortFilter.Bounds = new Rectangle(methodFilter.Right + 8, 12, Math.Max(130, filters.ClientSize.Width - methodFilter.Right - 30), 38);
        resultCount.Location = new Point(14, 65);
        var clear = filters.Controls.OfType<Button>().FirstOrDefault();
        if (clear is not null) clear.Location = new Point(filters.ClientSize.Width - clear.Width - 14, 62);

        var sideWidth = Math.Min(310, Math.Max(270, contentWidth / 3));
        var listWidth = contentWidth - sideWidth - 18;
        var rowHeight = 96;
        var visibleCount = rowPanels.Values.Count(row => row.Visible);
        historyList.Bounds = new Rectangle(left, 344, listWidth, Math.Max(220, visibleCount * (rowHeight + 10)));
        var y = 0;
        foreach (var row in historyList.Controls.OfType<Panel>())
        {
            row.Bounds = new Rectangle(0, y, listWidth, rowHeight);
            var status = row.Controls.Find("paymentStatus", false).FirstOrDefault();
            var meta = row.Controls.Find("paymentMeta", false).FirstOrDefault();
            var amount = row.Controls.Find("paymentAmount", false).FirstOrDefault();
            var link = row.Controls.Find("paymentViewButton", false).FirstOrDefault();
            if (status is not null) status.Location = new Point(79 + row.Controls[1].Width + 10, 12);
            if (meta is not null) meta.MaximumSize = new Size(Math.Max(100, listWidth - 260), 0);
            if (amount is not null) { amount.Location = new Point(listWidth - 112, 16); amount.Size = new Size(96, 24); }
            if (link is not null) link.Location = new Point(listWidth - 112, 49);
            if (row.Visible) y += rowHeight + 10;
        }
        breakdown.Bounds = new Rectangle(left + listWidth + 18, 344, sideWidth, 282);
        foreach (Control control in breakdown.Controls)
        {
            if (control.Name.StartsWith("paymentBar", StringComparison.Ordinal))
            {
                var percent = (int)control.Tag!;
                control.Width = sideWidth - 32;
                var fill = new Panel { BackColor = Blue, Size = new Size((control.Width - 2) * percent / 100, control.Height), Location = Point.Empty };
                control.Controls.Clear();
                control.Controls.Add(fill);
            }
            else if (control.Name.StartsWith("breakdownValue", StringComparison.Ordinal))
            {
                control.Location = new Point(sideWidth - 86, control.Location.Y);
            }
        }
        safety.Bounds = new Rectangle(left + listWidth + 18, 640, sideWidth, 184);
        safety.Controls.OfType<Label>().Last().Size = new Size(sideWidth - 32, 94);
        quickCard.Bounds = new Rectangle(left + listWidth + 18, 834, sideWidth, 150);
        quickCard.Controls.OfType<Label>().Last().Size = new Size(sideWidth - 32, 50);
        emptyState.Bounds = new Rectangle(left, 344, listWidth, 220);
        emptyState.Visible = visibleCount == 0;
        page.AutoScrollMinSize = new Size(0, Math.Max(quickCard.Bottom, historyList.Bottom) + 24);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void ApplyFilters()
    {
        if (!pageBuilt) return;
        var query = search.Text.Trim();
        var status = statusFilter.SelectedIndex;
        var method = methodFilter.SelectedIndex;
        var visible = DemoDataStore.Payments.Where(item =>
            (status == 0 || item.Status == statusFilter.SelectedItem?.ToString()?.Replace("Partially paid", "Partial")) &&
            (method == 0 || item.Method == methodFilter.SelectedItem?.ToString()) &&
            (query.Length == 0 || $"{item.Task} {item.Reference} {item.Person} {item.Category}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        visible = sortFilter.SelectedIndex switch
        {
            1 => visible.OrderByDescending(item => item.Amount).ToList(),
            2 => visible.OrderBy(item => item.Amount).ToList(),
            3 => visible.OrderBy(item => item.Date).ToList(),
            _ => visible.OrderByDescending(item => item.Date).ToList()
        };
        for (var i = 0; i < visible.Count; i++)
        {
            var row = rowPanels[visible[i]];
            row.Visible = true;
            historyList.Controls.SetChildIndex(row, i);
        }
        foreach (var item in DemoDataStore.Payments.Except(visible)) rowPanels[item].Visible = false;
        resultCount.Text = $"{visible.Count} payment{(visible.Count == 1 ? "" : "s")}";
        if (pageBuilt) LayoutPage();
    }

    private void ClearFilters()
    {
        search.Clear();
        statusFilter.SelectedIndex = 0;
        methodFilter.SelectedIndex = 0;
        sortFilter.SelectedIndex = 0;
        ShowToast("Payment filters cleared.");
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private Panel MakeCard() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

    private Label MakeLabel(string text, float size, Color color, bool bold, Point location, Size? dimensions = null) => new()
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

    private Button MakeLink(string text)
    {
        var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, TabStop = false };
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(0);
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
