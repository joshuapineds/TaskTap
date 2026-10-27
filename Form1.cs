using System.Drawing.Drawing2D;

namespace TaskTap
{
    public partial class Form1 : Form
    {
        private static readonly Color PageColor = Color.FromArgb(247, 251, 255);
        private static readonly Color NavyColor = Color.FromArgb(7, 30, 70);
        private static readonly Color BlueColor = Color.FromArgb(8, 103, 215);
        private static readonly Color MutedColor = Color.FromArgb(100, 116, 139);
        private readonly Panel pageLayout = new();
        private readonly Panel scrollHost = new();
        private readonly List<CategoryTile> categoryTiles = [];
        private Panel? categorySection;
        private readonly List<TaskCard> taskCards = [];
        private TextBox searchTextBox = null!;
        private ComboBox barangayComboBox = null!;
        private FlowLayoutPanel taskCardsLayout = null!;
        private Label noResultsLabel = null!;
        private Label toastLabel = null!;
        private System.Windows.Forms.Timer toastTimer = null!;
        private string? selectedCategory = null;

        public Form1()
        {
            InitializeComponent();
            BuildPage();
        }

        private void BuildPage()
        {
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;
            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = PageColor;
            scrollHost.Resize += (_, _) => ResizePage();
            Controls.Add(scrollHost);

            toastLabel = new Label
            {
                Name = "statusToast",
                AutoSize = false,
                Size = new Size(460, 40),
                BackColor = Color.FromArgb(12, 47, 100),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false,
                Anchor = AnchorStyles.Bottom
            };
            Controls.Add(toastLabel);
            toastLabel.BringToFront();
            toastTimer = new System.Windows.Forms.Timer { Interval = 2600 };
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toastLabel.Visible = false;
            };

            pageLayout.Name = "pageLayout";
            pageLayout.Width = 1220;
            pageLayout.Margin = Padding.Empty;
            pageLayout.Padding = new Padding(18, 0, 18, 30);
            pageLayout.BackColor = PageColor;
            scrollHost.Controls.Add(pageLayout);

            AddSection(BuildHeader(), 76);
            AddSection(BuildHero(), 440);
            AddSection(BuildCategories(), 185);
            AddSection(BuildNearbyTasks(), 425);
            AddSection(BuildTrustSection(), 150);
            AddSection(BuildCallToAction(), 205);
            AddSection(BuildFooter(), 185);

            WireNavigationActions();
            ResizePage();
            Activated += (_, _) => RefreshCategoryTiles();
        }

        private void AddSection(Control section, int height)
        {
            section.Height = height;
            section.Dock = DockStyle.None;
            section.Width = pageLayout.Width - pageLayout.Padding.Horizontal;
            section.Location = new Point(pageLayout.Padding.Left, pageLayout.Controls.Cast<Control>().Sum(control => control.Height + control.Margin.Vertical));
            section.Margin = new Padding(0, 0, 0, 10);
            pageLayout.Controls.Add(section);
        }

