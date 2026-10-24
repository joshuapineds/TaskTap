using TaskTap.Data;

namespace TaskTap
{
    public sealed class PasswordRecoveryForm : Form
    {
        private static readonly Color PageColor = Color.FromArgb(244, 249, 253);
        private static readonly Color NavyColor = Color.FromArgb(9, 35, 76);
        private static readonly Color BlueColor = Color.FromArgb(8, 112, 229);
        private static readonly Color MutedColor = Color.FromArgb(111, 132, 158);
        private readonly Panel header = new();
        private readonly Panel card = new();
        private readonly Panel iconPanel = new();
        private readonly Panel progressLine = new();
        private readonly Label firstStep = new();
        private readonly Label secondStep = new();
        private readonly Label stageLabel = new();
        private readonly Panel requestPanel = new();
        private readonly Panel resetPanel = new();
        private readonly Panel successPanel = new();
        private readonly TextBox contactInput = new();
        private readonly TextBox passwordInput = new();
        private readonly TextBox confirmInput = new();
        private readonly Label resetTarget = new();
        private readonly Label strengthText = new();
        private readonly Label matchText = new();
        private readonly Panel[] strengthBars = new Panel[4];
        private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
        private readonly Label toast = new();
        private int stage;
        private int resetUserId;
        private string contact = string.Empty;

        public PasswordRecoveryForm(bool resetInitially = false)
        {
            stage = resetInitially ? 2 : 1;
            BuildPage();
        }

        private void BuildPage()
        {
            Text = "TaskTap — Password Recovery";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 680);
            ClientSize = new Size(1360, 900);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            BuildHeader();
            BuildCard();
            Controls.Add(header);
            Controls.Add(card);

            toast.Name = "passwordRecoveryToast";
            toast.AutoSize = false;
            toast.Size = new Size(420, 40);
            toast.BackColor = NavyColor;
            toast.ForeColor = Color.White;
            toast.TextAlign = ContentAlignment.MiddleCenter;
            toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toast.Visible = false;
            Controls.Add(toast);
            toastTimer.Tick += (_, _) =>
            {
                toastTimer.Stop();
                toast.Visible = false;
            };
            Resize += (_, _) => LayoutPage();
            LayoutPage();
            ShowStage(stage);
        }

        private void BuildHeader()
        {
            header.Name = "recoveryHeader";
            header.BackColor = Color.White;
            var brand = new FlowLayoutPanel { Name = "recoveryBrand", AutoSize = true, WrapContents = false, BackColor = Color.White };
            var mark = MakeLabel("●", 20, BlueColor, true);
            mark.Size = new Size(28, 32);
            brand.Controls.Add(mark);
            brand.Controls.Add(MakeLabel("Task", 15, NavyColor, true));
            brand.Controls.Add(MakeLabel("Tap", 15, BlueColor, true));
            header.Controls.Add(brand);
            var back = MakeLinkButton("← Back to Login", "RecoveryBackToLoginButton", MutedColor);
            back.Click += (_, _) => Close();
            header.Controls.Add(back);
        }

        private void BuildCard()
        {
            card.Name = "passwordRecoveryCard";
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            iconPanel.Name = "recoveryStageIcon";
            iconPanel.Size = new Size(44, 44);
            var icon = MakeLabel("✉", 18, BlueColor, true);
            icon.Name = "recoveryIconGlyph";
            icon.Dock = DockStyle.Fill;
            icon.TextAlign = ContentAlignment.MiddleCenter;
            iconPanel.Controls.Add(icon);
            card.Controls.Add(iconPanel);
            progressLine.Name = "recoveryProgressLine";
            progressLine.Height = 2;
            card.Controls.Add(progressLine);
            ConfigureStep(firstStep, "1");
            ConfigureStep(secondStep, "2");
            card.Controls.Add(firstStep);
            card.Controls.Add(secondStep);
            stageLabel.Name = "recoveryStageLabel";
            stageLabel.AutoSize = true;
            stageLabel.Font = new Font("Segoe UI", 8F);
            stageLabel.ForeColor = MutedColor;
            card.Controls.Add(stageLabel);
            BuildRequestPanel();
            BuildResetPanel();
            BuildSuccessPanel();
            card.Controls.Add(requestPanel);
            card.Controls.Add(resetPanel);
            card.Controls.Add(successPanel);
        }

