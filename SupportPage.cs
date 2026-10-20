namespace TaskTap;

/// <summary>Support: send a request and track the member's tickets.</summary>
public sealed class SupportPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);

    private readonly Panel page = new();
    private readonly Panel formCard = new();
    private readonly Panel ticketsCard = new();
    private readonly Panel ticketsBody = new();
    private readonly ComboBox category = new();
    private readonly TextBox subject = new();
    private readonly TextBox message = new();
    private readonly Label feedback = new();
    private readonly Label countLabel = new();
    private readonly Label emptyLabel = new();
    private int _lastWidth = -1;

    public override Route RouteName => Route.Support;

    public SupportPage()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = PageColor;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = MakeLabel("SUPPORT", 8, Blue, true);
        eyebrow.Name = "supportEyebrow";
        page.Controls.Add(eyebrow);

        var title = MakeLabel("Support", 26, Navy, true);
        title.Name = "supportTitle";
        page.Controls.Add(title);

        var subtitle = MakeLabel("Send a request and track the team's replies.", 9, Muted);
        subtitle.Name = "supportSubtitle";
        page.Controls.Add(subtitle);

        BuildForm();
        BuildTickets();

        RefreshTickets();
        LayoutPage();

        DemoDataStore.DataChanged += OnDataChanged;
    }

    private void BuildForm()
    {
        formCard.BackColor = Color.White;
        formCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(formCard);

        var title = MakeLabel("Send a request", 11, Navy, true);
        title.Location = new Point(18, 14);
        formCard.Controls.Add(title);

        var categoryLabel = MakeLabel("Category", 8, Muted, true);
        categoryLabel.Location = new Point(18, 50);
        formCard.Controls.Add(categoryLabel);

        category.Name = "supportCategory";
        category.DropDownStyle = ComboBoxStyle.DropDownList;
        category.Font = new Font("Segoe UI", 9F);
        category.FlatStyle = FlatStyle.Flat;
        category.Items.AddRange(["Account", "Payments", "Task issue", "Verification", "Other"]);
        category.SelectedIndex = 0;
        formCard.Controls.Add(category);

        var subjectLabel = MakeLabel("Subject", 8, Muted, true);
        subjectLabel.Location = new Point(260, 50);
        formCard.Controls.Add(subjectLabel);

        subject.Name = "supportSubject";
        subject.BorderStyle = BorderStyle.FixedSingle;
        subject.Font = new Font("Segoe UI", 9F);
        subject.PlaceholderText = "Short summary of the issue";
        formCard.Controls.Add(subject);

        var messageLabel = MakeLabel("Message", 8, Muted, true);
        messageLabel.Location = new Point(18, 112);
        formCard.Controls.Add(messageLabel);

        message.Name = "supportMessage";
        message.BorderStyle = BorderStyle.FixedSingle;
        message.Font = new Font("Segoe UI", 9F);
        message.Multiline = true;
        message.ScrollBars = ScrollBars.Vertical;
        message.PlaceholderText = "Tell us what happened and what you need.";
        formCard.Controls.Add(message);

        var submit = MakeAction("Send request", "supportSubmitButton");
        submit.Click += (_, _) => Submit();
        formCard.Controls.Add(submit);

        feedback.AutoSize = true;
        feedback.ForeColor = Color.FromArgb(20, 153, 111);
        feedback.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        formCard.Controls.Add(feedback);
    }

    private void BuildTickets()
    {
        ticketsCard.BackColor = Color.White;
        ticketsCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(ticketsCard);

        var title = MakeLabel("My tickets", 11, Navy, true);
        title.Location = new Point(18, 14);
        ticketsCard.Controls.Add(title);

        countLabel.AutoSize = true;
        countLabel.ForeColor = Muted;
        countLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        countLabel.Location = new Point(110, 16);
        ticketsCard.Controls.Add(countLabel);

        emptyLabel.AutoSize = true;
        emptyLabel.ForeColor = Muted;
        emptyLabel.Font = new Font("Segoe UI", 9F);
        emptyLabel.Text = "No tickets yet. Send a request above.";
        emptyLabel.Location = new Point(18, 56);
        ticketsCard.Controls.Add(emptyLabel);

        ticketsBody.BackColor = Color.White;
        ticketsBody.AutoScroll = true;
        ticketsCard.Controls.Add(ticketsBody);
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
        var eyebrow = FindLabel("supportEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = FindLabel("supportTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = FindLabel("supportSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        formCard.Bounds = new Rectangle(left, 132, contentWidth, 280);
        category.Bounds = new Rectangle(18, 70, 220, 30);
        subject.Bounds = new Rectangle(260, 70, Math.Max(240, contentWidth - 278), 30);
        message.Bounds = new Rectangle(18, 132, contentWidth - 36, 80);
        var submit = formCard.Controls.Find("supportSubmitButton", false).FirstOrDefault();
        if (submit is not null) submit.Location = new Point(18, 226);
        feedback.Location = new Point(190, 236);

        ticketsCard.Bounds = new Rectangle(left, 428, contentWidth, 400);
        ticketsBody.Bounds = new Rectangle(14, 46, ticketsCard.Width - 28, ticketsCard.Height - 58);

        page.AutoScrollMinSize = new Size(0, 860);
        RefreshTickets();
    }

    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(subject.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            feedback.ForeColor = Color.FromArgb(185, 55, 67);
            feedback.Text = "Enter a subject and a message.";
            return;
        }

        DemoDataStore.RequestSupport(new DemoSupportTicket
        {
            Id = DemoDataStore.NextTicketId(),
            Name = AppSession.CurrentUser.FullName,
            Email = AppSession.CurrentUser.Email,
            Category = category.SelectedItem?.ToString() ?? "Other",
            Subject = subject.Text.Trim(),
            Message = message.Text.Trim(),
            Status = "Open",
            CreatedAt = DateTime.Today.ToString("MMM d, yyyy")
        });

        subject.Clear();
        message.Clear();
        feedback.ForeColor = Color.FromArgb(20, 153, 111);
        feedback.Text = "Request sent. We will reply to your account email.";
        RefreshTickets();
    }

    private void RefreshTickets()
    {
        if (ticketsBody.Width <= 0)
            return;

        var mine = DemoDataStore.SupportTickets
            .Where(ticket => ticket.Name == AppSession.CurrentUser.FullName)
            .ToList();

        countLabel.Text = $"{mine.Count} ticket{(mine.Count == 1 ? "" : "s")}";
        emptyLabel.Visible = mine.Count == 0;

        ticketsBody.SuspendLayout();
        ticketsBody.Controls.Clear();

        var width = Math.Max(420, ticketsBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var ticket in mine)
        {
            var row = BuildTicketRow(ticket, width);
            row.SetBounds(0, y, width, 96);
            ticketsBody.Controls.Add(row);
            y += 102;
        }

        ticketsBody.ResumeLayout();
    }

    private Panel BuildTicketRow(DemoSupportTicket ticket, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var title = MakeLabel($"{ticket.Id} · {ticket.Subject}", 10, Navy, true);
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var text = MakeLabel(ticket.Message, 8, Muted);
        text.MaximumSize = new Size(Math.Max(200, width - 300), 34);
        text.Location = new Point(14, 34);
        row.Controls.Add(text);

        var meta = MakeLabel($"{ticket.Category} · opened {ticket.CreatedAt}", 7.5F, Muted);
        meta.Location = new Point(14, 72);
        row.Controls.Add(meta);

        var (back, fore) = ticket.Status switch
        {
            "Open" => (Color.FromArgb(236, 244, 255), Blue),
            "Answered" => (Color.FromArgb(232, 248, 241), Color.FromArgb(20, 153, 111)),
            _ => (Color.FromArgb(240, 243, 247), Muted)
        };
        var status = MakeLabel(ticket.Status, 8, fore, true);
        status.BackColor = back;
        status.Padding = new Padding(8, 3, 8, 3);
        status.Location = new Point(width - 120, 14);
        row.Controls.Add(status);

        if (!string.IsNullOrWhiteSpace(ticket.Reply))
        {
            var reply = MakeLabel($"Reply: {ticket.Reply}", 7.5F, Color.FromArgb(20, 153, 111));
            reply.MaximumSize = new Size(Math.Max(200, width - 300), 34);
            reply.Location = new Point(14, 72);
            row.Controls.Add(reply);
        }

        return row;
    }

    private Button MakeAction(string text, string name)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(160, 38),
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

        RefreshTickets();
    }

    public override void OnNavigatedTo() => RefreshTickets();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
