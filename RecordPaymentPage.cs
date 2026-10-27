using TaskTap.Data;

namespace TaskTap;

public sealed class RecordPaymentPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Green = Color.FromArgb(19, 160, 113);
    private static readonly Color Muted = Color.FromArgb(102, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(228, 234, 242);
    private readonly PaymentRecordDetails payment;
    private readonly Panel page = new();
    private readonly Panel formCard = new();
    private readonly Panel previewCard = new();
    private readonly Panel checklistCard = new();
    private readonly Panel safetyCard = new();
    private readonly NumericUpDown amount = new() { Minimum = 0, Maximum = 10000000, Increment = 50, DecimalPlaces = 0, ThousandsSeparator = true };
    private readonly ComboBox method = new();
    private readonly ComboBox status = new();
    private readonly DateTimePicker paymentDate = new() { Format = DateTimePickerFormat.Long };
    private readonly TextBox reference = new() { MaxLength = 30 };
    private readonly TextBox note = new() { Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox confirmation = new();
    private readonly Label previewAmount = new();
    private readonly Label previewMethod = new();
    private readonly Label previewStatus = new();
    private readonly Label previewDate = new();
    private readonly Label previewReference = new();
    private readonly Label noteCount = new();
    private readonly Label amountError = new();
    private readonly Label dateError = new();
    private readonly Label confirmError = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private bool pageBuilt;

    public override Route RouteName => Route.RecordPayment;

    public RecordPaymentPage(PaymentRecordDetails? payment = null)
    {
        this.payment = payment ?? NewPaymentFromLatestTask();
        BuildPage();
    }

    /// <summary>Starts a blank payment record for the most recently completed task.</summary>
    private static PaymentRecordDetails NewPaymentFromLatestTask()
    {
        var outcome = DemoDataStore.TaskOutcomes.FirstOrDefault(item => item.Status == "Completed");
        var person = outcome is null
            ? ""
            : DemoDataStore.RequestsReceived.FirstOrDefault(applicant => applicant.TaskTitle == outcome.Title && applicant.Accepted)?.Name ?? "";
        return new PaymentRecordDetails(
            outcome?.Title ?? "Task", Backend.NextPaymentReference(), "Cash", "Paid", outcome?.Amount ?? 0, DateTime.Today,
            outcome?.Category ?? "", outcome?.Barangay ?? "", person, outcome?.Description ?? "");
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
        UpdatePreview();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Payment Record   /   Record Payment", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Completed task", 8, Blue, true, new Point(22, 40)));
        page.Controls.Add(MakeLabel("Record Payment", 23, Navy, true, new Point(22, 56)));
        page.Controls.Add(MakeLabel("Add or update the payment details agreed between the task poster and task doer.", 9, Muted, false, new Point(22, 104)));
        var cancelHeader = MakeButton("Cancel", false, 95);
        cancelHeader.Name = "RecordPaymentCancelHeader";
        cancelHeader.Click += (_, _) => Nav.Back();
        page.Controls.Add(cancelHeader);

        BuildFormCard();
        BuildPreviewCard();
        BuildChecklistCard();
        BuildSafetyCard();
    }

    private void BuildFormCard()
    {
        formCard.BackColor = Color.White;
        formCard.BorderStyle = BorderStyle.FixedSingle;
        formCard.AutoScroll = true;
        AddSectionHeading(formCard, "1", "Task & people", "Confirm which completed task this payment belongs to.", 18);
        var banner = new Panel { Name = "recordPaymentTaskBanner", BackColor = Color.FromArgb(247, 251, 255), BorderStyle = BorderStyle.FixedSingle };
        var icon = MakeLabel("✓", 12, Green, true, new Point(12, 12));
        icon.AutoSize = false;
        icon.Size = new Size(40, 40);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(223, 247, 238);
        banner.Controls.Add(icon);
        banner.Controls.Add(MakeLabel(payment.Task, 9, Navy, true, new Point(64, 10)));
        banner.Controls.Add(MakeLabel($"{payment.Person} · Task poster · Completed {payment.Date:MMM d, yyyy}", 8, Muted, false, new Point(64, 31)));
        var viewTask = MakeLink("View task →");
        viewTask.Name = "RecordPaymentViewTask";
        viewTask.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        banner.Controls.Add(viewTask);
        formCard.Controls.Add(banner);

        AddSectionHeading(formCard, "2", "Payment details", "Record the agreed amount, method, and status.", 132);
        AddFieldLabel(formCard, "Agreed amount *", 176);
        amount.Location = new Point(24, 198);
        amount.Size = new Size(300, 34);
        amount.Value = payment.Amount;
        amount.ValueChanged += (_, _) => UpdatePreview();
        formCard.Controls.Add(amount);
        AddFieldHint(formCard, "Enter the final amount agreed for the completed task.", 235);
        amountError.Location = new Point(24, 253);
        amountError.ForeColor = Color.FromArgb(214, 83, 83);
        amountError.AutoSize = true;
        formCard.Controls.Add(amountError);

        AddFieldLabel(formCard, "Payment method *", 276);
        ConfigureCombo(method, ["Cash"], "Cash");
        method.SetBounds(24, 298, 300, 34);
        method.SelectedIndexChanged += (_, _) => UpdatePreview();
        formCard.Controls.Add(method);

        AddFieldLabel(formCard, "Payment status *", 348);
        ConfigureCombo(status, ["Paid", "Pending", "Partially paid"], payment.Status == "Partially paid" ? "Partially paid" : payment.Status);
        status.SetBounds(24, 370, 300, 34);
        status.SelectedIndexChanged += (_, _) => UpdatePreview();
        formCard.Controls.Add(status);

        AddFieldLabel(formCard, "Payment date *", 420);
        paymentDate.SetBounds(24, 442, 300, 34);
        paymentDate.Value = payment.Date;
        paymentDate.ValueChanged += (_, _) => UpdatePreview();
        formCard.Controls.Add(paymentDate);
        dateError.Location = new Point(24, 478);
        dateError.ForeColor = Color.FromArgb(214, 83, 83);
        dateError.AutoSize = true;
        formCard.Controls.Add(dateError);

        AddFieldLabel(formCard, "Reference number", 500);
        reference.SetBounds(24, 522, 230, 34);
        reference.Text = payment.Reference;
        reference.TextChanged += (_, _) => UpdatePreview();
        formCard.Controls.Add(reference);
        var generate = MakeButton("Generate", false, 90);
        generate.SetBounds(264, 522, 90, 34);
        generate.Click += (_, _) => { reference.Text = Backend.NextPaymentReference(); ShowToast("New reference generated."); };
        formCard.Controls.Add(generate);
        AddFieldHint(formCard, "Optional reference. Do not enter bank credentials or OTPs.", 561);

        AddSectionHeading(formCard, "3", "Payment note", "Add context that both sides can understand later.", 600);
        note.SetBounds(24, 682, 500, 100);
        note.Text = payment.Status == "Paid" ? "Paid after the completed task was reviewed with the task poster." : "";
        note.TextChanged += (_, _) => { noteCount.Text = $"{note.TextLength} / 500"; };
        formCard.Controls.Add(note);
        noteCount.Location = new Point(24, 787);
        noteCount.AutoSize = true;
        noteCount.ForeColor = Muted;
        noteCount.Text = $"{note.TextLength} / 500";
        formCard.Controls.Add(noteCount);
        AddFieldHint(formCard, "Keep the note factual and avoid sensitive financial information.", 810);

        AddSectionHeading(formCard, "4", "Confirmation", "Review the information before saving the payment record.", 850);
        confirmation.SetBounds(24, 932, 26, 26);
        formCard.Controls.Add(confirmation);
        var confirmText = MakeLabel("I confirm that these payment details reflect the agreement between the users.", 8, Navy, false, new Point(54, 935), new Size(520, 26));
        formCard.Controls.Add(confirmText);
        confirmError.Location = new Point(24, 966);
        confirmError.AutoSize = true;
        confirmError.ForeColor = Color.FromArgb(214, 83, 83);
        formCard.Controls.Add(confirmError);
        var safety = new Panel { BackColor = Color.FromArgb(255, 248, 237), BorderStyle = BorderStyle.FixedSingle, Location = new Point(24, 990), Size = new Size(520, 62) };
        safety.Controls.Add(MakeLabel("✓", 11, Color.FromArgb(200, 120, 24), true, new Point(12, 14)));
        safety.Controls.Add(MakeLabel("Never enter sensitive credentials", 8, Navy, true, new Point(48, 8)));
        safety.Controls.Add(MakeLabel("TaskTap never needs your PIN, OTP, password, card number, or banking login.", 7, Muted, false, new Point(48, 29), new Size(450, 25)));
        formCard.Controls.Add(safety);
        var cancel = MakeButton("Cancel", false, 100);
        cancel.SetBounds(292, 1070, 100, 38);
        cancel.Click += (_, _) => Nav.Back();
        formCard.Controls.Add(cancel);
        var save = MakeButton("Save Payment Record", true, 170);
        save.SetBounds(400, 1070, 170, 38);
        save.Click += (_, _) => SavePayment();
        formCard.Controls.Add(save);
        page.Controls.Add(formCard);
    }

    private void BuildPreviewCard()
    {
        previewCard.BackColor = Color.White;
        previewCard.BorderStyle = BorderStyle.FixedSingle;
        previewCard.Controls.Add(MakeLabel("LIVE PREVIEW", 8, Blue, true, new Point(18, 18)));
        previewStatus.AutoSize = false;
        previewStatus.Size = new Size(94, 25);
        previewStatus.TextAlign = ContentAlignment.MiddleCenter;
        previewStatus.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        previewCard.Controls.Add(previewStatus);
        previewAmount.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
        previewAmount.ForeColor = Navy;
        previewAmount.Location = new Point(18, 55);
        previewAmount.AutoSize = true;
        previewCard.Controls.Add(previewAmount);
        previewCard.Controls.Add(MakeLabel(payment.Task, 8, Muted, false, new Point(18, 103)));
        AddPreviewRow("Method", previewMethod, 142);
        AddPreviewRow("Date", previewDate, 178);
        AddPreviewRow("Reference", previewReference, 214);
        page.Controls.Add(previewCard);
    }

    private void AddPreviewRow(string title, Label value, int y)
    {
        var row = new Panel { Location = new Point(18, y), Height = 34, Name = $"previewRow{y}" };
        row.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(237, 241, 246)); e.Graphics.DrawLine(pen, 0, 0, row.Width, 0); };
        row.Controls.Add(MakeLabel(title, 8, Muted, false, new Point(0, 10)));
        value.AutoSize = false;
        value.TextAlign = ContentAlignment.MiddleRight;
        value.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        value.ForeColor = Navy;
        row.Controls.Add(value);
        previewCard.Controls.Add(row);
    }

    private void BuildChecklistCard()
    {
        checklistCard.BackColor = Color.White;
        checklistCard.BorderStyle = BorderStyle.FixedSingle;
        checklistCard.Controls.Add(MakeLabel("Before you save", 11, Navy, true, new Point(18, 18)));
        var points = new[] { "Confirm the final agreed amount.", "Use the actual payment date.", "Keep references non-sensitive.", "Record partial payments honestly." };
        for (var i = 0; i < points.Length; i++)
        {
            checklistCard.Controls.Add(MakeLabel("✓", 8, Green, true, new Point(18, 52 + i * 30)));
            checklistCard.Controls.Add(MakeLabel(points[i], 8, Muted, false, new Point(38, 52 + i * 30), new Size(250, 25)));
        }
        page.Controls.Add(checklistCard);
    }

    private void BuildSafetyCard()
    {
        safetyCard.BackColor = Color.FromArgb(247, 251, 255);
        safetyCard.BorderStyle = BorderStyle.FixedSingle;
        safetyCard.Controls.Add(MakeLabel("✓", 12, Blue, true, new Point(18, 16)));
        safetyCard.Controls.Add(MakeLabel("Payment safety", 10, Navy, true, new Point(18, 48)));
        safetyCard.Controls.Add(MakeLabel("TaskTap only records payment information in this prototype. It does not process, hold, or transfer money.", 8, Muted, false, new Point(18, 76), new Size(270, 70)));
        page.Controls.Add(safetyCard);
    }

    private void AddSectionHeading(Control parent, string step, string title, string subtitle, int y)
    {
        var badge = MakeLabel(step, 8, Blue, true, new Point(24, y));
        badge.AutoSize = false;
        badge.Size = new Size(30, 30);
        badge.TextAlign = ContentAlignment.MiddleCenter;
        badge.BackColor = Color.FromArgb(234, 244, 255);
        parent.Controls.Add(badge);
        parent.Controls.Add(MakeLabel(title, 11, Navy, true, new Point(66, y - 1)));
        parent.Controls.Add(MakeLabel(subtitle, 8, Muted, false, new Point(66, y + 20), new Size(520, 22)));
    }

    private void AddFieldLabel(Control parent, string text, int y) => parent.Controls.Add(MakeLabel(text, 8, Navy, true, new Point(24, y)));
    private void AddFieldHint(Control parent, string text, int y) => parent.Controls.Add(MakeLabel(text, 7, Muted, false, new Point(24, y), new Size(530, 18)));

    private static void ConfigureCombo(ComboBox combo, string[] options, string selected)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 9F);
        combo.Items.AddRange(options);
        var index = combo.Items.IndexOf(selected);
        combo.SelectedIndex = index >= 0 ? index : 0;
    }

    private void UpdatePreview()
    {
        previewAmount.Text = $"₱{amount.Value:N0}";
        previewMethod.Text = method.SelectedItem?.ToString() ?? "Cash";
        previewDate.Text = paymentDate.Value.ToString("MMM d, yyyy");
        previewReference.Text = string.IsNullOrWhiteSpace(reference.Text) ? "—" : reference.Text;
        var isPending = status.SelectedItem?.ToString() == "Pending";
        var isPartial = status.SelectedItem?.ToString() == "Partially paid";
        previewStatus.Text = status.SelectedItem?.ToString() ?? "Paid";
        previewStatus.ForeColor = isPending ? Color.FromArgb(169, 101, 16) : isPartial ? Color.FromArgb(73, 101, 181) : Green;
        previewStatus.BackColor = isPending ? Color.FromArgb(255, 242, 220) : isPartial ? Color.FromArgb(233, 239, 255) : Color.FromArgb(224, 247, 239);
    }

    private void SavePayment()
    {
        amountError.Text = amount.Value <= 0 ? "Enter an amount greater than ₱0." : "";
        dateError.Text = paymentDate.Value.Date > DateTime.Today ? "Select a payment date no later than today." : "";
        confirmError.Text = confirmation.Checked ? "" : "Confirm the payment details before saving.";
        if (amount.Value <= 0 || paymentDate.Value.Date > DateTime.Today || !confirmation.Checked)
        {
            if (amount.Value <= 0) amount.Focus();
            else if (paymentDate.Value.Date > DateTime.Today) paymentDate.Focus();
            else confirmation.Focus();
            return;
        }
        DemoDataStore.AddPayment(new PaymentRecordDetails(
            payment.Task,
            string.IsNullOrWhiteSpace(reference.Text) ? Backend.NextPaymentReference() : reference.Text.Trim(),
            method.SelectedItem?.ToString() ?? "Cash",
            status.SelectedItem?.ToString() ?? "Paid",
            amount.Value,
            paymentDate.Value.Date,
            payment.Category,
            payment.Barangay,
            payment.Person,
            string.IsNullOrWhiteSpace(note.Text) ? "Completed task" : note.Text.Trim()));
        MessageBox.Show(this, $"Your payment record for {payment.Task} was saved. TaskTap does not move money; it only keeps the record.", "Payment record saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var margin = 22;
        var contentWidth = width - margin * 2;
        var edit = page.Controls.Find("RecordPaymentCancelHeader", false).FirstOrDefault();
        if (edit is not null) edit.Location = new Point(width - margin - edit.Width, 52);
        var gap = 20;
        var sideWidth = Math.Min(330, Math.Max(290, contentWidth / 3));
        var formWidth = contentWidth - sideWidth - gap;
        formCard.Bounds = new Rectangle(margin, 132, formWidth, 1140);
        formCard.Controls.Find("recordPaymentTaskBanner", false).FirstOrDefault()?.SetBounds(24, 62, formWidth - 48, 64);
        var viewTask = formCard.Controls.Find("recordPaymentTaskBanner", false).FirstOrDefault()?.Controls.Find("RecordPaymentViewTask", false).FirstOrDefault();
        if (viewTask is not null) viewTask.Location = new Point(formWidth - 180, 18);
        var banner = formCard.Controls.Find("recordPaymentTaskBanner", false).FirstOrDefault();
        if (banner is not null)
            foreach (Control control in banner.Controls)
                if (control is Label label && label.Location.X == 64) label.MaximumSize = new Size(formWidth - 260, 0);
        foreach (Control control in formCard.Controls)
            if (control is TextBox || control is NumericUpDown || control is ComboBox || control is DateTimePicker)
                control.Width = Math.Max(260, formWidth - 48);
        var amountGenerate = formCard.Controls.OfType<Button>().FirstOrDefault(button => button.Text == "Generate");
        if (amountGenerate is not null)
        {
            amountGenerate.Location = new Point(formWidth - 120, 522);
            reference.Width = Math.Max(100, formWidth - 48 - amountGenerate.Width - 10);
        }
        var noteControl = formCard.Controls.OfType<TextBox>().FirstOrDefault();
        if (noteControl is not null) noteControl.Width = formWidth - 48;
        var safety = formCard.Controls.OfType<Panel>().FirstOrDefault(panel => panel.BackColor == Color.FromArgb(255, 248, 237));
        if (safety is not null) safety.Width = formWidth - 48;
        previewCard.Bounds = new Rectangle(margin + formWidth + gap, 132, sideWidth, 282);
        previewStatus.Location = new Point(sideWidth - 112, 12);
        foreach (Control control in previewCard.Controls)
            if (control is Panel row) { row.Width = sideWidth - 36; row.Controls.OfType<Label>().Last().Bounds = new Rectangle(row.Width - 150, 4, 145, 26); }
        checklistCard.Bounds = new Rectangle(margin + formWidth + gap, 430, sideWidth, 202);
        safetyCard.Bounds = new Rectangle(margin + formWidth + gap, 648, sideWidth, 170);
        safetyCard.Controls.OfType<Label>().Last().Size = new Size(sideWidth - 36, 70);
        page.AutoScrollMinSize = new Size(0, Math.Max(formCard.Bottom, safetyCard.Bottom) + 24);
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
