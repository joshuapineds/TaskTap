using System.Drawing.Drawing2D;

namespace TaskTap
{
    public sealed class BrowseTasksForm : Form
    {
        private static readonly Color PageColor = Color.FromArgb(247, 251, 255);
        private static readonly Color NavyColor = Color.FromArgb(8, 38, 87);
        private static readonly Color BlueColor = Color.FromArgb(8, 117, 232);
        private static readonly Color MutedColor = Color.FromArgb(113, 131, 155);
        private readonly Panel scrollHost = new();
        private readonly Panel pageCanvas = new();
        private readonly Panel pageHeader = new();
        private readonly Panel browseHeading = new();
        private readonly Panel searchPanel = new();
        private readonly Panel filtersPanel = new();
        private readonly Panel resultsPanel = new();
        private readonly Panel contentSplit = new();
        private readonly FlowLayoutPanel taskListPanel = new();
        private readonly Panel mapPanel = new();
        private readonly Panel emptyStatePanel = new();
        private readonly Panel footerPanel = new();
        private readonly Panel mapContainer = new();
        private readonly List<BrowseTasksForm.BrowseTaskCard> taskCards = [];
        private readonly List<CheckBox> categoryChecks = [];
        private TextBox searchTextBox = null!;
        private ComboBox topBarangayComboBox = null!;
        private ComboBox filterBarangayComboBox = null!;
        private ComboBox deadlineComboBox = null!;
        private ComboBox sortComboBox = null!;
        private TextBox minimumPriceTextBox = null!;
        private TextBox maximumPriceTextBox = null!;
        private Label resultCountLabel = null!;
        private Label toastLabel = null!;
        private System.Windows.Forms.Timer toastTimer = null!;
        private Button listViewButton = null!;
        private Button gridViewButton = null!;
        private Button zoomInButton = null!;
        private Button zoomOutButton = null!;
        private bool gridView;
        private int mapZoom = 1;

        /// <summary>Open tasks from the database, shown to guests as a preview.</summary>
        private static BrowseTasksForm.TaskRecord[] TaskRecords => DemoDataStore.AvailableTasks
            .Select(task => new BrowseTasksForm.TaskRecord(
                task.Title, task.Category, task.Price, task.Barangay, task.Deadline, task.Days, task.Icon,
                task.Description.Length > 90 ? task.Description[..87] + "..." : task.Description,
                task.Description, task.IconBack))
            .ToArray();

        public BrowseTasksForm()
        {
            BuildPage();
        }

        private void BuildPage()
        {
            Text = "TaskTap — Browse Tasks";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 780);
            ClientSize = new Size(1360, 900);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            scrollHost.Name = "browseScrollHost";
            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = PageColor;
            scrollHost.Resize += (_, _) => LayoutPage();
            Controls.Add(scrollHost);

            pageCanvas.Name = "browsePageCanvas";
            pageCanvas.Width = 1220;
            pageCanvas.BackColor = PageColor;
            scrollHost.Controls.Add(pageCanvas);

