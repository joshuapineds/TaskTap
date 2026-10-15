using System.Drawing.Drawing2D;

namespace TaskTap
{
    public class PublicInfoFormBase : Form
    {
        protected static readonly Color PageColor = Color.White;
        protected static readonly Color NavyColor = Color.FromArgb(8, 38, 87);
        protected static readonly Color BlueColor = Color.FromArgb(8, 117, 232);
        protected static readonly Color OrangeColor = Color.FromArgb(255, 138, 61);
        protected static readonly Color MutedColor = Color.FromArgb(113, 131, 155);
        protected static readonly Color LineColor = Color.FromArgb(219, 229, 238);
        protected readonly Panel contentCanvas = new();
        private readonly Panel header = new();
        private readonly Panel scrollHost = new();
        private const int CanvasWidth = 1120;

        public PublicInfoFormBase() : this("Public Information")
        {
        }

        protected PublicInfoFormBase(string title)
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
            scrollHost.Resize += (_, _) => LayoutCanvas();
            Controls.Add(scrollHost);
            BuildHeader();
            contentCanvas.Width = CanvasWidth;
            contentCanvas.BackColor = PageColor;
            scrollHost.Controls.Add(contentCanvas);
        }

        protected void AddSection(Control section, int height, int top)
        {
            section.Location = new Point(24, top);
            section.Size = new Size(CanvasWidth - 48, height);
            section.Margin = Padding.Empty;
            contentCanvas.Controls.Add(section);
            contentCanvas.Height = Math.Max(contentCanvas.Height, section.Bottom + 35);
        }

        protected static Label MakeLabel(string text, float size, Color color, bool bold = false)
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

        protected static Button MakeLink(string text, string name, bool active = false)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = active ? BlueColor : MutedColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 10, 3, 0),
                Padding = new Padding(7, 4, 7, 4),
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        protected static Button MakeAction(string text, string name, bool primary)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(primary ? 154 : 150, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? BlueColor : Color.White,
                ForeColor = primary ? Color.White : NavyColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = primary ? BlueColor : LineColor;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            return button;
        }

        protected void NavigateTo(string destination)
        {
            Form next = destination switch
            {
                "Browse Tasks" => new BrowseTasksForm(),
                "How It Works" => new HowItWorksForm(),
                "About" => new AboutForm(),
                "Login" => new Form2(false),
                _ => new Form2(true)
            };
            next.FormClosed += (_, _) =>
            {
                Show();
                Activate();
            };
            Hide();
            next.Show();
        }

        private void BuildHeader()
        {
            header.Dock = DockStyle.Top;
            header.Height = 66;
            header.BackColor = Color.White;
            header.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 237, 245));
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };
            var brand = new Panel { Location = new Point(28, 17), Size = new Size(150, 34), BackColor = Color.Transparent };
            brand.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var blue = new SolidBrush(BlueColor);
                using var orange = new SolidBrush(OrangeColor);
                using var white = new SolidBrush(Color.White);
                e.Graphics.FillEllipse(blue, 0, 0, 18, 18);
                e.Graphics.FillEllipse(orange, 10, 13, 18, 18);
                e.Graphics.FillEllipse(white, 6, 6, 6, 6);
                e.Graphics.FillEllipse(white, 16, 19, 6, 6);
                TextRenderer.DrawText(e.Graphics, "TaskTap", new Font("Segoe UI", 16F, FontStyle.Bold), new Point(34, 3), NavyColor);
            };
            header.Controls.Add(brand);
            var nav = new FlowLayoutPanel
            {
                Name = "publicNavigation",
                Location = new Point(300, 10),
                Size = new Size(510, 45),
                WrapContents = false,
                BackColor = Color.White
            };
            nav.Controls.Add(MakeLink("Browse Tasks", "PublicBrowseButton"));
            nav.Controls.Add(MakeLink("How It Works", "PublicHowItWorksButton", this is HowItWorksForm));
            nav.Controls.Add(MakeLink("About", "PublicAboutButton", this is AboutForm));
            foreach (Button link in nav.Controls)
                link.Click += (_, _) => NavigateTo(link.Text);
            header.Controls.Add(nav);
            var signup = MakeAction("Sign Up", "PublicSignUpButton", true);
            signup.Size = new Size(90, 36);
            signup.Location = new Point(Width - 122, 14);
            signup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            signup.Click += (_, _) => NavigateTo("Sign Up");
            header.Controls.Add(signup);
            var login = MakeAction("Login", "PublicLoginButton", false);
            login.Size = new Size(72, 36);
            login.Location = new Point(Width - 204, 14);
            login.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            login.Click += (_, _) => NavigateTo("Login");
            header.Controls.Add(login);
            Controls.Add(header);
            header.Resize += (_, _) =>
            {
                signup.Location = new Point(header.ClientSize.Width - signup.Width - 24, 14);
                login.Location = new Point(signup.Left - login.Width - 10, 14);
            };
        }

        private void LayoutCanvas()
        {
            if (scrollHost.ClientSize.Width <= 0)
                return;
            contentCanvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - CanvasWidth) / 2), 0);
            contentCanvas.Height = contentCanvas.Controls.Cast<Control>().Select(control => control.Bottom).DefaultIfEmpty(0).Max() + 35;
        }
    }
}
