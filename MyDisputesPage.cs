namespace TaskTap;

/// <summary>Signed-in list of the member's disputes with status and resolution.</summary>
public sealed class MyDisputesPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);

    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Label countLabel = new();
    private readonly Label emptyLabel = new();
    private int _lastWidth = -1;

    public override Route RouteName => Route.MyDisputes;

    public MyDisputesPage()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = PageColor;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = MakeLabel("SUPPORT", 8, Blue, true);
        eyebrow.Name = "disputesMineEyebrow";
        page.Controls.Add(eyebrow);

        var title = MakeLabel("My Disputes", 26, Navy, true);
        title.Name = "disputesMineTitle";
        page.Controls.Add(title);

        var subtitle = MakeLabel("Disagreements you opened on completed tasks, and how they were resolved.", 9, Muted);
        subtitle.Name = "disputesMineSubtitle";
        page.Controls.Add(subtitle);

        var open = MakeAction("＋  Open a dispute", "MyDisputesOpenButton");
        open.Click += (_, _) => Nav.Navigate(Route.ArchivedTasks);
        page.Controls.Add(open);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var heading = MakeLabel("Your disputes", 11, Navy, true);
        heading.Location = new Point(18, 14);
        listCard.Controls.Add(heading);

        countLabel.AutoSize = true;
        countLabel.ForeColor = Muted;
        countLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        countLabel.Location = new Point(150, 16);
        listCard.Controls.Add(countLabel);

        emptyLabel.AutoSize = true;
        emptyLabel.ForeColor = Muted;
        emptyLabel.Font = new Font("Segoe UI", 9F);
        emptyLabel.Text = "No disputes yet. Open one from a completed task in Archived Tasks.";
        emptyLabel.Location = new Point(18, 56);
        listCard.Controls.Add(emptyLabel);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        RefreshList();
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
        var eyebrow = FindLabel("disputesMineEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = FindLabel("disputesMineTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = FindLabel("disputesMineSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var open = page.Controls.OfType<Button>().FirstOrDefault(button => button.Name == "MyDisputesOpenButton");
        if (open is not null) open.Location = new Point(left + contentWidth - open.Width, 40);

        listCard.Bounds = new Rectangle(left, 132, contentWidth, 560);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);
        page.AutoScrollMinSize = new Size(0, 730);

        RefreshList();
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        var mine = DemoDataStore.Disputes
            .Where(dispute => dispute.PosterName == AppSession.CurrentUser.FullName || dispute.DoerName == AppSession.CurrentUser.FullName)
            .ToList();

        countLabel.Text = $"{mine.Count} dispute{(mine.Count == 1 ? "" : "s")}";
        emptyLabel.Visible = mine.Count == 0;

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var dispute in mine)
        {
            var row = BuildRow(dispute, width);
            row.SetBounds(0, y, width, 96);
            listBody.Controls.Add(row);
            y += 102;
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(DemoDispute dispute, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var title = MakeLabel($"{dispute.Id} · {dispute.TaskTitle}", 10, Navy, true);
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var reason = MakeLabel(dispute.Reason, 8, Muted);
        reason.Location = new Point(14, 34);
        row.Controls.Add(reason);

        var counterpart = dispute.PosterName == AppSession.CurrentUser.FullName ? dispute.DoerName : dispute.PosterName;
        var meta = MakeLabel($"{dispute.PosterName} vs {dispute.DoerName} · ₱{dispute.Amount:N0} · opened {dispute.OpenedAt}", 7.5F, Muted);
        meta.Location = new Point(14, 54);
        row.Controls.Add(meta);

        var profile = MakeLink("View profile", $"DisputeProfile{dispute.Id}");
        profile.Location = new Point(width - 110, 54);
        profile.Click += (_, _) => Nav.Navigate(ProfileLinks.For(counterpart));
        row.Controls.Add(profile);

        if (!string.IsNullOrWhiteSpace(dispute.Resolution))
        {
            var resolution = MakeLabel($"Resolution: {dispute.Resolution}", 7.5F, Color.FromArgb(20, 153, 111));
            resolution.Location = new Point(14, 74);
            row.Controls.Add(resolution);
        }

        var status = MakePill(dispute.Status);
        status.Location = new Point(width - 130, 16);
        row.Controls.Add(status);

        return row;
    }

    private static Label MakePill(string status)
    {
        var (back, fore) = status switch
        {
            "Open" => (Color.FromArgb(255, 244, 230), Color.FromArgb(180, 130, 20)),
            "Resolved" => (Color.FromArgb(232, 248, 241), Color.FromArgb(20, 153, 111)),
            _ => (Color.FromArgb(240, 243, 247), Color.FromArgb(100, 119, 143))
        };

        var label = MakeLabel(status, 8, fore, true);
        label.BackColor = back;
        label.Padding = new Padding(8, 3, 8, 3);
        return label;
    }

    private Button MakeAction(string text, string name)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(170, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Blue,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button MakeLink(string text, string name)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Blue,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private System.Windows.Forms.Label? FindLabel(string name) =>
        page.Controls.Find(name, false).OfType<System.Windows.Forms.Label>().FirstOrDefault();

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

        RefreshList();
    }

    public override void OnNavigatedTo() => RefreshList();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
