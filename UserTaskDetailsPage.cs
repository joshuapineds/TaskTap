using System.Drawing.Drawing2D;

namespace TaskTap;

public sealed class UserTaskDetailsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly string taskTitle;
    private readonly string category;
    private readonly int price;
    private readonly string barangay;
    private readonly int days;
    private readonly string deadline;
    private readonly string icon;
    private readonly string description;
    private readonly Color iconBack;
    private readonly DemoTask? postedTask;
    private readonly DemoTask? taskRecord;
    private readonly int taskId;
    private readonly Panel scrollHost = new();
    private readonly Panel canvas = new();
    private readonly Panel content = new();
    private readonly Panel primary = new();
    private readonly Panel aside = new();
    private readonly Panel hero = new();
    private readonly Panel photo = new();
    private readonly Panel descriptionCard = new();
    private readonly Panel requirementsCard = new();
    private readonly Panel factsCard = new();
    private readonly Panel locationCard = new();
    private readonly Panel map = new();
    private readonly Panel posterCard = new();
    private readonly Panel requestCard = new();
    private readonly Panel safetyCard = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private Button saveButton = null!;
    private bool saved;

    public override Route RouteName => Route.UserTaskDetails;

    public UserTaskDetailsPage(string taskTitle, string category, int price, string barangay, int days, string deadline, string icon, string description, Color iconBack, int taskId = 0)
    {
        this.taskId = taskId > 0 ? taskId : DemoDataStore.AvailableTasks.Concat(DemoDataStore.PostedTasks).Concat(DemoDataStore.ArchivedTasks).FirstOrDefault(t => t.Title == taskTitle)?.Id ?? 0;
        this.taskTitle = taskTitle;
        this.category = category;
        this.price = price;
        this.barangay = barangay;
        this.days = days;
        this.deadline = deadline;
        this.icon = icon;
        this.description = description;
        this.iconBack = iconBack;
        taskRecord = DemoDataStore.AvailableTasks.Concat(DemoDataStore.PostedTasks).Concat(DemoDataStore.ArchivedTasks)
            .FirstOrDefault(task => task.Id == this.taskId || task.Title == taskTitle);
        postedTask = taskRecord;
        BuildPage();
    }

    private void BuildPage()
    {
        scrollHost.Dock = DockStyle.Fill;
        scrollHost.AutoScroll = true;
        scrollHost.BackColor = Page;
        scrollHost.Resize += (_, _) => LayoutPage();
        Controls.Add(scrollHost);
        canvas.BackColor = Page;
        scrollHost.Controls.Add(canvas);
        BuildContent();
        toast.Size = new Size(420, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeTextButton("← Back to Find Tasks", "UserDetailsBackButton");
        crumb.Location = new Point(0, 0);
        crumb.Click += (_, _) => Nav.Back();
        content.Controls.Add(crumb);
        var crumbTitle = MakeLabel("Task Details", 8, Muted);
        crumbTitle.Location = new Point(150, 5);
        content.Controls.Add(crumbTitle);
        content.Controls.Add(primary);
        content.Controls.Add(aside);
        BuildHero();
        BuildDescription();
        BuildRequirements();
        BuildFacts();
        BuildLocation();
        BuildPoster();
        BuildRequestCard();
        BuildSafety();
        canvas.Controls.Add(content);
    }

    private void BuildHero()
    {
        hero.Name = "userTaskDetailsHero";
        hero.BackColor = Color.White;
        hero.Controls.Add(photo);
        photo.Name = "userTaskPhoto";
        photo.BackColor = iconBack;
        var picture = MakeLabel(icon, 42, Navy, true);
        picture.Location = new Point(63, 58);
        photo.Controls.Add(picture);
        var local = MakeLabel("LOCAL TASK", 7, Muted, true);
        local.Location = new Point(12, 126);
        photo.Controls.Add(local);
        var available = MakeLabel("Available", 8, Color.FromArgb(34, 155, 111), true);
        available.BackColor = Color.FromArgb(230, 248, 241);
        available.Location = new Point(158, 24);
        hero.Controls.Add(available);
        var heading = MakeLabel(taskTitle, 23, Navy, true);
        heading.Name = "userTaskTitle";
        heading.Location = new Point(158, 49);
        heading.MaximumSize = new Size(465, 35);
        hero.Controls.Add(heading);
        var amount = MakeLabel($"₱{price:N0}", 20, Navy, true);
        amount.Location = new Point(158, 82);
        hero.Controls.Add(amount);
        var meta = MakeLabel($"⌖ Brgy. {barangay}     ◷ {deadline}     ▣ {category}", 8, Muted, true);
        meta.Location = new Point(158, 119);
        hero.Controls.Add(meta);
        saveButton = MakeTextButton("♡ Save task", "UserDetailsSaveButton");
        saveButton.Location = new Point(0, 20);
        saved = DemoDataStore.IsSaved(taskTitle);
        saveButton.Text = saved ? "♥ Saved" : "♡ Save task";
        saveButton.ForeColor = saved ? Color.FromArgb(229, 83, 110) : Muted;
        saveButton.Click += (_, _) =>
        {
            DemoDataStore.ToggleSaved(new DemoTask(taskTitle, category, price, barangay, days, deadline, icon, description, iconBack, new Point(50, 50)) { Id = taskId });
            saved = DemoDataStore.IsSaved(taskTitle);
            saveButton.Text = saved ? "♥ Saved" : "♡ Save task";
            saveButton.ForeColor = saved ? Color.FromArgb(229, 83, 110) : Muted;
            ShowToast(saved ? "Task saved. Open Saved Tasks to review it." : "Task removed from saved tasks.");
        };
        hero.Controls.Add(saveButton);

        if (postedTask is not null)
        {
            var complete = MakeAction("Mark complete", "UserDetailsCompleteButton", true, 130);
            complete.Click += (_, _) => CompletePostedTask(postedTask);
            hero.Controls.Add(complete);

            var cancelTaskButton = MakeAction("Cancel task", "UserDetailsCancelButton", false, 116);
            cancelTaskButton.ForeColor = Color.FromArgb(185, 55, 67);
            cancelTaskButton.Click += (_, _) => CancelPostedTask(postedTask);
            hero.Controls.Add(cancelTaskButton);
        }
        primary.Controls.Add(hero);
    }

    private void BuildDescription()
    {
        descriptionCard.Name = "userDetailsDescriptionCard";
        AddCardHeading(descriptionCard, "Description");
        var body = MakeLabel(description, 8.5F, Muted);
        body.Name = "userDetailsDescription";
        body.Location = new Point(14, 40);
        body.MaximumSize = new Size(690, 45);
        descriptionCard.Controls.Add(body);
        primary.Controls.Add(descriptionCard);
    }

    private void BuildRequirements()
    {
        requirementsCard.Name = "userDetailsRequirementsCard";
        AddCardHeading(requirementsCard, "Requirements");
        var requirements = category switch
        {
            "Repairs" => new[] { "Basic plumbing experience", "Bring basic tools", "Available before the deadline" },
            "Tutoring" => new[] { "Experience with the subject", "Bring learning materials", "Available before the deadline" },
            "Pet Care" => new[] { "Comfortable caring for pets", "Follow the owner’s instructions", "Available before the deadline" },
            "Shopping" => new[] { "Follow the provided shopping list", "Keep receipts for purchased items", "Available before the deadline" },
            _ => new[] { "Comfortable with household cleaning", "Bring or use basic cleaning supplies", "Available before the deadline" }
        };
        for (var index = 0; index < requirements.Length; index++)
        {
            var mark = MakeLabel("✓", 9, Color.FromArgb(35, 175, 137), true);
            mark.Location = new Point(14, 39 + index * 21);
            mark.Size = new Size(16, 18);
            requirementsCard.Controls.Add(mark);
            var requirement = MakeLabel(requirements[index], 8, Muted);
            requirement.Location = new Point(34, 40 + index * 21);
            requirementsCard.Controls.Add(requirement);
        }
        primary.Controls.Add(requirementsCard);
    }

    private void BuildFacts()
    {
        factsCard.Name = "userDetailsFactsCard";
        AddCardHeading(factsCard, "Task details");
        var schedule = $"{deadline} · 9:00 AM";
        AddFact(factsCard, "SCHEDULE", schedule, 14, 42);
        AddFact(factsCard, "ESTIMATED TIME", category == "Repairs" ? "1–2 hours" : "2–3 hours", 50, 42);
        AddFact(factsCard, "COMPENSATION", $"₱{price:N0} fixed", 14, 92);
        AddFact(factsCard, "TASK AREA", $"Brgy. {barangay}, Caloocan", 50, 92);
        primary.Controls.Add(factsCard);
    }

    private void BuildLocation()
    {
        locationCard.Name = "userDetailsLocationCard";
        AddCardHeading(locationCard, "Approximate Location");
        var privacy = MakeLabel("Task area is shown broadly for privacy. Exact address hidden.", 7.5F, Muted);
        privacy.Location = new Point(14, 35);
        locationCard.Controls.Add(privacy);
        map.Name = "userDetailsApproximateMap";
        map.BackColor = Color.FromArgb(232, 241, 242);
        map.Paint += PaintMap;
        locationCard.Controls.Add(map);
        primary.Controls.Add(locationCard);
    }

    private void PaintMap(object? sender, PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = map.ClientRectangle;
        using var land = new SolidBrush(Color.FromArgb(236, 245, 245));
        using var water = new SolidBrush(Color.FromArgb(215, 239, 245));
        using var road = new Pen(Color.FromArgb(218, 227, 228), 6);
        using var thinRoad = new Pen(Color.FromArgb(226, 233, 234), 3);
        using var block = new SolidBrush(Color.White);
        graphics.FillRectangle(land, bounds);
        graphics.FillPolygon(water, [new Point((int)(bounds.Width * .76), 0), new Point(bounds.Width, 0), new Point(bounds.Width, bounds.Height), new Point((int)(bounds.Width * .69), bounds.Height)]);
        var blocks = new[]
        {
            new Rectangle((int)(bounds.Width*.08),(int)(bounds.Height*.16),(int)(bounds.Width*.25),(int)(bounds.Height*.23)),
            new Rectangle((int)(bounds.Width*.48),(int)(bounds.Height*.16),(int)(bounds.Width*.25),(int)(bounds.Height*.23)),
            new Rectangle((int)(bounds.Width*.10),(int)(bounds.Height*.61),(int)(bounds.Width*.24),(int)(bounds.Height*.28)),
            new Rectangle((int)(bounds.Width*.48),(int)(bounds.Height*.59),(int)(bounds.Width*.22),(int)(bounds.Height*.26)),
            new Rectangle((int)(bounds.Width*.77),(int)(bounds.Height*.61),(int)(bounds.Width*.16),(int)(bounds.Height*.28))
        };
        foreach (var rect in blocks)
        {
            graphics.FillRectangle(block, rect);
            using var border = new Pen(Color.FromArgb(222, 231, 234));
            graphics.DrawRectangle(border, rect);
        }
        graphics.DrawLine(road, (int)(bounds.Width * .39), 0, (int)(bounds.Width * .39), bounds.Height);
        graphics.DrawLine(road, 0, (int)(bounds.Height * .50), bounds.Width, (int)(bounds.Height * .50));
        graphics.DrawLine(thinRoad, 0, (int)(bounds.Height * .72), bounds.Width, (int)(bounds.Height * .48));
        using var labelFont = new Font("Segoe UI", 7F, FontStyle.Bold);
        using var labelBrush = new SolidBrush(Color.FromArgb(131, 151, 166));
        graphics.DrawString($"Brgy. {barangay}", labelFont, labelBrush, bounds.Width * .16F, bounds.Height * .45F);
        graphics.DrawString("Nearby area", labelFont, labelBrush, bounds.Width * .75F, bounds.Height * .45F);
        using var pin = new SolidBrush(Blue);
        var pinX = bounds.Width / 2;
        var pinY = bounds.Height / 2 + 6;
        graphics.FillEllipse(pin, pinX - 5, pinY - 5, 10, 10);
        using var noteBack = new SolidBrush(Color.FromArgb(245, Color.White));
        graphics.FillRectangle(noteBack, 8, bounds.Height - 33, 180, 27);
        using var smallFont = new Font("Segoe UI", 6F, FontStyle.Bold);
        graphics.DrawString("Approximate task area", smallFont, labelBrush, 13, bounds.Height - 30);
        using var tinyFont = new Font("Segoe UI", 5.5F);
        graphics.DrawString("Exact user locations are not displayed.", tinyFont, labelBrush, 13, bounds.Height - 18);
    }

    private void BuildPoster()
    {
        posterCard.Name = "userDetailsPosterCard";
        AddCardHeading(posterCard, "Posted by");
        var profile = MakeTextButton("View profile", "UserDetailsViewProfileButton");
        profile.Location = new Point(0, 10);
        profile.Click += (_, _) => Nav.Navigate(ProfileLinks.For(taskRecord?.PosterName));
        posterCard.Controls.Add(profile);
        var posterName = string.IsNullOrWhiteSpace(taskRecord?.PosterName) ? "Task poster" : taskRecord.PosterName;
        var poster = DemoDataStore.Users.FirstOrDefault(user => user.FullName.Equals(posterName, StringComparison.OrdinalIgnoreCase));
        var avatar = MakeLabel(poster?.Initials ?? "TP", 11, Color.FromArgb(199, 106, 61), true);
        avatar.TextAlign = ContentAlignment.MiddleCenter;
        avatar.BackColor = Color.FromArgb(255, 237, 225);
        avatar.Location = new Point(14, 43);
        avatar.Size = new Size(38, 38);
        posterCard.Controls.Add(avatar);
        var name = MakeLabel(posterName, 9, Navy, true);
        name.Location = new Point(62, 41);
        name.Cursor = Cursors.Hand;
        name.Click += (_, _) => Nav.Navigate(ProfileLinks.For(posterName));
        posterCard.Controls.Add(name);
        var posted = MakeLabel($"{poster?.PostedTasks ?? 0} completed tasks posted", 7.5F, Muted);
        posted.Location = new Point(62, 58);
        posterCard.Controls.Add(posted);
        var rating = MakeLabel($"★★★★★  {(poster?.Rating ?? 0):0.0} ({poster?.Reviews ?? 0} reviews)", 8, Color.FromArgb(231, 144, 55), true);
        rating.Location = new Point(62, 73);
        posterCard.Controls.Add(rating);
        AddDivider(posterCard, 104);
        AddPosterStat(posterCard, $"{poster?.Rating ?? 0:0.0}", "Rating", 20);
        AddPosterStat(posterCard, (poster?.PostedTasks ?? 0).ToString(), "Tasks posted", 113);
        AddPosterStat(posterCard, (poster?.Reviews ?? 0).ToString(), "Reviews", 208);
        var quote = MakeLabel("“Very reliable and did a great job!”\nRecent community review", 7, Muted);
        quote.AutoSize = false;
        quote.Location = new Point(14, 148);
        quote.Size = new Size(270, 40);
        quote.BackColor = Page;
        posterCard.Controls.Add(quote);
        aside.Controls.Add(posterCard);
    }

    private void BuildRequestCard()
    {
        requestCard.Name = "userDetailsRequestCard";
        var label = MakeLabel("Agreed compensation", 8, Muted, true);
        label.Location = new Point(14, 14);
        requestCard.Controls.Add(label);
        var amount = MakeLabel($"₱{price:N0}", 21, Navy, true);
        amount.Location = new Point(14, 30);
        requestCard.Controls.Add(amount);
        AddDivider(requestCard, 62);
        var payment = MakeLabel("Payment is arranged directly between users.", 7, Muted);
        payment.Location = new Point(14, 72);
        payment.MaximumSize = new Size(245, 24);
        requestCard.Controls.Add(payment);
        var request = MakeAction("Request to Perform", "UserDetailsRequestButton", true, 260);
        request.Location = new Point(14, 103);
        request.Click += (_, _) => ShowRequestDialog();
        requestCard.Controls.Add(request);
        var report = MakeAction("Report Task", "UserDetailsReportButton", false, 260);
        report.Location = new Point(14, 145);
        report.Click += (_, _) => ShowReportDialog();
        requestCard.Controls.Add(report);
        var note = MakeLabel("Your request will be sent to the task poster with your short message. Never share passwords, OTPs, or sensitive financial information.", 7, Muted);
        note.Location = new Point(14, 194);
        note.MaximumSize = new Size(265, 46);
        requestCard.Controls.Add(note);
        aside.Controls.Add(requestCard);
    }

    private void ShowRequestDialog()
    {
        using var dialog = new Form
        {
            Text = "Request to perform",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(480, 390),
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        dialog.Controls.Add(new Label { Text = "Send a request for this task", Location = new Point(24, 24), Size = new Size(420, 30), Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Navy });
        dialog.Controls.Add(new Label { Text = $"Tell the poster briefly why you're a good fit and when you can complete “{taskTitle}”.", Location = new Point(24, 62), Size = new Size(428, 42), ForeColor = Muted });
        dialog.Controls.Add(new Label { Text = "Message to poster", Location = new Point(24, 116), Size = new Size(200, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Navy });
        var message = new TextBox { Name = "requestMessage", Location = new Point(24, 142), Size = new Size(428, 112), Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Hi, I can help with this task...", Font = new Font("Segoe UI", 9F) };
        dialog.Controls.Add(message);
        dialog.Controls.Add(new Label { Text = $"Task: {taskTitle}\r\nAgreed compensation: ₱{price:N0}", Location = new Point(24, 264), Size = new Size(428, 44), ForeColor = Muted });
        var cancel = MakeAction("Cancel", "RequestDialogCancel", false, 100);
        cancel.Location = new Point(228, 330);
        cancel.DialogResult = DialogResult.Cancel;
        dialog.Controls.Add(cancel);
        var send = MakeAction("Send Request", "RequestDialogSend", true, 130);
        send.Location = new Point(338, 330);
        send.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                MessageBox.Show(dialog, "Enter a short message for the task poster.", "Message required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                message.Focus();
                return;
            }
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };
        dialog.Controls.Add(send);
        dialog.AcceptButton = send;
        dialog.CancelButton = cancel;
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var sent = DemoDataStore.SendRequest(new DemoRequest
            {
                TaskId = taskId,
                Title = taskTitle,
                Category = category,
                Barangay = barangay,
                Amount = price,
                Status = "pending",
                Deadline = deadline,
                Date = DateTime.Today,
                RequestDate = DateTime.Today.ToString("yyyy-MM-dd"),
                Description = description,
                RequestMessage = message.Text.Trim(),
                Icon = icon,
                IconBack = iconBack
            });
            if (sent)
                Nav.Navigate(Route.MyRequests);
        }
    }

    private void ShowReportDialog()
    {
        using var dialog = new Form
        {
            Text = "Report task",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(480, 390),
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        dialog.Controls.Add(new Label { Text = "Report this task", Location = new Point(24, 24), Size = new Size(420, 30), Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Navy });
        dialog.Controls.Add(new Label { Text = "Choose a reason so the TaskTap team can review it.", Location = new Point(24, 62), Size = new Size(428, 24), ForeColor = Muted });
        dialog.Controls.Add(new Label { Text = "Reason", Location = new Point(24, 101), Size = new Size(200, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Navy });
        var reason = new ComboBox { Location = new Point(24, 127), Size = new Size(428, 34), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
        reason.Items.AddRange(["Suspicious or misleading task", "Unsafe request", "Inappropriate content", "Spam or duplicate", "Other"]);
        reason.SelectedIndex = 0;
        dialog.Controls.Add(reason);
        dialog.Controls.Add(new Label { Text = "Additional details (optional)", Location = new Point(24, 177), Size = new Size(250, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Navy });
        var details = new TextBox { Location = new Point(24, 203), Size = new Size(428, 95), Multiline = true, MaxLength = 1000, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Add details that may help with the review...", Font = new Font("Segoe UI", 9F) };
        dialog.Controls.Add(details);
        var cancel = MakeAction("Cancel", "ReportDialogCancel", false, 100);
        cancel.Location = new Point(228, 330);
        cancel.DialogResult = DialogResult.Cancel;
        dialog.Controls.Add(cancel);
        var submit = MakeAction("Submit Report", "ReportDialogSubmit", true, 130);
        submit.Location = new Point(338, 330);
        submit.Click += (_, _) =>
        {
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };
        dialog.Controls.Add(submit);
        dialog.AcceptButton = submit;
        dialog.CancelButton = cancel;
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            DemoDataStore.SubmitReport("Task", taskTitle, reason.SelectedItem?.ToString() ?? "Other", details.Text.Trim());
            ShowToast("Report submitted. The TaskTap team will review it.");
        }
    }

    private void BuildSafety()
    {
        safetyCard.Name = "userDetailsSafetyCard";
        safetyCard.BackColor = Color.FromArgb(237, 247, 255);
        var badge = MakeLabel("✓", 13, Blue, true);
        badge.Location = new Point(14, 14);
        badge.Size = new Size(30, 30);
        badge.TextAlign = ContentAlignment.MiddleCenter;
        badge.BackColor = Color.FromArgb(222, 240, 255);
        safetyCard.Controls.Add(badge);
        var heading = MakeLabel("Community safety", 8, Navy, true);
        heading.Location = new Point(54, 12);
        safetyCard.Controls.Add(heading);
        var body = MakeLabel("Keep agreements, messages, and task updates on TaskTap. Meet safely and confirm task details before starting.", 7, Muted);
        body.Location = new Point(54, 29);
        body.MaximumSize = new Size(250, 45);
        safetyCard.Controls.Add(body);
        var link = MakeTextButton("Learn how TaskTap works →", "UserDetailsSafetyLink");
        link.Location = new Point(54, 72);
        link.Click += (_, _) => ShowToast("How It Works is available from the Guest information pages.");
        safetyCard.Controls.Add(link);
        aside.Controls.Add(safetyCard);
    }

    private void LayoutPage()
    {
        if (scrollHost.ClientSize.Width <= 0 || content.Controls.Count == 0)
            return;
        var width = Math.Max(820, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 28);
        canvas.Size = new Size(width, 1000);
        canvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);
        var left = 22;
        content.Location = new Point(left, 14);
        content.Size = new Size(width - 44, 930);
        var columnGap = 16;
        var asideWidth = Math.Max(270, Math.Min(340, (content.Width - columnGap) / 3));
        var primaryWidth = content.Width - asideWidth - columnGap;
        primary.Location = new Point(0, 30);
        primary.Size = new Size(primaryWidth, 850);
        aside.Location = new Point(primaryWidth + columnGap, 30);
        aside.Size = new Size(asideWidth, 600);
        var crumb = content.Controls.Find("UserDetailsBackButton", false).FirstOrDefault();
        var crumbTitle = content.Controls.OfType<Label>().FirstOrDefault(label => label.Text == "Task Details");
        if (crumb is not null) crumb.Location = new Point(0, 0);
        if (crumbTitle is not null) crumbTitle.Location = new Point(150, 5);
        hero.Bounds = new Rectangle(0, 0, primaryWidth, 185);
        photo.Bounds = new Rectangle(14, 14, 132, 157);
        var heroSave = hero.Controls.Find("UserDetailsSaveButton", false).FirstOrDefault();
        if (heroSave is not null) heroSave.Location = new Point(primaryWidth - heroSave.Width - 14, 18);
        var heroComplete = hero.Controls.Find("UserDetailsCompleteButton", false).FirstOrDefault();
        var heroCancel = hero.Controls.Find("UserDetailsCancelButton", false).FirstOrDefault();
        if (heroComplete is not null && heroSave is not null)
            heroComplete.Location = new Point(heroSave.Left - heroComplete.Width - 8, 18);
        if (heroCancel is not null && heroComplete is not null)
            heroCancel.Location = new Point(heroComplete.Left - heroCancel.Width - 8, 18);
        descriptionCard.Bounds = new Rectangle(0, 199, primaryWidth, 82);
        requirementsCard.Bounds = new Rectangle(0, 295, primaryWidth, 116);
        factsCard.Bounds = new Rectangle(0, 425, primaryWidth, 142);
        locationCard.Bounds = new Rectangle(0, 581, primaryWidth, 310);
        var facts = factsCard.Controls.OfType<Panel>().Where(panel => panel.Name.StartsWith("factCell", StringComparison.Ordinal)).ToArray();
        for (var index = 0; index < facts.Length; index++)
        {
            facts[index].Location = new Point(14 + (index % 2) * ((primaryWidth - 34) / 2), 42 + (index / 2) * 50);
            facts[index].Size = new Size((primaryWidth - 34) / 2, 48);
        }
        map.Bounds = new Rectangle(14, 62, primaryWidth - 28, 232);
        posterCard.Bounds = new Rectangle(0, 0, asideWidth, 198);
        requestCard.Bounds = new Rectangle(0, 214, asideWidth, 252);
        safetyCard.Bounds = new Rectangle(0, 482, asideWidth, 110);
        var viewProfile = posterCard.Controls.Find("UserDetailsViewProfileButton", false).FirstOrDefault();
        if (viewProfile is not null) viewProfile.Location = new Point(asideWidth - viewProfile.Width - 12, 10);
        var requestButton = requestCard.Controls.Find("UserDetailsRequestButton", false).FirstOrDefault();
        var reportButton = requestCard.Controls.Find("UserDetailsReportButton", false).FirstOrDefault();
        if (requestButton is not null) requestButton.Width = asideWidth - 28;
        if (reportButton is not null) reportButton.Width = asideWidth - 28;
        canvas.Height = 950;
    }

    private void AddFact(Control parent, string title, string value, int column, int top)
    {
        var cell = new Panel { Name = $"factCell{parent.Controls.OfType<Panel>().Count() + 1}", BackColor = Color.White };
        var label = MakeLabel(title, 6.5F, Muted, true);
        label.Location = new Point(8, 5);
        cell.Controls.Add(label);
        var contentLabel = MakeLabel(value, 7.5F, Navy, true);
        contentLabel.Location = new Point(8, 22);
        contentLabel.MaximumSize = new Size(330, 20);
        cell.Controls.Add(contentLabel);
        parent.Controls.Add(cell);
    }

    private static void AddPosterStat(Control parent, string value, string label, int x)
    {
        var stat = MakeLabel($"{value}\n{label}", 7, Navy, true);
        stat.Location = new Point(x, 116);
        stat.Size = new Size(78, 30);
        stat.TextAlign = ContentAlignment.MiddleCenter;
        parent.Controls.Add(stat);
    }

    private static void AddCardHeading(Panel card, string text)
    {
        card.BackColor = Color.White;
        card.BorderStyle = BorderStyle.FixedSingle;
        var heading = MakeLabel(text, 10, Navy, true);
        heading.Location = new Point(14, 12);
        card.Controls.Add(heading);
    }

    private static void AddDivider(Control parent, int y)
    {
        parent.Controls.Add(new Panel { Name = "detailsDivider", BackColor = Line, Location = new Point(12, y), Height = 1, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
    }

    private void CompletePostedTask(DemoTask task)
    {
        if (MessageBox.Show(this, $"Mark \"{task.Title}\" complete?", "Complete task", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        DemoDataStore.CompleteTask(task);
        ShowToast("Task marked complete. It now appears in Archived Tasks.");
        Nav.Navigate(Route.ArchivedTasks);
    }

    private void CancelPostedTask(DemoTask task)
    {
        using var dialog = new ReasonDialog("Cancel task", $"Why is \"{task.Title}\" being cancelled?", ["No longer needed", "Found help elsewhere", "Schedule changed", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.CancelTask(task, dialog.SelectedReason ?? "Cancelled.");
        ShowToast("Task cancelled. It now appears in Archived Tasks.");
        Nav.Navigate(Route.ArchivedTasks);
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private static Button MakeAction(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 38), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false)
    {
        return new Label { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
