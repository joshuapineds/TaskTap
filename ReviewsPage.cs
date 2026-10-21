using TaskTap.Data;

namespace TaskTap;

public sealed class ReviewsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Orange = Color.FromArgb(255, 138, 61);
    private static readonly Color Green = Color.FromArgb(19, 160, 113);
    private static readonly Color Muted = Color.FromArgb(113, 128, 150);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(228, 234, 242);
    private readonly Panel page = new();
    private readonly Panel summaryGrid = new();
    private readonly Panel tabsPanel = new();
    private readonly Panel toolbar = new();
    private readonly FlowLayoutPanel reviewList = new();
    private readonly Panel emptyState = new();
    private readonly Panel guidanceCard = new();
    private readonly TextBox search = new();
    private readonly ComboBox ratingFilter = new();
    private readonly ComboBox sortFilter = new();
    private readonly Button receivedTab = new();
    private readonly Button givenTab = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2200 };
    private readonly List<ReviewItem> receivedReviews = [];
    private readonly List<ReviewItem> givenReviews = [];
    private string activeTab = "received";
    private bool pageBuilt;

    public override Route RouteName => Route.Reviews;

    private sealed record ReviewItem(string Name, string Initials, int Rating, DateTime Date, string Task, string Text, string Type);

    public ReviewsPage()
    {
        SeedReviews();
        BuildPage();
    }

    private void SeedReviews()
    {
        if (!AppSession.IsSignedIn)
            return;
        List<Backend.ReviewRow> received = [];
        List<Backend.ReviewRow> given = [];
        Db.Try(() =>
        {
            received = Backend.LoadReviews(AppSession.CurrentUser.Id, true);
            given = Backend.LoadReviews(AppSession.CurrentUser.Id, false);
        });
        receivedReviews.AddRange(received.Select(row => new ReviewItem(row.OtherName, row.OtherInitials, row.Rating, row.Date, row.Task, row.Body, "received")));
        givenReviews.AddRange(given.Select(row => new ReviewItem(row.OtherName, row.OtherInitials, row.Rating, row.Date, row.Task, row.Body, "given")));
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
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
        pageBuilt = true;
        RenderReviews();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Profile   /   Reviews", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Navigate(Route.Profile);
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Reputation", 8, Blue, true, new Point(22, 41)));
        page.Controls.Add(MakeLabel("Reviews", 23, Navy, true, new Point(22, 57)));
        page.Controls.Add(MakeLabel("See what community members say about working with you and the feedback you've shared.", 9, Muted, false, new Point(22, 104), new Size(760, 24)));
        var completedTasks = MakeButton("View completed tasks", true, 170);
        completedTasks.Name = "ReviewsCompletedTasksButton";
        completedTasks.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(completedTasks);
        BuildSummary();
        BuildTabs();
        BuildToolbar();
        reviewList.FlowDirection = FlowDirection.TopDown;
        reviewList.WrapContents = false;
        reviewList.AutoScroll = false;
        reviewList.BackColor = PageColor;
        page.Controls.Add(reviewList);
        BuildEmptyState();
        BuildGuidance();
    }

    private void BuildSummary()
    {
        var average = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        average.Controls.Add(MakeLabel("4.9", 30, Navy, true, new Point(18, 37)));
        average.Controls.Add(MakeLabel("★★★★★", 15, Orange, true, new Point(116, 28)));
        average.Controls.Add(MakeLabel("Average rating", 9, Navy, true, new Point(116, 56)));
        average.Controls.Add(MakeLabel("16 reviews received", 8, Muted, false, new Point(116, 78)));
        summaryGrid.Controls.Add(average);

        var breakdown = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        breakdown.Controls.Add(MakeLabel("Rating breakdown", 9, Navy, true, new Point(14, 12)));
        breakdown.Controls.Add(MakeLabel("16 total", 8, Muted, false, new Point(205, 12)));
        var values = new[] { (5, 14, 88), (4, 2, 13), (3, 0, 0), (2, 0, 0), (1, 0, 0) };
        for (var i = 0; i < values.Length; i++)
        {
            var y = 41 + i * 20;
            breakdown.Controls.Add(MakeLabel(values[i].Item1.ToString(), 7, Muted, false, new Point(14, y)));
            var track = new Panel { BackColor = Color.FromArgb(237, 242, 247), Location = new Point(35, y + 5), Size = new Size(150, 7) };
            track.Controls.Add(new Panel { BackColor = Color.FromArgb(255, 176, 102), Dock = DockStyle.Left, Width = values[i].Item3 * 150 / 100 });
            breakdown.Controls.Add(track);
            breakdown.Controls.Add(MakeLabel(values[i].Item2.ToString(), 7, Muted, false, new Point(193, y)));
        }
        summaryGrid.Controls.Add(breakdown);
        summaryGrid.Controls.Add(BuildStatCard("✓", "16", "Reviews received", "All from completed tasks", Green));
        summaryGrid.Controls.Add(BuildStatCard("↗", "12", "Reviews given", "Helpful feedback shared", Blue));
        page.Controls.Add(summaryGrid);
    }

    private Panel BuildStatCard(string iconText, string count, string label, string sub, Color color)
    {
        var card = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var icon = MakeLabel(iconText, 11, color, true, new Point(16, 12));
        icon.AutoSize = false;
        icon.Size = new Size(32, 32);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(235, 245, 255);
        card.Controls.Add(icon);
        card.Controls.Add(MakeLabel(count, 18, Navy, true, new Point(16, 49)));
        card.Controls.Add(MakeLabel(label, 8, Navy, true, new Point(16, 78)));
        card.Controls.Add(MakeLabel(sub, 7, Muted, false, new Point(16, 100)));
        return card;
    }

    private void BuildTabs()
    {
        tabsPanel.BackColor = Color.White;
        tabsPanel.BorderStyle = BorderStyle.FixedSingle;
        receivedTab.Text = "Received   16";
        givenTab.Text = "Given   12";
        foreach (var button in new[] { receivedTab, givenTab })
        {
            button.Size = new Size(150, 40);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            tabsPanel.Controls.Add(button);
        }
        receivedTab.Click += (_, _) => { activeTab = "received"; UpdateTabs(); RenderReviews(); };
        givenTab.Click += (_, _) => { activeTab = "given"; ratingFilter.SelectedIndex = 0; UpdateTabs(); RenderReviews(); };
        UpdateTabs();
        page.Controls.Add(tabsPanel);
    }

    private void BuildToolbar()
    {
        toolbar.BackColor = Color.Transparent;
        search.PlaceholderText = "Search reviews or task names...";
        search.TextChanged += (_, _) => RenderReviews();
        toolbar.Controls.Add(search);
        ConfigureCombo(ratingFilter, ["All ratings", "5 stars", "4 stars", "3 stars"]);
        ConfigureCombo(sortFilter, ["Newest first", "Oldest first", "Highest rating", "Lowest rating"]);
        ratingFilter.SelectedIndexChanged += (_, _) => RenderReviews();
        sortFilter.SelectedIndexChanged += (_, _) => RenderReviews();
        toolbar.Controls.Add(ratingFilter);
        toolbar.Controls.Add(sortFilter);
        page.Controls.Add(toolbar);
    }

    private void BuildEmptyState()
    {
        emptyState.BackColor = Color.White;
        emptyState.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("★", 18, Orange, true, Point.Empty);
        icon.AutoSize = false;
        icon.Size = new Size(48, 48);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        emptyState.Controls.Add(icon);
        emptyState.Controls.Add(MakeLabel("No reviews found", 12, Navy, true, Point.Empty));
        emptyState.Controls.Add(MakeLabel("Try another search or filter. Reviews will appear here after completed tasks are reviewed.", 8, Muted, false, Point.Empty, new Size(480, 40)));
        var clear = MakeButton("Clear filters", false, 110);
        clear.Click += (_, _) => ClearFilters();
        emptyState.Controls.Add(clear);
        page.Controls.Add(emptyState);
    }

    private void BuildGuidance()
    {
        guidanceCard.BackColor = Color.White;
        guidanceCard.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("✓", 11, Green, true, new Point(16, 16));
        icon.AutoSize = false;
        icon.Size = new Size(38, 38);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(234, 247, 242);
        guidanceCard.Controls.Add(icon);
        guidanceCard.Controls.Add(MakeLabel("Keep reviews useful", 10, Navy, true, new Point(68, 13)));
        guidanceCard.Controls.Add(MakeLabel("Focus feedback on task experience, communication, punctuality, and work quality. Never include passwords, OTPs, private addresses, or other sensitive information.", 8, Muted, false, new Point(68, 39), new Size(840, 46)));
        var community = MakeLink("Community guidelines →");
        community.Location = new Point(0, 18);
        community.Click += (_, _) => Nav.Navigate(Route.CommunityGuidelines);
        guidanceCard.Controls.Add(community);
        page.Controls.Add(guidanceCard);
    }

    private void RenderReviews()
    {
        if (reviewList.IsDisposed) return;
        reviewList.SuspendLayout();
        reviewList.Controls.Clear();
        var source = activeTab == "received" ? receivedReviews : givenReviews;
        var query = search.Text.Trim();
        var selectedRating = ratingFilter.SelectedIndex == 0 ? 0 : 6 - ratingFilter.SelectedIndex;
        var results = source.Where(review =>
                (selectedRating == 0 || review.Rating == selectedRating) &&
                (query.Length == 0 || $"{review.Name} {review.Task} {review.Text}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        results = sortFilter.SelectedIndex switch
        {
            1 => results.OrderBy(review => review.Date).ToList(),
            2 => results.OrderByDescending(review => review.Rating).ThenByDescending(review => review.Date).ToList(),
            3 => results.OrderBy(review => review.Rating).ThenByDescending(review => review.Date).ToList(),
            _ => results.OrderByDescending(review => review.Date).ToList()
        };
        foreach (var review in results) reviewList.Controls.Add(BuildReviewCard(review));
        reviewList.Height = results.Count == 0 ? 0 : results.Count * 180 + (results.Count - 1) * 12;
        reviewList.Visible = results.Count > 0;
        emptyState.Visible = results.Count == 0;
        reviewList.ResumeLayout();
        if (pageBuilt) LayoutPage();
    }

    private Panel BuildReviewCard(ReviewItem review)
    {
        var card = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Height = 200, Margin = new Padding(0, 0, 0, 12) };
        var avatar = MakeLabel(review.Initials, 8, Blue, true, new Point(18, 16));
        avatar.AutoSize = false;
        avatar.Size = new Size(42, 42);
        avatar.TextAlign = ContentAlignment.MiddleCenter;
        avatar.BackColor = Color.FromArgb(234, 243, 255);
        card.Controls.Add(avatar);
        var reviewer = MakeLabel(review.Name, 9, Navy, true, new Point(72, 16));
        reviewer.Cursor = Cursors.Hand;
        reviewer.Click += (_, _) => Nav.Navigate(ProfileLinks.For(review.Name));
        card.Controls.Add(reviewer);
        card.Controls.Add(MakeLabel(review.Type == "received" ? "Reviewed you" : "You reviewed this member", 7, Muted, false, new Point(72, 38)));
        var stars = MakeLabel(new string('★', review.Rating) + new string('☆', 5 - review.Rating), 11, Orange, true, Point.Empty);
        stars.TextAlign = ContentAlignment.MiddleRight;
        card.Controls.Add(stars);
        var date = MakeLabel(review.Date.ToString("MMM d, yyyy"), 7, Muted, false, Point.Empty);
        date.TextAlign = ContentAlignment.MiddleRight;
        card.Controls.Add(date);
        card.Controls.Add(MakeLabel(review.Text, 8, Color.FromArgb(76, 93, 114), false, new Point(18, 72), new Size(800, 58)));
        var rule = new Panel { BackColor = Color.FromArgb(237, 241, 245), Location = new Point(18, 138), Height = 1 };
        card.Controls.Add(rule);
        card.Controls.Add(MakeLabel($"Task · {review.Task}", 8, Muted, false, new Point(18, 148)));
        var link = MakeLink(review.Type == "received" ? "View profile →" : "View task →");
        link.Name = "reviewAction";
        link.Click += (_, _) =>
        {
            if (review.Type == "received") Nav.Navigate(new PublicProfilePage());
            else Nav.Navigate(Route.FindTasks);
        };
        card.Controls.Add(link);
        if (review.Type == "received" && review.Rating == 5)
            card.Controls.Add(MakeLabel("Great feedback: Your review helps other members understand what working with you is like.", 7, Muted, false, new Point(18, 168), new Size(800, 16)));
        return card;
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        const int margin = 22;
        var contentWidth = width - margin * 2;
        var action = page.Controls.Find("ReviewsCompletedTasksButton", false).FirstOrDefault();
        if (action is not null) action.Location = new Point(width - margin - action.Width, 58);
        var gap = 14;
        var averageWidth = (contentWidth - gap * 3) * 25 / 100;
        var breakdownWidth = (contentWidth - gap * 3) * 33 / 100;
        var statWidth = (contentWidth - gap * 3 - averageWidth - breakdownWidth) / 2;
        summaryGrid.Bounds = new Rectangle(margin, 132, contentWidth, 154);
        var widths = new[] { averageWidth, breakdownWidth, statWidth, statWidth };
        var sx = 0;
        for (var i = 0; i < summaryGrid.Controls.Count; i++)
        {
            summaryGrid.Controls[i].Bounds = new Rectangle(sx, 0, widths[i], 154);
            sx += widths[i] + gap;
        }
        tabsPanel.Bounds = new Rectangle(margin, 302, contentWidth, 54);
        receivedTab.Location = new Point(8, 7);
        givenTab.Location = new Point(162, 7);
        toolbar.Bounds = new Rectangle(margin, 368, contentWidth, 44);
        search.SetBounds(0, 0, Math.Max(250, contentWidth - 500), 42);
        ratingFilter.SetBounds(search.Right + 10, 0, 170, 42);
        sortFilter.SetBounds(ratingFilter.Right + 10, 0, 240, 42);
        var y = 426;
        reviewList.SetBounds(margin, y, contentWidth, reviewList.Height);
        foreach (var card in reviewList.Controls.OfType<Panel>())
        {
            card.Width = contentWidth;
            card.Controls.Find("reviewAction", false).FirstOrDefault()?.SetBounds(contentWidth - 150, 146, 130, 28);
            var stars = card.Controls.OfType<Label>().FirstOrDefault(label => label.Text.Contains('★'));
            if (stars is not null) stars.SetBounds(contentWidth - 180, 15, 150, 24);
            var date = card.Controls.OfType<Label>().Where(label => label.Location == Point.Empty).LastOrDefault();
            if (date is not null) date.SetBounds(contentWidth - 180, 41, 150, 20);
            var text = card.Controls.OfType<Label>().FirstOrDefault(label => label.Location == new Point(18, 72));
            if (text is not null) text.Size = new Size(contentWidth - 36, 58);
            var rule = card.Controls.OfType<Panel>().FirstOrDefault();
            if (rule is not null) rule.Width = contentWidth - 36;
        }
        emptyState.SetBounds(margin, y, contentWidth, 235);
        var emptyIcon = emptyState.Controls.OfType<Label>().First();
        emptyIcon.Location = new Point((contentWidth - 48) / 2, 24);
        var emptyTitle = emptyState.Controls.OfType<Label>().Skip(1).First();
        emptyTitle.Location = new Point((contentWidth - emptyTitle.Width) / 2, 82);
        var emptyText = emptyState.Controls.OfType<Label>().Skip(2).First();
        emptyText.Location = new Point((contentWidth - emptyText.Width) / 2, 111);
        var clear = emptyState.Controls.OfType<Button>().First();
        clear.Location = new Point((contentWidth - clear.Width) / 2, 162);
        var listBottom = reviewList.Visible ? reviewList.Bottom : emptyState.Bottom;
        guidanceCard.Bounds = new Rectangle(margin, listBottom + 18, contentWidth, 104);
        guidanceCard.Controls.OfType<Label>().Where(label => label.Location.Y == 39).First().Size = new Size(Math.Max(250, contentWidth - 280), 48);
        var community = guidanceCard.Controls.OfType<Button>().First();
        community.Location = new Point(contentWidth - community.Width - 15, 18);
        page.AutoScrollMinSize = new Size(0, guidanceCard.Bottom + 24);
        toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
    }

    private void ConfigureCombo(ComboBox combo, string[] values)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 8F);
        combo.Items.AddRange(values);
        combo.SelectedIndex = 0;
    }

    private void UpdateTabs()
    {
        var receivedActive = activeTab == "received";
        receivedTab.BackColor = receivedActive ? Color.FromArgb(237, 245, 255) : Color.White;
        receivedTab.ForeColor = receivedActive ? Blue : Muted;
        givenTab.BackColor = receivedActive ? Color.White : Color.FromArgb(237, 245, 255);
        givenTab.ForeColor = receivedActive ? Muted : Blue;
    }

    private void ClearFilters()
    {
        search.Clear();
        ratingFilter.SelectedIndex = 0;
        sortFilter.SelectedIndex = 0;
        ShowToast("Filters cleared.");
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
        var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, TabStop = false };
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(5, 2, 5, 2);
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
