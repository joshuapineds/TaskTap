using TaskTap.Data;

namespace TaskTap;

public sealed class RateReviewPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(101, 116, 139);
    private static readonly Color PageColor = Color.FromArgb(245, 248, 252);
    private static readonly Color Line = Color.FromArgb(228, 234, 242);
    private static readonly string[] RatingNames = ["", "Poor", "Fair", "Good", "Very good", "Excellent"];
    private static readonly string[] RatingHints = ["", "The experience needs significant improvement.", "There were several issues to work through.", "A solid experience with room for improvement.", "A positive experience overall.", "A reliable, respectful experience worth recommending."];
    private readonly PaymentRecordDetails payment;
    private readonly Panel page = new();
    private readonly Panel contextCard = new();
    private readonly Panel reviewCard = new();
    private readonly Panel personCard = new();
    private readonly Panel guidanceCard = new();
    private readonly Panel fairnessCard = new();
    private readonly Button[] stars = new Button[5];
    private readonly CheckBox[] highlights = new CheckBox[4];
    private readonly TextBox titleInput = new() { MaxLength = 70 };
    private readonly TextBox reviewInput = new() { MaxLength = 500, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox privacyConfirm = new();
    private readonly Label ratingName = new();
    private readonly Label ratingHint = new();
    private readonly Label titleCount = new();
    private readonly Label reviewCount = new();
    private readonly Label ratingError = new();
    private readonly Label titleError = new();
    private readonly Label reviewError = new();
    private readonly Label confirmError = new();
    private int selectedRating;
    private bool pageBuilt;

    public override Route RouteName => Route.RateReview;

    public RateReviewPage(PaymentRecordDetails payment)
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
        pageBuilt = true;
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Payment Record   /   Rate & Review", 8, Muted, true, new Point(22, 12));
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Back();
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("Close the task loop", 8, Blue, true, new Point(22, 40)));
        page.Controls.Add(MakeLabel("Rate & Review", 23, Navy, true, new Point(22, 56)));
        page.Controls.Add(MakeLabel("Share a fair, useful review after your TaskTap task is completed.", 9, Muted, false, new Point(22, 104)));
        BuildTaskContext();
        BuildReviewForm();
        BuildPersonCard();
        BuildGuidanceCard();
        BuildFairnessCard();
    }

    private void BuildTaskContext()
    {
        contextCard.BackColor = Color.White;
        contextCard.BorderStyle = BorderStyle.FixedSingle;
        var thumb = MakeLabel("🔧", 21, Blue, true, new Point(16, 17));
        thumb.AutoSize = false;
        thumb.Size = new Size(66, 66);
        thumb.TextAlign = ContentAlignment.MiddleCenter;
        thumb.BackColor = Color.FromArgb(234, 243, 255);
        contextCard.Controls.Add(thumb);
        var completed = MakeLabel("Completed", 8, Color.FromArgb(19, 132, 87), true, new Point(94, 13));
        contextCard.Controls.Add(completed);
        contextCard.Controls.Add(MakeLabel(payment.Task, 13, Navy, true, new Point(94, 32)));
        contextCard.Controls.Add(MakeLabel($"With {payment.Person} · {payment.Category} · Brgy. {payment.Barangay}", 8, Muted, false, new Point(94, 58)));
        contextCard.Controls.Add(MakeLabel($"₱{payment.Amount:N0}     Completed {payment.Date:MMM d, yyyy}", 8, Navy, true, new Point(94, 82)));
        var viewTask = MakeButton("View Task", false, 100);
        viewTask.Name = "RateReviewViewTaskButton";
        viewTask.Click += (_, _) => Nav.Navigate(Route.FindTasks);
        contextCard.Controls.Add(viewTask);
        page.Controls.Add(contextCard);
    }

    private void BuildReviewForm()
    {
        reviewCard.BackColor = Color.White;
        reviewCard.BorderStyle = BorderStyle.FixedSingle;
        reviewCard.AutoScroll = true;
        reviewCard.Controls.Add(MakeLabel("How was your experience?", 14, Navy, true, new Point(22, 18)));
        reviewCard.Controls.Add(MakeLabel("Reviews help the community choose reliable people and set clear expectations.", 8, Muted, false, new Point(22, 44), new Size(620, 25)));
        reviewCard.Controls.Add(MakeLabel("Required", 8, Muted, true, new Point(0, 22)));
        reviewCard.Controls.Add(MakeLabel("Your rating *", 9, Navy, true, new Point(22, 82)));
        for (var i = 0; i < stars.Length; i++)
        {
            var rating = i + 1;
            var star = new Button { Text = "★", Tag = rating, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(199, 210, 224), Font = new Font("Segoe UI Symbol", 26F), Size = new Size(48, 48), Location = new Point(22 + i * 48, 106), Cursor = Cursors.Hand, TabStop = false };
            star.FlatAppearance.BorderSize = 0;
            star.MouseEnter += (_, _) => PaintStars(rating);
            star.MouseLeave += (_, _) => PaintStars(selectedRating);
            star.Click += (_, _) => SetRating(rating);
            stars[i] = star;
            reviewCard.Controls.Add(star);
        }
        ratingName.Location = new Point(22, 158);
        ratingName.AutoSize = true;
        ratingName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        ratingName.ForeColor = Navy;
        ratingName.Text = "Select a rating";
        reviewCard.Controls.Add(ratingName);
        ratingHint.Location = new Point(128, 159);
        ratingHint.Size = new Size(580, 34);
        ratingHint.ForeColor = Muted;
        ratingHint.Font = new Font("Segoe UI", 8F);
        ratingHint.Text = "Tap the stars to rate this experience.";
        reviewCard.Controls.Add(ratingHint);
        ratingError.Location = new Point(22, 188);
        ratingError.AutoSize = true;
        ratingError.ForeColor = Color.FromArgb(198, 79, 66);
        reviewCard.Controls.Add(ratingError);

        AddFieldLabel("Review title *", 214);
        titleInput.PlaceholderText = "e.g. Reliable and easy to work with";
        titleInput.SetBounds(22, 236, 650, 36);
        titleInput.TextChanged += (_, _) => titleCount.Text = $"{titleInput.TextLength} / 70";
        reviewCard.Controls.Add(titleInput);
        titleCount.Text = "0 / 70";
        titleCount.TextAlign = ContentAlignment.MiddleRight;
        titleCount.SetBounds(582, 272, 90, 20);
        titleCount.ForeColor = Muted;
        reviewCard.Controls.Add(titleCount);
        titleError.SetBounds(22, 292, 650, 22);
        titleError.ForeColor = Color.FromArgb(198, 79, 66);
        reviewCard.Controls.Add(titleError);

        AddFieldLabel("Your review *", 318);
        reviewInput.PlaceholderText = "What went well? What should other TaskTap members know?";
        reviewInput.SetBounds(22, 340, 650, 140);
        reviewInput.TextChanged += (_, _) => reviewCount.Text = $"{reviewInput.TextLength} / 500";
        reviewCard.Controls.Add(reviewInput);
        reviewCount.Text = "0 / 500";
        reviewCount.TextAlign = ContentAlignment.MiddleRight;
        reviewCount.SetBounds(582, 480, 90, 20);
        reviewCount.ForeColor = Muted;
        reviewCard.Controls.Add(reviewCount);
        reviewError.SetBounds(22, 500, 650, 22);
        reviewError.ForeColor = Color.FromArgb(198, 79, 66);
        reviewCard.Controls.Add(reviewError);

        reviewCard.Controls.Add(MakeLabel("Helpful prompts:", 8, Muted, true, new Point(22, 528)));
        var tags = new[] { "Communication", "On time", "Quality of work", "Friendly", "Clear expectations" };
        var x = 128;
        foreach (var tag in tags)
        {
            var button = MakeLink(tag);
            button.Name = "tag" + tag.Replace(" ", "");
            button.Location = new Point(x, 523);
            button.Size = new Size(TextRenderer.MeasureText(tag, button.Font).Width + 18, 30);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Line;
            button.BackColor = Color.White;
            button.Click += (_, _) => AppendTag(tag);
            reviewCard.Controls.Add(button);
            x += button.Width + 6;
        }

        reviewCard.Controls.Add(MakeLabel("What went well? (optional)", 9, Navy, true, new Point(22, 570)));
        var highlightNames = new[] { "Clear communication", "Arrived on time", "Good quality", "Respectful" };
        for (var i = 0; i < highlights.Length; i++)
        {
            highlights[i] = new CheckBox { Text = highlightNames[i], AutoSize = false, Size = new Size(290, 32), Location = new Point(22 + (i % 2) * 310, 596 + (i / 2) * 38), Font = new Font("Segoe UI", 8F), ForeColor = Navy };
            reviewCard.Controls.Add(highlights[i]);
        }
        privacyConfirm.SetBounds(22, 678, 24, 24);
        reviewCard.Controls.Add(privacyConfirm);
        reviewCard.Controls.Add(MakeLabel("I confirm this review reflects my honest experience and includes no private or sensitive information.", 8, Navy, false, new Point(50, 678), new Size(620, 34)));
        confirmError.SetBounds(22, 711, 650, 22);
        confirmError.ForeColor = Color.FromArgb(198, 79, 66);
        reviewCard.Controls.Add(confirmError);
        var cancel = MakeButton("Cancel", false, 100);
        cancel.SetBounds(440, 742, 100, 38);
        cancel.Click += (_, _) => Nav.Back();
        reviewCard.Controls.Add(cancel);
        var publish = MakeButton("Publish Review", true, 140);
        publish.SetBounds(550, 742, 140, 38);
        publish.Click += (_, _) => PublishReview();
        reviewCard.Controls.Add(publish);
        page.Controls.Add(reviewCard);
    }

    private void BuildPersonCard()
    {
        personCard.BackColor = Color.White;
        personCard.BorderStyle = BorderStyle.FixedSingle;
        var initials = string.Join("", payment.Person.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));
        var avatar = MakeLabel(initials, 12, Color.FromArgb(7, 93, 189), true, new Point(18, 18));
        avatar.AutoSize = false;
        avatar.Size = new Size(56, 56);
        avatar.TextAlign = ContentAlignment.MiddleCenter;
        avatar.BackColor = Color.FromArgb(219, 238, 255);
        personCard.Controls.Add(avatar);
        personCard.Controls.Add(MakeLabel("YOU ARE REVIEWING", 7, Blue, true, new Point(86, 18)));
        personCard.Controls.Add(MakeLabel(payment.Person, 12, Navy, true, new Point(86, 37)));
        personCard.Controls.Add(MakeLabel($"Task poster · Brgy. {payment.Barangay}", 8, Muted, false, new Point(86, 59)));
        var reviewed = ProfileLinks.Find(payment.Person);
        var stats = new[] { ($"{reviewed?.Rating ?? 0:0.0}", "rating"), ((reviewed?.Reviews ?? 0).ToString(), "reviews"), ((reviewed?.PostedTasks ?? 0).ToString(), "tasks posted") };
        for (var i = 0; i < stats.Length; i++)
        {
            var x = 18 + i * 96;
            personCard.Controls.Add(MakeLabel(stats[i].Item1, 12, Navy, true, new Point(x + 15, 96)));
            personCard.Controls.Add(MakeLabel(stats[i].Item2, 7, Muted, false, new Point(x + 3, 118)));
        }
        var profile = MakeButton("View public profile", false, 260);
        profile.SetBounds(18, 151, 260, 34);
        profile.Click += (_, _) => Nav.Navigate(ProfileLinks.For(payment.Person));
        personCard.Controls.Add(profile);
        page.Controls.Add(personCard);
    }

    private void BuildGuidanceCard()
    {
        guidanceCard.BackColor = Color.White;
        guidanceCard.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("✓", 11, Color.FromArgb(22, 133, 92), true, new Point(18, 16));
        icon.AutoSize = false;
        icon.Size = new Size(32, 32);
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.BackColor = Color.FromArgb(231, 247, 239);
        guidanceCard.Controls.Add(icon);
        guidanceCard.Controls.Add(MakeLabel("Keep reviews useful", 11, Navy, true, new Point(18, 58)));
        var advice = new[] { "Be specific about the task experience.", "Focus on behavior and completed work.", "Do not include phone numbers, passwords, OTPs, or financial credentials.", "Keep feedback respectful, even when the experience was difficult." };
        for (var i = 0; i < advice.Length; i++)
            guidanceCard.Controls.Add(MakeLabel("• " + advice[i], 8, Muted, false, new Point(18, 88 + i * 43), new Size(270, 40)));
        page.Controls.Add(guidanceCard);
    }

    private void BuildFairnessCard()
    {
        fairnessCard.BackColor = Navy;
        fairnessCard.Controls.Add(MakeLabel("COMMUNITY STANDARD", 7, Color.FromArgb(169, 199, 237), true, new Point(18, 17)));
        fairnessCard.Controls.Add(MakeLabel("Fair reviews build trust.", 11, Color.White, true, new Point(18, 39)));
        fairnessCard.Controls.Add(MakeLabel("Reviewing the same task again replaces your earlier review.", 8, Color.FromArgb(200, 215, 234), false, new Point(18, 66), new Size(265, 62)));
        page.Controls.Add(fairnessCard);
    }

    private void AddFieldLabel(string text, int y) => reviewCard.Controls.Add(MakeLabel(text, 8, Navy, true, new Point(22, y)));

    private void SetRating(int rating)
    {
        selectedRating = rating;
        ratingName.Text = RatingNames[rating];
        ratingHint.Text = RatingHints[rating];
        ratingHint.ForeColor = Muted;
        ratingError.Text = "";
        PaintStars(rating);
    }

    private void PaintStars(int rating)
    {
        foreach (var star in stars)
            if (star is not null)
                star.ForeColor = (int)star.Tag! <= rating ? Color.FromArgb(245, 174, 49) : Color.FromArgb(199, 210, 224);
    }

    private void AppendTag(string tag)
    {
        var current = reviewInput.Text.Trim();
        var addition = current.Length == 0 ? $"{tag}." : $"{(current.EndsWith('.') ? "" : " · ")}{tag}.";
        if (reviewInput.TextLength + addition.Length > reviewInput.MaxLength) return;
        reviewInput.AppendText(addition);
        reviewInput.Focus();
        reviewInput.SelectionStart = reviewInput.TextLength;
    }

    private void PublishReview()
    {
        titleError.Text = string.IsNullOrWhiteSpace(titleInput.Text) ? "Add a short review title." : "";
        reviewError.Text = string.IsNullOrWhiteSpace(reviewInput.Text) || reviewInput.Text.Trim().Length < 10 ? "Write at least 10 characters so the review is useful." : "";
        confirmError.Text = privacyConfirm.Checked ? "" : "Confirm your review is honest and contains no sensitive information.";
        if (selectedRating == 0)
        {
            ratingName.Text = "Select a rating";
            ratingHint.Text = "Please choose a rating.";
            ratingHint.ForeColor = Color.FromArgb(198, 79, 66);
            ratingError.Text = "A rating is required.";
        }
        var valid = selectedRating > 0 && !string.IsNullOrWhiteSpace(titleInput.Text) && reviewInput.Text.Trim().Length >= 10 && privacyConfirm.Checked;
        if (!valid) return;
        var starsText = new string('★', selectedRating) + new string('☆', 5 - selectedRating);
        var saved = Backend.SaveReview(AppSession.CurrentUser.Id, payment.Person, payment.Task, selectedRating, titleInput.Text, reviewInput.Text);
        if (!saved.Success)
        {
            reviewError.Text = saved.Error;
            return;
        }

        MessageBox.Show(this, $"{starsText}\r\n{titleInput.Text.Trim()}\r\n\r\n{reviewInput.Text.Trim()}\r\n\r\nYour review for {payment.Person} was published.", "Review published", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        const int margin = 22;
        var contentWidth = width - margin * 2;
        var sideWidth = Math.Min(330, Math.Max(290, contentWidth / 3));
        var mainWidth = contentWidth - sideWidth - 22;
        contextCard.Bounds = new Rectangle(margin, 132, mainWidth, 104);
        contextCard.Controls.Find("RateReviewViewTaskButton", false).FirstOrDefault()?.SetBounds(mainWidth - 116, 34, 100, 36);
        reviewCard.Bounds = new Rectangle(margin, 250, mainWidth, 810);
        foreach (Control control in reviewCard.Controls)
        {
            if (control is TextBox textBox && textBox != reviewInput) textBox.Width = mainWidth - 44;
            if (control == reviewInput) reviewInput.Width = mainWidth - 44;
            if (control.Name.StartsWith("tag", StringComparison.Ordinal)) { }
        }
        var titleCountLabel = titleCount;
        titleCountLabel.Location = new Point(mainWidth - 110, 272);
        reviewCount.Location = new Point(mainWidth - 110, 480);
        var nameIndex = reviewCard.Controls.IndexOf(ratingName);
        if (nameIndex >= 0) ratingHint.Width = mainWidth - ratingHint.Left - 22;
        var tagButtons = reviewCard.Controls.OfType<Button>().Where(button => button.Name.StartsWith("tag", StringComparison.Ordinal)).ToArray();
        var tx = 128;
        var ty = 523;
        foreach (var button in tagButtons)
        {
            if (tx + button.Width > mainWidth - 20) { tx = 22; ty += 34; }
            button.Location = new Point(tx, ty);
            tx += button.Width + 6;
        }
        personCard.Bounds = new Rectangle(margin + mainWidth + 22, 132, sideWidth, 204);
        var profile = personCard.Controls.OfType<Button>().First();
        profile.Width = sideWidth - 36;
        guidanceCard.Bounds = new Rectangle(margin + mainWidth + 22, 352, sideWidth, 282);
        foreach (var label in guidanceCard.Controls.OfType<Label>().Where(label => label.Location.Y >= 88)) label.Size = new Size(sideWidth - 36, 40);
        fairnessCard.Bounds = new Rectangle(margin + mainWidth + 22, 650, sideWidth, 150);
        fairnessCard.Controls.OfType<Label>().Last().Size = new Size(sideWidth - 36, 62);
        page.AutoScrollMinSize = new Size(0, Math.Max(reviewCard.Bottom, fairnessCard.Bottom) + 24);
    }

    private void ShowToast(string message) => MessageBox.Show(this, message, "TaskTap", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
}
