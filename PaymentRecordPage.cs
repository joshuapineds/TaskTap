namespace TaskTap;

public sealed class PaymentRecordPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Green = Color.FromArgb(8, 122, 88);
    private static readonly Color Muted = Color.FromArgb(105, 124, 145);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(228, 234, 242);
    private readonly PaymentRecordDetails payment;
    private readonly Panel page = new();
    private readonly Panel hero = new();
    private readonly Panel detailsCard = new();
    private readonly Panel taskCard = new();
    private readonly Panel noticeCard = new();
    private readonly Panel receiptCard = new();
    private readonly Panel actionsCard = new();
    private readonly Panel safetyCard = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private bool pageBuilt;

    public override Route RouteName => Route.PaymentRecord;

    public PaymentRecordPage(PaymentRecordDetails payment)
    {
        this.payment = payment;
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
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Payment History   /   Payment Record", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Completed task", 8, Blue, true, new Point(22, 40)));
        page.Controls.Add(MakeLabel("Payment Record", 23, Navy, true, new Point(22, 56)));
        page.Controls.Add(MakeLabel("Review the payment details recorded for this TaskTap task.", 9, Muted, false, new Point(22, 104)));
        var history = MakeButton("Payment History", false, 134);
        history.Name = "PaymentRecordHistoryButton";
        history.Click += (_, _) => Nav.Back();
        page.Controls.Add(history);
        var edit = MakeButton("Edit Payment Record", true, 168);
        edit.Name = "PaymentRecordEditButton";
        edit.Click += (_, _) => Nav.Navigate(new RecordPaymentPage(payment));
        page.Controls.Add(edit);

        BuildHero();
        BuildDetails();
        BuildTaskContext();
        BuildNotice();
        BuildReceipt();
        BuildActions();
        BuildSafety();
    }

    private void BuildHero()
    {
        hero.BackColor = Color.FromArgb(241, 247, 255);
        hero.BorderStyle = BorderStyle.FixedSingle;
        var status = MakeLabel($"●  {payment.Status}", 9, payment.Status == "Paid" ? Green : Color.FromArgb(185, 92, 16), true, new Point(24, 18));
        hero.Controls.Add(status);
        hero.Controls.Add(MakeLabel(payment.Task, 8, Blue, true, new Point(24, 64)));
        hero.Controls.Add(MakeLabel($"₱{payment.Amount:N0}", 31, Navy, true, new Point(24, 82)));
        hero.Controls.Add(MakeLabel("Agreed compensation", 8, Muted, false, new Point(24, 129)));
        var refLabel = MakeLabel("Payment reference", 8, Muted, false, Point.Empty);
        var refValue = MakeLabel(payment.Reference, 15, Navy, true, Point.Empty);
        var date = MakeLabel($"Recorded {payment.Date:MMM d, yyyy}", 8, Muted, false, Point.Empty);
        hero.Controls.Add(refLabel);
        hero.Controls.Add(refValue);
        hero.Controls.Add(date);
        page.Controls.Add(hero);
    }

    private void BuildDetails()
    {
        detailsCard.BackColor = Color.White;
        detailsCard.BorderStyle = BorderStyle.FixedSingle;
        detailsCard.Controls.Add(MakeLabel("PAYMENT DETAILS", 8, Blue, true, new Point(20, 16)));
        detailsCard.Controls.Add(MakeLabel("Transaction summary", 14, Navy, true, new Point(20, 34)));
        detailsCard.Controls.Add(MakeLabel(payment.Status, 8, payment.Status == "Paid" ? Green : Color.FromArgb(185, 92, 16), true, new Point(0, 25)));
        var fields = new[]
        {
            ("Agreed amount", $"₱{payment.Amount:N0}"),
            ("Payment method", payment.Method),
            ("Payment status", payment.Status),
            ("Payment date", payment.Date.ToString("MMMM d, yyyy")),
            ("Reference number", payment.Reference),
            ("Task", payment.Task)
        };
        for (var i = 0; i < fields.Length; i++)
        {
            var cell = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            cell.Controls.Add(MakeLabel(fields[i].Item1, 8, Muted, false, new Point(12, 10)));
            cell.Controls.Add(MakeLabel(fields[i].Item2, 9, i == 2 && payment.Status == "Paid" ? Green : Navy, true, new Point(12, 32)));
            detailsCard.Controls.Add(cell);
        }
        page.Controls.Add(detailsCard);
    }

    private void BuildTaskContext()
    {
        taskCard.BackColor = Color.White;
        taskCard.BorderStyle = BorderStyle.FixedSingle;
        taskCard.Controls.Add(MakeLabel("TASK CONTEXT", 8, Blue, true, new Point(20, 16)));
        taskCard.Controls.Add(MakeLabel(payment.Task, 14, Navy, true, new Point(20, 34)));
        var viewTask = MakeLink("View task →");
        viewTask.Name = "PaymentRecordViewTaskButton";
        viewTask.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        taskCard.Controls.Add(viewTask);
        var avatar = MakeLabel(GetInitials(payment.Person), 9, Color.FromArgb(20, 84, 160), true, new Point(20, 78));
        avatar.AutoSize = false;
        avatar.Size = new Size(42, 42);
        avatar.TextAlign = ContentAlignment.MiddleCenter;
        avatar.BackColor = Color.FromArgb(219, 234, 254);
        taskCard.Controls.Add(avatar);
        var person = MakeLabel(payment.Person, 9, Navy, true, new Point(74, 80));
        person.Cursor = Cursors.Hand;
        person.Click += (_, _) => Nav.Navigate(ProfileLinks.For(payment.Person));
        taskCard.Controls.Add(person);
        taskCard.Controls.Add(MakeLabel("Completed task participant", 8, Muted, false, new Point(74, 101)));
        taskCard.Controls.Add(MakeLabel($"⌖ Brgy. {payment.Barangay}     ✓ Completed {payment.Date:MMM d, yyyy}", 8, Muted, false, new Point(20, 139)));
        taskCard.Controls.Add(MakeLabel(payment.Description, 8, Muted, false, new Point(20, 166), new Size(680, 58)));
        page.Controls.Add(taskCard);
    }

    private void BuildNotice()
    {
        noticeCard.BackColor = Color.FromArgb(255, 248, 239);
        noticeCard.BorderStyle = BorderStyle.FixedSingle;
        noticeCard.Controls.Add(MakeLabel("✓", 12, Color.FromArgb(214, 107, 25), true, new Point(16, 17)));
        noticeCard.Controls.Add(MakeLabel("Payment recorded directly between users", 9, Navy, true, new Point(58, 16)));
        noticeCard.Controls.Add(MakeLabel("TaskTap records the agreement and payment status for reference. It does not process, hold, or transfer real money.", 8, Muted, false, new Point(58, 40), new Size(680, 40)));
        page.Controls.Add(noticeCard);
    }

    private void BuildReceipt()
    {
        receiptCard.BackColor = Color.White;
        receiptCard.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("₱", 16, Blue, true, new Point(18, 18));
        icon.AutoSize = false;
        icon.Size = new Size(42, 42);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(234, 243, 255);
        receiptCard.Controls.Add(icon);
        receiptCard.Controls.Add(MakeLabel("Payment receipt", 10, Navy, true, new Point(72, 20)));
        receiptCard.Controls.Add(MakeLabel(payment.Reference, 8, Muted, false, new Point(72, 42)));
        receiptCard.Controls.Add(MakeLabel($"₱{payment.Amount:N0}", 26, Navy, true, new Point(18, 70)));
        AddReceiptRow("Status", payment.Status, 134);
        AddReceiptRow("Method", payment.Method, 170);
        AddReceiptRow("Date", payment.Date.ToString("MMM d, yyyy"), 206);
        AddReceiptRow("Task", payment.Task, 242);
        var print = MakeButton("Print / Save receipt", false, 240);
        print.Name = "PaymentRecordPrintButton";
        print.Location = new Point(18, 290);
        print.Click += (_, _) => ShowToast("Receipt print preview is available in the full app.");
        receiptCard.Controls.Add(print);
        page.Controls.Add(receiptCard);
    }

    private void AddReceiptRow(string label, string value, int y)
    {
        var row = new Panel { Name = $"receiptRow{y}", Location = new Point(18, y), Height = 34 };
        row.Controls.Add(MakeLabel(label, 8, Muted, false, new Point(0, 8)));
        var valueLabel = MakeLabel(value, 8, Navy, true, Point.Empty);
        valueLabel.Name = "receiptValue";
        row.Controls.Add(valueLabel);
        receiptCard.Controls.Add(row);
    }

    private void BuildActions()
    {
        actionsCard.BackColor = Color.White;
        actionsCard.BorderStyle = BorderStyle.FixedSingle;
        actionsCard.Controls.Add(MakeLabel("Payment actions", 12, Navy, true, new Point(18, 16)));
        AddActionRow("✎", "Edit payment", "Update the recorded details", 52, () => Nav.Navigate(new RecordPaymentPage(payment)));
        AddActionRow("₱", "Payment history", "See all recorded payments", 111, () => Nav.Back());
        AddActionRow("★", "Leave a review", "Rate this completed task", 170, () => Nav.Navigate(new RateReviewPage(payment)));
        page.Controls.Add(actionsCard);
    }

    private void AddActionRow(string icon, string title, string subtitle, int y, Action action)
    {
        var row = new Panel { Location = new Point(12, y), Size = new Size(290, 54), Cursor = Cursors.Hand, BackColor = Color.White };
        var symbol = MakeLabel(icon, 11, Blue, true, new Point(0, 8));
        symbol.AutoSize = false;
        symbol.Size = new Size(32, 32);
        symbol.TextAlign = ContentAlignment.MiddleCenter;
        symbol.BackColor = Color.FromArgb(240, 245, 251);
        row.Controls.Add(symbol);
        row.Controls.Add(MakeLabel(title, 8, Navy, true, new Point(42, 7)));
        row.Controls.Add(MakeLabel(subtitle, 7, Muted, false, new Point(42, 27)));
        row.Click += (_, _) => action();
        foreach (Control child in row.Controls) child.Click += (_, _) => action();
        actionsCard.Controls.Add(row);
    }

    private void BuildSafety()
    {
        safetyCard.BackColor = Color.FromArgb(245, 251, 255);
        safetyCard.BorderStyle = BorderStyle.FixedSingle;
        safetyCard.Controls.Add(MakeLabel("✓", 13, Blue, true, new Point(18, 16)));
        safetyCard.Controls.Add(MakeLabel("Stay safe with payments", 10, Navy, true, new Point(18, 48)));
        safetyCard.Controls.Add(MakeLabel("Confirm the agreed amount and keep your payment reference. Never share OTPs, passwords, PINs, or banking credentials.", 8, Muted, false, new Point(18, 76), new Size(275, 90)));
        page.Controls.Add(safetyCard);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var margin = 22;
        var contentWidth = width - margin * 2;
        var history = page.Controls.Find("PaymentRecordHistoryButton", false).FirstOrDefault();
        var edit = page.Controls.Find("PaymentRecordEditButton", false).FirstOrDefault();
        if (edit is not null) edit.Location = new Point(width - margin - edit.Width, 52);
        if (history is not null) history.Location = new Point((edit?.Left ?? width - margin) - history.Width - 8, 52);

        var gap = 20;
        var sideWidth = Math.Min(330, Math.Max(285, contentWidth / 3));
        var mainWidth = contentWidth - sideWidth - gap;
        hero.Bounds = new Rectangle(margin, 132, contentWidth, 174);
        var refLabels = hero.Controls.OfType<Label>().Skip(4).ToArray();
        if (refLabels.Length == 3)
        {
            refLabels[0].Location = new Point(contentWidth - 220, 68);
            refLabels[1].Location = new Point(contentWidth - 220, 91);
            refLabels[2].Location = new Point(contentWidth - 220, 119);
        }
        detailsCard.Bounds = new Rectangle(margin, 324, mainWidth, 260);
        var cellWidth = (mainWidth - 42) / 2;
        var cells = detailsCard.Controls.OfType<Panel>().ToArray();
        for (var i = 0; i < cells.Length; i++)
            cells[i].Bounds = new Rectangle(20 + (i % 2) * (cellWidth + 2), 78 + (i / 2) * 56, cellWidth, 54);
        var statusLabel = detailsCard.Controls.OfType<Label>().Last();
        statusLabel.Location = new Point(mainWidth - 90, 25);

        taskCard.Bounds = new Rectangle(margin, 602, mainWidth, 246);
        taskCard.Controls.Find("PaymentRecordViewTaskButton", false).FirstOrDefault()?.SetBounds(mainWidth - 110, 36, 95, 26);
        var desc = taskCard.Controls.OfType<Label>().Last();
        desc.Size = new Size(mainWidth - 40, 58);
        noticeCard.Bounds = new Rectangle(margin, 866, mainWidth, 92);
        noticeCard.Controls.OfType<Label>().Last().Size = new Size(mainWidth - 80, 40);
        receiptCard.Bounds = new Rectangle(margin + mainWidth + gap, 326, sideWidth, 340);
        var receiptButton = receiptCard.Controls.OfType<Button>().First();
        receiptButton.Width = sideWidth - 36;
        foreach (Control row in receiptCard.Controls)
        {
            if (row.Name.StartsWith("receiptRow", StringComparison.Ordinal))
            {
                row.Width = sideWidth - 36;
                var value = row.Controls.OfType<Label>().Last();
                value.Location = new Point(row.Width - Math.Max(100, TextRenderer.MeasureText(value.Text, value.Font).Width) - 2, 8);
                value.MaximumSize = new Size(row.Width / 2, 24);
            }
        }
        actionsCard.Bounds = new Rectangle(margin + mainWidth + gap, 686, sideWidth, 244);
        foreach (var row in actionsCard.Controls.OfType<Panel>()) row.Width = sideWidth - 24;
        safetyCard.Bounds = new Rectangle(margin + mainWidth + gap, 950, sideWidth, 180);
        safetyCard.Controls.OfType<Label>().Last().Size = new Size(sideWidth - 36, 90);
        page.AutoScrollMinSize = new Size(0, Math.Max(noticeCard.Bottom, safetyCard.Bottom) + 24);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private static string GetInitials(string name) => string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));

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
