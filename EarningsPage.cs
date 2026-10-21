namespace TaskTap;

/// <summary>Doer earnings: payments recorded to the current member.</summary>
public sealed class EarningsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);

    private readonly Panel page = new();
    private readonly Panel totalsRow = new();
    private readonly List<Panel> totalCards = new();
    private readonly List<Label> totalValues = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Label countLabel = new();
    private readonly Label emptyLabel = new();
    private int _lastWidth = -1;

    public override Route RouteName => Route.Earnings;

    public EarningsPage()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = PageColor;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = MakeLabel("EARNINGS", 8, Blue, true);
        eyebrow.Name = "earningsEyebrow";
        page.Controls.Add(eyebrow);

        var title = MakeLabel("Earnings", 26, Navy, true);
        title.Name = "earningsTitle";
        page.Controls.Add(title);

        var subtitle = MakeLabel("Payments recorded to you for completed work.", 9, Muted);
        subtitle.Name = "earningsSubtitle";
        page.Controls.Add(subtitle);
        page.Controls.Add(PaymentsTabs.Build(false));

        totalsRow.BackColor = Color.Transparent;
        page.Controls.Add(totalsRow);
        var headings = new[] { "Total earned", "Pending", "Records" };
        for (var index = 0; index < headings.Length; index++)
        {
            var card = MakeCard();
            card.Name = $"earningsTotal{index + 1}";

            var heading = MakeLabel(headings[index], 8, Muted, true);
            heading.Location = new Point(14, 12);
            card.Controls.Add(heading);

            var value = MakeLabel("₱0", 18, Navy, true);
            value.Location = new Point(14, 34);
            card.Controls.Add(value);
            totalValues.Add(value);

            totalsRow.Controls.Add(card);
            totalCards.Add(card);
        }

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var listTitle = MakeLabel("Payment records", 11, Navy, true);
        listTitle.Location = new Point(18, 14);
        listCard.Controls.Add(listTitle);

        countLabel.AutoSize = true;
        countLabel.ForeColor = Muted;
        countLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        countLabel.Location = new Point(160, 16);
        listCard.Controls.Add(countLabel);

        emptyLabel.AutoSize = true;
        emptyLabel.ForeColor = Muted;
        emptyLabel.Font = new Font("Segoe UI", 9F);
        emptyLabel.Text = "No payments recorded to this account yet.";
        emptyLabel.Location = new Point(18, 56);
        listCard.Controls.Add(emptyLabel);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        RefreshEarnings();
        LayoutPage();

        DemoDataStore.DataChanged += OnDataChanged;
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        if (contentWidth == _lastWidth)
            return;
        _lastWidth = contentWidth;

        const int left = 32;
        var eyebrow = FindLabel("earningsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = FindLabel("earningsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = FindLabel("earningsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var tabs = page.Controls.Find("paymentsTabs", false).FirstOrDefault();
        if (tabs is not null) tabs.Location = new Point(left + contentWidth - tabs.Width, 12);

        totalsRow.Bounds = new Rectangle(left, 132, contentWidth, 84);
        const int gap = 14;
        var cardWidth = (contentWidth - gap * 2) / 3;
        for (var index = 0; index < totalCards.Count; index++)
        {
            totalCards[index].Location = new Point(index * (cardWidth + gap), 0);
            totalCards[index].Size = new Size(cardWidth, 84);
        }

        listCard.Bounds = new Rectangle(left, 232, contentWidth, 460);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);
        page.AutoScrollMinSize = new Size(0, 730);

        RefreshEarnings();
    }

    private void RefreshEarnings()
    {
        if (listBody.Width <= 0 || totalValues.Count < 3)
            return;

        var mine = DemoDataStore.Payments
            .Where(payment => payment.Person == AppSession.CurrentUser.FullName)
            .OrderByDescending(payment => payment.Date)
            .ToList();

        totalValues[0].Text = $"₱{mine.Where(payment => payment.Status == "Paid").Sum(payment => payment.Amount):N0}";
        totalValues[1].Text = $"₱{mine.Where(payment => payment.Status == "Pending").Sum(payment => payment.Amount):N0}";
        totalValues[2].Text = mine.Count.ToString();

        countLabel.Text = $"{mine.Count} record{(mine.Count == 1 ? "" : "s")}";
        emptyLabel.Visible = mine.Count == 0;

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var payment in mine)
        {
            var row = BuildRow(payment, width);
            row.SetBounds(0, y, width, 74);
            listBody.Controls.Add(row);
            y += 80;
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(PaymentRecordDetails payment, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var title = MakeLabel(payment.Task, 10, Navy, true);
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var meta = MakeLabel($"{payment.Reference} · {payment.Method} · {payment.Date:MMM d, yyyy}", 8, Muted);
        meta.Location = new Point(14, 34);
        row.Controls.Add(meta);

        var amount = MakeLabel($"₱{payment.Amount:N0}", 11, Navy, true);
        amount.Location = new Point(width - 220, 14);
        row.Controls.Add(amount);

        var (back, fore) = payment.Status switch
        {
            "Paid" => (Color.FromArgb(232, 248, 241), Color.FromArgb(20, 153, 111)),
            "Pending" => (Color.FromArgb(255, 244, 230), Color.FromArgb(180, 130, 20)),
            _ => (Color.FromArgb(240, 243, 247), Muted)
        };
        var status = MakeLabel(payment.Status, 8, fore, true);
        status.BackColor = back;
        status.Padding = new Padding(8, 3, 8, 3);
        status.Location = new Point(width - 96, 34);
        row.Controls.Add(status);

        return row;
    }

    private System.Windows.Forms.Label? FindLabel(string name) =>
        page.Controls.Find(name, false).OfType<System.Windows.Forms.Label>().FirstOrDefault();

    private static Panel MakeCard() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
    };

    private void OnDataChanged()
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
        {
            BeginInvoke(OnDataChanged);
            return;
        }

        RefreshEarnings();
    }

    public override void OnNavigatedTo() => RefreshEarnings();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
