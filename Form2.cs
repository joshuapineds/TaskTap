using System.Drawing.Drawing2D;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;

namespace TaskTap
{
    public partial class Form2 : Form
    {
        // Connection string is read from appsettings.json at runtime. See appsettings.json.template
        private static readonly string MySqlConnectionString = LoadConnectionString();

        private static string LoadConnectionString()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(path))
                    return string.Empty;
                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("MySqlConnectionString", out var conn))
                    return conn.GetString() ?? string.Empty;
            }
            catch
            {
                // ignore and return empty
            }
            return string.Empty;
        }

        private static string HashPassword(string password)
        {
            // PBKDF2 with HMACSHA256
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            const int iterations = 100_000;
            using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);
            // store as iterations.salt.hash (base64)
            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string stored, string password)
        {
            try
            {
                var parts = stored.Split('.');
                if (parts.Length != 3)
                    return false;
                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var hash = Convert.FromBase64String(parts[2]);
                using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var candidate = derive.GetBytes(hash.Length);
                return CryptographicOperations.FixedTimeEquals(candidate, hash);
            }
            catch
            {
                return false;
            }
        }
        private static readonly Color PageColor = Color.FromArgb(244, 249, 253);
        private static readonly Color NavyColor = Color.FromArgb(9, 35, 76);
        private static readonly Color BlueColor = Color.FromArgb(8, 112, 229);
        private static readonly Color MutedColor = Color.FromArgb(111, 132, 158);
        private readonly Panel pageHost = new();
        private readonly Panel pageShell = new();
        private readonly Panel brandPanel = new();
        private readonly Panel formPanel = new();
        private readonly Panel illustrationPanel = new();
        private readonly Panel authBox = new();
        private readonly Panel loginView = new();
        private readonly Panel registerView = new();
        private readonly Panel tabUnderline = new();
        private Label pageTitle = null!;
        private Label authModeTitle = null!;
        private Label pageSubtitle = null!;
        private Label feedbackLabel = null!;
        private Label toastLabel = null!;
        private TextBox loginIdentityTextBox = null!;
        private TextBox loginPasswordTextBox = null!;
        private TextBox registerFirstNameTextBox = null!;
        private TextBox registerLastNameTextBox = null!;
        private TextBox registerEmailTextBox = null!;
        private TextBox registerPhoneTextBox = null!;
        private TextBox registerPasswordTextBox = null!;
        private TextBox registerConfirmPasswordTextBox = null!;
        private ComboBox registerBarangayComboBox = null!;
        private CheckBox rememberCheckBox = null!;
        private CheckBox termsCheckBox = null!;
        private CheckBox updatesCheckBox = null!;
        private Button loginTabButton = null!;
        private Button registerTabButton = null!;
        private Button loginSwitchButton = null!;
        private Button registerSwitchButton = null!;
        private Button loginSubmitButton = null!;
        private Button registerSubmitButton = null!;
        private System.Windows.Forms.Timer toastTimer = null!;
        private bool registerMode;

        public Form2() : this(false)
        {
        }

        public Form2(bool registerInitially)
        {
            InitializeComponent();
            registerMode = registerInitially;
            BuildAuthenticationPage();
        }

        private void BuildAuthenticationPage()
        {
            Text = "TaskTap — Login / Register";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = true;
            MaximizeBox = true;
            MinimumSize = new Size(1000, 780);
            ClientSize = new Size(1460, 840);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            var topBar = new Panel
            {
                Name = "authenticationHeader",
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 0, 1)
            };
            topBar.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(222, 231, 241));
                e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };
            var brandMark = new BrandMark { Name = "authenticationBrandMark", Location = new Point(22, 10), Size = new Size(30, 34) };
            topBar.Controls.Add(brandMark);
            var brandName = new Label
            {
                Name = "authenticationBrandName",
                Text = "TaskTap",
                AutoSize = true,
                Location = new Point(56, 15),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = NavyColor
            };
            topBar.Controls.Add(brandName);
            var backButton = MakeLinkButton("← Back to TaskTap", "BackToLandingButton");
            backButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            backButton.Location = new Point(Width - 185, 14);
            backButton.Click += (_, _) => Close();
            topBar.Controls.Add(backButton);
            topBar.Resize += (_, _) => backButton.Location = new Point(topBar.ClientSize.Width - backButton.Width - 25, 14);
            Controls.Add(topBar);

            pageHost.Name = "authenticationPageHost";
            pageHost.Dock = DockStyle.Fill;
            pageHost.BackColor = PageColor;
            pageHost.Resize += (_, _) => LayoutPage();
            Controls.Add(pageHost);
            pageHost.SendToBack();
            topBar.BringToFront();

            pageShell.Name = "authenticationSplitShell";
            pageShell.BackColor = Color.White;
            pageHost.Controls.Add(pageShell);

            brandPanel.Name = "authenticationBrandPanel";
            brandPanel.BackColor = Color.FromArgb(235, 247, 255);
            pageShell.Controls.Add(brandPanel);
            BuildBrandPanel();

            formPanel.Name = "authenticationFormPanel";
            formPanel.BackColor = Color.White;
            pageShell.Controls.Add(formPanel);
            BuildAuthPanel();

            toastLabel = new Label
            {
                Name = "authenticationToast",
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
            toastTimer = new System.Windows.Forms.Timer { Interval = 2300 };
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toastLabel.Visible = false;
            };

            LayoutPage();
            SetMode(registerMode);
        }

        private void BuildBrandPanel()
        {
            var bluePin = new Label
            {
                Name = "blueCommunityPin",
                Text = "●",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = BlueColor,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(24, 30),
                Location = new Point(42, 38)
            };
            bluePin.Region = new Region(new Rectangle(0, 0, 24, 26));
            var orangePin = new Label
            {
                Name = "orangeCommunityPin",
                Text = "●",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(255, 134, 57),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(24, 30),
                Location = new Point(57, 48)
            };
            orangePin.Region = new Region(new Rectangle(0, 0, 24, 26));
            brandPanel.Controls.Add(bluePin);
            brandPanel.Controls.Add(orangePin);

            pageTitle = new Label
            {
                Name = "authenticationTagline",
                Text = "Local Tasks.\nReal People.",
                AutoSize = false,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = NavyColor,
                Location = new Point(42, 87),
                Size = new Size(390, 68)
            };
            brandPanel.Controls.Add(pageTitle);
            var accentHeadline = new Label
            {
                Name = "authenticationTaglineAccent",
                Text = "Stronger Communities.",
                AutoSize = false,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 136, 62),
                Location = new Point(42, 153),
                Size = new Size(410, 37)
            };
            brandPanel.Controls.Add(accentHeadline);
            var description = new Label
            {
                Name = "authenticationDescription",
                Text = "Connect with people nearby for small tasks and services — whether you're getting help or earning from your skills.",
                AutoSize = false,
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedColor,
                Location = new Point(43, 196),
                Size = new Size(390, 48)
            };
            brandPanel.Controls.Add(description);

            illustrationPanel.Name = "communityIllustrationPanel";
            illustrationPanel.BackColor = Color.FromArgb(220, 242, 255);
            illustrationPanel.Paint += PaintVillageIllustration;
            brandPanel.Controls.Add(illustrationPanel);

            AddBenefit("Local-first", "Find tasks in your community.", 0);
            AddBenefit("Simple & transparent", "See compensation and deadlines upfront.", 1);
            AddBenefit("Community trust", "Build reputation through completed tasks.", 2);
        }

        private void AddBenefit(string title, string description, int index)
        {
            var y = index * 30;
            var check = new Label
            {
                Name = $"benefitCheck{index + 1}",
                Text = "✓",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(38, 177, 128),
                BackColor = Color.FromArgb(222, 248, 236),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(20, 20),
                Location = new Point(43, y)
            };
            brandPanel.Controls.Add(check);
            var benefit = new Label
            {
                Name = $"benefitText{index + 1}",
                Text = title + "  " + description,
                AutoSize = false,
                ForeColor = MutedColor,
                Font = new Font("Segoe UI", 8F),
                Location = new Point(70, y + 1),
                Size = new Size(365, 20)
            };
            brandPanel.Controls.Add(benefit);
        }

        private void PaintVillageIllustration(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.ClientSize.Width <= 0 || panel.ClientSize.Height <= 0)
                return;

            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = panel.ClientRectangle;
            using var clip = CreateRoundedPath(bounds, 18);
            graphics.SetClip(clip);
            using (var sky = new LinearGradientBrush(bounds, Color.FromArgb(215, 241, 255), Color.FromArgb(242, 250, 255), LinearGradientMode.Vertical))
                graphics.FillRectangle(sky, bounds);

            var scaleX = bounds.Width / 420F;
            var scaleY = bounds.Height / 190F;
            using var transform = new Matrix(scaleX, 0, 0, scaleY, 0, 0);
            graphics.Transform = transform;

            using (var sun = new SolidBrush(Color.FromArgb(255, 211, 99)))
                graphics.FillEllipse(sun, 315, 20, 58, 58);
            DrawCloud(graphics, 42, 28, 72);
            DrawCloud(graphics, 352, 68, 48);

            using (var hillBack = new SolidBrush(Color.FromArgb(164, 218, 165)))
            using (var hillPath = new GraphicsPath())
            {
                hillPath.AddPolygon([new Point(0, 132), new Point(70, 116), new Point(146, 124), new Point(220, 143), new Point(280, 122), new Point(350, 130), new Point(420, 118), new Point(420, 190), new Point(0, 190)]);
                graphics.FillPath(hillBack, hillPath);
            }
            using (var hillFront = new SolidBrush(Color.FromArgb(126, 196, 139)))
            using (var hillPath = new GraphicsPath())
            {
                hillPath.AddPolygon([new Point(160, 190), new Point(228, 153), new Point(305, 127), new Point(365, 136), new Point(420, 130), new Point(420, 190)]);
                graphics.FillPath(hillFront, hillPath);
            }

            DrawHouse(graphics, 60, 99, 48, 43);
            DrawHouse(graphics, 165, 99, 55, 52);
            DrawHouse(graphics, 328, 103, 44, 40);
            DrawTree(graphics, 136, 101);
            DrawTree(graphics, 292, 101);
            DrawLocationMarker(graphics, 257, 96);
            graphics.ResetTransform();
        }

        private static void DrawCloud(Graphics graphics, int x, int y, int width)
        {
            using var brush = new SolidBrush(Color.FromArgb(255, 255, 255));
            graphics.FillEllipse(brush, x, y + 10, width / 2, 25);
            graphics.FillEllipse(brush, x + width / 4, y, width / 2, 38);
            graphics.FillEllipse(brush, x + width / 2, y + 10, width / 2, 25);
        }

        private static void DrawHouse(Graphics graphics, int x, int y, int width, int height)
        {
            using var wall = new SolidBrush(Color.White);
            using var roof = new SolidBrush(Color.FromArgb(235, 164, 107));
            using var trim = new SolidBrush(Color.FromArgb(113, 153, 174));
            graphics.FillPolygon(roof, [new Point(x - 6, y + 10), new Point(x + width / 2, y - 12), new Point(x + width + 6, y + 10)]);
            graphics.FillRectangle(wall, x, y + 10, width, height - 10);
            graphics.DrawRectangle(Pens.LightSteelBlue, x, y + 10, width, height - 10);
            graphics.FillRectangle(trim, x + width / 2 - 6, y + height - 18, 12, 18);
        }

        private static void DrawTree(Graphics graphics, int x, int y)
        {
            using var trunk = new SolidBrush(Color.FromArgb(126, 90, 61));
            using var leaves = new SolidBrush(Color.FromArgb(79, 169, 112));
            graphics.FillRectangle(trunk, x + 12, y + 27, 15, 52);
            graphics.FillEllipse(leaves, x, y, 40, 44);
        }

        private static void DrawLocationMarker(Graphics graphics, int x, int y)
        {
            using var marker = new SolidBrush(BlueColor);
            using var center = new SolidBrush(Color.White);
            graphics.FillEllipse(marker, x, y, 31, 31);
            graphics.FillPolygon(marker, [new Point(x + 3, y + 23), new Point(x + 15, y + 43), new Point(x + 28, y + 22)]);
            graphics.FillEllipse(center, x + 12, y + 11, 7, 7);
        }

        private void BuildAuthPanel()
        {
            authBox.Name = "authenticationFormContent";
            formPanel.Controls.Add(authBox);

            loginTabButton = MakeTab("Login", "LoginModeTabButton");
            registerTabButton = MakeTab("Register", "RegisterModeTabButton");
            loginTabButton.Click += (_, _) => SetMode(false);
            registerTabButton.Click += (_, _) => SetMode(true);
            authBox.Controls.Add(loginTabButton);
            authBox.Controls.Add(registerTabButton);

            var tabRule = new Panel { Name = "authenticationTabRule", BackColor = Color.FromArgb(223, 232, 241), Height = 1 };
            authBox.Controls.Add(tabRule);
            tabUnderline.Name = "authenticationTabUnderline";
            tabUnderline.BackColor = BlueColor;
            authBox.Controls.Add(tabUnderline);

            authModeTitle = MakeLabel("Welcome back!", 21, Color.FromArgb(17, 30, 48), true);
            authModeTitle.Name = "authenticationModeTitle";
            authBox.Controls.Add(authModeTitle);
            pageSubtitle = MakeLabel("Sign in to your account.", 9, MutedColor);
            pageSubtitle.Name = "authenticationModeSubtitle";
            authBox.Controls.Add(pageSubtitle);

            BuildLoginView();
            BuildRegisterView();

            feedbackLabel = MakeLabel(string.Empty, 8, Color.FromArgb(185, 55, 67));
            feedbackLabel.Name = "authenticationFeedback";
            feedbackLabel.Location = new Point(0, 472);
            feedbackLabel.MaximumSize = new Size(370, 18);
            authBox.Controls.Add(feedbackLabel);

            var securityNote = MakeLabel("🔒  Your account information is used to manage your TaskTap profile and tasks.", 7.5F, Color.FromArgb(139, 155, 174));
            securityNote.Name = "authenticationSecurityNote";
            securityNote.TextAlign = ContentAlignment.MiddleCenter;
            authBox.Controls.Add(securityNote);

            var adminPrompt = MakeLabel("Are you an administrator?", 9, NavyColor);
            adminPrompt.Name = "adminSignInPrompt";
            authBox.Controls.Add(adminPrompt);
            var adminButton = MakeLinkButton("Admin sign in  →", "AdminSignInButton");
            adminButton.Name = "AdminSignInButton";
            adminButton.Click += (_, _) => ShowToast("Admin sign-in will be available when the native admin page is added.");
            authBox.Controls.Add(adminButton);
        }

        private void BuildLoginView()
        {
            loginView.Name = "loginAuthenticationView";
            authBox.Controls.Add(loginView);

            AddField(loginView, "Email or Phone Number", "you@example.com", "loginIdentityInput", 0, out loginIdentityTextBox);
            AddField(loginView, "Password", "Enter your password", "loginPasswordInput", 64, out loginPasswordTextBox, true);
            AddPasswordToggle(loginView, loginPasswordTextBox, "loginPasswordToggle", 94);

            rememberCheckBox = new CheckBox
            {
                Name = "rememberMeCheckBox",
                Text = "Remember me",
                AutoSize = true,
                ForeColor = MutedColor,
                Font = new Font("Segoe UI", 8F),
                Location = new Point(0, 132)
            };
            loginView.Controls.Add(rememberCheckBox);
            var forgotButton = MakeLinkButton("Forgot password?", "ForgotPasswordButton");
            forgotButton.Location = new Point(244, 127);
            forgotButton.Click += (_, _) => NavigateToPasswordRecovery();
            loginView.Controls.Add(forgotButton);

            loginSubmitButton = MakePrimaryButton("Login", "LoginSubmitButton");
            loginSubmitButton.Location = new Point(0, 158);
            loginSubmitButton.Click += (_, _) => SubmitLogin();
            loginView.Controls.Add(loginSubmitButton);

            AddDivider(loginView, 213);
            var googleButton = MakeSocialButton("G", "Continue with Google", "GoogleSignInButton");
            googleButton.Location = new Point(0, 236);
            googleButton.Click += (_, _) => ShowToast("Social sign-in is a frontend placeholder for now.");
            loginView.Controls.Add(googleButton);
            var facebookButton = MakeSocialButton("f", "Continue with Facebook", "FacebookSignInButton");
            facebookButton.Location = new Point(0, 278);
            facebookButton.Click += (_, _) => ShowToast("Social sign-in is a frontend placeholder for now.");
            loginView.Controls.Add(facebookButton);

            var noAccount = MakeLabel("Don't have an account?", 8, MutedColor);
            noAccount.Location = new Point(56, 326);
            loginView.Controls.Add(noAccount);
            loginSwitchButton = MakeLinkButton("Sign up", "SwitchToRegisterButton");
            loginSwitchButton.Location = new Point(184, 321);
            loginSwitchButton.Click += (_, _) => SetMode(true);
            loginView.Controls.Add(loginSwitchButton);

            // removed demo-only dashboard shortcut
        }

        private void BuildRegisterView()
        {
            registerView.Name = "registerAuthenticationView";
            authBox.Controls.Add(registerView);

            AddField(registerView, "First Name", "Juan", "registerFirstNameInput", 14, out registerFirstNameTextBox);
            AddField(registerView, "Last Name", "Dela Cruz", "registerLastNameInput", 14, out registerLastNameTextBox);
            AddField(registerView, "Email Address", "you@example.com", "registerEmailInput", 72, out registerEmailTextBox);
            AddField(registerView, "Phone Number", "+63 9XX XXX XXXX", "registerPhoneInput", 130, out registerPhoneTextBox);

            var barangayLabel = MakeLabel("Barangay", 8, Color.FromArgb(83, 101, 126), true);
            barangayLabel.Name = "registerBarangayLabel";
            barangayLabel.Location = new Point(0, 188);
            registerView.Controls.Add(barangayLabel);
            registerBarangayComboBox = new ComboBox
            {
                Name = "registerBarangayComboBox",
                Location = new Point(0, 207),
                Size = new Size(360, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            registerBarangayComboBox.Items.AddRange(["Select your barangay", "Barangay 176", "Barangay 178", "Barangay 179", "Other"]);
            registerBarangayComboBox.SelectedIndex = 0;
            registerView.Controls.Add(registerBarangayComboBox);

            AddField(registerView, "Password", "At least 8 characters", "registerPasswordInput", 244, out registerPasswordTextBox, true);
            AddField(registerView, "Confirm Password", "Re-enter password", "registerConfirmPasswordInput", 244, out registerConfirmPasswordTextBox, true);
            AddPasswordToggle(registerView, registerPasswordTextBox, "registerPasswordToggle", 254);
            AddPasswordToggle(registerView, registerConfirmPasswordTextBox, "registerConfirmPasswordToggle", 254);
            var passwordHint = MakeLabel("Use 8+ characters with a number or special character.", 6.5F, Color.FromArgb(139, 155, 174));
            passwordHint.Name = "registrationPasswordHint";
            passwordHint.Location = new Point(0, 282);
            registerView.Controls.Add(passwordHint);
            var strengthTrack = new FlowLayoutPanel
            {
                Name = "registrationPasswordStrengthIndicator",
                Location = new Point(0, 315),
                Size = new Size(360, 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            for (var segment = 0; segment < 3; segment++)
            {
                strengthTrack.Controls.Add(new Panel
                {
                    Name = $"passwordStrengthSegment{segment + 1}",
                    Size = new Size(114, 3),
                    BackColor = Color.FromArgb(226, 233, 241),
                    Margin = new Padding(0, 0, 3, 0)
                });
            }
            registerView.Controls.Add(strengthTrack);

            termsCheckBox = new CheckBox
            {
                Name = "termsAndGuidelinesCheckBox",
                Text = "I agree to the TaskTap terms and community guidelines.",
                AutoSize = false,
                ForeColor = MutedColor,
                Font = new Font("Segoe UI", 7F),
                Location = new Point(0, 330),
                Size = new Size(360, 18)
            };
            registerView.Controls.Add(termsCheckBox);

            updatesCheckBox = new CheckBox
            {
                Name = "taskTapUpdatesCheckBox",
                Text = "Send me occasional TaskTap updates and helpful community tips.",
                AutoSize = false,
                ForeColor = MutedColor,
                Font = new Font("Segoe UI", 7F),
                Location = new Point(0, 351),
                Size = new Size(360, 18)
            };
            registerView.Controls.Add(updatesCheckBox);

            registerSubmitButton = MakePrimaryButton("Create Account", "RegistrationSubmitButton");
            registerSubmitButton.Text = "Create My Account";
            registerSubmitButton.Location = new Point(0, 378);
            registerSubmitButton.Click += (_, _) => SubmitRegistration();
            registerView.Controls.Add(registerSubmitButton);

            var existingAccount = MakeLabel("Already have an account?", 8, MutedColor);
            existingAccount.Location = new Point(68, 427);
            registerView.Controls.Add(existingAccount);
            registerSwitchButton = MakeLinkButton("Login", "SwitchToLoginButton");
            registerSwitchButton.Location = new Point(211, 422);
            registerSwitchButton.Click += (_, _) => SetMode(false);
            registerView.Controls.Add(registerSwitchButton);
        }

        private void LayoutPage()
        {
            if (pageHost.ClientSize.Width <= 0 || pageHost.ClientSize.Height <= 0 || toastLabel is null)
                return;

            var shellWidth = Math.Min(990, pageHost.ClientSize.Width - 32);
            var shellHeight = pageHost.ClientSize.Height;
            pageShell.Bounds = new Rectangle((pageHost.ClientSize.Width - shellWidth) / 2, 0, shellWidth, shellHeight);
            var leftWidth = (int)(shellWidth * 0.47);
            brandPanel.Bounds = new Rectangle(0, 0, leftWidth, shellHeight);
            formPanel.Bounds = new Rectangle(leftWidth, 0, shellWidth - leftWidth, shellHeight);

            var illustrationHeight = Math.Min(190, Math.Max(150, shellHeight / 3));
            illustrationPanel.Bounds = new Rectangle(22, Math.Max(265, shellHeight - illustrationHeight - 150), brandPanel.Width - 44, illustrationHeight);
            for (var index = 0; index < 3; index++)
            {
                var check = brandPanel.Controls.Find($"benefitCheck{index + 1}", false).FirstOrDefault();
                var benefit = brandPanel.Controls.Find($"benefitText{index + 1}", false).FirstOrDefault();
                var y = shellHeight - 110 + index * 30;
                if (check is not null)
                    check.Location = new Point(43, y);
                if (benefit is not null)
                    benefit.Location = new Point(70, y + 1);
            }

            authBox.Size = new Size(Math.Min(370, formPanel.Width - 70), registerMode ? 640 : 552);
            authBox.Location = new Point((formPanel.Width - authBox.Width) / 2, Math.Max(18, (shellHeight - authBox.Height) / 2));
            var innerWidth = authBox.Width;
            loginTabButton.Bounds = new Rectangle(0, 0, innerWidth / 2, 34);
            registerTabButton.Bounds = new Rectangle(innerWidth / 2, 0, innerWidth - innerWidth / 2, 34);
            foreach (var rule in authBox.Controls.OfType<Panel>().Where(control => control.Name == "authenticationTabRule"))
                rule.Bounds = new Rectangle(0, 34, innerWidth, 1);
            tabUnderline.Bounds = new Rectangle(registerMode ? innerWidth / 2 : 0, 33, innerWidth / 2, 2);
            authModeTitle.Location = new Point(0, 56);
            pageSubtitle.Location = new Point(0, 87);
            loginView.Bounds = new Rectangle(0, 119, innerWidth, 365);
            registerView.Bounds = new Rectangle(0, 119, innerWidth, 460);
            foreach (var input in new[] { loginIdentityTextBox, loginPasswordTextBox, registerFirstNameTextBox, registerLastNameTextBox, registerEmailTextBox, registerPhoneTextBox, registerPasswordTextBox, registerConfirmPasswordTextBox })
                input.Width = innerWidth;

            var registrationColumnWidth = (innerWidth - 12) / 2;
            LayoutField(registerView, registerFirstNameTextBox, 0, 14, registrationColumnWidth);
            LayoutField(registerView, registerLastNameTextBox, registrationColumnWidth + 12, 14, registrationColumnWidth);
            LayoutField(registerView, registerEmailTextBox, 0, 72, innerWidth);
            LayoutField(registerView, registerPhoneTextBox, 0, 130, innerWidth);
            LayoutField(registerView, registerPasswordTextBox, 0, 244, registrationColumnWidth);
            LayoutField(registerView, registerConfirmPasswordTextBox, registrationColumnWidth + 12, 244, registrationColumnWidth);
            registerBarangayComboBox.Bounds = new Rectangle(0, 207, innerWidth, 32);

            termsCheckBox.Size = new Size(innerWidth, 18);
            updatesCheckBox.Size = new Size(innerWidth, 18);
            foreach (var indicator in registerView.Controls.Find("registrationPasswordStrengthIndicator", false).OfType<FlowLayoutPanel>())
            {
                indicator.Width = innerWidth;
                var segmentWidth = (innerWidth - 6) / 3;
                foreach (Control segment in indicator.Controls)
                    segment.Width = segmentWidth;
            }
            var loginEye = loginView.Controls.Find("loginPasswordToggle", false).FirstOrDefault();
            if (loginEye is not null)
                loginEye.Location = new Point(innerWidth - loginEye.Width, 94);
            var registerEye = registerView.Controls.Find("registerPasswordToggle", false).FirstOrDefault();
            if (registerEye is not null)
                registerEye.Location = new Point(registrationColumnWidth - registerEye.Width, 254);
            var confirmEye = registerView.Controls.Find("registerConfirmPasswordToggle", false).FirstOrDefault();
            if (confirmEye is not null)
                confirmEye.Location = new Point(registrationColumnWidth + 12 + registrationColumnWidth - confirmEye.Width, 254);

            foreach (var button in new[] { loginSubmitButton, registerSubmitButton })
                button.Width = innerWidth;
            foreach (var button in FindControls<Button>(loginView).Where(control => control.Name is "GoogleSignInButton" or "FacebookSignInButton"))
                button.Width = innerWidth;
            if (registerBarangayComboBox is not null)
                registerBarangayComboBox.Width = innerWidth;
            var securityNote = authBox.Controls.Find("authenticationSecurityNote", false).FirstOrDefault();
            if (securityNote is not null)
            {
                securityNote.Location = new Point(0, registerMode ? 592 : 494);
                securityNote.Size = new Size(innerWidth, 18);
            }
            var adminPrompt = authBox.Controls.Find("adminSignInPrompt", false).FirstOrDefault();
            var adminButton = authBox.Controls.Find("AdminSignInButton", false).FirstOrDefault();
            if (adminPrompt is not null)
                adminPrompt.Location = new Point(0, registerMode ? 620 : 523);
            if (adminButton is not null)
                adminButton.Location = new Point(adminPrompt?.Width + 5 ?? 150, registerMode ? 615 : 518);
            feedbackLabel.Location = new Point(0, registerMode ? 570 : 472);
            toastLabel.Location = new Point((ClientSize.Width - toastLabel.Width) / 2, ClientSize.Height - toastLabel.Height - 22);
        }

        private void SetMode(bool showRegister)
        {
            registerMode = showRegister;
            registerView.Visible = showRegister;
            loginView.Visible = !showRegister;
            authModeTitle.Text = showRegister ? "Create your account" : "Welcome back!";
            pageSubtitle.Text = showRegister ? "Join TaskTap and connect with your local community." : "Sign in to your account.";
            tabUnderline.Left = showRegister ? authBox.Width / 2 : 0;
            loginTabButton.ForeColor = showRegister ? MutedColor : BlueColor;
            registerTabButton.ForeColor = showRegister ? BlueColor : MutedColor;
            feedbackLabel.Text = string.Empty;
            AcceptButton = showRegister ? registerSubmitButton : loginSubmitButton;
            LayoutPage();
        }

        private void SubmitLogin()
        {
            if (string.IsNullOrWhiteSpace(loginIdentityTextBox.Text) || string.IsNullOrWhiteSpace(loginPasswordTextBox.Text))
            {
                feedbackLabel.Text = "Enter your email or phone number and password.";
                return;
            }

            feedbackLabel.Text = string.Empty;
            if (string.IsNullOrEmpty(MySqlConnectionString))
            {
                ShowToast("Database connection string not configured. Update appsettings.json.");
                return;
            }

            try
            {
                using var conn = new MySqlConnection(MySqlConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT UserId, FirstName, PasswordHash FROM Users WHERE Email = @id OR Phone = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", loginIdentityTextBox.Text.Trim());
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var stored = reader["PasswordHash"]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(stored) && VerifyPassword(stored, loginPasswordTextBox.Text))
                    {
                        OpenUserDashboard();
                    }
                    else
                    {
                        feedbackLabel.Text = "Invalid credentials.";
                    }
                }
                else
                {
                    feedbackLabel.Text = "Invalid credentials.";
                }
            }
            catch (Exception ex)
            {
                ShowToast("Unable to connect to database: " + ex.Message);
            }
        }

        private void OpenUserDashboard()
        {
            var dashboard = new UserDashboardForm();
            dashboard.FormClosed += (_, _) =>
            {
                Show();
                Activate();
            };
            Hide();
            dashboard.Show();
        }

        private void NavigateToPasswordRecovery()
        {
            var recoveryPage = new PasswordRecoveryForm();
            recoveryPage.FormClosed += (_, _) =>
            {
                Show();
                Activate();
            };
            Hide();
            recoveryPage.Show();
        }

        private void SubmitRegistration()
        {
            if (string.IsNullOrWhiteSpace(registerFirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(registerLastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(registerEmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(registerPhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(registerPasswordTextBox.Text) ||
                string.IsNullOrWhiteSpace(registerConfirmPasswordTextBox.Text) ||
                registerBarangayComboBox.SelectedIndex == 0 ||
                !termsCheckBox.Checked)
            {
                feedbackLabel.Text = "Complete all required fields, select your barangay, and accept the terms.";
                return;
            }

            if (registerPasswordTextBox.Text.Length < 8)
            {
                feedbackLabel.Text = "Your password must be at least 8 characters.";
                return;
            }

            if (!string.Equals(registerPasswordTextBox.Text, registerConfirmPasswordTextBox.Text, StringComparison.Ordinal))
            {
                feedbackLabel.Text = "Your passwords do not match.";
                return;
            }

            feedbackLabel.Text = string.Empty;
            if (string.IsNullOrEmpty(MySqlConnectionString))
            {
                ShowToast("Database connection string not configured. Update appsettings.json.");
                return;
            }

            feedbackLabel.Text = string.Empty;
            try
            {
                var passwordHash = HashPassword(registerPasswordTextBox.Text);
                using var conn = new MySqlConnection(MySqlConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Users (FirstName, LastName, Email, Phone, Barangay, PasswordHash) VALUES (@fn, @ln, @em, @ph, @br, @pw)";
                cmd.Parameters.AddWithValue("@fn", registerFirstNameTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@ln", registerLastNameTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@em", registerEmailTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@ph", registerPhoneTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@pw", passwordHash);
                cmd.Parameters.AddWithValue("@br", registerBarangayComboBox.SelectedItem?.ToString() ?? string.Empty);
                cmd.ExecuteNonQuery();
                ShowToast("Account created. You may now sign in.");
                SetMode(false);
            }
            catch (MySqlException mex)
            {
                // Duplicate entry or other DB errors
                if (mex.Number == 1062)
                    feedbackLabel.Text = "An account with that email or phone already exists.";
                else
                    ShowToast("Database error: " + mex.Message);
            }
            catch (Exception ex)
            {
                ShowToast("Unable to connect to database: " + ex.Message);
            }
        }

        private void AddField(Control parent, string labelText, string placeholder, string name, int top, out TextBox textBox, bool password = false)
        {
            var label = MakeLabel(labelText, 8, Color.FromArgb(83, 101, 126), true);
            label.Name = $"{name}Label";
            label.Location = new Point(0, top);
            parent.Controls.Add(label);
            textBox = new TextBox
            {
                Name = name,
                PlaceholderText = placeholder,
                Location = new Point(0, top + 20),
                Size = new Size(360, 31),
                Font = new Font("Segoe UI", 8.5F),
                UseSystemPasswordChar = password
            };
            parent.Controls.Add(textBox);
        }

        private static void LayoutField(Control parent, TextBox textBox, int left, int top, int width)
        {
            var label = parent.Controls.Find($"{textBox.Name}Label", false).FirstOrDefault();
            if (label is not null)
            {
                label.AutoSize = false;
                label.Location = new Point(left, top);
                label.Size = new Size(width, 18);
            }
            textBox.Location = new Point(left, top + 20);
            textBox.Width = width;
        }

        private void AddPasswordToggle(Control parent, TextBox textBox, string name, int top)
        {
            var toggle = MakeLinkButton("◉", name);
            toggle.Font = new Font("Segoe UI Symbol", 10F);
            toggle.Location = new Point(325, top);
            toggle.Click += (_, _) => textBox.UseSystemPasswordChar = !textBox.UseSystemPasswordChar;
            parent.Controls.Add(toggle);
        }

        private void AddDivider(Control parent, int top)
        {
            var left = new Panel { Name = "socialDividerLeft", BackColor = Color.FromArgb(226, 233, 241), Location = new Point(0, top + 8), Size = new Size(150, 1) };
            var right = new Panel { Name = "socialDividerRight", BackColor = Color.FromArgb(226, 233, 241), Location = new Point(210, top + 8), Size = new Size(150, 1) };
            var or = MakeLabel("or", 8, MutedColor);
            or.Location = new Point(174, top);
            parent.Controls.Add(left);
            parent.Controls.Add(right);
            parent.Controls.Add(or);
        }

        private Button MakeTab(string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = MutedColor,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Button MakePrimaryButton(string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(360, 38),
                BackColor = BlueColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Button MakeSocialButton(string icon, string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = $"{icon}     {text}",
                Size = new Size(360, 36),
                BackColor = Color.White,
                ForeColor = NavyColor,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(215, 227, 239);
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private Button MakeLinkButton(string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = BlueColor,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = Padding.Empty
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

        private void ShowToast(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastLabel.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
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

        private sealed class BrandMark : Control
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var blue = new SolidBrush(BlueColor);
                using var orange = new SolidBrush(Color.FromArgb(255, 134, 57));
                using var white = new SolidBrush(Color.White);
                e.Graphics.FillEllipse(blue, 2, 2, 20, 20);
                e.Graphics.FillEllipse(orange, 11, 13, 18, 18);
                e.Graphics.FillEllipse(white, 8, 8, 5, 5);
                e.Graphics.FillEllipse(white, 18, 19, 5, 5);
            }
        }
    }
}