        private void ResizePage()
        {
            if (scrollHost.ClientSize.Width <= 0 || pageLayout.Controls.Count == 0)
                return;

            var pageWidth = Math.Min(1220, Math.Max(900, scrollHost.ClientSize.Width - 36));
            pageLayout.Width = pageWidth;
            pageLayout.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - pageWidth) / 2), 0);

            var sectionTop = pageLayout.Padding.Top;
            foreach (Control section in pageLayout.Controls)
            {
                section.Dock = DockStyle.None;
                section.Width = pageWidth - pageLayout.Padding.Horizontal;
                section.Location = new Point(pageLayout.Padding.Left, sectionTop);
                sectionTop += section.Height + section.Margin.Vertical;
            }

            pageLayout.Height = sectionTop + pageLayout.Padding.Bottom;

            toastLabel.Location = new Point((ClientSize.Width - toastLabel.Width) / 2, ClientSize.Height - toastLabel.Height - 22);

            if (pageLayout.Controls[1] is TableLayoutPanel hero)
                hero.ColumnStyles[0].Width = scrollHost.ClientSize.Width < 1080 ? 52 : 48;

            if (pageLayout.Controls[2] is Panel categories)
                LayoutCategoryTiles(categories.ClientSize.Width);

            if (pageLayout.Controls[3] is Panel nearby)
                LayoutTaskCards(nearby.ClientSize.Width);
        }

        private Control BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Name = "headerLayout",
                ColumnCount = 3,
                RowCount = 1,
                BackColor = PageColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(0, 12, 0, 8)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            var brand = MakeBrand(false);
            brand.Anchor = AnchorStyles.Left;
            header.Controls.Add(brand, 0, 0);

            var nav = new FlowLayoutPanel
            {
                Name = "primaryNavigation",
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = PageColor,
                Padding = new Padding(20, 5, 0, 0)
            };
            nav.Controls.Add(MakeLinkButton("Browse Tasks", "Browse Tasks"));
            nav.Controls.Add(MakeLinkButton("How It Works", "How It Works"));
            nav.Controls.Add(MakeLinkButton("About", "About"));
            header.Controls.Add(nav, 1, 0);

            var accountActions = new FlowLayoutPanel
            {
                Name = "accountNavigation",
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = PageColor,
                Padding = new Padding(0, 1, 0, 0)
            };
            accountActions.Controls.Add(MakeButton("Sign Up", "SignUpButton", BlueColor, Color.White, 94, 36));
            accountActions.Controls.Add(MakeButton("Login", "LoginButton", Color.White, Color.FromArgb(11, 45, 99), 76, 36, Color.FromArgb(220, 230, 242)));
            header.Controls.Add(accountActions, 2, 0);
            return header;
        }

        private Control BuildHero()
        {
            var hero = new TableLayoutPanel
            {
                Name = "heroSection",
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
                BackColor = PageColor,
                Margin = Padding.Empty,
                Padding = new Padding(0, 8, 0, 10)
            };
            hero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            hero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            hero.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var copy = new FlowLayoutPanel
            {
                Name = "heroCopy",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = PageColor,
                Padding = new Padding(6, 28, 12, 0)
            };
            var eyebrow = MakeLabel("●  LOCAL HELP, RIGHT IN YOUR COMMUNITY", 10, BlueColor, true);
            eyebrow.Margin = new Padding(0, 4, 0, 8);
            copy.Controls.Add(eyebrow);

            var headline = MakeLabel("Find a task.\nGet help.\nEarn locally.", 38, NavyColor, true);
            headline.Name = "heroHeadline";
            headline.AutoSize = true;
            headline.MaximumSize = new Size(540, 200);
            headline.Margin = new Padding(0, 0, 0, 8);
            copy.Controls.Add(headline);

            var lead = MakeLabel("TaskTap connects people in your community for small tasks like cleaning, tutoring, shopping, repairs, and pet care.", 11, MutedColor);
            lead.MaximumSize = new Size(530, 50);
            lead.Margin = new Padding(0, 0, 0, 16);
            copy.Controls.Add(lead);

            var search = BuildSearchControls();
            search.Margin = new Padding(0, 0, 0, 12);
            copy.Controls.Add(search);

            var trustLine = MakeLabel("✓  Community-based     ✓  Local opportunities     ✓  Built for short-term tasks", 8, Color.FromArgb(108, 126, 151), true);
            trustLine.MaximumSize = new Size(570, 32);
            copy.Controls.Add(trustLine);
            hero.Controls.Add(copy, 0, 0);

            var illustrationPanel = new Panel
            {
                Name = "heroIllustrationPanel",
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 14, 0, 12),
                BackColor = Color.FromArgb(226, 241, 255),
                Padding = new Padding(8)
            };
            illustrationPanel.Paint += (_, e) =>
            {
                using var path = RoundedRectangle(illustrationPanel.ClientRectangle, 30);
                using var brush = new SolidBrush(Color.FromArgb(226, 241, 255));
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            };
            var picture = new PictureBox
            {
                Name = "heroPeoplePicture",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(226, 241, 255),
                ImageLocation = Path.Combine(AppContext.BaseDirectory, "assets", "hero-people.png")
            };
            illustrationPanel.Controls.Add(picture);
            hero.Controls.Add(illustrationPanel, 1, 0);
            return hero;
        }

        private Control BuildSearchControls()
        {
            var searchPanel = new TableLayoutPanel
            {
                Name = "taskSearchPanel",
                ColumnCount = 3,
                RowCount = 1,
                Width = 560,
                Height = 43,
                BackColor = Color.White,
                Margin = Padding.Empty,
                Padding = new Padding(5),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            searchTextBox = new TextBox
            {
                Name = "searchTasksTextBox",
                PlaceholderText = "Search tasks...",
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(7, 7, 4, 4)
            };
            barangayComboBox = new ComboBox
            {
                Name = "barangayFilterComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(4, 4, 4, 4)
            };
            barangayComboBox.Items.Add("Barangay");
            foreach (var name in DemoDataStore.ActiveBarangayNames())
                barangayComboBox.Items.Add(name.Replace("Brgy. ", string.Empty));
            barangayComboBox.SelectedIndex = 0;
            searchPanel.Controls.Add(searchTextBox, 0, 0);
            searchPanel.Controls.Add(barangayComboBox, 1, 0);
            searchTextBox.TextChanged += (_, _) => ApplyTaskFilters();
            var searchButton = MakeButton("⌕", "SearchTasksButton", BlueColor, Color.White, 50, 31);
            searchButton.Click += (_, _) =>
            {
                if (!string.IsNullOrWhiteSpace(searchTextBox.Text))
                    ShowToast($"Showing results for “{searchTextBox.Text.Trim()}”.");
                scrollHost.ScrollControlIntoView(pageLayout.Controls[3]);
                ApplyTaskFilters();
            };
            searchPanel.Controls.Add(searchButton, 2, 0);
            return searchPanel;
        }

        private Control BuildCategories()
        {
            var section = new Panel { Name = "popularCategoriesSection", Dock = DockStyle.Fill, BackColor = PageColor, Padding = new Padding(0, 5, 0, 0) };
            var kicker = MakeLabel("DISCOVER OPPORTUNITIES", 8, BlueColor, true);
            kicker.Location = new Point(0, 0);
            section.Controls.Add(kicker);
            var heading = MakeLabel("Popular categories", 20, NavyColor, true);
            heading.Location = new Point(0, 19);
            section.Controls.Add(heading);
            var viewAll = MakeLinkButton("View all  →", "ViewAllCategoriesButton");
            viewAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            viewAll.Location = new Point(980, 15);
            viewAll.Click += (_, _) => SelectAllCategories();
            section.Controls.Add(viewAll);

            categorySection = section;
            RefreshCategoryTiles();
            viewAll.Click += (_, _) => SelectAllCategories();
            return section;
        }

        /// <summary>Rebuilds the category tiles from the shared catalog (admin edits show here).</summary>
        private void RefreshCategoryTiles()
        {
            if (categorySection is null)
                return;

            foreach (var tile in categoryTiles)
                categorySection.Controls.Remove(tile.Panel);
            categoryTiles.Clear();

            foreach (var category in DemoDataStore.Categories
                         .Where(category => category.Active)
                         .OrderBy(category => category.SortOrder)
                         .Take(6))
            {
                var tile = CreateCategoryTile(category.Name, category.Icon, category.Color, category.Accent);
                categorySection.Controls.Add(tile.Panel);
                categoryTiles.Add(tile);
            }

            UpdateCategorySelection();
            LayoutCategoryTiles(Math.Max(1, categorySection.ClientSize.Width));
        }

        private CategoryTile CreateCategoryTile(string title, string icon, Color tint, Color accent)
        {
            var tile = new Panel
            {
                Name = $"{title.Replace(" ", string.Empty)}CategoryTile",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 96,
                Cursor = Cursors.Hand,
                Tag = title
            };
            var iconBox = new Label
            {
                Name = $"{title.Replace(" ", string.Empty)}CategoryIcon",
                Text = icon,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = accent,
                BackColor = tint,
                Size = new Size(38, 38),
                Location = new Point(0, 12)
            };
            var label = MakeLabel(title, 9, Color.FromArgb(70, 90, 117), true);
            label.Name = $"{title.Replace(" ", string.Empty)}CategoryLabel";
            label.TextAlign = ContentAlignment.MiddleCenter;
            tile.Controls.Add(iconBox);
            tile.Controls.Add(label);
            tile.Resize += (_, _) =>
            {
                iconBox.Location = new Point((tile.Width - iconBox.Width) / 2, 11);
                label.Location = new Point(3, 57);
                label.Size = new Size(tile.Width - 6, 24);
            };
            tile.Click += (_, _) => SelectCategory(title);
            iconBox.Click += (_, _) => SelectCategory(title);
            label.Click += (_, _) => SelectCategory(title);
            return new CategoryTile(title, tile);
        }

        private void LayoutCategoryTiles(int width)
        {
            if (categoryTiles.Count == 0)
                return;
            const int gap = 12;
            var tileWidth = Math.Max(110, (width - gap * 5) / 6);
            for (var index = 0; index < categoryTiles.Count; index++)
            {
                var tile = categoryTiles[index].Panel;
                tile.Size = new Size(tileWidth, 96);
                tile.Location = new Point(index * (tileWidth + gap), 75);
            }
        }

        private Control BuildNearbyTasks()
        {
            var section = new Panel { Name = "availableTasksSection", Dock = DockStyle.Fill, BackColor = PageColor, Padding = new Padding(0, 4, 0, 0) };
            var kicker = MakeLabel("LOCAL TASKS", 8, BlueColor, true);
            kicker.Location = new Point(0, 0);
            section.Controls.Add(kicker);
            var heading = MakeLabel("Available Near You", 20, NavyColor, true);
            heading.Location = new Point(0, 18);
            section.Controls.Add(heading);
            var description = MakeLabel("Explore small jobs posted by people in your selected area.", 9, MutedColor);
            description.Location = new Point(0, 48);
            section.Controls.Add(description);
            var viewAll = MakeLinkButton("View all  →", "ViewAllTasksButton");
            viewAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            viewAll.Location = new Point(980, 23);
            viewAll.Click += (_, _) => ApplyTaskFilters();
            section.Controls.Add(viewAll);

            taskCardsLayout = new FlowLayoutPanel
            {
                Name = "taskCardsLayout",
                Location = new Point(0, 80),
                Height = 290,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                BackColor = PageColor,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            section.Controls.Add(taskCardsLayout);
            foreach (var task in DemoDataStore.AvailableTasks.Take(4))
                taskCards.Add(CreateTaskCard(task.Title, task.Category, task.Icon, $"₱{task.Price:N0}", task.Barangay, task.Deadline, task.Description, task.IconBack));
            foreach (var card in taskCards)
                taskCardsLayout.Controls.Add(card.Panel);

            noResultsLabel = MakeLabel("No tasks found. Try another search or choose a different category.", 10, MutedColor);
            noResultsLabel.Name = "noTasksFoundLabel";
            noResultsLabel.Location = new Point(0, 373);
            noResultsLabel.Visible = false;
            section.Controls.Add(noResultsLabel);
            var resetButton = MakeButton("Reset filters", "ResetTaskFiltersButton", Color.White, Color.FromArgb(13, 63, 134), 112, 30, Color.FromArgb(189, 208, 230));
            resetButton.Location = new Point(0, 399);
            resetButton.Visible = false;
            resetButton.Click += (_, _) =>
            {
                searchTextBox.Clear();
                selectedCategory = null;
                UpdateCategorySelection();
                ApplyTaskFilters();
            };
            section.Controls.Add(resetButton);
            noResultsLabel.Tag = resetButton;
            LayoutTaskCards(Math.Max(1, section.ClientSize.Width));
            return section;
        }

        private TaskCard CreateTaskCard(string title, string category, string icon, string price, string location, string deadline, string description, Color thumbnailColor)
        {
            var card = new Panel
            {
                Name = $"{title.Replace(" ", string.Empty)}TaskCard",
                Height = 280,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 14, 12),
                Tag = title
            };
            var thumbnail = new Panel { Name = "taskThumbnail", BackColor = thumbnailColor, Dock = DockStyle.Top, Height = 105 };
            var categoryLabel = MakeLabel(category, 8, Color.FromArgb(68, 90, 118), true);
            categoryLabel.AutoSize = false;
            categoryLabel.BackColor = Color.FromArgb(245, 250, 255);
            categoryLabel.TextAlign = ContentAlignment.MiddleCenter;
            categoryLabel.Size = new Size(80, 22);
            categoryLabel.Location = new Point(10, 10);
            var iconLabel = MakeLabel(icon, 34, NavyColor);
            iconLabel.Dock = DockStyle.Fill;
            iconLabel.TextAlign = ContentAlignment.MiddleCenter;
            thumbnail.Controls.Add(iconLabel);
            thumbnail.Controls.Add(categoryLabel);

            var body = new Panel { Name = "taskCardContent", Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 9), BackColor = Color.White };
            var availability = MakeLabel("Available", 8, Color.FromArgb(20, 155, 105), true);
            availability.Location = new Point(12, 7);
            var heart = MakeButton("♡", $"Save{title.Replace(" ", string.Empty)}Button", Color.White, Color.FromArgb(155, 173, 192), 34, 26, flat: true);
            heart.Font = new Font("Segoe UI Symbol", 17F);
            heart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            heart.Location = new Point(190, 3);
            var taskTitle = MakeLabel(title, 11, NavyColor, true);
            taskTitle.Location = new Point(12, 34);
            taskTitle.MaximumSize = new Size(220, 22);
            var taskMeta = MakeLabel($"{price}   •   Brgy. {location}", 9, MutedColor);
            taskMeta.Location = new Point(12, 59);
            var deadlineLabel = MakeLabel($"◷  {deadline}", 8, MutedColor);
            deadlineLabel.Location = new Point(12, 79);
            var viewButton = MakeButton("View Task", $"View{title.Replace(" ", string.Empty)}Button", BlueColor, Color.White, 220, 32);
            viewButton.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            viewButton.Location = new Point(12, 105);
            body.Controls.Add(availability);
            body.Controls.Add(heart);
            body.Controls.Add(taskTitle);
            body.Controls.Add(taskMeta);
            body.Controls.Add(deadlineLabel);
            body.Controls.Add(viewButton);
            card.Controls.Add(body);
            card.Controls.Add(thumbnail);
            return new TaskCard(title, category, price, location, deadline, description, icon, thumbnailColor, card, heart, viewButton);
        }

        private void LayoutTaskCards(int width)
        {
            if (taskCardsLayout is null || taskCards.Count == 0)
                return;
            var gap = 14;
            var cardWidth = Math.Max(205, (width - gap * 4) / 4);
            foreach (var card in taskCards)
            {
                card.Panel.Width = cardWidth;
                if (card.Panel.Controls.Find("taskCardContent", false).FirstOrDefault() is Panel body)
                {
                    foreach (Control child in body.Controls)
                    {
                        if (child.Name.StartsWith("View", StringComparison.Ordinal))
                        {
                            child.Width = cardWidth - 26;
                            child.Location = new Point(12, 105);
                        }
                        else if (child.Name.StartsWith("Save", StringComparison.Ordinal))
                        {
                            child.Location = new Point(cardWidth - 44, 3);
                        }
                    }
                }
                card.SaveButton.Click -= ToggleTaskSaved;
                card.SaveButton.Click += ToggleTaskSaved;
                card.ViewButton.Click -= ShowTaskPreview;
                card.ViewButton.Click += ShowTaskPreview;
            }
            taskCardsLayout.Width = width;
            ApplyTaskFilters();
        }

        private Control BuildTrustSection()
        {
            var section = new Panel
            {
                Name = "communityTrustSection",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(9, 43, 95),
                Padding = new Padding(30, 22, 30, 18)
            };
            var heading = MakeLabel("COMMUNITY & TRUST\nDesigned for local, organized interactions.", 17, Color.White, true);
            heading.Location = new Point(20, 25);
            heading.Size = new Size(360, 84);
            section.Controls.Add(heading);
            var features = new FlowLayoutPanel
            {
                Name = "trustFeaturesLayout",
                Dock = DockStyle.Right,
                Width = 620,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(9, 43, 95),
                Padding = new Padding(0, 28, 0, 0)
            };
            features.Controls.Add(MakeTrustFeature("Profiles & ratings", "Build accountability through task history and reviews."));
            features.Controls.Add(MakeTrustFeature("In-app messaging", "Keep task conversations inside the platform."));
            features.Controls.Add(MakeTrustFeature("Approximate locations", "Discover tasks without live GPS sharing."));
            section.Controls.Add(features);
            return section;
        }

        private Control MakeTrustFeature(string title, string description)
        {
            var panel = new Panel { Width = 190, Height = 85, BackColor = Color.FromArgb(9, 43, 95), Margin = new Padding(0, 0, 12, 0) };
            var check = MakeLabel("✓", 12, Color.FromArgb(107, 211, 169), true);
            check.BackColor = Color.FromArgb(18, 63, 123);
            check.TextAlign = ContentAlignment.MiddleCenter;
            check.Size = new Size(26, 26);
            check.Location = new Point(0, 4);
            var titleLabel = MakeLabel(title, 9, Color.White, true);
            titleLabel.Location = new Point(34, 3);
            var descriptionLabel = MakeLabel(description, 8, Color.FromArgb(184, 202, 225));
            descriptionLabel.Location = new Point(34, 23);
            descriptionLabel.MaximumSize = new Size(145, 50);
            panel.Controls.Add(check);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(descriptionLabel);
            return panel;
        }

        private Control BuildCallToAction()
        {
            var section = new Panel { Name = "communityCallToActionSection", Dock = DockStyle.Fill, BackColor = PageColor };
            var content = new Panel { Name = "callToActionContent", Width = 760, Height = 175, BackColor = PageColor };
            var kicker = MakeLabel("READY TO TAP INTO YOUR COMMUNITY?", 8, BlueColor, true);
            kicker.TextAlign = ContentAlignment.MiddleCenter;
            kicker.Dock = DockStyle.Top;
            kicker.Height = 23;
            var heading = MakeLabel("Find help. Find work. Start local.", 28, NavyColor, true);
            heading.TextAlign = ContentAlignment.MiddleCenter;
            heading.Dock = DockStyle.Top;
            heading.Height = 48;
            var lead = MakeLabel("Join TaskTap and make small tasks easier to discover, request, and manage.", 10, MutedColor);
            lead.TextAlign = ContentAlignment.MiddleCenter;
            lead.Dock = DockStyle.Top;
            lead.Height = 32;
            var actions = new FlowLayoutPanel
            {
                Name = "callToActionButtons",
                Dock = DockStyle.Top,
                Height = 55,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = PageColor,
                Padding = new Padding(210, 7, 0, 0)
            };
            actions.Controls.Add(MakeButton("Create an Account", "CreateAccountButton", BlueColor, Color.White, 158, 40));
            actions.Controls.Add(MakeButton("Browse Tasks", "BrowseTasksButton", Color.White, Color.FromArgb(11, 45, 99), 132, 40, Color.FromArgb(220, 230, 242)));
            content.Controls.Add(actions);
            content.Controls.Add(lead);
            content.Controls.Add(heading);
            content.Controls.Add(kicker);
            section.Controls.Add(content);
            section.Resize += (_, _) => content.Location = new Point((section.Width - content.Width) / 2, 8);
            return section;
        }

        private Control BuildFooter()
        {
            var footer = new Panel
            {
                Name = "pageFooter",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(7, 31, 67),
                Padding = new Padding(28, 24, 28, 12)
            };
            var brand = MakeBrand(true);
            brand.Location = new Point(20, 20);
            footer.Controls.Add(brand);
            var slogan = MakeLabel("Local tasks. Real people.\nStronger communities.", 9, Color.FromArgb(129, 151, 179));
            slogan.Location = new Point(20, 55);
            footer.Controls.Add(slogan);

            var linkGroups = new FlowLayoutPanel
            {
                Name = "footerLinkGroups",
                Dock = DockStyle.Right,
                Width = 580,
                Height = 115,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(7, 31, 67),
                Padding = new Padding(0, 5, 0, 0)
            };
            linkGroups.Controls.Add(MakeFooterGroup("EXPLORE", "Browse Tasks", "How It Works", "About"));
            linkGroups.Controls.Add(MakeFooterGroup("ACCOUNT", "Login", "Sign Up"));
            linkGroups.Controls.Add(MakeFooterGroup("SUPPORT", "Help / FAQ", "Community Guidelines", "Terms of Service", "Privacy Policy"));
            footer.Controls.Add(linkGroups);
            var bottomLine = new Panel { Name = "footerDivider", Height = 1, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(26, 59, 101) };
            footer.Controls.Add(bottomLine);
            var copyright = MakeLabel("© 2026 TaskTap", 8, Color.FromArgb(114, 138, 167));
            copyright.Location = new Point(20, 148);
            footer.Controls.Add(copyright);
            var footerDescription = MakeLabel("Local Task and Short-Term Job Management System", 8, Color.FromArgb(114, 138, 167));
            footerDescription.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            footerDescription.Location = new Point(footer.Width - 330, 148);
            footer.Controls.Add(footerDescription);
            footer.Resize += (_, _) => footerDescription.Location = new Point(footer.ClientSize.Width - footerDescription.Width - 20, 148);
            return footer;
        }

        private void SelectCategory(string category)
        {
            selectedCategory = category;
            searchTextBox.Clear();
            UpdateCategorySelection();
            ApplyTaskFilters();
            scrollHost.ScrollControlIntoView(pageLayout.Controls[3]);
        }

        private void SelectAllCategories()
        {
            selectedCategory = null;
            UpdateCategorySelection();
            ApplyTaskFilters();
            ShowToast("Showing more local task categories.");
        }

        private void UpdateCategorySelection()
        {
            foreach (var tile in categoryTiles)
            {
                var selected = tile.Title == selectedCategory;
                tile.Panel.BorderStyle = selected ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
                tile.Panel.BackColor = selected ? Color.FromArgb(244, 249, 255) : Color.White;
            }
        }

        private void ApplyTaskFilters()
        {
            if (searchTextBox is null || taskCardsLayout is null)
                return;

            var query = searchTextBox.Text.Trim();
            var visibleCount = 0;
            foreach (var card in taskCards)
            {
                var matchesText = query.Length == 0 || card.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || card.Category.Contains(query, StringComparison.OrdinalIgnoreCase);
                var matchesCategory = selectedCategory is null || string.Equals(selectedCategory, card.Category, StringComparison.Ordinal);
                card.Panel.Visible = matchesText && matchesCategory;
                if (card.Panel.Visible)
                    visibleCount++;
            }

            noResultsLabel.Visible = visibleCount == 0;
            if (noResultsLabel.Tag is Control resetButton)
                resetButton.Visible = visibleCount == 0;
        }

        private void ToggleTaskSaved(object? sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            var card = taskCards.FirstOrDefault(candidate => ReferenceEquals(candidate.SaveButton, button));
            if (card is null)
                return;

            var priceValue = int.TryParse(card.Price.Replace("₱", string.Empty).Replace(",", string.Empty), out var parsed) ? parsed : 0;
            DemoDataStore.ToggleSaved(new DemoTask(card.Title, card.Category, priceValue, card.Location, 3, card.Deadline, card.Icon, card.Description, card.IconBack, new Point(50, 50)));

            var saved = DemoDataStore.IsSaved(card.Title);
            button.Text = saved ? "♥" : "♡";
            button.ForeColor = saved ? Color.FromArgb(237, 104, 120) : Color.FromArgb(155, 173, 192);
            ShowToast(saved ? "Task saved to your list. View it from Saved Tasks." : "Task removed from saved tasks.");
        }

        private void ShowTaskPreview(object? sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            var task = taskCards.FirstOrDefault(card => ReferenceEquals(card.ViewButton, button));
            if (task is null)
                return;

            using var preview = new Form
            {
                Name = "taskPreviewDialog",
                Text = "Task Preview",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(600, 340),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            var closeButton = MakeButton("×", "CloseTaskPreviewButton", Color.FromArgb(241, 245, 249), Color.FromArgb(96, 112, 137), 34, 34, flat: true);
            closeButton.Font = new Font("Segoe UI", 18F);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Location = new Point(preview.ClientSize.Width - 48, 12);
            closeButton.Click += (_, _) => preview.Close();
            preview.Controls.Add(closeButton);

            var kicker = MakeLabel("TASK PREVIEW", 8, BlueColor, true);
            kicker.Location = new Point(32, 28);
            preview.Controls.Add(kicker);
            var heading = MakeLabel(task.Title, 23, NavyColor, true);
            heading.Name = "taskPreviewTitle";
            heading.Location = new Point(32, 52);
            preview.Controls.Add(heading);

            var details = new TableLayoutPanel
            {
                Name = "taskPreviewDetails",
                ColumnCount = 3,
                RowCount = 1,
                Location = new Point(32, 105),
                Size = new Size(536, 72),
                BackColor = PageColor,
                Padding = new Padding(8)
            };
            for (var index = 0; index < 3; index++)
                details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            details.Controls.Add(MakePreviewDetail("Compensation", task.Price), 0, 0);
            details.Controls.Add(MakePreviewDetail("Location", $"Brgy. {task.Location}"), 1, 0);
            details.Controls.Add(MakePreviewDetail("Deadline", task.Deadline), 2, 0);
            preview.Controls.Add(details);

            var description = MakeLabel(task.Description, 9, MutedColor);
            description.Location = new Point(34, 193);
            description.MaximumSize = new Size(530, 54);
            preview.Controls.Add(description);

            var requestButton = MakeButton("Login to Request", "LoginToRequestButton", BlueColor, Color.White, 536, 40);
            requestButton.Location = new Point(32, 275);
            requestButton.Click += (_, _) => preview.DialogResult = DialogResult.OK;
            preview.Controls.Add(requestButton);
            preview.ShowDialog(this);
            if (preview.DialogResult == DialogResult.OK)
            {
                var price = int.TryParse(task.Price.Replace("₱", "").Replace(",", "").Trim(), out var parsed) ? parsed : 0;
                AppSession.PendingTaskAfterLogin = new DemoTask(
                    task.Title, task.Category, price, task.Location, 2, task.Deadline, "📌", task.Description,
                    Color.FromArgb(232, 241, 255), new Point(50, 50));
                NavigateToAuthentication(false);
            }
        }

        private static Control MakePreviewDetail(string title, string value)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(246, 249, 252), Margin = new Padding(3) };
            var titleLabel = MakeLabel(title, 8, Color.FromArgb(132, 149, 169));
            titleLabel.Location = new Point(8, 8);
            var valueLabel = MakeLabel(value, 10, Color.FromArgb(13, 63, 134), true);
            valueLabel.Location = new Point(8, 32);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);
            return panel;
        }

        private void ShowToast(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastLabel.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private void WireNavigationActions()
        {
            foreach (var button in FindControls<Button>(this))
            {
                if (button.Name == "LoginButton" || button.Name.EndsWith("LoginButton", StringComparison.Ordinal))
                    button.Click += (_, _) => NavigateToAuthentication(false);
                else if (button.Name is "SignUpButton" or "CreateAccountButton" || button.Name.EndsWith("SignUpButton", StringComparison.Ordinal))
                    button.Click += (_, _) => NavigateToAuthentication(true);
                else if (button.Name.Contains("BrowseTasks", StringComparison.Ordinal) || button.Text == "Browse Tasks")
                    button.Click += (_, _) => NavigateToBrowseTasks();
                else if ((button.Name.StartsWith("Footer", StringComparison.Ordinal) || button.Name is "How It Works" or "About") &&
                         button.Text is "How It Works" or "About" or "Help / FAQ" or "Community Guidelines" or "Terms of Service" or "Privacy Policy")
                    button.Click += (_, _) => NavigateToPublicInfo(button.Text);
            }
        }

        private void NavigateToBrowseTasks()
        {
            var browsePage = new BrowseTasksForm();
            browsePage.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };

            Hide();
            browsePage.Show();
        }

        private void NavigateToPublicInfo(string page)
        {
            Form informationPage = page switch
            {
                "How It Works" => new HowItWorksForm(),
                "About" => new AboutForm(),
                "Help / FAQ" => new InfoArticleForm(InfoArticleCatalog.HelpFaq),
                "Community Guidelines" => new InfoArticleForm(InfoArticleCatalog.CommunityGuidelines),
                "Terms of Service" => new InfoArticleForm(InfoArticleCatalog.Terms),
                "Privacy Policy" => new InfoArticleForm(InfoArticleCatalog.Privacy),
                _ => new InfoArticleForm(InfoArticleCatalog.HelpFaq)
            };
            informationPage.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };
            Hide();
            informationPage.Show();
        }

        private void NavigateToAuthentication(bool registerInitially)
        {
            var authenticationPage = new Form2(registerInitially);
            authenticationPage.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };

            Hide();
            authenticationPage.Show();
        }

        private static IEnumerable<TControl> FindControls<TControl>(Control parent) where TControl : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TControl match)
                    yield return match;
                foreach (var child in FindControls<TControl>(control))
                    yield return child;
            }
        }

        private Control MakeFooterGroup(string title, params string[] links)
        {
            var group = new FlowLayoutPanel
            {
                Name = $"footer{title.Replace(" ", string.Empty)}Links",
                Width = 175,
                Height = 108,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(7, 31, 67),
                Margin = new Padding(0, 0, 10, 0)
            };
            var heading = MakeLabel(title, 8, Color.White, true);
            heading.Margin = new Padding(0, 0, 0, 4);
            group.Controls.Add(heading);
            foreach (var link in links)
            {
                var button = MakeLinkButton(link, $"Footer{title.Replace(" ", string.Empty)}{link.Replace(" ", string.Empty)}Button", Color.FromArgb(158, 177, 202));
                button.BackColor = group.BackColor;
                button.Margin = new Padding(0, 1, 0, 1);
                group.Controls.Add(button);
            }
            return group;
        }

        private Control MakeBrand(bool dark)
        {
            var logo = new TaskTapLogo
            {
                Name = dark ? "footerBrand" : "taskTapBrand",
                TextColor = dark ? Color.White : NavyColor,
                BackColor = dark ? Color.FromArgb(7, 31, 67) : PageColor,
                Size = new Size(160, 34),
                Cursor = Cursors.Hand
            };
            logo.Click += (_, _) => AppNavigation.GoToLanding();
            return logo;
        }

        private Button MakeButton(string text, string name, Color backColor, Color foreColor, int width, int height, Color? borderColor = null, bool flat = false)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(width, height),
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(5, 3, 5, 3)
            };
            button.FlatAppearance.BorderSize = flat ? 0 : 1;
            button.FlatAppearance.BorderColor = borderColor ?? backColor;
            return button;
        }

        private Button MakeLinkButton(string text, string name, Color? color = null)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = PageColor,
                ForeColor = color ?? Color.FromArgb(51, 71, 101),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(4, 1, 4, 1)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label MakeLabel(string text, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = color,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                Margin = Padding.Empty
            };
        }

        private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private sealed record CategoryTile(string Title, Panel Panel);

        private sealed record TaskCard(string Title, string Category, string Price, string Location, string Deadline, string Description, string Icon, Color IconBack, Panel Panel, Button SaveButton, Button ViewButton);
    }
}
