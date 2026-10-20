namespace TaskTap;

/// <summary>
/// Verification Queue: pending identity requests with a review panel, approve and
/// reject-with-reason actions over the shared user list.
/// </summary>
public sealed class VerificationQueuePage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel queueCard = new();
    private readonly Panel queueBody = new();
    private readonly Panel previewCard = new();
    private readonly Panel previewBody = new();
    private readonly TextBox searchBox = new();
    private readonly ComboBox barangayFilter = new();
    private DemoUser? selectedUser;
    private int _lastQueueWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.VerificationQueue;

    public VerificationQueuePage()
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

        var eyebrow = AdminUi.MakeLabel("TRUST & SAFETY", 8, AdminUi.Blue, true);
        eyebrow.Name = "verificationEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Verification Queue", 26, AdminUi.Navy, true);
        title.Name = "verificationTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Review identity checks before members unlock higher-trust features.", 9, AdminUi.Muted);
        subtitle.Name = "verificationSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        queueCard.BackColor = Color.White;
        queueCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(queueCard);

        var queueTitle = AdminUi.MakeLabel("Pending requests", 11, AdminUi.Navy, true);
        queueTitle.Location = new Point(18, 14);
        queueCard.Controls.Add(queueTitle);

        searchBox.Name = "verificationSearchBox";
        searchBox.Font = new Font("Segoe UI", 9F);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search name, email or barangay…";
        searchBox.TextChanged += (_, _) => RefreshQueue();
        queueCard.Controls.Add(searchBox);

        barangayFilter.Name = "verificationBarangayFilter";
        barangayFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        barangayFilter.Font = new Font("Segoe UI", 9F);
        barangayFilter.FlatStyle = FlatStyle.Flat;
        barangayFilter.Items.Add("All barangays");
        foreach (var barangay in DemoDataStore.Barangays.OrderBy(b => b.Id))
            barangayFilter.Items.Add(barangay.Name);
        barangayFilter.SelectedIndex = 0;
        barangayFilter.SelectedIndexChanged += (_, _) => RefreshQueue();
        queueCard.Controls.Add(barangayFilter);

        queueBody.BackColor = Color.White;
        queueCard.Controls.Add(queueBody);

        previewCard.BackColor = Color.White;
        previewCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(previewCard);

        var previewTitle = AdminUi.MakeLabel("Review request", 11, AdminUi.Navy, true);
        previewTitle.Location = new Point(18, 14);
        previewCard.Controls.Add(previewTitle);

        previewBody.BackColor = Color.White;
        previewCard.Controls.Add(previewBody);

        RefreshQueue();
        LayoutPage();
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "verificationEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "verificationTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "verificationSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        var leftWidth = (int)(contentWidth * 0.55);
        var rightWidth = contentWidth - leftWidth - 18;

        queueCard.Bounds = new Rectangle(left, 130, leftWidth, 560);
        previewCard.Bounds = new Rectangle(left + leftWidth + 18, 130, rightWidth, 560);

        searchBox.Bounds = new Rectangle(18, 42, Math.Max(180, queueCard.Width - 36 - 180), 32);
        barangayFilter.Bounds = new Rectangle(queueCard.Width - 192, 42, 174, 32);
        queueBody.Bounds = new Rectangle(14, 86, queueCard.Width - 28, queueCard.Height - 98);
        previewBody.Bounds = new Rectangle(14, 42, previewCard.Width - 28, previewCard.Height - 54);

        page.AutoScrollMinSize = new Size(0, 730);

        if (queueBody.Width != _lastQueueWidth)
        {
            _lastQueueWidth = queueBody.Width;
            RefreshQueue();
        }
    }

    private void RefreshQueue()
    {
        if (queueBody.Width <= 0)
            return;

        var query = searchBox.Text.Trim();
        var barangay = barangayFilter.SelectedItem?.ToString() ?? "All barangays";
        var pending = DemoDataStore.Users
            .Where(user => user.IsPendingVerification)
            .Where(user => barangay == "All barangays" || $"Brgy. {user.Barangay}" == barangay)
            .Where(user => string.IsNullOrEmpty(query) ||
                           $"{user.FullName} {user.Email} {user.Barangay}".Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.FullName)
            .ToList();

        if (selectedUser is not null && !pending.Contains(selectedUser))
            selectedUser = null;
        selectedUser ??= pending.FirstOrDefault();

        queueBody.SuspendLayout();
        queueBody.Controls.Clear();

        var width = Math.Max(300, queueBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var user in pending)
        {
            var row = BuildRow(user, width);
            row.SetBounds(0, y, width, 80);
            queueBody.Controls.Add(row);
            y += 86;
        }

        if (pending.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No pending verification requests for this filter.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            queueBody.Controls.Add(empty);
        }

        queueBody.ResumeLayout();
        BuildPreview();
    }

    private Panel BuildRow(DemoUser user, int width)
    {
        var selected = ReferenceEquals(selectedUser, user);
        var row = new Panel
        {
            BackColor = selected ? Color.FromArgb(236, 244, 255) : Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        row.Click += (_, _) => Select(user);

        var avatar = new Label
        {
            Text = user.Initials,
            Size = new Size(40, 40),
            Location = new Point(14, 18),
            BackColor = user.AvatarColor,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        row.Controls.Add(avatar);

        var name = AdminUi.MakeLabel(user.FullName, 10, AdminUi.Navy, true);
        name.Location = new Point(68, 12);
        row.Controls.Add(name);

        var email = AdminUi.MakeLabel(user.Email, 8, AdminUi.Muted);
        email.Location = new Point(68, 32);
        row.Controls.Add(email);

        var meta = AdminUi.MakeLabel($"Brgy. {user.Barangay} · joined {user.Joined} · {user.Phone}", 7.5F, AdminUi.Muted);
        meta.Location = new Point(68, 52);
        row.Controls.Add(meta);

        var review = AdminUi.MakeSecondaryButton(selected ? "Reviewing" : "Review", $"verificationReview{user.Id}", 94);
        review.Size = new Size(94, 28);
        review.Location = new Point(width - 110, 24);
        review.Click += (_, _) => Select(user);
        row.Controls.Add(review);

        return row;
    }

    private void Select(DemoUser user)
    {
        selectedUser = user;
        RefreshQueue();
    }

    private void BuildPreview()
    {
        if (previewBody.Width <= 0)
            return;

        previewBody.SuspendLayout();
        previewBody.Controls.Clear();

        if (selectedUser is null)
        {
            var empty = AdminUi.MakeLabel("Select a pending request to review.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 4);
            previewBody.Controls.Add(empty);
            previewBody.ResumeLayout();
            return;
        }

        var user = selectedUser;
        var width = previewBody.ClientSize.Width;

        var avatar = new Label
        {
            Text = user.Initials,
            Size = new Size(56, 56),
            Location = new Point(0, 4),
            BackColor = user.AvatarColor,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        previewBody.Controls.Add(avatar);

        var name = AdminUi.MakeLabel(user.FullName, 14, AdminUi.Navy, true);
        name.Location = new Point(68, 4);
        previewBody.Controls.Add(name);

        var email = AdminUi.MakeLabel(user.Email, 8, AdminUi.Muted);
        email.Location = new Point(68, 30);
        previewBody.Controls.Add(email);

        var phone = AdminUi.MakeLabel($"{user.Phone} · Brgy. {user.Barangay} · joined {user.Joined}", 8, AdminUi.Muted);
        phone.Location = new Point(68, 48);
        previewBody.Controls.Add(phone);

        var document = new Panel
        {
            BackColor = Color.FromArgb(247, 250, 253),
            BorderStyle = BorderStyle.FixedSingle
        };
        document.SetBounds(0, 78, width, 210);
        previewBody.Controls.Add(document);

        var docTitle = AdminUi.MakeLabel("Identity document", 9, AdminUi.Navy, true);
        docTitle.Location = new Point(14, 12);
        document.Controls.Add(docTitle);

        var docNote = AdminUi.MakeWrappedLabel("Documents are reviewed outside the app. Approving confirms the member's identity and contact details.", 8, AdminUi.Muted, Math.Max(200, width - 200));
        docNote.Location = new Point(14, 34);
        document.Controls.Add(docNote);

        var placeholder = new Label
        {
            Text = user.Initials,
            BackColor = Color.White,
            ForeColor = AdminUi.Blue,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold)
        };
        placeholder.SetBounds(14, 74, 150, 116);
        document.Controls.Add(placeholder);

        if (user.VerificationRequested)
        {
            var request = AdminUi.MakeWrappedLabel($"Requested {user.RequestedAt}\n{user.VerificationNote}", 8, AdminUi.Navy, Math.Max(120, width - 200));
            request.Location = new Point(180, 80);
            document.Controls.Add(request);
        }

        var approve = AdminUi.MakePrimaryButton("Approve verification", "verificationApprove", 190);
        approve.Location = new Point(0, 300);
        approve.Click += (_, _) => Approve(user);
        previewBody.Controls.Add(approve);

        var reject = AdminUi.MakeSecondaryButton("Reject", "verificationReject", 110);
        reject.ForeColor = AdminUi.Bad;
        reject.Location = new Point(200, 300);
        reject.Click += (_, _) => Reject(user);
        previewBody.Controls.Add(reject);

        previewBody.ResumeLayout();
    }

    private void Approve(DemoUser user)
    {
        DemoDataStore.VerifyUser(user);
        selectedUser = null;
        RefreshQueue();
        Toast($"{user.FullName} verified.");
    }

    private void Reject(DemoUser user)
    {
        using var dialog = new ReasonDialog(
            "Reject verification",
            $"Why is {user.FullName}'s verification being rejected?",
            ["Documents unclear", "Name mismatch", "Expired ID", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.RejectVerification(user, dialog.SelectedReason);
        selectedUser = null;
        RefreshQueue();
        Toast($"{user.FullName}'s verification was rejected.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshQueue();
}
