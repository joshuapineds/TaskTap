namespace TaskTap
{
    /// <summary>
    /// Honest "forgot password" screen. Self-service reset is not wired to email/SMS in this
    /// build, so instead of faking a reset we direct members to TaskTap support (or an admin),
    /// who can reset the password from the admin portal.
    /// </summary>
    public sealed class PasswordRecoveryForm : Form
    {
        private static readonly Color PageColor = Color.FromArgb(244, 249, 253);
        private static readonly Color NavyColor = Color.FromArgb(9, 35, 76);
        private static readonly Color BlueColor = Color.FromArgb(8, 112, 229);
        private static readonly Color MutedColor = Color.FromArgb(111, 132, 158);

        private readonly Panel header = new();
        private readonly Panel card = new();

        public PasswordRecoveryForm(bool resetInitially = false)
        {
            BuildPage();
        }

        private void BuildPage()
        {
            Text = "TaskTap — Password Recovery";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 560);
            ClientSize = new Size(1000, 700);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 9F);

            BuildHeader();
            BuildCard();
            Controls.Add(header);
            Controls.Add(card);

            Resize += (_, _) => LayoutPage();
            LayoutPage();
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

            var iconPanel = new Panel { Name = "recoveryStageIcon", Size = new Size(44, 44), BackColor = Color.FromArgb(231, 242, 255) };
            var icon = MakeLabel("✉", 18, BlueColor, true);
            icon.Dock = DockStyle.Fill;
            icon.TextAlign = ContentAlignment.MiddleCenter;
            iconPanel.Controls.Add(icon);
            card.Controls.Add(iconPanel);

            var heading = MakeLabel("Forgot your password?", 21, NavyColor, true);
            heading.Name = "recoveryHeading";
            card.Controls.Add(heading);

            var description = MakeLabel(
                "Self-service password reset isn't available in this build. Contact TaskTap support and an administrator will reset your password.",
                8, MutedColor);
            description.Name = "recoveryDescription";
            description.MaximumSize = new Size(365, 0);
            card.Controls.Add(description);

            var support = MakeLabel($"Support email: {DemoDataStore.Settings.SupportEmail}", 9, BlueColor, true);
            support.Name = "recoverySupportEmail";
            card.Controls.Add(support);

            var login = MakePrimaryButton("Back to Sign In", "RecoveryReturnToLoginButton");
            login.Click += (_, _) => Close();
            card.Controls.Add(login);
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

            card.Size = new Size(430, 320);
            card.Location = new Point((ClientSize.Width - card.Width) / 2, Math.Max(76, (ClientSize.Height - card.Height) / 2 + 20));

            var icon = card.Controls.Find("recoveryStageIcon", false).FirstOrDefault();
            if (icon is not null) icon.Location = new Point(28, 26);

            var heading = card.Controls.Find("recoveryHeading", false).FirstOrDefault();
            if (heading is not null) heading.Location = new Point(28, 88);

            var description = card.Controls.Find("recoveryDescription", false).FirstOrDefault();
            if (description is not null) description.Location = new Point(28, 126);

            var support = card.Controls.Find("recoverySupportEmail", false).FirstOrDefault();
            if (support is not null) support.Location = new Point(28, 178);

            var login = card.Controls.Find("RecoveryReturnToLoginButton", false).FirstOrDefault();
            if (login is not null) login.Location = new Point(28, 224);
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
    }
}