        private void BuildRequestPanel()
        {
            requestPanel.Name = "recoveryRequestStage";
            requestPanel.BackColor = Color.White;
            AddHeading(requestPanel, "Forgot your password?", "Enter the email address or phone number connected to your TaskTap account.");
            AddFieldLabel(requestPanel, "Email or Phone Number", "recoveryFieldLabel");
            contactInput.Name = "recoveryContactInput";
            contactInput.PlaceholderText = "you@example.com or +63 9XX XXX XXXX";
            ConfigureTextBox(contactInput);
            requestPanel.Controls.Add(contactInput);
            var next = MakePrimaryButton("Continue", "RecoveryContinueButton");
            next.Click += (_, _) => RequestReset();
            requestPanel.Controls.Add(next);
            var notice = MakeInfoLabel("If an account matches the information entered, password reset instructions will be sent.");
            notice.Name = "recoveryRequestNotice";
            requestPanel.Controls.Add(notice);
        }

        private void BuildResetPanel()
        {
            resetPanel.Name = "recoveryResetStage";
            resetPanel.BackColor = Color.White;
            AddHeading(resetPanel, "Create a new password", "Choose a new password for");
            resetTarget.Name = "recoveryResetTarget";
            resetTarget.Text = "your account.";
            resetTarget.AutoSize = true;
            resetTarget.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            resetTarget.ForeColor = MutedColor;
            resetPanel.Controls.Add(resetTarget);
            AddFieldLabel(resetPanel, "New Password", "recoveryPasswordLabel");
            AddPasswordBox(resetPanel, passwordInput, "newRecoveryPasswordInput", "At least 8 characters", "ToggleRecoveryPasswordButton");
            var bars = new FlowLayoutPanel { Name = "recoveryStrengthBars", WrapContents = false, BackColor = Color.White, Margin = Padding.Empty };
            for (var i = 0; i < strengthBars.Length; i++)
            {
                strengthBars[i] = new Panel { Name = $"strengthBar{i + 1}", Size = new Size(76, 4), BackColor = Color.FromArgb(231, 237, 242), Margin = new Padding(0, 0, 4, 0) };
                bars.Controls.Add(strengthBars[i]);
            }
            resetPanel.Controls.Add(bars);
            strengthText.Name = "recoveryStrengthText";
            strengthText.AutoSize = true;
            strengthText.Font = new Font("Segoe UI", 7F);
            strengthText.ForeColor = MutedColor;
            resetPanel.Controls.Add(strengthText);
            AddFieldLabel(resetPanel, "Confirm New Password", "recoveryConfirmPasswordLabel");
            AddPasswordBox(resetPanel, confirmInput, "confirmRecoveryPasswordInput", "Re-enter your password", "ToggleConfirmRecoveryPasswordButton");
            matchText.Name = "recoveryPasswordMatchText";
            matchText.AutoSize = true;
            matchText.Font = new Font("Segoe UI", 7F);
            resetPanel.Controls.Add(matchText);
            var rules = MakeLabel("•  At least 8 characters\n•  Include a number or special character", 7, MutedColor);
            rules.Name = "recoveryPasswordRules";
            resetPanel.Controls.Add(rules);
            var save = MakePrimaryButton("Save New Password", "SaveRecoveredPasswordButton");
            save.Click += (_, _) => SavePassword();
            resetPanel.Controls.Add(save);
            var help = MakeInfoLabel("Choose a unique password that you have not used before.");
            help.Name = "recoveryPasswordHelp";
            resetPanel.Controls.Add(help);
            var change = MakeLinkButton("Use a different email or phone", "ChangeRecoveryContactButton", BlueColor);
            change.Click += (_, _) => ShowStage(1);
            resetPanel.Controls.Add(change);
            passwordInput.TextChanged += (_, _) => UpdatePasswordFeedback();
            confirmInput.TextChanged += (_, _) => UpdatePasswordFeedback();
        }

