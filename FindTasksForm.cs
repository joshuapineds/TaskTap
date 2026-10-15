namespace TaskTap
{
    public sealed class FindTasksForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(8, 38, 87);
        private static readonly Color SidebarNavy = Color.FromArgb(8, 35, 77);
        private static readonly Color Blue = Color.FromArgb(8, 117, 232);
        private static readonly Color Muted = Color.FromArgb(113, 131, 155);
        private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
        private static readonly Color Line = Color.FromArgb(225, 233, 241);
        private readonly Panel sidebar = new();
        private readonly Panel main = new();
        private readonly Panel topbar = new();
        private readonly Panel canvas = new();
        private readonly Panel searchPanel = new();
        private readonly Panel filterPanel = new();
        private readonly Panel resultsPanel = new();
        private readonly Panel listPanel = new();
        private readonly Panel mapPanel = new();
        private readonly Panel mapCanvas = new();
        private readonly Panel emptyPanel = new();
        private readonly TextBox taskSearch = new();
        private readonly ComboBox quickBarangay = new();
        private readonly ComboBox barangayFilter = new();
        private readonly ComboBox deadlineFilter = new();
        private readonly ComboBox sortTasks = new();
        private readonly TextBox minPrice = new();
        private readonly TextBox maxPrice = new();
        private readonly CheckBox nearbyOnly = new();
        private readonly Label resultCount = new();
        private readonly Label emptyLabel = new();
        private readonly FlowLayoutPanel taskList = new();
        private readonly List<TaskRecord> tasks = [];
        private readonly List<CheckBox> categoryFilters = [];
        private readonly Dictionary<TaskRecord, Panel> taskRows = [];
        private readonly Dictionary<TaskRecord, Button> mapPins = [];
        private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
        private readonly Label toast = new();
        private Button listViewButton = null!;
        private Button mapViewButton = null!;
        private Label emptyIcon = null!;
        private bool mapOnly;
        private bool listOnly;
        private int mapZoom = 100;
        private string selectedBarangay = "all";

        private sealed record TaskRecord(string Title, string Category, int Price, string Barangay, int Days, string Deadline, string Icon, string Description, Point MapPoint, Color IconBack);

        public FindTasksForm()
        {
            SeedTasks();
            BuildPage();
            ApplyFilters();
        }

        private void SeedTasks()
        {
            tasks.AddRange([
                new("House Cleaning", "Cleaning", 500, "176", 1, "Oct 7, 2026", "🧹", "Help with general house cleaning for a local resident.", new Point(22, 35), Color.FromArgb(230, 248, 241)),
                new("Math Tutor (Grade 10)", "Tutoring", 300, "178", 4, "Oct 10, 2026", "📚", "Grade 10 math tutoring for a student in the community.", new Point(48, 52), Color.FromArgb(232, 241, 255)),
                new("Pet Care", "Pet Care", 400, "176", 6, "Oct 12, 2026", "🐶", "Looking for help caring for a pet while the owner is away.", new Point(28, 70), Color.FromArgb(255, 241, 231)),
                new("Fix a leaking faucet", "Repairs", 600, "176", 2, "Oct 8, 2026", "🔧", "Basic plumbing assistance needed for a leaking kitchen faucet.", new Point(64, 25), Color.FromArgb(232, 241, 255)),
                new("Grocery Shopping", "Shopping", 450, "179", 3, "Oct 9, 2026", "🛒", "Pick up a short grocery list and deliver it locally.", new Point(73, 54), Color.FromArgb(255, 244, 224)),
                new("Apartment Cleaning", "Cleaning", 550, "178", 5, "Oct 11, 2026", "🧽", "One-time apartment cleaning before the weekend.", new Point(58, 82), Color.FromArgb(230, 248, 241))
            ]);
        }

        private void BuildPage()
        {
            Text = "TaskTap — Find Tasks";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 740);
            ClientSize = new Size(1540, 900);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            main.Dock = DockStyle.Fill;
            main.BackColor = PageColor;
            Controls.Add(main);

            var scrollHost = new Panel { Name = "findTasksScrollHost", Dock = DockStyle.Fill, AutoScroll = true, BackColor = PageColor };
            scrollHost.Resize += (_, _) => LayoutPage();
            main.Controls.Add(scrollHost);
            BuildSidebar();
            canvas.Name = "findTasksCanvas";
            canvas.BackColor = PageColor;
            scrollHost.Controls.Add(canvas);
            BuildContent();
            BuildTopbar();

            toast.Name = "findTasksToast";
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

        private void BuildSidebar()
        {
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 220;
            sidebar.BackColor = SidebarNavy;
            main.Controls.Add(sidebar);
            var brand = MakeLabel("◉ TaskTap", 18, Color.White, true);
            brand.Location = new Point(22, 20);
            sidebar.Controls.Add(brand);
            var navItems = new[] { "Dashboard", "Find Tasks", "Post Task", "My Tasks", "Messages", "Notifications", "Profile" };
            for (var i = 0; i < navItems.Length; i++)
            {
                var name = navItems[i];
                var button = MakeNavButton(name, i == 1);
                button.Location = new Point(12, 76 + i * 43);
                button.Click += (_, _) => HandleNavigation(name);
                sidebar.Controls.Add(button);
                if (name is "Messages" or "Notifications")
                {
                    var badge = MakeLabel(name == "Messages" ? "3" : "5", 8, Color.White, true);
                    badge.AutoSize = false;
                    badge.Size = new Size(20, 20);
                    badge.TextAlign = ContentAlignment.MiddleCenter;
                    badge.BackColor = Color.FromArgb(255, 132, 77);
                    badge.Location = new Point(178, 85 + i * 43);
                    sidebar.Controls.Add(badge);
                }
            }
            var account = MakeLabel("ACCOUNT", 8, Color.FromArgb(126, 159, 198), true);
            account.Location = new Point(22, 432);
            sidebar.Controls.Add(account);
            var logout = MakeNavButton("Log out", false);
            logout.Location = new Point(12, 0);
            logout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            logout.Click += (_, _) => Close();
            sidebar.Controls.Add(logout);
            sidebar.Resize += (_, _) => logout.Location = new Point(12, sidebar.ClientSize.Height - 56);
        }

        private void BuildTopbar()
        {
            topbar.Dock = DockStyle.Top;
            topbar.Height = 58;
            topbar.BackColor = Color.White;
            topbar.Paint += (_, e) => { using var pen = new Pen(Line); e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1); };
            var notification = MakeSmallButton("♧  •", "FindTaskNotificationsButton", 44, 36);
            notification.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            notification.Location = new Point(topbar.Width - 245, 10);
            notification.Click += (_, _) => MessageBox.Show("Sample notifications are not connected to a backend.", "Notifications (Demo)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            topbar.Controls.Add(notification);
            var avatar = new Label { Text = "JD", Size = new Size(34, 34), Location = new Point(topbar.Width - 190, 11), BackColor = Color.FromArgb(224, 240, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            topbar.Controls.Add(avatar);
            var user = MakeLabel("Joshua D. Pineda\nStandard User", 8, Navy, true);
            user.Location = new Point(topbar.Width - 148, 11);
            user.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topbar.Controls.Add(user);
            topbar.Resize += (_, _) =>
            {
                notification.Location = new Point(topbar.ClientSize.Width - 245, 10);
                avatar.Location = new Point(topbar.ClientSize.Width - 190, 11);
                user.Location = new Point(topbar.ClientSize.Width - 148, 11);
            };
            main.Controls.Add(topbar);
        }

        private void BuildContent()
        {
            var eyebrow = MakeLabel("TASK DISCOVERY", 9, Blue, true);
            eyebrow.Name = "findEyebrow";
            canvas.Controls.Add(eyebrow);
            var title = MakeLabel("Find a task near you.", 26, Navy, true);
            title.Name = "findTitle";
            title.MaximumSize = new Size(720, 0);
            canvas.Controls.Add(title);
            var subtitle = MakeLabel("Search local opportunities by category, barangay, compensation, and deadline.", 10, Muted);
            subtitle.Name = "findSubtitle";
            canvas.Controls.Add(subtitle);
            var postTask = MakeAction("＋  Post a Task", "FindPostTaskButton", true, 122);
            postTask.Click += (_, _) => OpenPostTask();
            canvas.Controls.Add(postTask);

            BuildSearchPanel();
            BuildFilterPanel();
            BuildResultsPanel();
        }

        private void BuildSearchPanel()
        {
            searchPanel.Name = "findSearchPanel";
            searchPanel.BackColor = Color.White;
            searchPanel.BorderStyle = BorderStyle.FixedSingle;
            var searchGlyph = MakeLabel("⌕", 12, Muted, true);
            searchGlyph.Location = new Point(15, 12);
            searchPanel.Controls.Add(searchGlyph);
            taskSearch.Name = "findTaskSearch";
            taskSearch.PlaceholderText = "Search tasks, e.g. cleaning, tutor, repairs";
            taskSearch.Location = new Point(38, 8);
            taskSearch.Size = new Size(600, 34);
            taskSearch.TextChanged += (_, _) => ApplyFilters();
            searchPanel.Controls.Add(taskSearch);
            var barangayLabel = MakeLabel("Barangay", 7, Muted);
            barangayLabel.Location = new Point(650, 3);
            searchPanel.Controls.Add(barangayLabel);
            quickBarangay.Name = "findQuickBarangay";
            quickBarangay.DropDownStyle = ComboBoxStyle.DropDownList;
            quickBarangay.Items.AddRange(["All Barangays", "Brgy. 176", "Brgy. 178", "Brgy. 179"]);
            quickBarangay.SelectedIndex = 0;
            quickBarangay.Location = new Point(646, 18);
            quickBarangay.Size = new Size(142, 29);
            quickBarangay.SelectedIndexChanged += (_, _) =>
            {
                selectedBarangay = quickBarangay.SelectedIndex == 0 ? "all" : quickBarangay.SelectedItem!.ToString()![6..];
                barangayFilter.SelectedIndex = quickBarangay.SelectedIndex;
                ApplyFilters();
            };
            searchPanel.Controls.Add(quickBarangay);
            var searchButton = MakeAction("Search", "FindSearchButton", true, 74);
            searchButton.Size = new Size(74, 35);
            searchButton.Location = new Point(798, 8);
            searchButton.Click += (_, _) => ApplyFilters();
            searchPanel.Controls.Add(searchButton);
            canvas.Controls.Add(searchPanel);
        }

        private void BuildFilterPanel()
        {
            filterPanel.Name = "findFiltersPanel";
            filterPanel.BackColor = Color.White;
            filterPanel.BorderStyle = BorderStyle.FixedSingle;
            var heading = MakeLabel("Filters", 11, Navy, true);
            heading.Location = new Point(14, 12);
            filterPanel.Controls.Add(heading);
            var refine = MakeLabel("Refine your results", 8, Muted);
            refine.Location = new Point(14, 31);
            filterPanel.Controls.Add(refine);
            var reset = MakeTextButton("Reset", "FindResetFiltersButton");
            reset.Location = new Point(128, 11);
            reset.Click += (_, _) => ResetFilters();
            filterPanel.Controls.Add(reset);
            AddDivider(filterPanel, 55);
            AddFilterHeading("Category", 65);
            var names = new[] { "Cleaning", "Tutoring", "Repairs", "Shopping", "Pet Care", "Others" };
            for (var index = 0; index < names.Length; index++)
            {
                var check = new CheckBox { Name = $"category{names[index].Replace(" ", string.Empty)}", Text = names[index], Location = new Point(14, 88 + index * 24), Size = new Size(166, 22), Font = new Font("Segoe UI", 8F), ForeColor = Navy, AutoSize = false };
                check.CheckedChanged += (_, _) => ApplyFilters();
                categoryFilters.Add(check);
                filterPanel.Controls.Add(check);
            }
            AddDivider(filterPanel, 239);
            AddFilterHeading("Price Range", 248);
            minPrice.Name = "findMinPrice";
            minPrice.PlaceholderText = "₱0";
            minPrice.Location = new Point(14, 271);
            minPrice.Size = new Size(68, 26);
            minPrice.TextChanged += (_, _) => ApplyFilters();
            filterPanel.Controls.Add(minPrice);
            var to = MakeLabel("to", 8, Muted);
            to.Location = new Point(88, 276);
            filterPanel.Controls.Add(to);
            maxPrice.Name = "findMaxPrice";
            maxPrice.PlaceholderText = "₱1,000+";
            maxPrice.Location = new Point(108, 271);
            maxPrice.Size = new Size(74, 26);
            maxPrice.TextChanged += (_, _) => ApplyFilters();
            filterPanel.Controls.Add(maxPrice);
            AddDivider(filterPanel, 307);
            AddFilterHeading("Deadline", 316);
            deadlineFilter.Name = "findDeadlineFilter";
            deadlineFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            deadlineFilter.Items.AddRange(["Any deadline", "Within 3 days", "Within 7 days", "Within 14 days"]);
            deadlineFilter.SelectedIndex = 0;
            deadlineFilter.Location = new Point(14, 339);
            deadlineFilter.Size = new Size(168, 28);
            deadlineFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
            filterPanel.Controls.Add(deadlineFilter);
            AddDivider(filterPanel, 379);
            AddFilterHeading("Barangay", 388);
            barangayFilter.Name = "findBarangayFilter";
            barangayFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            barangayFilter.Items.AddRange(["All Barangays", "Brgy. 176", "Brgy. 178", "Brgy. 179"]);
            barangayFilter.SelectedIndex = 0;
            barangayFilter.Location = new Point(14, 411);
            barangayFilter.Size = new Size(168, 28);
            barangayFilter.SelectedIndexChanged += (_, _) =>
            {
                if (barangayFilter.SelectedIndex != quickBarangay.SelectedIndex)
                    quickBarangay.SelectedIndex = barangayFilter.SelectedIndex;
                selectedBarangay = barangayFilter.SelectedIndex == 0 ? "all" : barangayFilter.SelectedItem!.ToString()![6..];
                ApplyFilters();
            };
            filterPanel.Controls.Add(barangayFilter);
            nearbyOnly.Name = "findNearbyOnly";
            nearbyOnly.Text = "Show nearby only";
            nearbyOnly.Location = new Point(14, 448);
            nearbyOnly.Size = new Size(168, 34);
            nearbyOnly.Font = new Font("Segoe UI", 8F);
            nearbyOnly.ForeColor = Navy;
            nearbyOnly.CheckedChanged += (_, _) =>
            {
                if (nearbyOnly.Checked && selectedBarangay == "all")
                    ShowToast("Select a barangay to use Nearby only.");
                ApplyFilters();
            };
            filterPanel.Controls.Add(nearbyOnly);
            var note = MakeLabel("Uses your selected barangay, not GPS.", 7, Muted);
            note.Location = new Point(14, 480);
            note.MaximumSize = new Size(170, 30);
            filterPanel.Controls.Add(note);
            var apply = MakeAction("Apply Filters", "FindApplyFiltersButton", true, 168);
            apply.Location = new Point(14, 515);
            apply.Click += (_, _) => { ApplyFilters(); ShowToast("Filters applied to sample tasks."); };
            filterPanel.Controls.Add(apply);
            canvas.Controls.Add(filterPanel);
        }

        private void BuildResultsPanel()
        {
            resultsPanel.Name = "findResultsPanel";
            resultsPanel.BackColor = Color.White;
            resultsPanel.BorderStyle = BorderStyle.FixedSingle;
            var count = MakeLabel("6 available tasks", 9, Navy);
            count.Location = new Point(14, 14);
            resultCount.Name = "findResultsCount";
            resultsPanel.Controls.Add(count);
            count.Name = "findResultsCaption";
            var sortLabel = MakeLabel("Sort", 8, Muted);
            sortLabel.Location = new Point(0, 14);
            sortLabel.Name = "findSortLabel";
            resultsPanel.Controls.Add(sortLabel);
            sortTasks.Name = "findSortTasks";
            sortTasks.DropDownStyle = ComboBoxStyle.DropDownList;
            sortTasks.Items.AddRange(["Recommended", "Price: Low to High", "Price: High to Low", "Deadline Soonest"]);
            sortTasks.SelectedIndex = 0;
            sortTasks.Size = new Size(160, 28);
            sortTasks.SelectedIndexChanged += (_, _) => ApplyFilters();
            resultsPanel.Controls.Add(sortTasks);
            listViewButton = MakeViewButton("☷", "FindListViewButton");
            listViewButton.Click += (_, _) => SetView("list");
            resultsPanel.Controls.Add(listViewButton);
            mapViewButton = MakeViewButton("⌖", "FindMapViewButton");
            mapViewButton.Click += (_, _) => SetView("map");
            resultsPanel.Controls.Add(mapViewButton);
            AddDivider(resultsPanel, 45);

            listPanel.Name = "findTaskListPanel";
            listPanel.BackColor = Color.White;
            taskList.Name = "findTaskList";
            taskList.Dock = DockStyle.Fill;
            taskList.AutoScroll = true;
            taskList.FlowDirection = FlowDirection.TopDown;
            taskList.WrapContents = false;
            taskList.Margin = Padding.Empty;
            taskList.Padding = new Padding(2, 4, 2, 4);
            taskList.BackColor = Color.White;
            listPanel.Controls.Add(taskList);
            resultsPanel.Controls.Add(listPanel);

            mapPanel.Name = "findMapPanel";
            mapPanel.BackColor = Color.FromArgb(232, 241, 244);
            mapPanel.BorderStyle = BorderStyle.FixedSingle;
            mapCanvas.Name = "findMapCanvas";
            mapCanvas.BackColor = Color.FromArgb(232, 241, 244);
            mapCanvas.Paint += PaintMap;
            mapPanel.Controls.Add(mapCanvas);
            var zoomIn = MakeMapButton("+", "FindMapZoomInButton");
            zoomIn.Location = new Point(0, 10);
            zoomIn.Click += (_, _) => { mapZoom = Math.Min(140, mapZoom + 10); mapCanvas.Invalidate(); };
            var zoomOut = MakeMapButton("−", "FindMapZoomOutButton");
            zoomOut.Location = new Point(0, 43);
            zoomOut.Click += (_, _) => { mapZoom = Math.Max(90, mapZoom - 10); mapCanvas.Invalidate(); };
            mapPanel.Controls.Add(zoomIn);
            mapPanel.Controls.Add(zoomOut);
            var mapNote = MakeLabel("Approximate locations\nExact user locations are not displayed.", 7, Navy, true);
            mapNote.Name = "findMapNote";
            mapNote.AutoSize = false;
            mapNote.Size = new Size(194, 39);
            mapNote.TextAlign = ContentAlignment.MiddleLeft;
            mapNote.BackColor = Color.White;
            mapNote.Location = new Point(10, 0);
            mapPanel.Controls.Add(mapNote);
            resultsPanel.Controls.Add(mapPanel);

            emptyPanel.Name = "findEmptyState";
            emptyPanel.BackColor = Color.White;
            emptyIcon = MakeLabel("⌕", 27, Blue, true);
            emptyLabel.Name = "findEmptyMessage";
            emptyLabel.Text = "No tasks match these filters.";
            emptyLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            emptyLabel.ForeColor = Navy;
            emptyLabel.AutoSize = true;
            var reset = MakeAction("Reset Filters", "FindEmptyResetButton", false, 120);
            reset.Click += (_, _) => ResetFilters();
            emptyPanel.Controls.Add(emptyIcon);
            emptyPanel.Controls.Add(emptyLabel);
            emptyPanel.Controls.Add(reset);
            resultsPanel.Controls.Add(emptyPanel);
            canvas.Controls.Add(resultsPanel);

            foreach (var task in tasks)
                BuildTaskRow(task);
            BuildMapPins();
        }

        private void BuildTaskRow(TaskRecord task)
        {
            var row = new Panel { Name = $"taskRow{tasks.IndexOf(task)}", Size = new Size(440, 144), BackColor = Color.White, Margin = new Padding(4, 5, 4, 2), Tag = task };
            row.BorderStyle = BorderStyle.FixedSingle;
            var thumb = new Label { Text = task.Icon, Location = new Point(10, 21), Size = new Size(55, 58), BackColor = task.IconBack, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 21F) };
            row.Controls.Add(thumb);
            var status = MakeLabel("Available", 7, Color.FromArgb(35, 153, 111), true);
            status.BackColor = Color.FromArgb(230, 248, 241);
            status.Location = new Point(76, 9);
            row.Controls.Add(status);
            var save = MakeTextButton("♡", $"SaveTask{tasks.IndexOf(task)}");
            save.Font = new Font("Segoe UI", 14F);
            save.Location = new Point(0, 5);
            save.Click += (_, _) =>
            {
                var saved = save.Tag is true;
                save.Tag = !saved;
                save.Text = saved ? "♡" : "♥";
                save.ForeColor = saved ? Muted : Color.FromArgb(229, 83, 110);
                ShowToast(saved ? "Task removed from saved tasks." : "Task saved for this demo session.");
            };
            row.Controls.Add(save);
            var title = MakeLabel(task.Title, 10, Navy, true);
            title.Name = "taskTitleLabel";
            title.Location = new Point(76, 28);
            title.MaximumSize = new Size(330, 24);
            row.Controls.Add(title);
            var price = MakeLabel($"₱{task.Price}  •  {task.Category}", 8, Navy, true);
            price.Location = new Point(76, 48);
            row.Controls.Add(price);
            var desc = MakeLabel(task.Description, 7.5F, Muted);
            desc.Name = "taskDescriptionLabel";
            desc.Location = new Point(76, 64);
            desc.MaximumSize = new Size(390, 32);
            row.Controls.Add(desc);
            var meta = MakeLabel($"⌖ Brgy. {task.Barangay}   ·   ◷ {task.Deadline}", 7, Muted);
            meta.Location = new Point(76, 100);
            row.Controls.Add(meta);
            var details = MakeTextButton("View Details", $"ViewTaskDetails{tasks.IndexOf(task)}");
            details.Location = new Point(76, 124);
            details.Click += (_, _) => OpenTaskDetails(task);
            row.Controls.Add(details);
            row.Click += (_, _) => SelectTask(task);
            foreach (Control child in row.Controls)
                if (child is not Button)
                    child.Click += (_, _) => SelectTask(task);
            taskRows.Add(task, row);
            taskList.Controls.Add(row);
        }

        private void BuildMapPins()
        {
            foreach (var task in tasks)
            {
                var pin = new Button
                {
                    Name = $"mapPin{tasks.IndexOf(task)}",
                    Text = "●",
                    Size = new Size(28, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Blue,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Tag = task,
                    Cursor = Cursors.Hand
                };
                pin.FlatAppearance.BorderSize = 0;
                pin.Click += (_, _) => SelectTask(task);
                mapPins.Add(task, pin);
                mapCanvas.Controls.Add(pin);
            }
            mapCanvas.Resize += (_, _) => PositionMapPins();
            PositionMapPins();
        }

        private void PaintMap(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var w = mapCanvas.ClientSize.Width;
            var h = mapCanvas.ClientSize.Height;
            using var block = new SolidBrush(Color.FromArgb(239, 244, 235));
            using var roadEdge = new Pen(Color.FromArgb(207, 220, 218), 10);
            using var road = new Pen(Color.FromArgb(255, 255, 252), 7);
            using var roadFine = new Pen(Color.FromArgb(216, 228, 224), 2);
            using var water = new SolidBrush(Color.FromArgb(215, 239, 245));
            using var park = new SolidBrush(Color.FromArgb(225, 239, 219));
            using var building = new SolidBrush(Color.FromArgb(226, 233, 226));
            using var residential = new SolidBrush(Color.FromArgb(232, 237, 226));
            using var commercial = new SolidBrush(Color.FromArgb(238, 230, 216));
            g.FillRectangle(water, (int)(w * .73), 0, (int)(w * .27), h);
            g.FillRectangle(park, (int)(w * .06), (int)(h * .34), (int)(w * .22), (int)(h * .13));
            g.FillRectangle(park, (int)(w * .43), (int)(h * .72), (int)(w * .15), (int)(h * .10));
            var blocks = new[]
            {
                new Rectangle((int)(w*.05),(int)(h*.10),(int)(w*.24),(int)(h*.20)), new Rectangle((int)(w*.36),(int)(h*.07),(int)(w*.23),(int)(h*.24)),
                new Rectangle((int)(w*.70),(int)(h*.19),(int)(w*.20),(int)(h*.17)), new Rectangle((int)(w*.10),(int)(h*.53),(int)(w*.24),(int)(h*.21)),
                new Rectangle((int)(w*.40),(int)(h*.51),(int)(w*.20),(int)(h*.22)), new Rectangle((int)(w*.70),(int)(h*.55),(int)(w*.19),(int)(h*.20)),
                new Rectangle((int)(w*.17),(int)(h*.84),(int)(w*.25),(int)(h*.12)), new Rectangle((int)(w*.51),(int)(h*.83),(int)(w*.27),(int)(h*.13))
            };
            foreach (var bounds in blocks)
            {
                g.FillRectangle(block, bounds);
                var lotInset = Rectangle.Inflate(bounds, -4, -4);
                using var lotFill = new SolidBrush(Color.FromArgb(246, 248, 240));
                g.FillRectangle(lotFill, lotInset);
                var buildingBounds = Rectangle.Inflate(bounds, -10, -10);
                g.FillRectangle(building, buildingBounds);
                var innerBuilding = Rectangle.Inflate(buildingBounds, -5, -5);
                g.FillRectangle(bounds.X % 2 == 0 ? residential : commercial, innerBuilding);
                using var outline = new Pen(Color.FromArgb(207, 219, 212));
                g.DrawRectangle(outline, bounds);
                using var buildingOutline = new Pen(Color.FromArgb(219, 226, 217));
                g.DrawRectangle(buildingOutline, buildingBounds);
                using var lotLine = new Pen(Color.FromArgb(221, 228, 216), 1);
                for (var x = bounds.Left + 12; x < bounds.Right - 8; x += 18)
                    g.DrawLine(lotLine, x, bounds.Top + 3, x, bounds.Bottom - 3);
            }
            g.DrawLine(roadEdge, (int)(w*.33), 0, (int)(w*.33), h);
            g.DrawLine(roadEdge, (int)(w*.65), 0, (int)(w*.65), h);
            g.DrawLine(roadEdge, 0, (int)(h*.43), w, (int)(h*.43));
            g.DrawLine(roadEdge, 0, (int)(h*.78), w, (int)(h*.78));
            g.DrawLine(road, (int)(w*.33), 0, (int)(w*.33), h);
            g.DrawLine(road, (int)(w*.65), 0, (int)(w*.65), h);
            g.DrawLine(road, 0, (int)(h*.43), w, (int)(h*.43));
            g.DrawLine(road, 0, (int)(h*.78), w, (int)(h*.78));
            g.DrawLine(roadFine, (int)(w*.26), 0, (int)(w*.06), h);
            g.DrawLine(roadFine, (int)(w*.77), 0, (int)(w*.91), h);
            g.DrawLine(roadFine, 0, (int)(h*.64), w, (int)(h*.50));
            using var roadDash = new Pen(Color.FromArgb(223, 210, 168), 1) { DashPattern = [4, 6] };
            g.DrawLine(roadDash, (int)(w*.33), 0, (int)(w*.33), h);
            g.DrawLine(roadDash, 0, (int)(h*.43), w, (int)(h*.43));
            using var neighborhoodLine = new Pen(Color.FromArgb(232, 219, 194), 1);
            g.DrawLine(neighborhoodLine, (int)(w*.12), (int)(h*.08), (int)(w*.24), (int)(h*.91));
            g.DrawLine(neighborhoodLine, (int)(w*.57), (int)(h*.06), (int)(w*.50), (int)(h*.94));
            using var labelFont = new Font("Segoe UI", 7F, FontStyle.Bold);
            using var labelBrush = new SolidBrush(Color.FromArgb(131, 151, 166));
            g.DrawString("Brgy. 176", labelFont, labelBrush, w * .16F, h * .36F);
            g.DrawString("Brgy. 178", labelFont, labelBrush, w * .47F, h * .69F);
            g.DrawString("Brgy. 179", labelFont, labelBrush, w * .76F, h * .36F);
        }

        private void LayoutPage()
        {
            if (canvas.IsDisposed || canvas.Parent is null || canvas.Controls.Find("findEyebrow", false).Length == 0)
                return;
            var host = canvas.Parent;
            var width = Math.Max(860, host.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 28);
            canvas.Size = new Size(width, 850);
            canvas.Location = new Point(Math.Max(0, (host.ClientSize.Width - width) / 2), 0);
            var left = 22;
            var right = width - 22;
            Position("findEyebrow", left, 18);
            Position("findTitle", left, 36);
            Position("findSubtitle", left, 82);
            var post = canvas.Controls.OfType<Button>().First(button => button.Name == "FindPostTaskButton");
            post.Location = new Point(right - post.Width, 48);
            searchPanel.Location = new Point(left, 126);
            searchPanel.Size = new Size(width - 44, 56);
            var searchInput = searchPanel.Controls.Find("findTaskSearch", false).FirstOrDefault();
            var quick = searchPanel.Controls.Find("findQuickBarangay", false).FirstOrDefault();
            var label = searchPanel.Controls.OfType<Label>().FirstOrDefault();
            if (searchInput is not null && quick is not null)
            {
                var searchButton = searchPanel.Controls.OfType<Button>().First(button => button.Name == "FindSearchButton");
                var searchWidth = Math.Max(180, searchPanel.Width - quick.Width - searchButton.Width - 122);
                searchInput.Width = searchWidth;
                searchInput.Location = new Point(38, 14);
                searchInput.Height = 27;
                quick.Location = new Point(searchInput.Right + 10, 17);
                quick.Height = 27;
                var quickLabel = searchPanel.Controls.OfType<Label>().FirstOrDefault(control => control.Location.Y == 3);
                if (quickLabel is not null)
                    quickLabel.Location = new Point(quick.Left + 4, 3);
                searchButton.Bounds = new Rectangle(searchPanel.Width - searchButton.Width - 10, 14, searchButton.Width, 27);
            }
            var resultsTop = 196;
            var resultWidth = width - 44;
            var filterWidth = 194;
            filterPanel.Location = new Point(left, resultsTop);
            filterPanel.Size = new Size(filterWidth, 570);
            resultsPanel.Location = new Point(left + filterWidth + 14, resultsTop);
            resultsPanel.Size = new Size(resultWidth - filterWidth - 14, 570);
            var resultsWidth = resultsPanel.Width;
            var caption = resultsPanel.Controls.Find("findResultsCaption", false).FirstOrDefault();
            if (caption is not null)
                caption.Text = $"{resultCount.Text} available tasks";
            var sort = resultsPanel.Controls.Find("findSortTasks", false).FirstOrDefault();
            var sortLabel = resultsPanel.Controls.Find("findSortLabel", false).FirstOrDefault();
            mapViewButton.Location = new Point(resultsWidth - 45, 8);
            listViewButton.Location = new Point(resultsWidth - 80, 8);
            if (sort is not null)
                sort.Location = new Point(resultsWidth - 245, 9);
            if (sortLabel is not null)
                sortLabel.Location = new Point(resultsWidth - 282, 14);
            var listWidth = mapOnly ? 0 : listOnly ? resultsWidth - 20 : (resultsWidth - 28) / 2;
            var mapWidth = listOnly ? 0 : mapOnly ? resultsWidth - 20 : resultsWidth - listWidth - 28;
            listPanel.Visible = listWidth > 0;
            mapPanel.Visible = mapWidth > 0;
            listPanel.Bounds = new Rectangle(12, 48, listWidth, resultsPanel.Height - 60);
            mapPanel.Bounds = new Rectangle(20 + listWidth, 48, mapWidth, resultsPanel.Height - 60);
            taskList.Padding = new Padding(2, 4, 2, 4);
            foreach (Control item in taskList.Controls)
            {
                item.Width = Math.Max(180, listWidth - taskList.Padding.Horizontal - item.Margin.Horizontal - SystemInformation.VerticalScrollBarWidth - 4);
                var details = item.Controls.OfType<Button>().FirstOrDefault(button => button.Name.StartsWith("ViewTaskDetails", StringComparison.Ordinal));
                if (details is not null)
                    details.Location = new Point(76, 112);
                var save = item.Controls.OfType<Button>().FirstOrDefault(button => button.Name.StartsWith("SaveTask", StringComparison.Ordinal));
                if (save is not null)
                    save.Location = new Point(item.Width - save.Width - 8, 4);
                var title = item.Controls.Find("taskTitleLabel", false).FirstOrDefault();
                if (title is not null)
                    title.MaximumSize = new Size(Math.Max(100, item.Width - 132), 24);
                var description = item.Controls.Find("taskDescriptionLabel", false).FirstOrDefault();
                if (description is not null)
                    description.MaximumSize = new Size(Math.Max(100, item.Width - 100), 32);
            }
            mapCanvas.Bounds = new Rectangle(0, 0, mapPanel.ClientSize.Width, mapPanel.ClientSize.Height);
            var zoomIn = mapPanel.Controls.Find("FindMapZoomInButton", false).FirstOrDefault();
            var zoomOut = mapPanel.Controls.Find("FindMapZoomOutButton", false).FirstOrDefault();
            if (zoomIn is not null) zoomIn.Location = new Point(mapPanel.Width - 43, 10);
            if (zoomOut is not null) zoomOut.Location = new Point(mapPanel.Width - 43, 43);
            var note = mapPanel.Controls.Find("findMapNote", false).FirstOrDefault();
            if (note is not null) note.Location = new Point(10, mapPanel.Height - 48);
            emptyPanel.Bounds = new Rectangle(12, 48, resultsPanel.Width - 24, resultsPanel.Height - 60);
            emptyIcon.Location = new Point(emptyPanel.Width / 2 - 18, emptyPanel.Height / 2 - 80);
            emptyLabel.Location = new Point((emptyPanel.Width - emptyLabel.PreferredWidth) / 2, emptyPanel.Height / 2 - 30);
            if (emptyPanel.Controls.OfType<Button>().FirstOrDefault() is { } reset)
                reset.Location = new Point((emptyPanel.Width - reset.Width) / 2, emptyPanel.Height / 2 + 10);
            PositionMapPins();
            canvas.Height = 770;
        }

        private void ApplyFilters()
        {
            if (resultCount.IsDisposed || resultsPanel.IsDisposed)
                return;
            var query = taskSearch.Text.Trim();
            var categories = categoryFilters.Where(check => check.Checked).Select(check => check.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var min = int.TryParse(minPrice.Text, out var minimum) ? minimum : 0;
            var max = int.TryParse(maxPrice.Text, out var maximum) ? maximum : int.MaxValue;
            var maxDays = deadlineFilter.SelectedIndex switch { 1 => 3, 2 => 7, 3 => 14, _ => int.MaxValue };
            var barangay = selectedBarangay;
            var filtered = tasks.Where(task =>
                    (query.Length == 0 || task.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || task.Category.Contains(query, StringComparison.OrdinalIgnoreCase) || task.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) &&
                    (categories.Count == 0 || categories.Contains(task.Category)) && task.Price >= min && task.Price <= max && task.Days <= maxDays &&
                    (barangay == "all" || task.Barangay == barangay))
                .ToList();
            if (sortTasks.SelectedIndex == 1) filtered = filtered.OrderBy(task => task.Price).ToList();
            else if (sortTasks.SelectedIndex == 2) filtered = filtered.OrderByDescending(task => task.Price).ToList();
            else if (sortTasks.SelectedIndex == 3) filtered = filtered.OrderBy(task => task.Days).ToList();
            foreach (var task in tasks)
            {
                taskRows[task].Visible = filtered.Contains(task);
                mapPins[task].Visible = filtered.Contains(task);
            }
            foreach (var task in filtered)
                taskList.Controls.SetChildIndex(taskRows[task], filtered.IndexOf(task));
            resultCount.Text = filtered.Count.ToString();
            var caption = resultsPanel.Controls.Find("findResultsCaption", false).FirstOrDefault();
            if (caption is not null)
                caption.Text = $"{filtered.Count} available tasks";
            emptyPanel.Visible = filtered.Count == 0;
            listPanel.Visible = filtered.Count > 0 && !mapOnly;
            mapPanel.Visible = filtered.Count > 0 && !listOnly;
            if (nearbyOnly.Checked && barangay == "all")
                ShowToast("Select a barangay to use Nearby only.");
            LayoutPage();
        }

        private void ResetFilters()
        {
            foreach (var check in categoryFilters)
                check.Checked = false;
            minPrice.Clear();
            maxPrice.Clear();
            deadlineFilter.SelectedIndex = 0;
            barangayFilter.SelectedIndex = 0;
            quickBarangay.SelectedIndex = 0;
            selectedBarangay = "all";
            nearbyOnly.Checked = false;
            taskSearch.Clear();
            sortTasks.SelectedIndex = 0;
            ApplyFilters();
            ShowToast("Filters reset.");
        }

        private void SetView(string view)
        {
            listOnly = view == "list";
            mapOnly = view == "map";
            listViewButton.BackColor = listOnly ? Color.FromArgb(232, 243, 255) : Color.White;
            mapViewButton.BackColor = mapOnly ? Color.FromArgb(232, 243, 255) : Color.White;
            LayoutPage();
        }

        private void SelectTask(TaskRecord task)
        {
            foreach (var pair in taskRows)
                pair.Value.BackColor = ReferenceEquals(pair.Key, task) ? Color.FromArgb(242, 248, 255) : Color.White;
            foreach (var pair in mapPins)
                pair.Value.ForeColor = ReferenceEquals(pair.Key, task) ? Color.FromArgb(242, 117, 47) : Blue;
        }

        private void PositionMapPins()
        {
            if (mapCanvas.ClientSize.Width <= 0 || mapCanvas.ClientSize.Height <= 0)
                return;
            foreach (var (task, pin) in mapPins)
            {
                var x = (int)(mapCanvas.ClientSize.Width * task.MapPoint.X / 100.0 * mapZoom / 100.0);
                var y = (int)(mapCanvas.ClientSize.Height * task.MapPoint.Y / 100.0 * mapZoom / 100.0);
                pin.Location = new Point(Math.Clamp(x - pin.Width / 2, 0, mapCanvas.ClientSize.Width - pin.Width), Math.Clamp(y - pin.Height / 2, 0, mapCanvas.ClientSize.Height - pin.Height));
                pin.BringToFront();
            }
            mapCanvas.Invalidate();
        }

        private void OpenTaskDetails(TaskRecord task)
        {
            var details = new UserTaskDetailsForm(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack, () =>
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            });
            details.FormClosed += (_, _) =>
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            };
            Hide();
            details.Show();
        }

        private void HandleNavigation(string name)
        {
            if (name == "Dashboard")
            {
                var dashboard = new UserDashboardForm();
                dashboard.FormClosed += (_, _) => { if (!IsDisposed) { Show(); Activate(); } };
                Hide();
                dashboard.Show();
            }
            else if (name == "Find Tasks")
                return;
            else if (name == "Post Task")
            {
                OpenPostTask();
                return;
            }
            else if (name == "My Tasks")
            {
                Hide();
                var myTasks = new MyTasksForm(() => { if (!IsDisposed) { Show(); Activate(); } });
                myTasks.Show();
                return;
            }
            else if (name == "Log out")
                Close();
            else
                ShowToast($"{name} is not implemented yet in the native User section.");
        }

        private void OpenPostTask()
        {
            Hide();
            var postTask = new PostTaskForm(() =>
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            });
            postTask.Show();
        }

        private void ShowToast(string message)
        {
            toast.Text = message;
            toast.Visible = true;
            toast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private void Position(string name, int x, int y)
        {
            var control = canvas.Controls.Find(name, false).FirstOrDefault();
            if (control is not null)
                control.Location = new Point(x, y);
        }

        private void AddFilterHeading(string text, int top)
        {
            var label = MakeLabel(text, 8, Navy, true);
            label.Location = new Point(14, top);
            filterPanel.Controls.Add(label);
        }

        private static void AddDivider(Control parent, int top)
        {
            parent.Controls.Add(new Panel { Height = 1, BackColor = Line, Location = new Point(12, top), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top });
        }

        private static Button MakeNavButton(string text, bool active)
        {
            var glyph = text switch { "Dashboard" => "⌂", "Find Tasks" => "⌕", "Post Task" => "＋", "My Tasks" => "▤", "Messages" => "▱", "Notifications" => "♧", "Profile" => "◉", _ => "↪" };
            var button = new Button { Text = $"{glyph}   {text}", Size = new Size(196, 39), FlatStyle = FlatStyle.Flat, BackColor = active ? Color.FromArgb(31, 63, 108) : Color.Transparent, ForeColor = Color.FromArgb(222, 234, 250), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button MakeAction(string text, string name, bool primary, int width)
        {
            var button = new Button { Name = name, Text = text, Size = new Size(width, 38), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderColor = primary ? Blue : Line;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            return button;
        }

        private static Button MakeSmallButton(string text, string name, int width, int height)
        {
            var button = new Button { Name = name, Text = text, Size = new Size(width, height), FlatStyle = FlatStyle.Flat, BackColor = PageColor, ForeColor = Muted, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button MakeTextButton(string text, string name)
        {
            var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Button MakeViewButton(string text, string name)
        {
            var button = new Button { Name = name, Text = text, Size = new Size(32, 30), FlatStyle = FlatStyle.Flat, BackColor = name.Contains("List", StringComparison.Ordinal) ? Color.FromArgb(232, 243, 255) : Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 10F, FontStyle.Bold), UseVisualStyleBackColor = false, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderColor = Line;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private static Button MakeMapButton(string text, string name)
        {
            var button = new Button { Name = name, Text = text, Size = new Size(30, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Navy, Font = new Font("Segoe UI", 12F, FontStyle.Bold), UseVisualStyleBackColor = false, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderColor = Line;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private static Label MakeLabel(string text, float size, Color color, bool bold = false)
        {
            return new Label { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