            pageHeader.Name = "browsePageHeader";
            pageHeader.BackColor = Color.White;
            pageHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 237, 245));
                e.Graphics.DrawLine(pen, 0, pageHeader.Height - 1, pageHeader.Width, pageHeader.Height - 1);
            };
            BuildPageHeader();

            browseHeading.Name = "browseHeadingSection";
            browseHeading.BackColor = PageColor;
            BuildBrowseHeading();

            searchPanel.Name = "browseSearchPanel";
            searchPanel.BackColor = Color.White;
            searchPanel.BorderStyle = BorderStyle.FixedSingle;
            BuildSearchPanel();

            filtersPanel.Name = "browseFiltersPanel";
            filtersPanel.BackColor = Color.White;
            filtersPanel.BorderStyle = BorderStyle.FixedSingle;
            BuildFiltersPanel();

            resultsPanel.Name = "browseResultsPanel";
            resultsPanel.BackColor = PageColor;
            BuildResultsPanel();

            contentSplit.Name = "browseContentSplit";
            contentSplit.BackColor = PageColor;
            resultsPanel.Controls.Add(contentSplit);

            taskListPanel.Name = "browseTaskList";
            taskListPanel.FlowDirection = FlowDirection.TopDown;
            taskListPanel.WrapContents = false;
            taskListPanel.AutoScroll = false;
            taskListPanel.Margin = Padding.Empty;
            taskListPanel.Padding = Padding.Empty;
            taskListPanel.BackColor = PageColor;
            contentSplit.Controls.Add(taskListPanel);
            foreach (var record in TaskRecords)
            {
                var card = BuildTaskCard(record);
                taskCards.Add(card);
                taskListPanel.Controls.Add(card.CardPanel);
            }

            mapPanel.Name = "approximateLocationMap";
            mapPanel.BackColor = Color.FromArgb(233, 242, 231);
            mapPanel.BorderStyle = BorderStyle.FixedSingle;
            mapPanel.Paint += PaintMap;
            BuildMapControls();
            mapContainer.Name = "mapContainer";
            mapContainer.BackColor = Color.Transparent;
            mapContainer.Controls.Add(mapPanel);
            contentSplit.Controls.Add(mapContainer);

            BuildEmptyState();
            BuildFooter();

            pageCanvas.Controls.Add(pageHeader);
            pageCanvas.Controls.Add(browseHeading);
            pageCanvas.Controls.Add(searchPanel);
            pageCanvas.Controls.Add(filtersPanel);
            pageCanvas.Controls.Add(resultsPanel);

            toastLabel = new Label
            {
                Name = "browseToast",
                AutoSize = false,
                Size = new Size(460, 40),
                BackColor = Color.FromArgb(9, 44, 97),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false,
                Anchor = AnchorStyles.Bottom
            };
            Controls.Add(toastLabel);
            toastLabel.BringToFront();
            toastTimer = new System.Windows.Forms.Timer { Interval = 2400 };
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toastLabel.Visible = false;
            };

            LayoutPage();
            ApplyFilters();
            WireInteractions();
        }

        private void BuildPageHeader()
        {
            var brand = MakeBrand();
            brand.Location = new Point(0, 13);
            pageHeader.Controls.Add(brand);

            var navigation = new FlowLayoutPanel
            {
                Name = "browseMainNavigation",
                Location = new Point(175, 14),
                Size = new Size(400, 36),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White
            };
            navigation.Controls.Add(MakeLinkButton("Browse Tasks", "BrowseTasksNavButton", BlueColor));
            navigation.Controls.Add(MakeLinkButton("How It Works", "HowItWorksNavButton", MutedColor));
            navigation.Controls.Add(MakeLinkButton("About", "AboutNavButton", MutedColor));
            pageHeader.Controls.Add(navigation);

            var accountButtons = new FlowLayoutPanel
            {
                Name = "browseAccountActions",
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Size = new Size(180, 40),
                Location = new Point(1020, 9)
            };
            accountButtons.Controls.Add(MakeButton("Sign Up", "BrowseSignUpButton", BlueColor, Color.White, 80, 34));
            accountButtons.Controls.Add(MakeButton("Login", "BrowseLoginButton", Color.White, NavyColor, 70, 34, Color.FromArgb(203, 217, 231)));
            pageHeader.Controls.Add(accountButtons);
        }

        private void BuildBrowseHeading()
        {
            var kicker = MakeLabel("LOCAL TASKS", 8, BlueColor, true);
            kicker.Location = new Point(0, 0);
            browseHeading.Controls.Add(kicker);
            var title = MakeLabel("Find a task near you.", 29, NavyColor, true);
            title.Name = "browsePageTitle";
            title.Location = new Point(0, 14);
            browseHeading.Controls.Add(title);
            var description = MakeLabel("Search available short-term jobs by category, barangay, price, and deadline.", 10, MutedColor);
            description.Location = new Point(0, 60);
            browseHeading.Controls.Add(description);
            var postTaskButton = MakeButton("Post a Task  +", "PostTaskButton", BlueColor, Color.White, 132, 36);
            postTaskButton.Location = new Point(0, 31);
            postTaskButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseHeading.Controls.Add(postTaskButton);
        }

        private void BuildSearchPanel()
        {
            searchTextBox = new TextBox
            {
                Name = "browseSearchTextBox",
                PlaceholderText = "Search tasks, e.g. cleaning, tutor, repairs",
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            searchPanel.Controls.Add(searchTextBox);

            var barangayGroup = new Panel
            {
                Name = "topBarangaySelectGroup",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var barangayLabel = MakeLabel("Barangay", 7, MutedColor);
            barangayLabel.Name = "topBarangayLabel";
            barangayLabel.Location = new Point(8, 3);
            barangayGroup.Controls.Add(barangayLabel);
            topBarangayComboBox = MakeBarangayComboBox("topBarangayComboBox");
            topBarangayComboBox.Location = new Point(5, 18);
            topBarangayComboBox.Size = new Size(180, 25);
            barangayGroup.Controls.Add(topBarangayComboBox);
            searchPanel.Controls.Add(barangayGroup);

            var searchButton = MakeButton("Search", "ApplyBrowseSearchButton", BlueColor, Color.White, 96, 42);
            searchPanel.Controls.Add(searchButton);
        }

        private void BuildFiltersPanel()
        {
            var filterTitle = MakeLabel("Filters", 11, NavyColor, true);
            filterTitle.Location = new Point(14, 13);
            filtersPanel.Controls.Add(filterTitle);
            var resetButton = MakeLinkButton("Reset", "ResetBrowseFiltersButton", BlueColor);
            resetButton.Location = new Point(144, 8);
            filtersPanel.Controls.Add(resetButton);
            var divider = new Panel { Name = "filterHeaderDivider", BackColor = Color.FromArgb(237, 241, 245), Height = 1 };
            filtersPanel.Controls.Add(divider);

            AddFilterHeading("Category", 50);
            var categories = new[] { ("Cleaning", "12"), ("Tutoring", "8"), ("Repairs", "6"), ("Shopping", "9"), ("Pet Care", "5"), ("Others", "4") };
            for (var index = 0; index < categories.Length; index++)
            {
                var (category, count) = categories[index];
                var check = new CheckBox
                {
                    Name = $"category{category.Replace(" ", string.Empty)}CheckBox",
                    Text = category,
                    AutoSize = false,
                    Size = new Size(138, 20),
                    Location = new Point(12, 68 + index * 23),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = MutedColor
                };
                var countLabel = MakeLabel(count, 7, Color.FromArgb(154, 170, 188));
                countLabel.Name = $"{category.Replace(" ", string.Empty)}CategoryCount";
                countLabel.Location = new Point(174, check.Top + 3);
                categoryChecks.Add(check);
                filtersPanel.Controls.Add(check);
                filtersPanel.Controls.Add(countLabel);
            }

            AddFilterHeading("Price Range", 218);
            minimumPriceTextBox = new TextBox { Name = "minimumPriceTextBox", PlaceholderText = "₱0", Font = new Font("Segoe UI", 8F), BorderStyle = BorderStyle.FixedSingle };
            maximumPriceTextBox = new TextBox { Name = "maximumPriceTextBox", PlaceholderText = "₱1,000+", Font = new Font("Segoe UI", 8F), BorderStyle = BorderStyle.FixedSingle };
            var toLabel = MakeLabel("to", 7, MutedColor);
            toLabel.Name = "priceRangeToLabel";
            filtersPanel.Controls.Add(minimumPriceTextBox);
            filtersPanel.Controls.Add(maximumPriceTextBox);
            filtersPanel.Controls.Add(toLabel);

            AddFilterHeading("Deadline", 271);
            deadlineComboBox = new ComboBox { Name = "deadlineFilterComboBox", DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8F), FlatStyle = FlatStyle.Flat };
            deadlineComboBox.Items.AddRange(["Any deadline", "Within 3 days", "Within 7 days"]);
            deadlineComboBox.SelectedIndex = 0;
            filtersPanel.Controls.Add(deadlineComboBox);

            AddFilterHeading("Barangay", 326);
            filterBarangayComboBox = MakeBarangayComboBox("filterBarangayComboBox");
            filtersPanel.Controls.Add(filterBarangayComboBox);

            var applyButton = MakeButton("Apply Filters", "ApplyBrowseFiltersButton", BlueColor, Color.White, 175, 36);
            filtersPanel.Controls.Add(applyButton);
        }

        private void BuildResultsPanel()
        {
            resultCountLabel = MakeLabel("6", 8, NavyColor, true);
            resultCountLabel.Name = "browseResultCount";
            resultsPanel.Controls.Add(resultCountLabel);
            var availableText = MakeLabel("tasks available", 8, MutedColor);
            availableText.Name = "browseAvailableText";
            resultsPanel.Controls.Add(availableText);

            sortComboBox = new ComboBox
            {
                Name = "sortTasksComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat
            };
            sortComboBox.Items.AddRange(["Recommended", "Price: Low to High", "Price: High to Low", "Deadline Soonest"]);
            sortComboBox.SelectedIndex = 0;
            resultsPanel.Controls.Add(sortComboBox);

            listViewButton = MakeButton("☷", "ListViewButton", Color.White, BlueColor, 30, 29, Color.FromArgb(169, 201, 235));
            listViewButton.Font = new Font("Segoe UI Symbol", 13F);
            gridViewButton = MakeButton("▦", "GridViewButton", Color.White, Color.FromArgb(139, 160, 182), 30, 29, Color.FromArgb(220, 230, 241));
            gridViewButton.Font = new Font("Segoe UI Symbol", 12F);
            resultsPanel.Controls.Add(listViewButton);
            resultsPanel.Controls.Add(gridViewButton);

            emptyStatePanel.Name = "browseNoMatchesPanel";
            emptyStatePanel.BackColor = Color.White;
            emptyStatePanel.BorderStyle = BorderStyle.FixedSingle;
            var emptyTitle = MakeLabel("No matching tasks", 17, NavyColor, true);
            emptyTitle.Name = "emptyTitle";
            emptyTitle.Location = new Point(0, 0);
            emptyStatePanel.Controls.Add(emptyTitle);
            var emptyDescription = MakeLabel("Try changing your search or filters.", 9, MutedColor);
            emptyDescription.Name = "emptyDescription";
            emptyDescription.Location = new Point(0, 35);
            emptyStatePanel.Controls.Add(emptyDescription);
            var emptyResetButton = MakeButton("Reset filters", "EmptyBrowseResetButton", Color.White, BlueColor, 120, 34, Color.FromArgb(190, 211, 233));
            emptyResetButton.Location = new Point(0, 62);
            emptyStatePanel.Controls.Add(emptyResetButton);
            emptyStatePanel.Visible = false;
            resultsPanel.Controls.Add(emptyStatePanel);
        }

        private BrowseTaskCard BuildTaskCard(TaskRecord record)
        {
            var card = new Panel
            {
                Name = $"browse{record.Title.Replace(" ", string.Empty)}Card",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8),
                Tag = record
            };
            var imagePanel = new Panel
            {
                Name = "taskThumbnail",
                BackColor = record.ThumbnailColor
            };
            var icon = MakeLabel(record.Icon, 32, NavyColor);
            icon.Name = "taskIcon";
            icon.TextAlign = ContentAlignment.MiddleCenter;
            imagePanel.Controls.Add(icon);
            card.Controls.Add(imagePanel);

            var body = new Panel { Name = "taskDescriptionPanel", BackColor = Color.White };
            var badge = MakeLabel("Available", 7, Color.FromArgb(17, 150, 106), true);
            badge.Name = "availabilityBadge";
            badge.BackColor = Color.FromArgb(231, 248, 240);
            badge.TextAlign = ContentAlignment.MiddleCenter;
            badge.Size = new Size(60, 18);
            body.Controls.Add(badge);
            var saveButton = MakeLinkButton("♡", $"Save{record.Title.Replace(" ", string.Empty)}TaskButton", Color.FromArgb(163, 178, 194));
            saveButton.Name = "saveTaskButton";
            saveButton.Font = new Font("Segoe UI Symbol", 16F);
            body.Controls.Add(saveButton);
            var title = MakeLabel(record.Title, 11, NavyColor, true);
            title.Name = "taskTitle";
            body.Controls.Add(title);
            var price = MakeLabel($"₱{record.Price}  •  {record.Category}", 8.5F, BlueColor, true);
            price.Name = "taskPriceCategory";
            body.Controls.Add(price);
            var description = MakeLabel(record.Summary, 8, MutedColor);
            description.Name = "taskSummary";
            body.Controls.Add(description);
            var meta = MakeLabel($"⌖ Brgy. {record.Barangay}     ◷ {record.Deadline}", 7, MutedColor);
            meta.Name = "taskMetadata";
            body.Controls.Add(meta);
            card.Controls.Add(body);

            var detailsButton = MakeButton("View Details", $"View{record.Title.Replace(" ", string.Empty)}DetailsButton", Color.White, BlueColor, 88, 30, Color.FromArgb(188, 210, 234));
            detailsButton.Name = "viewTaskDetailsButton";
            detailsButton.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            card.Controls.Add(detailsButton);
            return new BrowseTaskCard(record, card, imagePanel, icon, body, badge, saveButton, title, price, description, meta, detailsButton);
        }

        private void BuildMapControls()
        {
            var zoomPanel = new Panel { Name = "mapZoomControls", Size = new Size(32, 62), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            zoomInButton = MakeButton("+", "MapZoomInButton", Color.White, Color.FromArgb(92, 113, 138), 28, 29, flat: true);
            zoomOutButton = MakeButton("−", "MapZoomOutButton", Color.White, Color.FromArgb(92, 113, 138), 28, 29, flat: true);
            zoomInButton.Click += (_, _) => { mapZoom = Math.Min(6, mapZoom + 1); mapPanel.Invalidate(); };
            zoomOutButton.Click += (_, _) => { mapZoom = Math.Max(1, mapZoom - 1); mapPanel.Invalidate(); };
            zoomPanel.Controls.Add(zoomInButton);
            zoomOutButton.Location = new Point(0, 30);
            zoomPanel.Controls.Add(zoomOutButton);
            zoomPanel.Location = new Point(0, 0);
            mapPanel.Controls.Add(zoomPanel);

            var note = new Panel { Name = "approximateLocationNote", Size = new Size(190, 44), BackColor = Color.FromArgb(247, 251, 255), BorderStyle = BorderStyle.FixedSingle };
            var noteTitle = MakeLabel("Approximate locations", 7, NavyColor, true);
            noteTitle.Location = new Point(8, 5);
            note.Controls.Add(noteTitle);
            var noteText = MakeLabel("Exact user locations are not displayed.", 6.5F, MutedColor);
            noteText.Location = new Point(8, 21);
            note.Controls.Add(noteText);
            note.Location = new Point(0, 0);
            mapPanel.Controls.Add(note);
        }

        private void BuildEmptyState()
        {
            var resetButton = emptyStatePanel.Controls.Find("EmptyBrowseResetButton", false).FirstOrDefault();
            if (resetButton is not null)
                resetButton.Click += (_, _) => ResetFilters();
        }

        private void BuildFooter()
        {
            footerPanel.Name = "browseFooter";
            footerPanel.BackColor = Color.FromArgb(7, 31, 67);
            var brand = MakeLabel("TaskTap", 15, Color.White, true);
            brand.Location = new Point(0, 12);
            footerPanel.Controls.Add(brand);
            var tagline = MakeLabel("Local tasks. Real people. Stronger communities.", 7, Color.FromArgb(130, 152, 180));
            tagline.Location = new Point(0, 36);
            footerPanel.Controls.Add(tagline);
            var copyright = MakeLabel("© 2026 TaskTap", 7, Color.FromArgb(114, 138, 167));
            copyright.Name = "browseCopyright";
            footerPanel.Controls.Add(copyright);
            pageCanvas.Controls.Add(footerPanel);
        }

        private void LayoutPage()
        {
            if (scrollHost.ClientSize.Width <= 0 || toastLabel is null)
                return;

            var pageWidth = Math.Min(1220, Math.Max(900, scrollHost.ClientSize.Width - 44));
            pageCanvas.Width = pageWidth;
            pageCanvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - pageWidth) / 2), 0);

            var headerX = Math.Max(22, (pageWidth - 1220) / 2);
            pageHeader.Bounds = new Rectangle(0, 0, pageWidth, 56);
            var brand = pageHeader.Controls.Find("browseBrand", false).FirstOrDefault();
            if (brand is not null)
                brand.Location = new Point(headerX, 13);
            var navigation = pageHeader.Controls.Find("browseMainNavigation", false).FirstOrDefault();
            if (navigation is not null)
                navigation.Location = new Point(headerX + 172, 12);
            var accountActions = pageHeader.Controls.Find("browseAccountActions", false).FirstOrDefault();
            if (accountActions is not null)
                accountActions.Location = new Point(pageWidth - headerX - accountActions.Width, 8);

            var innerWidth = pageWidth - 50;
            browseHeading.Bounds = new Rectangle(25, 78, innerWidth, 84);
            var postTask = browseHeading.Controls.Find("PostTaskButton", false).FirstOrDefault();
            if (postTask is not null)
                postTask.Location = new Point(innerWidth - postTask.Width, 28);

            searchPanel.Bounds = new Rectangle(25, 174, innerWidth, 54);
            searchTextBox.Bounds = new Rectangle(8, 7, Math.Max(250, innerWidth - 390), 38);
            var barangayGroup = searchPanel.Controls.Find("topBarangaySelectGroup", false).FirstOrDefault();
            if (barangayGroup is not null)
            {
                barangayGroup.Bounds = new Rectangle(searchTextBox.Right + 8, 7, 190, 38);
                topBarangayComboBox.Bounds = new Rectangle(5, 15, 176, 21);
            }
            var searchButton = searchPanel.Controls.Find("ApplyBrowseSearchButton", false).FirstOrDefault();
            if (searchButton is not null)
                searchButton.Location = new Point(innerWidth - searchButton.Width - 8, 6);

            var contentY = 232;
            var filterWidth = 205;
            var columnGap = 22;
            var resultsWidth = innerWidth - filterWidth - columnGap;
            filtersPanel.Bounds = new Rectangle(25, contentY, filterWidth, 475);
            LayoutFilterControls(filterWidth);
            resultsPanel.Bounds = new Rectangle(25 + filterWidth + columnGap, contentY, resultsWidth, 1);

            resultCountLabel.Location = new Point(0, 8);
            var availableText = resultsPanel.Controls.Find("browseAvailableText", false).FirstOrDefault();
            if (availableText is not null)
                availableText.Location = new Point(resultCountLabel.Right + 4, 8);
            sortComboBox.Bounds = new Rectangle(resultsWidth - 150, 0, 116, 29);
            listViewButton.Location = new Point(resultsWidth - 66, 0);
            gridViewButton.Location = new Point(resultsWidth - 32, 0);

            var contentTop = 35;
            var isCompact = pageWidth < 1080;
            var mapWidth = isCompact ? resultsWidth : 330;
            var listWidth = isCompact ? resultsWidth : resultsWidth - mapWidth - 14;
            var cardWidth = gridView ? Math.Max(220, (listWidth - 10) / 2) : listWidth;
            var cardHeight = gridView ? 230 : 112;
            foreach (var card in taskCards)
                LayoutTaskCard(card, cardWidth, cardHeight, gridView);

            taskListPanel.Location = new Point(0, contentTop);
            var filteredCount = taskCards.Count(card => card.IsFilteredVisible);
            var listHeight = filteredCount == 0 ? 0 : gridView ? ((filteredCount + 1) / 2) * (cardHeight + 8) : filteredCount * (cardHeight + 8);
            taskListPanel.FlowDirection = gridView ? FlowDirection.LeftToRight : FlowDirection.TopDown;
            taskListPanel.WrapContents = gridView;
            taskListPanel.Size = new Size(listWidth, listHeight);
            var mapHeight = isCompact ? 360 : 650;
            if (isCompact)
            {
                mapContainer.Bounds = new Rectangle(0, contentTop + listHeight + 14, resultsWidth, mapHeight);
                mapPanel.Bounds = new Rectangle(0, 0, resultsWidth, mapHeight);
            }
            else
            {
                mapContainer.Bounds = new Rectangle(listWidth + 14, contentTop, mapWidth, mapHeight);
                mapPanel.Bounds = new Rectangle(0, 0, mapWidth, mapHeight);
            }

            var contentHeight = isCompact ? listHeight + mapHeight + 14 : Math.Max(listHeight, mapHeight);
            contentSplit.Bounds = new Rectangle(0, 35, resultsWidth, contentHeight);
            emptyStatePanel.Bounds = new Rectangle(0, 35, resultsWidth, 180);
            var emptyHeading = emptyStatePanel.Controls.Cast<Control>().FirstOrDefault(control => control.Name == "emptyTitle");
            if (emptyHeading is not null)
                emptyHeading.Location = new Point((resultsWidth - emptyHeading.Width) / 2, 58);
            var emptyDescription = emptyStatePanel.Controls.Cast<Control>().FirstOrDefault(control => control.Name == "emptyDescription");
            if (emptyDescription is not null)
                emptyDescription.Location = new Point((resultsWidth - emptyDescription.Width) / 2, 88);

            var resultsHeight = contentTop + contentHeight + 6;
            resultsPanel.Height = resultsHeight;
            resultsPanel.Width = resultsWidth;
            filtersPanel.Height = Math.Min(resultsHeight, 475);
            var footerTop = contentY + resultsHeight + 30;
            footerPanel.Bounds = new Rectangle(0, footerTop, pageWidth, 72);
            var copyright = footerPanel.Controls.Find("browseCopyright", false).FirstOrDefault();
            if (copyright is not null)
                copyright.Location = new Point(pageWidth - copyright.Width - headerX, 28);
            pageCanvas.Height = footerTop + footerPanel.Height + 30;
            var zoomPanel = mapPanel.Controls.Find("mapZoomControls", false).FirstOrDefault();
            if (zoomPanel is not null)
                zoomPanel.Location = new Point(mapPanel.Width - zoomPanel.Width - 10, 10);
            var note = mapPanel.Controls.Find("approximateLocationNote", false).FirstOrDefault();
            if (note is not null)
                note.Location = new Point(8, mapPanel.Height - note.Height - 8);
            toastLabel.Location = new Point((ClientSize.Width - toastLabel.Width) / 2, ClientSize.Height - toastLabel.Height - 22);
        }

        private void WireInteractions()
        {
            searchTextBox.TextChanged += (_, _) => ApplyFilters();
            topBarangayComboBox.SelectedIndexChanged += (_, _) =>
            {
                filterBarangayComboBox.SelectedIndex = topBarangayComboBox.SelectedIndex;
                ApplyFilters();
            };
            searchPanel.Controls.Find("ApplyBrowseSearchButton", false).FirstOrDefault()?.Click += (_, _) =>
            {
                ApplyFilters();
                ShowToast("Search updated.");
            };
            filtersPanel.Controls.Find("ApplyBrowseFiltersButton", false).FirstOrDefault()?.Click += (_, _) =>
            {
                ApplyFilters();
                ShowToast("Filters applied.");
            };
            filtersPanel.Controls.Find("ResetBrowseFiltersButton", false).FirstOrDefault()?.Click += (_, _) => ResetFilters();
            foreach (var check in categoryChecks)
                check.CheckedChanged += (_, _) => ApplyFilters();
            foreach (var priceInput in new[] { minimumPriceTextBox, maximumPriceTextBox })
                priceInput.TextChanged += (_, _) => ApplyFilters();
            deadlineComboBox.SelectedIndexChanged += (_, _) => ApplyFilters();
            filterBarangayComboBox.SelectedIndexChanged += (_, _) =>
            {
                if (topBarangayComboBox.SelectedIndex != filterBarangayComboBox.SelectedIndex)
                    topBarangayComboBox.SelectedIndex = filterBarangayComboBox.SelectedIndex;
                ApplyFilters();
            };
            sortComboBox.SelectedIndexChanged += (_, _) => SortTasks();
            listViewButton.Click += (_, _) => SetGridView(false);
            gridViewButton.Click += (_, _) => SetGridView(true);

            foreach (var task in taskCards)
            {
                task.IsSaved = DemoDataStore.IsSaved(task.Record.Title);
                task.SaveButton.Text = task.IsSaved ? "♥" : "♡";
                task.SaveButton.ForeColor = task.IsSaved ? Color.FromArgb(226, 86, 102) : Color.FromArgb(163, 178, 194);
                task.SaveButton.Click += (_, _) =>
                {
                    var record = task.Record;
                    DemoDataStore.ToggleSaved(new DemoTask(record.Title, record.Category, record.Price, record.Barangay, record.Days, record.Deadline, record.Icon, record.Details, record.ThumbnailColor, new Point(50, 50)));
                    task.IsSaved = DemoDataStore.IsSaved(record.Title);
                    task.SaveButton.Text = task.IsSaved ? "♥" : "♡";
                    task.SaveButton.ForeColor = task.IsSaved ? Color.FromArgb(226, 86, 102) : Color.FromArgb(163, 178, 194);
                    ShowToast(task.IsSaved ? "Task saved." : "Task removed from saved tasks.");
                };
                task.DetailsButton.Click += (_, _) => ShowTaskDetails(task.Record);
            }

            foreach (var button in pageHeader.Controls.OfType<FlowLayoutPanel>().SelectMany(panel => panel.Controls.OfType<Button>()).Concat(browseHeading.Controls.OfType<Button>()))
            {
                if (button.Name is "BrowseLoginButton" or "BrowseSignUpButton")
                    button.Click += (_, _) => OpenAuthentication(button.Name == "BrowseSignUpButton");
                else if (button.Name == "PostTaskButton")
                    button.Click += (_, _) => OpenAuthentication(false);
                else if (button.Name == "HowItWorksNavButton")
                    button.Click += (_, _) => NavigateToPublicInfo("How It Works");
                else if (button.Name == "AboutNavButton")
                    button.Click += (_, _) => NavigateToPublicInfo("About");
            }
        }

        private void ApplyFilters()
        {
            var query = searchTextBox.Text.Trim();
            var selectedCategoryNames = categoryChecks.Where(check => check.Checked).Select(check => check.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var selectedBarangay = filterBarangayComboBox.SelectedIndex == 0 ? null : filterBarangayComboBox.SelectedItem?.ToString()?.Replace("Brgy. ", string.Empty);
            var hasMin = TryParsePrice(minimumPriceTextBox.Text, out var minimum);
            var hasMax = TryParsePrice(maximumPriceTextBox.Text, out var maximum);
            var daysLimit = deadlineComboBox.SelectedIndex switch { 1 => 3, 2 => 7, _ => int.MaxValue };
            var visibleCount = 0;

            foreach (var task in taskCards)
            {
                var record = task.Record;
                var matchesQuery = string.IsNullOrEmpty(query) || record.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || record.Category.Contains(query, StringComparison.OrdinalIgnoreCase);
                var matchesCategory = selectedCategoryNames.Count == 0 || selectedCategoryNames.Contains(record.Category);
                var matchesBarangay = selectedBarangay is null || record.Barangay == selectedBarangay;
                var matchesPrice = (!hasMin || record.Price >= minimum) && (!hasMax || record.Price <= maximum);
                task.IsFilteredVisible = matchesQuery && matchesCategory && matchesBarangay && matchesPrice && record.Days <= daysLimit;
                task.CardPanel.Visible = task.IsFilteredVisible;
                if (task.IsFilteredVisible)
                    visibleCount++;
            }

            resultCountLabel.Text = visibleCount.ToString();
            emptyStatePanel.Visible = visibleCount == 0;
            mapPanel.Visible = visibleCount > 0;
            LayoutPage();
        }

        private static bool TryParsePrice(string text, out int price)
        {
            var digits = new string(text.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out price);
        }

        private void ResetFilters()
        {
            searchTextBox.Clear();
            topBarangayComboBox.SelectedIndex = 0;
            filterBarangayComboBox.SelectedIndex = 0;
            minimumPriceTextBox.Clear();
            maximumPriceTextBox.Clear();
            deadlineComboBox.SelectedIndex = 0;
            foreach (var check in categoryChecks)
                check.Checked = false;
            ApplyFilters();
            ShowToast("Filters reset.");
        }

        private void SortTasks()
        {
            var sorted = sortComboBox.SelectedIndex switch
            {
                1 => taskCards.OrderBy(task => task.Record.Price),
                2 => taskCards.OrderByDescending(task => task.Record.Price),
                3 => taskCards.OrderBy(task => task.Record.Days),
                _ => taskCards.OrderBy(task => Array.IndexOf(TaskRecords, task.Record))
            };
            var orderedTasks = sorted.ToArray();
            taskListPanel.SuspendLayout();
            foreach (var task in orderedTasks)
                taskListPanel.Controls.SetChildIndex(task.CardPanel, Array.IndexOf(orderedTasks, task));
            taskListPanel.ResumeLayout();
            ApplyFilters();
        }

        private void SetGridView(bool enabled)
        {
            gridView = enabled;
            listViewButton.ForeColor = enabled ? Color.FromArgb(139, 160, 182) : BlueColor;
            gridViewButton.ForeColor = enabled ? BlueColor : Color.FromArgb(139, 160, 182);
            LayoutPage();
        }

        private void ShowTaskDetails(TaskRecord task)
        {
            var detailsPage = new TaskDetailsForm(task.Title, task.Category, task.Price, task.Barangay, task.Deadline, task.Details, task.Icon);
            detailsPage.FormClosed += (_, _) =>
            {
                if (IsDisposed)
                    return;
                Show();
                Activate();
            };
            Hide();
            detailsPage.Show();
        }

        private void OpenAuthentication(bool registerInitially)
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void ShowToast(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastLabel.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private void LayoutFilterControls(int width)
        {
            var divider = filtersPanel.Controls.Find("filterHeaderDivider", false).FirstOrDefault();
            if (divider is not null)
                divider.Bounds = new Rectangle(12, 40, width - 24, 1);
            foreach (var count in filtersPanel.Controls.Cast<Control>().Where(control => control.Name.EndsWith("CategoryCount", StringComparison.Ordinal)))
                count.Location = new Point(width - 25, count.Top);
            var min = filtersPanel.Controls.Find("minimumPriceTextBox", false).FirstOrDefault();
            var max = filtersPanel.Controls.Find("maximumPriceTextBox", false).FirstOrDefault();
            var to = filtersPanel.Controls.Find("priceRangeToLabel", false).FirstOrDefault();
            if (min is not null) min.Bounds = new Rectangle(12, 237, 72, 26);
            if (to is not null) to.Location = new Point(88, 243);
            if (max is not null) max.Bounds = new Rectangle(105, 237, width - 117, 26);
            deadlineComboBox.Bounds = new Rectangle(12, 290, width - 24, 30);
            filterBarangayComboBox.Bounds = new Rectangle(12, 345, width - 24, 30);
            var applyButton = filtersPanel.Controls.Find("ApplyBrowseFiltersButton", false).FirstOrDefault();
            if (applyButton is not null)
                applyButton.Bounds = new Rectangle(12, 390, width - 24, 36);
            var reset = filtersPanel.Controls.Find("ResetBrowseFiltersButton", false).FirstOrDefault();
            if (reset is not null)
                reset.Location = new Point(width - reset.Width - 12, 8);
        }

        private void LayoutTaskCard(BrowseTaskCard task, int width, int height, bool asGrid)
        {
            var card = task.CardPanel;
            card.Size = new Size(width, height);
            task.ThumbnailPanel.Location = new Point(9, 8);
            task.ThumbnailPanel.Size = asGrid ? new Size(width - 18, 86) : new Size(78, height - 16);
            task.IconLabel.Dock = DockStyle.Fill;

            if (asGrid)
            {
                task.BodyPanel.Bounds = new Rectangle(9, 101, width - 18, 118);
                task.TitleLabel.Location = new Point(0, 25);
                task.PriceLabel.Location = new Point(0, 47);
                task.SummaryLabel.Location = new Point(0, 65);
                task.SummaryLabel.MaximumSize = new Size(width - 22, 28);
                task.MetadataLabel.Location = new Point(0, 94);
                task.DetailsButton.Bounds = new Rectangle(width - 102, height - 39, 88, 28);
                task.SaveButton.Location = new Point(width - 32, 0);
            }
            else
            {
                var bodyLeft = 99;
                var detailsWidth = 88;
                task.BodyPanel.Bounds = new Rectangle(bodyLeft, 6, Math.Max(150, width - bodyLeft - detailsWidth - 28), height - 12);
                task.TitleLabel.Location = new Point(0, 24);
                task.PriceLabel.Location = new Point(0, 43);
                task.SummaryLabel.Location = new Point(0, 59);
                task.SummaryLabel.MaximumSize = new Size(Math.Max(130, task.BodyPanel.Width - 4), 18);
                task.MetadataLabel.Location = new Point(0, 81);
                task.DetailsButton.Bounds = new Rectangle(width - detailsWidth - 8, (height - 30) / 2, detailsWidth, 30);
                task.SaveButton.Location = new Point(task.BodyPanel.Width - 28, 0);
            }
            task.AvailabilityBadge.Location = new Point(0, 0);
        }

        private void PaintMap(object? sender, PaintEventArgs e)
        {
            if (mapPanel.ClientSize.Width <= 0 || mapPanel.ClientSize.Height <= 0)
                return;
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.FromArgb(233, 242, 231));
            var bounds = mapPanel.ClientRectangle;
            var step = Math.Max(55, 105 - mapZoom * 10);
            using var smallRoad = new Pen(Color.FromArgb(210, 223, 207), 2);
            for (var x = -bounds.Height; x < bounds.Width + bounds.Height; x += step)
                graphics.DrawLine(smallRoad, x, 0, x - bounds.Height / 3, bounds.Height);
            for (var y = -bounds.Width; y < bounds.Height + bounds.Width; y += step)
                graphics.DrawLine(smallRoad, 0, y, bounds.Width, y + bounds.Width / 5);

            using var roadEdge = new Pen(Color.FromArgb(216, 226, 216), 18);
            using var mainRoad = new Pen(Color.White, 15);
            graphics.DrawLine(roadEdge, -20, bounds.Height * 0.34F, bounds.Width + 30, bounds.Height * 0.55F);
            graphics.DrawLine(mainRoad, -20, bounds.Height * 0.34F, bounds.Width + 30, bounds.Height * 0.55F);
            graphics.DrawLine(roadEdge, bounds.Width * 0.69F, -20, bounds.Width * 0.43F, bounds.Height + 30);
            graphics.DrawLine(mainRoad, bounds.Width * 0.69F, -20, bounds.Width * 0.43F, bounds.Height + 30);
            graphics.DrawLine(roadEdge, bounds.Width * 0.03F, bounds.Height * 0.72F, bounds.Width * 0.96F, bounds.Height * 0.47F);
            graphics.DrawLine(mainRoad, bounds.Width * 0.03F, bounds.Height * 0.72F, bounds.Width * 0.96F, bounds.Height * 0.47F);

            DrawMapLabel(graphics, "Brgy. 176", bounds.Width * 0.22F, bounds.Height * 0.27F);
            DrawMapLabel(graphics, "Brgy. 178", bounds.Width * 0.52F, bounds.Height * 0.51F);
            DrawMapLabel(graphics, "Brgy. 179", bounds.Width * 0.62F, bounds.Height * 0.77F);
            DrawMapPin(graphics, bounds.Width * 0.37F, bounds.Height * 0.22F, Color.FromArgb(8, 120, 233));
            DrawMapPin(graphics, bounds.Width * 0.80F, bounds.Height * 0.39F, Color.FromArgb(8, 120, 233));
            DrawMapPin(graphics, bounds.Width * 0.67F, bounds.Height * 0.50F, Color.FromArgb(241, 109, 99));
            DrawMapPin(graphics, bounds.Width * 0.27F, bounds.Height * 0.69F, Color.FromArgb(245, 156, 45));
        }

        private static void DrawMapLabel(Graphics graphics, string text, float x, float y)
        {
            using var font = new Font("Segoe UI", 6.5F, FontStyle.Bold);
            var size = graphics.MeasureString(text, font);
            var rect = new RectangleF(x, y, size.Width + 12, size.Height + 6);
            using var background = new SolidBrush(Color.FromArgb(225, 255, 255, 255));
            using var foreground = new SolidBrush(Color.FromArgb(101, 121, 104));
            graphics.FillRectangle(background, rect);
            graphics.DrawString(text, font, foreground, x + 6, y + 3);
        }

        private static void DrawMapPin(Graphics graphics, float x, float y, Color color)
        {
            using var brush = new SolidBrush(color);
            using var center = new SolidBrush(Color.White);
            graphics.FillEllipse(brush, x, y, 16, 16);
            graphics.FillPolygon(brush, [new PointF(x + 2, y + 12), new PointF(x + 8, y + 23), new PointF(x + 14, y + 12)]);
            graphics.FillEllipse(center, x + 6, y + 5, 4, 4);
        }

        private void AddFilterHeading(string text, int top)
        {
            var heading = MakeLabel(text, 8, NavyColor, true);
            heading.Location = new Point(12, top);
            filtersPanel.Controls.Add(heading);
        }

        private ComboBox MakeBarangayComboBox(string name)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat
            };
            comboBox.Items.AddRange(["All Barangays", "Brgy. 176", "Brgy. 178", "Brgy. 179"]);
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private Control MakeBrand()
        {
            var logo = new TaskTapLogo { Name = "browseBrand", BackColor = Color.White, Size = new Size(160, 34), Cursor = Cursors.Hand };
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

        private static Button MakeLinkButton(string text, string name, Color color)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = color,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(6, 2, 6, 2)
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

        private sealed record TaskRecord(string Title, string Category, int Price, string Barangay, string Deadline, int Days, string Icon, string Summary, string Details, Color ThumbnailColor);

        private sealed class BrowseTaskCard(TaskRecord record, Panel cardPanel, Panel thumbnailPanel, Label iconLabel, Panel bodyPanel, Label availabilityBadge, Button saveButton, Label titleLabel, Label priceLabel, Label summaryLabel, Label metadataLabel, Button detailsButton)
        {
            public TaskRecord Record { get; } = record;
            public Panel CardPanel { get; } = cardPanel;
            public Panel ThumbnailPanel { get; } = thumbnailPanel;
            public Label IconLabel { get; } = iconLabel;
            public Panel BodyPanel { get; } = bodyPanel;
            public Label AvailabilityBadge { get; } = availabilityBadge;
            public Button SaveButton { get; } = saveButton;
            public Label TitleLabel { get; } = titleLabel;
            public Label PriceLabel { get; } = priceLabel;
            public Label SummaryLabel { get; } = summaryLabel;
            public Label MetadataLabel { get; } = metadataLabel;
            public Button DetailsButton { get; } = detailsButton;
            public bool IsSaved { get; set; }
            public bool IsFilteredVisible { get; set; } = true;
        }
    }
}
