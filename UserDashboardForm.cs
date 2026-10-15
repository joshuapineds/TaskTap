namespace TaskTap
{
    public sealed class UserDashboardForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(8, 38, 87);
        private static readonly Color SidebarNavy = Color.FromArgb(8, 35, 77);
        private static readonly Color Blue = Color.FromArgb(8, 117, 232);
        private static readonly Color Muted = Color.FromArgb(113, 131, 155);
        private static readonly Color Page = Color.FromArgb(244, 248, 252);
        private static readonly Color Line = Color.FromArgb(225, 233, 241);
        private readonly Panel sidebar = new();
        private readonly Panel mainArea = new();
        private readonly Panel topbar = new();
        private readonly Panel scrollHost = new();
        private readonly Panel page = new();
        private readonly Label toast = new();
        private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
        private readonly FlowLayoutPanel taskList = new();
        private readonly FlowLayoutPanel activityList = new();
        private Panel taskCard = null!;
        private Panel activityCard = null!;
        private Panel helpCard = null!;
        private Button[] navButtons = [];

        private sealed record DashboardTask(string Title, string Category, int Price, string Barangay, int Days, string Deadline, string Icon, string Description, Color IconBack);

        private static readonly DashboardTask[] Tasks =
        [
            new("Fix a leaking faucet", "Repairs", 600, "176", 1, "Oct 8, 2026", "🔧", "Basic plumbing assistance needed for a leaking kitchen faucet.", Color.FromArgb(219, 234, 254)),
            new("Math Tutor (Grade 10)", "Tutoring", 300, "178", 3, "Oct 10, 2026", "📚", "Grade 10 math tutoring for a student in the community.", Color.FromArgb(220, 252, 231)),
            new("Grocery Shopping", "Shopping", 450, "179", 1, "Oct 11, 2026", "🛍️", "Pick up a short grocery list and deliver it locally.", Color.FromArgb(254, 249, 195))
        ];

        public UserDashboardForm()
        {
            BuildPage();
        }

        private void BuildPage()
        {
            Text = "TaskTap — User Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1080, 720);
            ClientSize = new Size(1540, 900);
            BackColor = Page;
            Font = new Font("Segoe UI", 9F);

            mainArea.Dock = DockStyle.Fill;
            mainArea.BackColor = Page;
            Controls.Add(mainArea);

            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 220;
            sidebar.BackColor = SidebarNavy;
            Controls.Add(sidebar);
            BuildSidebar();

            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = Page;
            scrollHost.Resize += (_, _) => LayoutPage();
            mainArea.Controls.Add(scrollHost);
            BuildTopbar();
            BuildDashboardContent();

            toast.Name = "dashboardToast";
            toast.Size = new Size(430, 42);
            toast.BackColor = Navy;
            toast.ForeColor = Color.White;
            toast.TextAlign = ContentAlignment.MiddleCenter;
            toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toast.Visible = false;
            toast.Anchor = AnchorStyles.Bottom;
            Controls.Add(toast);
            toast.BringToFront();
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toast.Visible = false;
            };
            Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
            LayoutPage();
        }

        private void BuildSidebar()
        {
            var brand = new Label
            {
                Text = "● TaskTap",
                Location = new Point(22, 22),
                Size = new Size(180, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            sidebar.Controls.Add(brand);

            var nav = new[]
            {
                ("⌂", "Dashboard"), ("⌕", "Find Tasks"), ("＋", "Post Task"), ("▤", "My Tasks"),
                ("▱", "Messages"), ("♧", "Notifications"), ("◉", "Profile")
            };
            navButtons = new Button[nav.Length];
            for (var index = 0; index < nav.Length; index++)
            {
                var item = nav[index];
                var button = MakeSidebarButton($"{item.Item1}   {item.Item2}", item.Item2);
                button.Location = new Point(12, 76 + index * 43);
                if (index == 0)
                    button.BackColor = Color.FromArgb(31, 63, 108);
                button.Click += (_, _) => HandleNavigation(item.Item2);
                navButtons[index] = button;
                sidebar.Controls.Add(button);
            }

            var accountLabel = new Label
            {
                Text = "ACCOUNT",
                Location = new Point(23, 393),
                AutoSize = true,
                ForeColor = Color.FromArgb(126, 159, 198),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            sidebar.Controls.Add(accountLabel);
            var logout = MakeSidebarButton("↪   Log out", "LogOutButton");
            logout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            logout.Location = new Point(12, ClientSize.Height - 66);
            logout.Click += (_, _) => Close();
            sidebar.Controls.Add(logout);
            sidebar.Resize += (_, _) => logout.Location = new Point(12, sidebar.ClientSize.Height - logout.Height - 18);
        }

        private void BuildTopbar()
        {
            topbar.Dock = DockStyle.Top;
            topbar.Height = 62;
            topbar.BackColor = Color.White;
            topbar.Paint += (_, e) =>
            {
                using var pen = new Pen(Line);
                e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
            };
            var search = MakeSmallButton("⌕", "DashboardSearchButton", 42, 38);
            search.Location = new Point(24, 12);
            search.Click += (_, _) => ShowToast("Dashboard search is a frontend demo action.");
            topbar.Controls.Add(search);
            var notifications = MakeSmallButton("♧  •", "DashboardNotificationsButton", 48, 38);
            notifications.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            notifications.Location = new Point(topbar.ClientSize.Width - 242, 12);
            notifications.Click += (_, _) => MessageBox.Show("Sample notifications:\n\n• Maria accepted your task request.\n• Juan sent you a new message.\n• Payment of ₱600 was recorded.", "Notifications (Demo)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            topbar.Controls.Add(notifications);
            var avatar = new Label
            {
                Text = "JD",
                Location = new Point(topbar.ClientSize.Width - 190, 13),
                Size = new Size(36, 36),
                BackColor = Color.FromArgb(224, 240, 255),
                ForeColor = Blue,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topbar.Controls.Add(avatar);
            var user = new Label
            {
                Text = "Joshua D. Pineda\nStandard User",
                Location = new Point(topbar.ClientSize.Width - 146, 10),
                Size = new Size(135, 43),
                ForeColor = Navy,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            user.Click += (_, _) => ShowToast("Profile menu is a frontend demo action.");
            topbar.Controls.Add(user);
            topbar.Resize += (_, _) =>
            {
                notifications.Location = new Point(topbar.ClientSize.Width - 242, 12);
                avatar.Location = new Point(topbar.ClientSize.Width - 190, 13);
                user.Location = new Point(topbar.ClientSize.Width - 146, 10);
            };
        }

        private void BuildDashboardContent()
        {
            page.Name = "dashboardContentCanvas";
            page.BackColor = Page;
            scrollHost.Controls.Add(page);

            var eyebrow = MakeLabel("YOUR WORKSPACE", 9, Blue, true);
            eyebrow.Name = "dashboardEyebrow";
            page.Controls.Add(eyebrow);
            var title = MakeLabel("Good morning, Joshua!", 28, Navy, true);
            title.Name = "dashboardTitle";
            page.Controls.Add(title);
            var subtitle = MakeLabel("Here’s what’s happening with your tasks.", 11, Muted);
            subtitle.Name = "dashboardSubtitle";
            page.Controls.Add(subtitle);
            var find = MakeAction("⌕  Find a Task", "DashboardFindTaskButton", false, 124);
            find.Click += (_, _) => OpenBrowseTasks();
            page.Controls.Add(find);
            var post = MakeAction("＋  Post a Task", "DashboardPostTaskButton", true, 124);
            post.Click += (_, _) => OpenPostTask();
            page.Controls.Add(post);

            AddStatCard("▤", "3", "Posted Tasks", Color.FromArgb(232, 243, 255), Blue, 0);
            AddStatCard("✓", "2", "Active Tasks", Color.FromArgb(230, 248, 241), Color.FromArgb(32, 175, 132), 1);
            AddStatCard("₱", "₱1,200", "Total Earned", Color.FromArgb(255, 241, 231), Color.FromArgb(240, 132, 58), 2);

            taskCard = MakeCard("Tasks for You", "DashboardTasksCard");
            taskList.Name = "dashboardTaskList";
            taskList.Location = new Point(16, 52);
            taskList.Size = new Size(660, 360);
            taskList.FlowDirection = FlowDirection.TopDown;
            taskList.WrapContents = false;
            taskList.AutoScroll = true;
            taskList.BackColor = Color.White;
            taskCard.Controls.Add(taskList);
            for (var index = 0; index < Tasks.Length; index++)
                AddTaskRow(Tasks[index], index);
            var viewAll = MakeTextButton("View All →", "DashboardViewAllTasksButton");
            viewAll.Location = new Point(570, 13);
            viewAll.Click += (_, _) => OpenBrowseTasks();
            taskCard.Controls.Add(viewAll);
            page.Controls.Add(taskCard);

            activityCard = MakeCard("Recent Activity", "DashboardActivityCard");
            var activityViewAll = MakeTextButton("View All →", "DashboardViewAllActivityButton");
            activityViewAll.Location = new Point(250, 13);
            activityViewAll.Click += (_, _) => ShowToast("Activity history is not implemented yet in the native User section.");
            activityCard.Controls.Add(activityViewAll);
            activityList.Name = "dashboardActivityList";
            activityList.Location = new Point(16, 52);
            activityList.Size = new Size(300, 300);
            activityList.FlowDirection = FlowDirection.TopDown;
            activityList.WrapContents = false;
            activityList.BackColor = Color.White;
            activityCard.Controls.Add(activityList);
            AddActivity("✓", "Task request accepted", "by Maria Santos", "2 hours ago", Color.FromArgb(230, 248, 241), Color.FromArgb(32, 175, 132));
            AddActivity("●", "New message", "from Juan Dela Cruz", "3 hours ago", Color.FromArgb(238, 245, 252), Navy);
            AddActivity("₱", "Payment recorded", "₱600 · Oct 6, 2026", "5 hours ago", Color.FromArgb(255, 241, 231), Color.FromArgb(240, 132, 58));
            AddActivity("✓", "Task completed", "House Cleaning", "1 day ago", Color.FromArgb(230, 248, 241), Color.FromArgb(32, 175, 132));
            page.Controls.Add(activityCard);

            helpCard = new Panel { Name = "dashboardPostCallout", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var helpTitle = MakeLabel("Need help from someone nearby?", 13, Navy, true);
            helpTitle.Location = new Point(16, 15);
            helpCard.Controls.Add(helpTitle);
            var helpBody = MakeLabel("Post a task with the details, location, compensation, and deadline so people in your community can request to help.", 9, Muted);
            helpBody.Location = new Point(16, 43);
            helpBody.MaximumSize = new Size(310, 54);
            helpCard.Controls.Add(helpBody);
            var helpPost = MakeAction("＋  Post a Task", "DashboardCalloutPostTaskButton", true, 124);
            helpPost.Location = new Point(16, 100);
            helpPost.Click += (_, _) => OpenPostTask();
            helpCard.Controls.Add(helpPost);
            page.Controls.Add(helpCard);
        }

        private void AddStatCard(string icon, string value, string caption, Color iconBack, Color iconColor, int index)
        {
            var card = new Panel
            {
                Name = $"dashboardStatCard{index + 1}",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var iconLabel = new Label
            {
                Text = icon,
                Location = new Point(16, 17),
                Size = new Size(40, 40),
                BackColor = iconBack,
                ForeColor = iconColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };
            var valueLabel = MakeLabel(value, 21, Navy, true);
            valueLabel.Location = new Point(64, 14);
            var captionLabel = MakeLabel(caption, 9, Muted);
            captionLabel.Location = new Point(65, 42);
            card.Controls.Add(iconLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(captionLabel);
            page.Controls.Add(card);
        }

        private void AddTaskRow(DashboardTask task, int index)
        {
            var row = new Panel
            {
                Name = $"dashboardTask{index + 1}",
                Size = new Size(642, 81),
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            var icon = new Label
            {
                Text = task.Icon,
                Location = new Point(0, 13),
                Size = new Size(46, 46),
                BackColor = Color.FromArgb(235, 244, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 19F)
            };
            row.Controls.Add(icon);
            var title = MakeLabel(task.Title, 11, Navy, true);
            title.Location = new Point(58, 10);
            row.Controls.Add(title);
            var meta = MakeLabel($"₱{task.Price} · {task.Category} · Brgy. {task.Barangay}\nDeadline: {task.Deadline}", 8, Muted);
            meta.Location = new Point(58, 32);
            meta.Size = new Size(310, 38);
            row.Controls.Add(meta);
            var price = MakeLabel($"₱{task.Price}", 11, Navy, true);
            price.Location = new Point(430, 28);
            row.Controls.Add(price);
            var details = MakeAction("View Details", $"DashboardTaskDetailsButton{index + 1}", false, 86);
            details.Size = new Size(86, 32);
            details.Location = new Point(493, 22);
            details.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            details.Click += (_, _) => OpenTaskDetails(task);
            row.Controls.Add(details);
            var divider = new Panel { BackColor = Line, Location = new Point(0, 79), Size = new Size(642, 1) };
            if (index < Tasks.Length - 1)
                row.Controls.Add(divider);
            taskList.Controls.Add(row);
        }

        private void AddActivity(string icon, string title, string detail, string time, Color iconBack, Color iconColor)
        {
            var item = new Panel { Size = new Size(310, 68), BackColor = Color.White, Margin = new Padding(0) };
            var badge = new Label
            {
                Text = icon,
                Location = new Point(0, 8),
                Size = new Size(30, 30),
                BackColor = iconBack,
                ForeColor = iconColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            item.Controls.Add(badge);
            var heading = MakeLabel(title, 9, Navy, true);
            heading.Location = new Point(40, 5);
            item.Controls.Add(heading);
            var description = MakeLabel(detail, 8, Muted);
            description.Location = new Point(40, 24);
            item.Controls.Add(description);
            var when = MakeLabel(time, 8, Muted);
            when.Location = new Point(40, 41);
            item.Controls.Add(when);
            item.Controls.Add(new Panel { BackColor = Line, Location = new Point(0, 66), Size = new Size(310, 1) });
            activityList.Controls.Add(item);
        }

        private void LayoutPage()
        {
            if (scrollHost.ClientSize.Width <= 0 || page.Controls.Count == 0)
                return;
            var width = Math.Max(800, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
            page.Size = new Size(width, 720);
            page.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);
            var left = 24;
            var right = width - 24;
            var contentWidth = width - 48;
            Position("dashboardEyebrow", left, 24);
            Position("dashboardTitle", left, 42);
            Position("dashboardSubtitle", left, 83);
            var find = Find<Button>("DashboardFindTaskButton");
            var post = Find<Button>("DashboardPostTaskButton");
            if (find is not null)
                find.Location = new Point(right - 270, 52);
            if (post is not null)
                post.Location = new Point(right - 136, 52);

            var statGap = 14;
            var statWidth = (contentWidth - statGap * 2) / 3;
            for (var index = 0; index < 3; index++)
            {
                var stat = Find<Panel>($"dashboardStatCard{index + 1}");
                if (stat is not null)
                {
                    stat.Location = new Point(left + index * (statWidth + statGap), 126);
                    stat.Size = new Size(statWidth, 74);
                }
            }

            var rightWidth = Math.Max(300, Math.Min(360, (contentWidth - 16) / 3));
            var taskWidth = contentWidth - rightWidth - 16;
            var contentTop = 218;
            taskCard.Location = new Point(left, contentTop);
            taskCard.Size = new Size(taskWidth, 450);
            taskList.Size = new Size(taskWidth - 32, 378);
            foreach (Control control in taskList.Controls)
            {
                if (control is not Panel row)
                    continue;
                row.Width = taskList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                var meta = row.Controls.OfType<Label>().FirstOrDefault(label => label.Location.Y == 32);
                if (meta is not null)
                    meta.Width = Math.Max(100, row.Width - 360);
                var price = row.Controls.OfType<Label>().FirstOrDefault(label => label.Location.Y == 28);
                if (price is not null)
                    price.Location = new Point(row.Width - 212, 28);
                var details = row.Controls.OfType<Button>().FirstOrDefault();
                if (details is not null)
                    details.Location = new Point(row.Width - details.Width - 5, 22);
                var divider = row.Controls.OfType<Panel>().FirstOrDefault();
                if (divider is not null)
                    divider.Width = row.Width;
            }
            var all = Find<Button>("DashboardViewAllTasksButton");
            if (all is not null)
                all.Location = new Point(taskWidth - all.Width - 15, 13);
            activityCard.Location = new Point(left + taskWidth + 16, contentTop);
            activityCard.Size = new Size(rightWidth, 298);
            var activityAll = Find<Button>("DashboardViewAllActivityButton");
            if (activityAll is not null)
                activityAll.Location = new Point(rightWidth - activityAll.Width - 14, 13);
            activityList.Size = new Size(rightWidth - 32, 230);
            helpCard.Location = new Point(left + taskWidth + 16, contentTop + 314);
            helpCard.Size = new Size(rightWidth, 136);
            var body = helpCard.Controls.OfType<Label>().FirstOrDefault(label => label.Location.Y == 43);
            if (body is not null)
                body.MaximumSize = new Size(rightWidth - 32, 52);
            page.Height = contentTop + 475;
            toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
        }

        private void Position(string name, int x, int y)
        {
            var control = page.Controls.Find(name, false).FirstOrDefault();
            if (control is not null)
                control.Location = new Point(x, y);
        }

        private void HandleNavigation(string destination)
        {
            if (destination == "Find Tasks")
            {
                OpenBrowseTasks();
                return;
            }
            if (destination == "Dashboard")
                return;
            if (destination == "Post Task")
            {
                OpenPostTask();
                return;
            }
            if (destination == "My Tasks")
            {
                OpenMyTasks();
                return;
            }
            ShowToast(destination == "Post Task"
                ? "Post Task is not implemented yet in the native User section."
                : $"{destination} is not implemented yet in the native User section.");
        }

        private void OpenMyTasks()
        {
            Hide();
            var myTasks = new MyTasksForm(() =>
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            });
            myTasks.Show();
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

        private void OpenBrowseTasks()
        {
            var browse = new FindTasksForm();
            browse.FormClosed += (_, _) =>
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            };
            Hide();
            browse.Show();
        }

        private void OpenTaskDetails(DashboardTask task)
        {
            var details = new TaskDetailsForm(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack);
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

        private void ShowToast(string message)
        {
            toast.Text = message;
            toast.Visible = true;
            toast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private static Button MakeSidebarButton(string text, string name)
        {
            return new Button
            {
                Name = name,
                Text = text,
                Size = new Size(196, 39),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(222, 234, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            }.WithFlatBorder();
        }

        private static Button MakeSmallButton(string text, string name, int width, int height)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = Page,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button MakeAction(string text, string name, bool primary, int width)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(width, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? Blue : Color.White,
                ForeColor = primary ? Color.White : Navy,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = primary ? Blue : Line;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            return button;
        }

        private static Button MakeTextButton(string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Blue,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Panel MakeCard(string title, string name)
        {
            var card = new Panel { Name = name, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var heading = MakeLabel(title, 13, Navy, true);
            heading.Location = new Point(16, 14);
            card.Controls.Add(heading);
            return card;
        }

        private static Label MakeLabel(string text, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = color,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private void InitializeComponent()
        {

        }

        private TControl? Find<TControl>(string name) where TControl : Control => page.Controls.Find(name, false).FirstOrDefault() as TControl;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            base.OnFormClosed(e);
        }
    }

    internal static class DashboardButtonExtensions
    {
        public static Button WithFlatBorder(this Button button)
        {
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
