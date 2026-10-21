using System.Globalization;

namespace TaskTap;

/// <summary>
/// Payment Records: totals, filters and a payment list plus a detail panel with
/// mark-paid, void and refund actions over the shared payment history.
/// </summary>
public sealed class PaymentRecordsPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel totalsRow = new();
    private readonly List<Panel> totalCards = new();
    private readonly List<Label> totalValues = new();
    private readonly Panel toolbar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Panel detailCard = new();
    private readonly Panel detailBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox methodFilter = new();
    private readonly ComboBox sortFilter = new();
    private PaymentRecordDetails? selectedPayment;
    private int _lastListWidth = -1;
    private int _lastDetailWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Payments;

    public PaymentRecordsPage()
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

        var eyebrow = AdminUi.MakeLabel("MONEY", 8, AdminUi.Blue, true);
        eyebrow.Name = "paymentsEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Payment Records", 26, AdminUi.Navy, true);
        title.Name = "paymentsTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Review recorded payments and settle pending records.", 9, AdminUi.Muted);
        subtitle.Name = "paymentsSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var export = AdminUi.MakeSecondaryButton("Export", "paymentsExportButton", 150);
        export.Click += (_, _) => ExportCsv();
        page.Controls.Add(export);

        BuildTotals();
        BuildToolbar();
        BuildListCard();
        BuildDetailCard();

        RefreshTotals();
        RefreshList();
        LayoutPage();
    }

    private void BuildTotals()
    {
        totalsRow.BackColor = Color.Transparent;
        page.Controls.Add(totalsRow);

        var headings = new[] { "Total recorded", "Paid", "Pending", "Void / refunded" };
        for (var index = 0; index < headings.Length; index++)
        {
            var card = AdminUi.MakeCard();
            card.Name = $"paymentTotal{index + 1}";

            var heading = AdminUi.MakeLabel(headings[index], 8, AdminUi.Muted, true);
            heading.Location = new Point(14, 12);
            card.Controls.Add(heading);

            var value = AdminUi.MakeLabel("₱0", 18, AdminUi.Navy, true);
            value.Name = $"paymentTotalValue{index + 1}";
            value.Location = new Point(14, 34);
            card.Controls.Add(value);
            totalValues.Add(value);

            totalsRow.Controls.Add(card);
            totalCards.Add(card);
        }
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.White;
        toolbar.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(toolbar);

        searchBox.Name = "paymentsSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search reference, task or person…";
        searchBox.TextChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(searchBox);

        ConfigureFilter(statusFilter, "paymentsStatusFilter", ["All statuses", "Paid", "Pending", "Void", "Refunded"]);
        ConfigureFilter(methodFilter, "paymentsMethodFilter", ["All methods", "Cash", "GCash", "Maya", "Bank transfer"]);
        ConfigureFilter(sortFilter, "paymentsSortFilter", ["Newest first", "Oldest first", "Highest amount", "Lowest amount"]);

        var reset = AdminUi.MakeLinkButton("Reset filters", "paymentsResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            statusFilter.SelectedIndex = 0;
            methodFilter.SelectedIndex = 0;
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

        var title = AdminUi.MakeLabel("Payments", 11, AdminUi.Navy, true);
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

        var title = AdminUi.MakeLabel("Payment detail", 11, AdminUi.Navy, true);
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

        var eyebrow = AdminUi.Find<Label>(page, "paymentsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "paymentsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "paymentsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var export = AdminUi.Find<Button>(page, "paymentsExportButton");
        if (export is not null) export.Location = new Point(left + contentWidth - 150, 40);

        totalsRow.Bounds = new Rectangle(left, 130, contentWidth, 84);
        const int gap = 14;
        var cardWidth = (contentWidth - gap * 3) / 4;
        for (var index = 0; index < totalCards.Count; index++)
        {
            totalCards[index].Location = new Point(index * (cardWidth + gap), 0);
            totalCards[index].Size = new Size(cardWidth, 84);
        }

        toolbar.Bounds = new Rectangle(left, 230, contentWidth, 64);
        searchBox.Bounds = new Rectangle(12, 16, 240, 32);
        statusFilter.Bounds = new Rectangle(264, 16, 140, 32);
        methodFilter.Bounds = new Rectangle(416, 16, 150, 32);
        sortFilter.Bounds = new Rectangle(578, 16, 160, 32);
        var reset = AdminUi.Find<Button>(toolbar, "paymentsResetButton");
        if (reset is not null)
            reset.Location = new Point(toolbar.Width - reset.Width - 12, 22);

        var rightWidth = Math.Max(340, Math.Min(430, contentWidth * 34 / 100));
        var mainWidth = contentWidth - rightWidth - 18;

        listCard.Bounds = new Rectangle(left, 308, mainWidth, 500);
        listBody.Bounds = new Rectangle(14, 44, listCard.Width - 28, listCard.Height - 56);

        detailCard.Bounds = new Rectangle(left + mainWidth + 18, 308, rightWidth, 500);
        detailBody.Bounds = new Rectangle(14, 44, detailCard.Width - 28, detailCard.Height - 56);

        page.AutoScrollMinSize = new Size(0, 852);

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

    private void RefreshTotals()
    {
        if (totalValues.Count < 4)
            return;

        totalValues[0].Text = $"₱{DemoDataStore.Payments.Sum(payment => payment.Amount):N0}";
        totalValues[1].Text = $"₱{DemoDataStore.Payments.Where(payment => payment.Status == "Paid").Sum(payment => payment.Amount):N0}";
        totalValues[2].Text = $"₱{DemoDataStore.Payments.Where(payment => payment.Status == "Pending").Sum(payment => payment.Amount):N0}";
        totalValues[3].Text = $"₱{DemoDataStore.Payments.Where(payment => payment.Status is "Void" or "Refunded").Sum(payment => payment.Amount):N0}";
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        var query = searchBox.Text.Trim();
        var status = statusFilter.SelectedItem?.ToString() ?? "All statuses";
        var method = methodFilter.SelectedItem?.ToString() ?? "All methods";
        var sort = sortFilter.SelectedItem?.ToString() ?? "Newest first";

        var payments = DemoDataStore.Payments
            .Where(payment => status == "All statuses" || payment.Status == status)
            .Where(payment => method == "All methods" || payment.Method == method)
            .Where(payment => string.IsNullOrEmpty(query) ||
                              $"{payment.Reference} {payment.Task} {payment.Person} {payment.Method}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        payments = sort switch
        {
            "Oldest first" => payments.OrderBy(payment => payment.Date).ToList(),
            "Highest amount" => payments.OrderByDescending(payment => payment.Amount).ToList(),
            "Lowest amount" => payments.OrderBy(payment => payment.Amount).ToList(),
            _ => payments.OrderByDescending(payment => payment.Date).ToList()
        };

        if (selectedPayment is not null && !payments.Any(payment => payment.Reference == selectedPayment.Reference))
            selectedPayment = null;
        selectedPayment ??= payments.FirstOrDefault();

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var payment in payments)
        {
            var row = BuildRow(payment, width);
            row.SetBounds(0, y, width, 84);
            listBody.Controls.Add(row);
            y += 90;
        }

        if (payments.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No payments match these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
        RefreshTotals();
        BuildDetail();
    }

    private Panel BuildRow(PaymentRecordDetails payment, int width)
    {
        var selected = selectedPayment is not null && selectedPayment.Reference == payment.Reference;
        var row = new Panel
        {
            BackColor = selected ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(payment);

        var title = AdminUi.MakeLabel($"{payment.Reference} · {payment.Task}", 10, AdminUi.Navy, true);
        title.Name = "paymentRowTitle";
        title.MaximumSize = new Size(Math.Max(200, width - 360), 18);
        title.AutoEllipsis = true;
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var person = AdminUi.MakeLabel($"Paid to {payment.Person}", 8, AdminUi.Muted);
        person.Location = new Point(14, 34);
        row.Controls.Add(person);

        var meta = AdminUi.MakeLabel($"{payment.Method} · {payment.Date:MMM d, yyyy}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(14, 54);
        row.Controls.Add(meta);

        var amount = AdminUi.MakeLabel($"₱{payment.Amount:N0}", 11, AdminUi.Navy, true);
        amount.Location = new Point(width - 320, 14);
        row.Controls.Add(amount);

        var status = AdminUi.MakePill(payment.Status, StatusBack(payment.Status), StatusFore(payment.Status));
        status.Location = new Point(width - 320, 44);
        row.Controls.Add(status);

        var review = AdminUi.MakeSecondaryButton(selected ? "Reviewing" : "Review", $"paymentReview{payment.Reference}", 94);
        review.Size = new Size(94, 30);
        review.Location = new Point(width - 110, 27);
        review.Click += (_, _) => Select(payment);
        row.Controls.Add(review);

        return row;
    }

    private void Select(PaymentRecordDetails payment)
    {
        selectedPayment = payment;
        RefreshList();
    }

    private void BuildDetail()
    {
        if (detailBody.Width <= 0)
            return;

        detailBody.SuspendLayout();
        detailBody.Controls.Clear();

        if (selectedPayment is null)
        {
            var empty = AdminUi.MakeLabel("Select a payment to review it.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            detailBody.Controls.Add(empty);
            detailBody.ResumeLayout();
            return;
        }

        var payment = selectedPayment;
        var width = detailBody.ClientSize.Width;

        var title = AdminUi.MakeLabel($"{payment.Reference} · {payment.Task}", 12, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 8), 0);
        title.Location = new Point(0, 0);
        detailBody.Controls.Add(title);

        var amount = AdminUi.MakeLabel($"₱{payment.Amount:N0}", 20, AdminUi.Navy, true);
        amount.Location = new Point(0, 32);
        detailBody.Controls.Add(amount);

        var status = AdminUi.MakePill(payment.Status, StatusBack(payment.Status), StatusFore(payment.Status));
        status.Location = new Point(140, 40);
        detailBody.Controls.Add(status);

        var meta = AdminUi.MakeWrappedLabel($"Paid to {payment.Person}\n{payment.Category} · Brgy. {payment.Barangay}\n{payment.Method} · {payment.Date:MMM d, yyyy}", 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        meta.Location = new Point(0, 84);
        detailBody.Controls.Add(meta);

        var description = AdminUi.MakeWrappedLabel(payment.Description, 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        description.Location = new Point(0, 156);
        detailBody.Controls.Add(description);

        var openPerson = AdminUi.MakeLinkButton("Open person →", "paymentOpenPerson");
        openPerson.Location = new Point(0, 192);
        openPerson.Click += (_, _) => OpenPerson(payment.Person);
        detailBody.Controls.Add(openPerson);

        if (payment.Status == "Pending")
        {
            var markPaid = AdminUi.MakePrimaryButton("Mark paid", "paymentMarkPaid", 150);
            markPaid.Location = new Point(0, 228);
            markPaid.Click += (_, _) => MarkPaid(payment);
            detailBody.Controls.Add(markPaid);
        }

        var voidButton = AdminUi.MakeSecondaryButton("Void", "paymentVoid", 130);
        voidButton.ForeColor = AdminUi.Bad;
        voidButton.Location = new Point(0, 274);
        voidButton.Click += (_, _) => Void(payment);
        detailBody.Controls.Add(voidButton);

        var refund = AdminUi.MakeSecondaryButton("Refund", "paymentRefund", 130);
        refund.ForeColor = AdminUi.Warn;
        refund.Location = new Point(142, 274);
        refund.Click += (_, _) => Refund(payment);
        detailBody.Controls.Add(refund);

        detailBody.ResumeLayout();
    }

    private void MarkPaid(PaymentRecordDetails payment)
    {
        DemoDataStore.MarkPaymentPaid(payment);
        selectedPayment = DemoDataStore.Payments.FirstOrDefault(candidate => candidate.Reference == payment.Reference);
        RefreshList();
    }

    private void Void(PaymentRecordDetails payment)
    {
        DemoDataStore.VoidPayment(payment);
        selectedPayment = DemoDataStore.Payments.FirstOrDefault(candidate => candidate.Reference == payment.Reference);
        RefreshList();
    }

    private void Refund(PaymentRecordDetails payment)
    {
        DemoDataStore.RefundPayment(payment);
        selectedPayment = DemoDataStore.Payments.FirstOrDefault(candidate => candidate.Reference == payment.Reference);
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
        "Paid" => Color.FromArgb(232, 248, 241),
        "Pending" => Color.FromArgb(255, 244, 230),
        "Void" => Color.FromArgb(253, 236, 238),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color StatusFore(string status) => status switch
    {
        "Paid" => AdminUi.Good,
        "Pending" => AdminUi.Warn,
        "Void" => AdminUi.Bad,
        _ => AdminUi.Muted
    };

    private void ExportCsv()
    {
        var payments = DemoDataStore.Payments.OrderByDescending(payment => payment.Date).ToList();
        if (payments.Count == 0)
        {
            Toast("There are no payments to export.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Export payments",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            FileName = $"tasktap-payments-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
            DefaultExt = "csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var lines = new List<string>
        {
            "Reference,Task,Person,Method,Status,Amount,Date,Category,Barangay,Description"
        };
        foreach (var payment in payments)
        {
            lines.Add(string.Join(",", new[]
            {
                Csv(payment.Reference), Csv(payment.Task), Csv(payment.Person), Csv(payment.Method), Csv(payment.Status),
                payment.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                payment.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Csv(payment.Category), Csv(payment.Barangay), Csv(payment.Description)
            }));
        }

        File.WriteAllLines(dialog.FileName, lines);
        Toast($"Exported {payments.Count} payments.");
    }

    private static string Csv(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