        private void BuildSuccessPanel()
        {
            successPanel.Name = "recoverySuccessStage";
            successPanel.BackColor = Color.White;
            var heading = MakeLabel("Password updated", 22, NavyColor, true);
            heading.Name = "recoverySuccessTitle";
            successPanel.Controls.Add(heading);
            var message = MakeLabel("Your password has been changed. Sign in using your new password.", 9, MutedColor);
            message.Name = "recoverySuccessMessage";
            message.MaximumSize = new Size(310, 0);
            successPanel.Controls.Add(message);
            var login = MakePrimaryButton("Return to Sign In", "RecoveryReturnToLoginButton");
            login.Click += (_, _) => Close();
            successPanel.Controls.Add(login);
        }

        private void RequestReset()
        {
            contact = contactInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(contact))
            {
                ShowToast("Enter your email address or phone number.");
                contactInput.Focus();
                return;
            }
            Backend.PasswordResetAccount? account = null;
            try { account = Backend.FindPasswordResetAccount(contact); }
            catch (Exception ex) { ShowToast($"Could not reach the database: {ex.Message}"); return; }
            if (account is null)
            {
                ShowToast("No account is associated with that email or phone number.");
                contactInput.Focus();
                return;
            }
            resetUserId = account.Id;
            resetTarget.Text = $"{account.DisplayName} ({contact}).";
            ShowStage(2);
            passwordInput.Focus();
            ShowToast("Account found. Create a new password to continue.");
        }

        private void SavePassword()
        {
            var password = passwordInput.Text;
            if (password.Length < 8)
            {
                ShowToast("Your password must be at least 8 characters.");
                passwordInput.Focus();
                return;
            }
            if (!password.Any(char.IsDigit) && !password.Any(character => !char.IsLetterOrDigit(character)))
            {
                ShowToast("Include a number or special character.");
                passwordInput.Focus();
                return;
            }
            if (!string.Equals(password, confirmInput.Text, StringComparison.Ordinal))
            {
                matchText.Text = "Passwords do not match.";
                matchText.ForeColor = Color.FromArgb(196, 61, 73);
                confirmInput.Focus();
                return;
            }
            var result = Backend.ResetPassword(resetUserId, password);
            if (!result.Success)
            {
                ShowToast(result.Error);
                return;
            }
            passwordInput.Clear();
            confirmInput.Clear();
            ShowStage(3);
        }

        private void UpdatePasswordFeedback()
        {
            var password = passwordInput.Text;
            var score = (password.Length >= 8 ? 1 : 0) + (password.Any(char.IsDigit) ? 1 : 0) + (password.Any(character => !char.IsLetterOrDigit(character)) ? 1 : 0) + (password.Length >= 12 ? 1 : 0);
            for (var index = 0; index < strengthBars.Length; index++)
            {
                if (strengthBars[index] is not null)
                    strengthBars[index].BackColor = index < score ? BlueColor : Color.FromArgb(231, 237, 242);
            }
            strengthText.Text = password.Length == 0 ? "Enter a password" : score < 2 ? "Weak password" : score < 4 ? "Good password" : "Strong password";
            if (confirmInput.Text.Length == 0)
            {
                matchText.Text = string.Empty;
                return;
            }
            var matches = string.Equals(password, confirmInput.Text, StringComparison.Ordinal);
            matchText.Text = matches ? "Passwords match." : "Passwords do not match.";
            matchText.ForeColor = matches ? Color.FromArgb(17, 150, 106) : Color.FromArgb(196, 61, 73);
        }

        private void ShowStage(int nextStage)
        {
            stage = nextStage;
            requestPanel.Visible = stage == 1;
            resetPanel.Visible = stage == 2;
            successPanel.Visible = stage == 3;
            stageLabel.Visible = stage != 3;
            stageLabel.Text = stage == 1 ? "Step 1 of 2" : "Step 2 of 2";
            firstStep.Text = stage == 1 ? "1" : "✓";
            secondStep.Text = stage == 3 ? "✓" : "2";
            firstStep.BackColor = stage == 1 ? BlueColor : Color.FromArgb(223, 245, 237);
            firstStep.ForeColor = stage == 1 ? Color.White : Color.FromArgb(17, 150, 106);
            secondStep.BackColor = stage == 2 ? BlueColor : stage == 3 ? Color.FromArgb(223, 245, 237) : Color.FromArgb(237, 242, 247);
            secondStep.ForeColor = stage == 2 ? Color.White : stage == 3 ? Color.FromArgb(17, 150, 106) : MutedColor;
            progressLine.BackColor = stage >= 2 ? Color.FromArgb(162, 222, 197) : Color.FromArgb(224, 233, 242);
            iconPanel.BackColor = stage == 1 ? Color.FromArgb(231, 242, 255) : Color.FromArgb(226, 247, 239);
            iconPanel.Controls.OfType<Label>().First().Text = stage == 1 ? "✉" : "✓";
            LayoutPage();
        }

        private void LayoutPage()
        {
            header.Bounds = new Rectangle(0, 0, ClientSize.Width, 58);
            var brand = header.Controls.Find("recoveryBrand", false).FirstOrDefault();
            if (brand is not null)
                brand.Location = new Point(Math.Max(24, (ClientSize.Width - 748) / 2), 11);
            var back = header.Controls.Find("RecoveryBackToLoginButton", false).FirstOrDefault();
            if (back is not null)
                back.Location = new Point(Math.Min(ClientSize.Width - back.Width - 24, (ClientSize.Width + 748) / 2 - back.Width), 14);

            var cardHeight = stage == 2 ? 500 : stage == 3 ? 360 : 390;
            card.Size = new Size(430, cardHeight);
            card.Location = new Point((ClientSize.Width - card.Width) / 2, Math.Max(76, (ClientSize.Height - card.Height) / 2 + 20));
            iconPanel.Location = new Point(28, 26);
            firstStep.Bounds = new Rectangle(28, 82, 24, 24);
            secondStep.Bounds = new Rectangle(card.Width - 52, 82, 24, 24);
            progressLine.Bounds = new Rectangle(40, 93, card.Width - 80, 2);
            progressLine.SendToBack();
            firstStep.BringToFront();
            secondStep.BringToFront();
            stageLabel.Location = new Point(28, 124);

            var stageTop = stage == 3 ? 120 : 155;
            var stageBounds = new Rectangle(28, stageTop, card.Width - 56, card.Height - stageTop - 20);
            requestPanel.Bounds = stageBounds;
            resetPanel.Bounds = stageBounds;
            successPanel.Bounds = stageBounds;
            LayoutStageControls();
            toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 20);
        }

        private void LayoutStageControls()
        {
            Position(requestPanel, "recoveryHeading", 0, 0);
            Position(requestPanel, "recoveryDescription", 0, 36);
            Position(requestPanel, "recoveryFieldLabel", 0, 95);
            contactInput.Bounds = new Rectangle(0, 116, requestPanel.Width, 42);
            Position(requestPanel, "RecoveryContinueButton", 0, 168);
            var requestNotice = requestPanel.Controls.Find("recoveryRequestNotice", false).FirstOrDefault();
            if (requestNotice is not null)
                requestNotice.Bounds = new Rectangle(0, 224, requestPanel.Width, 42);

            Position(resetPanel, "recoveryHeading", 0, 0);
            Position(resetPanel, "recoveryDescription", 0, 36);
            Position(resetPanel, "recoveryResetTarget", 153, 36);
            Position(resetPanel, "recoveryPasswordLabel", 0, 68);
            passwordInput.Bounds = new Rectangle(0, 89, resetPanel.Width, 42);
            Position(resetPanel, "ToggleRecoveryPasswordButton", resetPanel.Width - 34, 96);
            var strengthPanel = resetPanel.Controls.Find("recoveryStrengthBars", false).FirstOrDefault();
            if (strengthPanel is not null)
            {
                strengthPanel.Location = new Point(0, 140);
                foreach (Control bar in strengthPanel.Controls)
                    bar.Width = Math.Max(20, (resetPanel.Width - 16) / 4);
            }
            Position(resetPanel, "recoveryStrengthText", 0, 150);
            Position(resetPanel, "recoveryConfirmPasswordLabel", 0, 176);
            confirmInput.Bounds = new Rectangle(0, 197, resetPanel.Width, 42);
            Position(resetPanel, "ToggleConfirmRecoveryPasswordButton", resetPanel.Width - 34, 204);
            Position(resetPanel, "recoveryPasswordMatchText", 0, 241);
            Position(resetPanel, "recoveryPasswordRules", 0, 262);
            var save = resetPanel.Controls.Find("SaveRecoveredPasswordButton", false).FirstOrDefault();
            if (save is not null)
                save.Bounds = new Rectangle(0, 310, resetPanel.Width, 40);
            var help = resetPanel.Controls.Find("recoveryPasswordHelp", false).FirstOrDefault();
            if (help is not null)
                help.Bounds = new Rectangle(0, 360, resetPanel.Width, 38);
            var change = resetPanel.Controls.Find("ChangeRecoveryContactButton", false).FirstOrDefault();
            if (change is not null)
                change.Location = new Point((resetPanel.Width - change.Width) / 2, 410);

            Position(successPanel, "recoverySuccessTitle", 0, 0);
            Position(successPanel, "recoverySuccessMessage", 0, 42);
            var returnButton = successPanel.Controls.Find("RecoveryReturnToLoginButton", false).FirstOrDefault();
            if (returnButton is not null)
                returnButton.Bounds = new Rectangle(0, 100, successPanel.Width, 40);
        }

        private void AddPasswordBox(Panel parent, TextBox textBox, string name, string placeholder, string toggleName)
        {
            textBox.Name = name;
            textBox.PlaceholderText = placeholder;
            textBox.UseSystemPasswordChar = true;
            ConfigureTextBox(textBox);
            parent.Controls.Add(textBox);
            var toggle = MakeLinkButton("◉", toggleName, MutedColor);
            toggle.Font = new Font("Segoe UI Symbol", 9F);
            toggle.Click += (_, _) => textBox.UseSystemPasswordChar = !textBox.UseSystemPasswordChar;
            parent.Controls.Add(toggle);
        }

        private static void AddHeading(Control parent, string title, string description)
        {
            var heading = MakeLabel(title, 21, NavyColor, true);
            heading.Name = "recoveryHeading";
            parent.Controls.Add(heading);
            var descriptionLabel = MakeLabel(description, 8, MutedColor);
            descriptionLabel.Name = "recoveryDescription";
            descriptionLabel.MaximumSize = new Size(365, 0);
            parent.Controls.Add(descriptionLabel);
        }

        private static void AddFieldLabel(Control parent, string text, string name)
        {
            var label = MakeLabel(text, 8, NavyColor, true);
            label.Name = name;
            parent.Controls.Add(label);
        }

        private static void ConfigureTextBox(TextBox textBox)
        {
            textBox.Font = new Font("Segoe UI", 9F);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
        }

        private static void ConfigureStep(Label label, string text)
        {
            label.Text = text;
            label.Size = new Size(24, 24);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            label.BackColor = Color.FromArgb(237, 242, 247);
            label.ForeColor = MutedColor;
        }

        private static Button MakePrimaryButton(string text, string name)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(360, 40),
                BackColor = BlueColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button MakeLinkButton(string text, string name, Color foreColor)
        {
            var button = new Button
            {
                Name = name,
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = Padding.Empty
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label MakeInfoLabel(string text)
        {
            var label = MakeLabel(text, 7, MutedColor);
            label.AutoSize = false;
            label.BackColor = Color.FromArgb(247, 250, 253);
            label.BorderStyle = BorderStyle.FixedSingle;
            return label;
        }

        private static Label MakeLabel(string text, float size, Color foreColor, bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = foreColor,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                Margin = Padding.Empty
            };
        }

        private void Position(Control parent, string name, int x, int y)
        {
            var control = parent.Controls.Find(name, false).FirstOrDefault();
            if (control is not null)
                control.Location = new Point(x, y);
        }

        private void ShowToast(string message)
        {
            toast.Text = message;
            toast.Visible = true;
            toast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toastTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
