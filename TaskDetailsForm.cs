using System.Drawing.Drawing2D;

namespace TaskTap
{
    public sealed class TaskDetailsForm : Form
    {
        private static readonly Color PageColor = Color.FromArgb(247, 251, 255);
        private static readonly Color NavyColor = Color.FromArgb(8, 38, 87);
        private static readonly Color BlueColor = Color.FromArgb(8, 117, 232);
        private static readonly Color MutedColor = Color.FromArgb(113, 131, 155);
        private readonly Panel scrollHost = new();
        private readonly Panel pageCanvas = new();
        private readonly Panel header = new();
        private readonly Panel mainContent = new();
        private readonly Panel primaryColumn = new();
        private readonly Panel sideColumn = new();
        private readonly Panel heroCard = new();
        private readonly Panel photoPanel = new();
        private readonly Panel descriptionCard = new();
        private readonly Panel requirementsCard = new();
        private readonly Panel locationCard = new();
        private readonly Panel mapPanel = new();
        private readonly Panel posterCard = new();
        private readonly Panel actionCard = new();
        private readonly Panel safetyCard = new();
        private readonly Panel footer = new();
        private readonly Label toastLabel = new();
        private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
        private readonly string title;
        private readonly string category;
        private readonly int price;
        private readonly string barangay;
        private readonly string deadline;
        private readonly string description;
        private readonly string icon;
        private bool saved;
        private bool layoutReady;
        private Button saveButton = null!;

        public TaskDetailsForm(string title, string category, int price, string barangay, string deadline, string description, string icon)
        {
            this.title = title;
            this.category = category;
            this.price = price;
            this.barangay = barangay;
            this.deadline = deadline;
            this.description = description;
            this.icon = icon;
            BuildPage();
        }

        private void BuildPage()
        {
            Text = $"TaskTap — {title}";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 760);
            ClientSize = new Size(1360, 900);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = PageColor;
            scrollHost.Resize += (_, _) => LayoutPage();
            Controls.Add(scrollHost);

            pageCanvas.BackColor = PageColor;
            scrollHost.Controls.Add(pageCanvas);

            BuildHeader();
            BuildHero();
            BuildDescription();
            BuildRequirements();
            BuildLocation();
            BuildPoster();
            BuildActions();
            BuildSafety();
            BuildFooter();

            var crumb = MakeLinkButton("← Back to Tasks", "DetailsBackToTasksButton", BlueColor);
            crumb.Name = "detailsCrumb";
            crumb.Click += (_, _) => Close();
            mainContent.Controls.Add(crumb);
            primaryColumn.Controls.Add(heroCard);
            primaryColumn.Controls.Add(descriptionCard);
            primaryColumn.Controls.Add(requirementsCard);
            primaryColumn.Controls.Add(locationCard);
            sideColumn.Controls.Add(posterCard);
            sideColumn.Controls.Add(actionCard);
            sideColumn.Controls.Add(safetyCard);
            mainContent.Controls.Add(primaryColumn);
            mainContent.Controls.Add(sideColumn);

            pageCanvas.Controls.Add(header);
            pageCanvas.Controls.Add(mainContent);
            pageCanvas.Controls.Add(footer);

            toastLabel.Name = "taskDetailsToast";
            toastLabel.AutoSize = false;
            toastLabel.Size = new Size(420, 40);
            toastLabel.BackColor = NavyColor;
            toastLabel.ForeColor = Color.White;
            toastLabel.TextAlign = ContentAlignment.MiddleCenter;
            toastLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toastLabel.Visible = false;
            Controls.Add(toastLabel);
            toastLabel.BringToFront();
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toastLabel.Visible = false;
            };

