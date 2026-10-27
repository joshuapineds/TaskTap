namespace TaskTap;

/// <summary>Support Tickets: triage member requests and reply, resolve or close them.</summary>
public sealed class SupportTicketsPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel toolbar = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Panel detailCard = new();
    private readonly Panel detailBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly TextBox replyBox = new();
    private DemoSupportTicket? selectedTicket;
    private int _lastListWidth = -1;
    private int _lastDetailWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.SupportTickets;

    public SupportTicketsPage()
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

        var eyebrow = AdminUi.MakeLabel("SUPPORT", 8, AdminUi.Blue, true);
        eyebrow.Name = "ticketsEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Support Tickets", 26, AdminUi.Navy, true);
        title.Name = "ticketsTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Reply to member requests and close them out.", 9, AdminUi.Muted);
        subtitle.Name = "ticketsSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        toolbar.BackColor = Color.White;
        toolbar.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(toolbar);

        searchBox.Name = "ticketsSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search ticket id, subject or member…";
        searchBox.TextChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(searchBox);

        statusFilter.Name = "ticketsStatusFilter";
        statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilter.Font = new Font("Segoe UI", 9F);
        statusFilter.FlatStyle = FlatStyle.Flat;
        statusFilter.Items.AddRange(["All statuses", "Open", "Answered", "Resolved", "Closed"]);
        statusFilter.SelectedIndex = 0;
        statusFilter.SelectedIndexChanged += (_, _) => RefreshList();
        toolbar.Controls.Add(statusFilter);

        var reset = AdminUi.MakeLinkButton("Reset filters", "ticketsResetButton");
        reset.Click += (_, _) =>
        {
            searchBox.Clear();
            statusFilter.SelectedIndex = 0;
        };
        toolbar.Controls.Add(reset);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var heading = AdminUi.MakeLabel("Tickets", 11, AdminUi.Navy, true);
        heading.Location = new Point(18, 14);
        listCard.Controls.Add(heading);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        detailCard.BackColor = Color.White;
        detailCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(detailCard);

        var detailTitle = AdminUi.MakeLabel("Ticket detail", 11, AdminUi.Navy, true);
        detailTitle.Location = new Point(18, 14);
        detailCard.Controls.Add(detailTitle);

        detailBody.BackColor = Color.White;
        detailCard.Controls.Add(detailBody);

        RefreshList();
        LayoutPage();
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "ticketsEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "ticketsTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "ticketsSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        toolbar.Bounds = new Rectangle(left, 132, contentWidth, 64);
        searchBox.Bounds = new Rectangle(12, 16, 260, 32);
        statusFilter.Bounds = new Rectangle(284, 16, 150, 32);
        var reset = AdminUi.Find<Button>(toolbar, "ticketsResetButton");
        if (reset is not null) reset.Location = new Point(toolbar.Width - reset.Width - 12, 22);

        var rightWidth = Math.Max(340, Math.Min(430, contentWidth * 34 / 100));
        var mainWidth = contentWidth - rightWidth - 18;

        listCard.Bounds = new Rectangle(left, 208, mainWidth, 540);
        listBody.Bounds = new Rectangle(14, 44, listCard.Width - 28, listCard.Height - 56);

        detailCard.Bounds = new Rectangle(left + mainWidth + 18, 208, rightWidth, 540);
        detailBody.Bounds = new Rectangle(14, 44, detailCard.Width - 28, detailCard.Height - 56);

        page.AutoScrollMinSize = new Size(0, 780);

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

        var tickets = DemoDataStore.SupportTickets
            .Where(ticket => status == "All statuses" || ticket.Status == status)
            .Where(ticket => string.IsNullOrEmpty(query) ||
                             $"{ticket.Id} {ticket.Subject} {ticket.Name} {ticket.Category}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (selectedTicket is not null && !tickets.Contains(selectedTicket))
            selectedTicket = null;
        selectedTicket ??= tickets.FirstOrDefault();

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var ticket in tickets)
        {
            var row = BuildRow(ticket, width);
            row.SetBounds(0, y, width, 84);
            listBody.Controls.Add(row);
            y += 90;
        }

        if (tickets.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No tickets match these filters.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
        BuildDetail();
    }

    private Panel BuildRow(DemoSupportTicket ticket, int width)
    {
        var selected = ReferenceEquals(selectedTicket, ticket);
        var row = new Panel
        {
            BackColor = selected ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(ticket);

        var title = AdminUi.MakeLabel($"{ticket.Id} · {ticket.Subject}", 10, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 300), 18);
        title.AutoEllipsis = true;
        title.Location = new Point(14, 12);
        row.Controls.Add(title);

        var member = AdminUi.MakeLabel($"{ticket.Name} · {ticket.Category}", 8, AdminUi.Muted);
        member.Location = new Point(14, 34);
        row.Controls.Add(member);

        var meta = AdminUi.MakeLabel($"Opened {ticket.CreatedAt}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(14, 54);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(ticket.Status, StatusBack(ticket.Status), StatusFore(ticket.Status));
        status.Location = new Point(width - 120, 16);
        row.Controls.Add(status);

        var review = AdminUi.MakeSecondaryButton(selected ? "Reviewing" : "Review", $"ticketReview{ticket.Id}", 94);
        review.Size = new Size(94, 30);
        review.Location = new Point(width - 110, 44);
        review.Click += (_, _) => Select(ticket);
        row.Controls.Add(review);

        return row;
    }

    private void Select(DemoSupportTicket ticket)
    {
        selectedTicket = ticket;
        replyBox.Clear();
        RefreshList();
    }

    private void BuildDetail()
    {
        if (detailBody.Width <= 0)
            return;

        detailBody.SuspendLayout();
        detailBody.Controls.Clear();

        if (selectedTicket is null)
        {
            var empty = AdminUi.MakeLabel("Select a ticket to review it.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            detailBody.Controls.Add(empty);
            detailBody.ResumeLayout();
            return;
        }

        var ticket = selectedTicket;
        var width = detailBody.ClientSize.Width;

        var title = AdminUi.MakeLabel($"{ticket.Id} · {ticket.Subject}", 12, AdminUi.Navy, true);
        title.MaximumSize = new Size(Math.Max(200, width - 8), 0);
        title.Location = new Point(0, 0);
        detailBody.Controls.Add(title);

        var member = AdminUi.MakeWrappedLabel($"{ticket.Name} · {ticket.Email}\n{ticket.Category} · opened {ticket.CreatedAt}", 8, AdminUi.Muted, Math.Max(200, width - 8));
        member.Location = new Point(0, 32);
        detailBody.Controls.Add(member);

        var status = AdminUi.MakePill(ticket.Status, StatusBack(ticket.Status), StatusFore(ticket.Status));
        status.Location = new Point(0, 76);
        detailBody.Controls.Add(status);

        var message = AdminUi.MakeWrappedLabel(ticket.Message, 8.5F, AdminUi.Muted, Math.Max(200, width - 8));
        message.Location = new Point(0, 110);
        detailBody.Controls.Add(message);

        if (!string.IsNullOrWhiteSpace(ticket.Reply))
        {
            var reply = AdminUi.MakeWrappedLabel($"Your reply: {ticket.Reply}", 8, AdminUi.Good, Math.Max(200, width - 8));
            reply.Location = new Point(0, 170);
            detailBody.Controls.Add(reply);
        }

        replyBox.Name = "ticketReplyBox";
        replyBox.BorderStyle = BorderStyle.FixedSingle;
        replyBox.Font = new Font("Segoe UI", 9F);
        replyBox.Multiline = true;
        replyBox.ScrollBars = ScrollBars.Vertical;
        replyBox.PlaceholderText = "Write a reply to the member…";
        replyBox.Location = new Point(0, 200);
        replyBox.Size = new Size(width, 72);
        detailBody.Controls.Add(replyBox);

        var send = AdminUi.MakePrimaryButton("Reply", "ticketReplyButton", 140);
        send.Location = new Point(0, 284);
        send.Click += (_, _) => Reply(ticket);
        detailBody.Controls.Add(send);

        var resolve = AdminUi.MakeSecondaryButton("Resolve", "ticketResolveButton", 120);
        resolve.Location = new Point(152, 284);
        resolve.Click += (_, _) => { DemoDataStore.ResolveTicket(ticket); RefreshList(); };
        detailBody.Controls.Add(resolve);

        var close = AdminUi.MakeSecondaryButton("Close", "ticketCloseButton", 110);
        close.ForeColor = AdminUi.Bad;
        close.Location = new Point(0, 330);
        close.Click += (_, _) => { DemoDataStore.CloseTicket(ticket); RefreshList(); };
        detailBody.Controls.Add(close);

        detailBody.ResumeLayout();
    }

    private void Reply(DemoSupportTicket ticket)
    {
        var text = replyBox.Text.Trim();
        if (text.Length == 0)
        {
            Toast("Write a reply before sending.");
            return;
        }

        DemoDataStore.ReplyToTicket(ticket, text);
        RefreshList();
        replyBox.Clear();
        Toast("Reply sent to the member.");
    }

    private static Color StatusBack(string status) => status switch
    {
        "Open" => Color.FromArgb(236, 244, 255),
        "Answered" => Color.FromArgb(232, 248, 241),
        "Resolved" => Color.FromArgb(232, 248, 241),
        _ => Color.FromArgb(240, 243, 247)
    };

    private static Color StatusFore(string status) => status switch
    {
        "Open" => AdminUi.Blue,
        "Answered" => AdminUi.Good,
        "Resolved" => AdminUi.Good,
        _ => AdminUi.Muted
    };

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