            layoutReady = true;
            LayoutPage();
        }

        private void BuildHeader()
        {
            header.Name = "taskDetailsHeader";
            header.BackColor = Color.White;
            header.Controls.Add(MakeBrand());

            var navigation = new FlowLayoutPanel
            {
                Name = "taskDetailsNavigation",
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.White
            };
            var browse = MakeLinkButton("Browse Tasks", "DetailsBrowseButton", BlueColor);
            browse.Click += (_, _) => Close();
            navigation.Controls.Add(browse);
            var howItWorks = MakeLinkButton("How It Works", "DetailsHowItWorksButton", MutedColor);
            howItWorks.Click += (_, _) => NavigateToPublicInfo("How It Works");
            navigation.Controls.Add(howItWorks);
            var about = MakeLinkButton("About", "DetailsAboutButton", MutedColor);
            about.Click += (_, _) => NavigateToPublicInfo("About");
            navigation.Controls.Add(about);
            header.Controls.Add(navigation);

            var account = new FlowLayoutPanel
            {
                Name = "taskDetailsAccountActions",
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Size = new Size(180, 38),
                BackColor = Color.White
            };
            var signUp = MakeButton("Sign Up", "DetailsSignUpButton", BlueColor, Color.White, 78, 32);
            signUp.Click += (_, _) => OpenAuthentication(true);
            var login = MakeButton("Login", "DetailsLoginButton", Color.White, NavyColor, 70, 32, Color.FromArgb(204, 218, 233));
            login.Click += (_, _) => OpenAuthentication(false);
            account.Controls.Add(signUp);
            account.Controls.Add(login);
            header.Controls.Add(account);
        }

        private void BuildHero()
        {
            heroCard.Name = "detailsHeroCard";
            ConfigureCard(heroCard);
            photoPanel.Name = "detailsTaskPhoto";
            photoPanel.BackColor = Color.FromArgb(235, 243, 251);
            var taskIcon = MakeLabel(icon, 54, Color.FromArgb(22, 42, 64));
            taskIcon.Dock = DockStyle.Fill;
            taskIcon.TextAlign = ContentAlignment.MiddleCenter;
            photoPanel.Controls.Add(taskIcon);
            heroCard.Controls.Add(photoPanel);

            var available = MakeLabel("Available", 8, Color.FromArgb(17, 150, 106), true);
            available.Name = "detailsAvailabilityBadge";
            available.BackColor = Color.FromArgb(231, 248, 240);
            available.TextAlign = ContentAlignment.MiddleCenter;
            available.Size = new Size(67, 22);
            heroCard.Controls.Add(available);

            saveButton = MakeButton("♡ Save task", "DetailsSaveTaskButton", Color.White, MutedColor, 112, 28, flat: true);
            saveButton.Font = new Font("Segoe UI", 8F);
            saved = DemoDataStore.IsSaved(title);
            saveButton.Text = saved ? "♥ Saved" : "♡ Save task";
            saveButton.ForeColor = saved ? Color.FromArgb(222, 77, 91) : MutedColor;
            saveButton.Click += (_, _) =>
            {
                DemoDataStore.ToggleSaved(new DemoTask(title, category, price, barangay, 3, deadline, icon, description, Color.FromArgb(234, 243, 255), new Point(50, 50)));
                saved = DemoDataStore.IsSaved(title);
                saveButton.Text = saved ? "♥ Saved" : "♡ Save task";
                saveButton.ForeColor = saved ? Color.FromArgb(222, 77, 91) : MutedColor;
                ShowToast(saved ? "Task saved." : "Task removed from saved tasks.");
            };
            heroCard.Controls.Add(saveButton);

            var taskTitle = MakeLabel(title, 24, NavyColor, true);
            taskTitle.Name = "detailsTaskTitle";
            heroCard.Controls.Add(taskTitle);
            var compensation = MakeLabel($"₱{price}", 17, BlueColor, true);
            compensation.Name = "detailsTaskPrice";
            heroCard.Controls.Add(compensation);
            var meta = MakeLabel($"⌖ Brgy. {barangay}   ◷ Deadline: {deadline}   ▣ {category}", 8, MutedColor);
            meta.Name = "detailsTaskMetadata";
            heroCard.Controls.Add(meta);
        }

        private void BuildDescription()
        {
            ConfigureCard(descriptionCard);
            descriptionCard.Name = "detailsDescriptionCard";
            AddSectionTitle(descriptionCard, "Description");
            var descriptionLabel = MakeLabel(description, 9, MutedColor);
            descriptionLabel.Name = "detailsDescriptionText";
            descriptionLabel.MaximumSize = new Size(620, 0);
            descriptionCard.Controls.Add(descriptionLabel);
        }

        private void BuildRequirements()
        {
            ConfigureCard(requirementsCard);
            requirementsCard.Name = "detailsRequirementsCard";
            AddSectionTitle(requirementsCard, "Requirements");
            var requirements = GetRequirements(category);
            var list = new FlowLayoutPanel
            {
                Name = "detailsRequirementsList",
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.White
            };
            foreach (var requirement in requirements)
            {
                var item = MakeLabel($"•   {requirement}", 9, MutedColor);
                item.Margin = new Padding(0, 3, 0, 2);
                list.Controls.Add(item);
            }
            requirementsCard.Controls.Add(list);
        }

        private void BuildLocation()
        {
            ConfigureCard(locationCard);
            locationCard.Name = "detailsLocationCard";
            AddSectionTitle(locationCard, "Approximate Location");
            var privacy = MakeLabel("Exact address hidden for privacy", 7, MutedColor);
            privacy.Name = "detailsLocationPrivacy";
            locationCard.Controls.Add(privacy);

            mapPanel.Name = "detailsApproximateMap";
            mapPanel.BackColor = Color.FromArgb(233, 242, 231);
            mapPanel.Paint += PaintMap;
            locationCard.Controls.Add(mapPanel);
        }

        private void BuildPoster()
        {
            ConfigureCard(posterCard);
            posterCard.Name = "detailsPosterCard";
            AddSectionTitle(posterCard, "Posted by");
            var poster = DemoDataStore.AvailableTasks.FirstOrDefault(candidate => candidate.Title == title)?.PosterName ?? "TaskTap member";
            var posterInitials = string.Concat(poster.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));
            var avatar = MakeLabel(posterInitials, 12, BlueColor, true);
            avatar.Name = "posterAvatar";
            avatar.BackColor = Color.FromArgb(232, 242, 255);
            avatar.Size = new Size(44, 44);
            avatar.TextAlign = ContentAlignment.MiddleCenter;
            posterCard.Controls.Add(avatar);
            var posterName = MakeLabel(poster, 9, NavyColor, true);
            posterName.Name = "posterName";
            posterCard.Controls.Add(posterName);
            var completed = MakeLabel("Member of the TaskTap community", 7, MutedColor);
            completed.Name = "posterCompletedTasks";
            posterCard.Controls.Add(completed);
            var rating = MakeLabel("Sign in to see ratings", 8, Color.FromArgb(221, 149, 38), true);
            rating.Name = "posterRating";
            posterCard.Controls.Add(rating);
            var review = MakeLabel("Sign in to view this member's profile\nand reviews.", 7, MutedColor);
            review.Name = "posterReview";
            review.BackColor = Color.FromArgb(247, 250, 253);
            review.AutoSize = false;
            posterCard.Controls.Add(review);
        }

        private void BuildActions()
        {
            ConfigureCard(actionCard);
            actionCard.Name = "detailsActionCard";
            var compensationLabel = MakeLabel("Agreed compensation", 8, MutedColor);
            compensationLabel.Name = "agreedCompensationLabel";
            actionCard.Controls.Add(compensationLabel);
            var compensationAmount = MakeLabel($"₱{price}", 15, BlueColor, true);
            compensationAmount.Name = "agreedCompensationAmount";
            actionCard.Controls.Add(compensationAmount);

            var request = MakeButton("Request to Perform", "RequestToPerformButton", BlueColor, Color.White, 280, 36);
            request.Click += (_, _) => RequestToPerform();
            actionCard.Controls.Add(request);
            var report = MakeButton("Report Task", "ReportTaskButton", Color.White, BlueColor, 280, 34, Color.FromArgb(195, 213, 234));
            report.Click += (_, _) => OpenAuthentication(false);
            actionCard.Controls.Add(report);
            var notice = MakeLabel("You'll need to sign in before submitting a request. Never share sensitive information in a public task description.", 7, MutedColor);
            notice.Name = "requestSignInNotice";
            notice.AutoSize = false;
            actionCard.Controls.Add(notice);
        }

        private void BuildSafety()
        {
            ConfigureCard(safetyCard);
            safetyCard.Name = "detailsSafetyCard";
            safetyCard.BackColor = Color.FromArgb(243, 249, 255);
            var shield = MakeLabel("✓", 13, BlueColor, true);
            shield.Name = "safetyShield";
            shield.BackColor = Color.FromArgb(225, 240, 255);
            shield.Size = new Size(32, 32);
            shield.TextAlign = ContentAlignment.MiddleCenter;
            safetyCard.Controls.Add(shield);
            var titleLabel = MakeLabel("Keep interactions on TaskTap", 8, NavyColor, true);
            titleLabel.Name = "safetyTitle";
            safetyCard.Controls.Add(titleLabel);
            var safetyText = MakeLabel("Use TaskTap messages and task records to keep requests, agreements, and status changes organized.", 7, MutedColor);
            safetyText.Name = "safetyDescription";
            safetyText.AutoSize = false;
            safetyCard.Controls.Add(safetyText);
        }

        private void BuildFooter()
        {
            footer.Name = "detailsFooter";
            footer.BackColor = Color.FromArgb(7, 31, 67);
            var brand = MakeLabel("TaskTap", 15, Color.White, true);
            brand.Location = new Point(0, 13);
            footer.Controls.Add(brand);
            var tagline = MakeLabel("Local tasks. Real people. Stronger communities.", 7, Color.FromArgb(130, 152, 180));
            tagline.Location = new Point(0, 37);
            footer.Controls.Add(tagline);
            var copyright = MakeLabel("© 2026 TaskTap", 7, Color.FromArgb(130, 152, 180));
            copyright.Name = "detailsCopyright";
            footer.Controls.Add(copyright);
        }

        private void LayoutPage()
        {
            if (!layoutReady || scrollHost.ClientSize.Width <= 0)
                return;

            var pageWidth = Math.Min(1220, Math.Max(920, scrollHost.ClientSize.Width - 40));
            pageCanvas.Size = new Size(pageWidth, pageCanvas.Height);
            pageCanvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - pageWidth) / 2), 0);
            header.Bounds = new Rectangle(0, 0, pageWidth, 58);
            var brandControl = header.Controls.Find("taskDetailsBrand", false).FirstOrDefault();
            if (brandControl is not null)
                brandControl.Location = new Point(24, 12);
            var navigation = header.Controls.Find("taskDetailsNavigation", false).FirstOrDefault();
            if (navigation is not null)
                navigation.Location = new Point(190, 14);
            var account = header.Controls.Find("taskDetailsAccountActions", false).FirstOrDefault();
            if (account is not null)
                account.Location = new Point(pageWidth - 205, 10);

            var contentWidth = pageWidth - 48;
            mainContent.Bounds = new Rectangle(24, 78, contentWidth, 600);
            var primaryWidth = Math.Min(650, contentWidth - 360);
            var gap = 16;
            var sideWidth = contentWidth - primaryWidth - gap;
            primaryColumn.Bounds = new Rectangle(0, 22, primaryWidth, 560);
            sideColumn.Bounds = new Rectangle(primaryWidth + gap, 22, sideWidth, 390);

            var crumb = mainContent.Controls.Find("detailsCrumb", false).FirstOrDefault();
            if (crumb is not null) crumb.Location = new Point(0, 0);

            heroCard.Bounds = new Rectangle(0, 0, primaryWidth, 168);
            photoPanel.Bounds = new Rectangle(10, 10, 150, 148);
            SetControlLocation(heroCard, "detailsAvailabilityBadge", 178, 18);
            SetControlLocation(heroCard, "DetailsSaveTaskButton", primaryWidth - 128, 14);
            SetControlLocation(heroCard, "detailsTaskTitle", 178, 56);
            SetControlLocation(heroCard, "detailsTaskPrice", 178, 91);
            SetControlLocation(heroCard, "detailsTaskMetadata", 178, 124);

            descriptionCard.Bounds = new Rectangle(0, 178, primaryWidth, 74);
            SetControlLocation(descriptionCard, "detailsDescriptionText", 14, 39);
            requirementsCard.Bounds = new Rectangle(0, 262, primaryWidth, 96);
            SetControlLocation(requirementsCard, "detailsRequirementsList", 14, 38);
            locationCard.Bounds = new Rectangle(0, 368, primaryWidth, 244);
            SetControlLocation(locationCard, "detailsLocationPrivacy", primaryWidth - 178, 15);
            mapPanel.Bounds = new Rectangle(12, 42, primaryWidth - 24, 188);

            posterCard.Bounds = new Rectangle(0, 0, sideWidth, 138);
            SetControlLocation(posterCard, "posterAvatar", 14, 38);
            SetControlLocation(posterCard, "posterName", 69, 43);
            SetControlLocation(posterCard, "posterCompletedTasks", 69, 62);
            SetControlLocation(posterCard, "posterRating", 69, 79);
            var review = posterCard.Controls.Find("posterReview", false).FirstOrDefault();
            if (review is not null) review.Bounds = new Rectangle(12, 96, sideWidth - 24, 32);

            actionCard.Bounds = new Rectangle(0, 150, sideWidth, 190);
            SetControlLocation(actionCard, "agreedCompensationLabel", 14, 15);
            SetControlLocation(actionCard, "agreedCompensationAmount", sideWidth - 74, 9);
            var request = actionCard.Controls.Find("RequestToPerformButton", false).FirstOrDefault();
            if (request is not null) request.Bounds = new Rectangle(12, 41, sideWidth - 24, 36);
            var report = actionCard.Controls.Find("ReportTaskButton", false).FirstOrDefault();
            if (report is not null) report.Bounds = new Rectangle(12, 83, sideWidth - 24, 34);
            var notice = actionCard.Controls.Find("requestSignInNotice", false).FirstOrDefault();
            if (notice is not null) notice.Bounds = new Rectangle(13, 126, sideWidth - 26, 54);

            safetyCard.Bounds = new Rectangle(0, 352, sideWidth, 80);
            SetControlLocation(safetyCard, "safetyShield", 12, 20);
            SetControlLocation(safetyCard, "safetyTitle", 54, 16);
            var safetyText = safetyCard.Controls.Find("safetyDescription", false).FirstOrDefault();
            if (safetyText is not null) safetyText.Bounds = new Rectangle(54, 34, sideWidth - 66, 38);

            var footerTop = 740;
            footer.Bounds = new Rectangle(0, footerTop, pageWidth, 78);
            var copyright = footer.Controls.Find("detailsCopyright", false).FirstOrDefault();
            if (copyright is not null) copyright.Location = new Point(pageWidth - copyright.Width - 24, 30);
            pageCanvas.Height = footerTop + footer.Height + 28;
            toastLabel.Location = new Point((ClientSize.Width - toastLabel.Width) / 2, ClientSize.Height - toastLabel.Height - 20);
        }

        private void PaintMap(object? sender, PaintEventArgs e)
        {
            var bounds = mapPanel.ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.FromArgb(233, 242, 231));
            using var minorRoad = new Pen(Color.FromArgb(210, 223, 207), 3);
            for (var x = -bounds.Height; x < bounds.Width + bounds.Height; x += 82)
                graphics.DrawLine(minorRoad, x, 0, x - bounds.Height / 3, bounds.Height);
            for (var y = -bounds.Width; y < bounds.Height + bounds.Width; y += 75)
                graphics.DrawLine(minorRoad, 0, y, bounds.Width, y + bounds.Width / 5);
            using var roadEdge = new Pen(Color.FromArgb(216, 226, 216), 23);
            using var road = new Pen(Color.White, 19);
            graphics.DrawLine(roadEdge, -20, bounds.Height * 0.2F, bounds.Width + 20, bounds.Height * 0.8F);
            graphics.DrawLine(road, -20, bounds.Height * 0.2F, bounds.Width + 20, bounds.Height * 0.8F);
            graphics.DrawLine(roadEdge, bounds.Width * 0.92F, -10, bounds.Width * 0.28F, bounds.Height + 10);
            graphics.DrawLine(road, bounds.Width * 0.92F, -10, bounds.Width * 0.28F, bounds.Height + 10);
            graphics.DrawLine(roadEdge, bounds.Width * 0.08F, bounds.Height + 10, bounds.Width * 0.82F, -10);
            graphics.DrawLine(road, bounds.Width * 0.08F, bounds.Height + 10, bounds.Width * 0.82F, -10);
            DrawMapLabel(graphics, $"Brgy. {barangay}", bounds.Width * 0.25F, bounds.Height * 0.30F);
            DrawMapLabel(graphics, "Nearby area", bounds.Width * 0.70F, bounds.Height * 0.58F);
            DrawMapPin(graphics, bounds.Width * 0.54F, bounds.Height * 0.47F);
            using var captionBrush = new SolidBrush(Color.FromArgb(245, 255, 255, 255));
            graphics.FillRectangle(captionBrush, 9, bounds.Height - 36, Math.Min(270, bounds.Width - 18), 28);
            using var captionFont = new Font("Segoe UI", 7F, FontStyle.Bold);
            using var captionText = new SolidBrush(NavyColor);
            graphics.DrawString("Approximate task area", captionFont, captionText, 16, bounds.Height - 32);
            using var smallFont = new Font("Segoe UI", 6F);
            using var smallText = new SolidBrush(MutedColor);
            graphics.DrawString("Location is shown broadly and does not reveal an exact address.", smallFont, smallText, 16, bounds.Height - 20);
        }

        private static void DrawMapLabel(Graphics graphics, string text, float x, float y)
        {
            using var font = new Font("Segoe UI", 7F, FontStyle.Bold);
            var measured = graphics.MeasureString(text, font);
            var rect = new RectangleF(x, y, measured.Width + 12, measured.Height + 6);
            using var background = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
            using var foreground = new SolidBrush(Color.FromArgb(101, 121, 104));
            graphics.FillRectangle(background, rect);
            graphics.DrawString(text, font, foreground, x + 6, y + 3);
        }

        private static void DrawMapPin(Graphics graphics, float x, float y)
        {
            using var brush = new SolidBrush(Color.FromArgb(241, 109, 99));
            using var center = new SolidBrush(Color.White);
            graphics.FillEllipse(brush, x, y, 20, 20);
            graphics.FillPolygon(brush, [new PointF(x + 2, y + 14), new PointF(x + 10, y + 29), new PointF(x + 18, y + 14)]);
            graphics.FillEllipse(center, x + 8, y + 6, 4, 4);
        }

        private void RequestToPerform()
        {
            AppSession.PendingTaskAfterLogin = new DemoTask(
                title, category, price, barangay, 2, deadline, icon, description,
                Color.FromArgb(232, 241, 255), new Point(50, 50));
            OpenAuthentication(false);
        }

        private void NavigateToPublicInfo(string page)
        {
            Form infoPage = page == "About" ? new AboutForm() : new HowItWorksForm();
            infoPage.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };
            Hide();
            infoPage.Show();
        }

        private void OpenAuthentication(bool registerInitially)
        {
            var authentication = new Form2(registerInitially);
            authentication.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };
            Hide();
            authentication.Show();
        }

        private void ShowToast(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastLabel.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private static string[] GetRequirements(string taskCategory) => taskCategory switch
        {
            "Repairs" => ["Basic plumbing experience", "Bring basic tools"],
            "Tutoring" => ["Experience with the subject", "Bring learning materials"],
            "Pet Care" => ["Follow the owner's care instructions", "Treat pets gently"],
            "Shopping" => ["Confirm the shopping list", "Keep receipts for purchases"],
            _ => ["Complete the task as described", "Coordinate details through TaskTap"]
        };

        private static void ConfigureCard(Panel panel)
        {
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void AddSectionTitle(Control parent, string text)
        {
            var label = MakeLabel(text, 11, NavyColor, true);
            label.Location = new Point(14, 11);
            parent.Controls.Add(label);
        }

        private void SetControlLocation(Control parent, string name, int x, int y)
        {
            var control = parent.Controls.Find(name, false).FirstOrDefault();
            if (control is not null)
                control.Location = new Point(x, y);
        }

        private static Control MakeBrand()
        {
            var logo = new TaskTapLogo { Name = "taskDetailsBrand", BackColor = Color.White, Size = new Size(160, 34), Cursor = Cursors.Hand };
            logo.Click += (_, _) => AppNavigation.GoToLanding();
            return logo;
        }

        private static Button MakeButton(string text, string name, Color back, Color fore, int width, int height, Color? border = null, bool flat = false)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(width, height),
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(4, 2, 4, 2)
            };
            button.FlatAppearance.BorderSize = flat ? 0 : 1;
            button.FlatAppearance.BorderColor = border ?? back;
            return button;
        }

        private static Button MakeLinkButton(string text, string name, Color fore)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = fore,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(6, 2, 6, 2)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label MakeLabel(string text, float size, Color fore, bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = fore,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                Margin = Padding.Empty
            };
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
