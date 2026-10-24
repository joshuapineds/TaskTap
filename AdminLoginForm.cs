using TaskTap.Data;

namespace TaskTap;

/// <summary>
/// Admin sign-in. Credentials are checked against the users table (administrators only); the form then
/// hides itself and opens <see cref="AdminShellForm"/>, mirroring Form2 → MainShellForm.
/// </summary>
public sealed class AdminLoginForm : Form
{
    private static readonly Color Navy = AdminUi.Navy;
    private static readonly Color Blue = AdminUi.Blue;
    private static readonly Color Accent = AdminUi.Accent;
    private static readonly Color Muted = AdminUi.Muted;
    private static readonly Color Page = Color.FromArgb(246, 250, 255);
    private static readonly Color Line = AdminUi.Line;

    private readonly Panel topBar = new();
    private readonly Panel body = new();
    private readonly Panel shell = new();
    private readonly Panel brandPanel = new();
    private readonly Panel card = new();
    private readonly TextBox emailBox = new();
    private readonly TextBox passwordBox = new();
    private readonly Label feedback = new();
    private readonly Button signInButton = new();
    private readonly CheckBox rememberBox = new();
    private readonly Button forgotButton = new();
    private readonly Label hint = new();
    private readonly Button backLink = new();

    public AdminLoginForm()
    {
        BuildForm();
    }

    private void BuildForm()
    {
        Text = "TaskTap Admin";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Page;
        Font = new Font("Segoe UI", 9F);

        BuildTopBar();

        body.Dock = DockStyle.Fill;
        body.BackColor = Page;
        Controls.Add(body);
        body.Resize += (_, _) => LayoutPage();

        shell.BackColor = Color.White;
        shell.BorderStyle = BorderStyle.FixedSingle;
        body.Controls.Add(shell);

        BuildBrandPanel();
        BuildCard();

        LayoutPage();
    }

    private void BuildTopBar()
    {
        topBar.Dock = DockStyle.Top;
        topBar.Height = 64;
        topBar.BackColor = Color.White;
        topBar.Paint += (_, e) =>
        {
            using var pen = new Pen(Line);
            e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
        };
        Controls.Add(topBar);

        var logo = new TaskTapLogo
        {
            Name = "adminLoginBrand",
            TextColor = Navy,
            BackColor = Color.White,
            Location = new Point(24, 15),
            Size = new Size(160, 34),
            Cursor = Cursors.Hand
        };
        logo.Click += (_, _) => Close();
        topBar.Controls.Add(logo);

        var badge = new Label
        {
            Name = "adminLoginBadge",
            Text = "ADMIN",
            AutoSize = true,
            BackColor = AdminUi.AdminBadgeBack,
            ForeColor = AdminUi.AdminBadgeFore,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Padding = new Padding(8, 3, 8, 3),
            Location = new Point(196, 20)
        };
        topBar.Controls.Add(badge);

        var back = AdminUi.MakeLinkButton("← Back to TaskTap", "adminLoginBackLink");
        back.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        back.Click += (_, _) => Close();
        topBar.Controls.Add(back);
        topBar.Resize += (_, _) => back.Location = new Point(topBar.ClientSize.Width - back.Width - 24, 20);
    }

    private void BuildBrandPanel()
    {
        brandPanel.BackColor = Color.FromArgb(232, 243, 255);
        shell.Controls.Add(brandPanel);

        var eyebrow = AdminUi.MakeLabel("ADMIN CONSOLE", 8, Blue, true);
        eyebrow.Location = new Point(36, 48);
        brandPanel.Controls.Add(eyebrow);

        var line1 = AdminUi.MakeLabel("TaskTap Admin.", 26, Navy, true);
        line1.Location = new Point(36, 76);
        brandPanel.Controls.Add(line1);

        var line2 = AdminUi.MakeLabel("Run the community.", 20, Accent, true);
        line2.Location = new Point(36, 122);
        brandPanel.Controls.Add(line2);

        var description = AdminUi.MakeWrappedLabel("Moderate users and tasks, resolve reports and disputes, and keep the marketplace healthy from one console.", 9, Muted, 300);
        description.Location = new Point(36, 168);
        brandPanel.Controls.Add(description);

        var checks = new[]
        {
            "One database shared with the user app",
            "Live cross-portal updates",
            "Authorized administrators only"
        };
        for (var index = 0; index < checks.Length; index++)
        {
            var check = AdminUi.MakeWrappedLabel("✓  " + checks[index], 8, Color.FromArgb(25, 120, 90), 300);
            check.Location = new Point(36, 250 + index * 26);
            brandPanel.Controls.Add(check);
        }
    }

    private void BuildCard()
    {
        card.BackColor = Color.White;
        shell.Controls.Add(card);

        var eyebrow = AdminUi.MakeLabel("ADMIN ACCESS", 8, Blue, true);
        eyebrow.Location = new Point(36, 40);
        card.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Admin sign in", 22, Navy, true);
        title.Location = new Point(36, 62);
        card.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Use your admin account to manage TaskTap.", 9, Muted);
        subtitle.Location = new Point(36, 98);
        card.Controls.Add(subtitle);

        var emailLabel = AdminUi.MakeLabel("Email address", 8, Navy, true);
        emailLabel.Location = new Point(36, 138);
        card.Controls.Add(emailLabel);

        emailBox.Name = "adminEmailTextBox";
        emailBox.Font = new Font("Segoe UI", 10F);
        emailBox.BorderStyle = BorderStyle.FixedSingle;
        card.Controls.Add(emailBox);

        var passwordLabel = AdminUi.MakeLabel("Password", 8, Navy, true);
        passwordLabel.Location = new Point(36, 202);
        card.Controls.Add(passwordLabel);

        passwordBox.Name = "adminPasswordTextBox";
        passwordBox.Font = new Font("Segoe UI", 10F);
        passwordBox.BorderStyle = BorderStyle.FixedSingle;
        passwordBox.UseSystemPasswordChar = true;
        card.Controls.Add(passwordBox);
        PasswordReveal.Attach(passwordBox);

        rememberBox.Name = "adminRememberCheckBox";
        rememberBox.Text = "Remember me";
        rememberBox.AutoSize = true;
        rememberBox.ForeColor = Muted;
        rememberBox.Font = new Font("Segoe UI", 8F);
        rememberBox.BackColor = Color.Transparent;
        card.Controls.Add(rememberBox);

        forgotButton.Name = "adminForgotButton";
        forgotButton.Text = "Forgot password?";
        forgotButton.AutoSize = true;
        forgotButton.FlatStyle = FlatStyle.Flat;
        forgotButton.BackColor = Color.Transparent;
        forgotButton.ForeColor = Blue;
        forgotButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        forgotButton.Cursor = Cursors.Hand;
        forgotButton.UseVisualStyleBackColor = false;
        forgotButton.FlatAppearance.BorderSize = 0;
        forgotButton.Click += (_, _) => feedback.Text = "Ask another Super Admin to reset your password.";
        card.Controls.Add(forgotButton);

        feedback.Name = "adminLoginFeedback";
        feedback.AutoSize = false;
        feedback.ForeColor = AdminUi.Bad;
        feedback.Font = new Font("Segoe UI", 8F);
        feedback.BackColor = Color.Transparent;
        card.Controls.Add(feedback);

        signInButton.Name = "adminSignInButton";
        signInButton.Text = "Sign in to Admin Portal";
        signInButton.BackColor = Blue;
        signInButton.ForeColor = Color.White;
        signInButton.FlatStyle = FlatStyle.Flat;
        signInButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        signInButton.Cursor = Cursors.Hand;
        signInButton.UseVisualStyleBackColor = false;
        signInButton.FlatAppearance.BorderSize = 0;
        signInButton.Click += (_, _) => Submit();
        card.Controls.Add(signInButton);

        hint.Name = "adminHint";
        hint.Text = "Sign in with your administrator account.";
        hint.AutoSize = true;
        hint.ForeColor = Color.FromArgb(120, 140, 160);
        hint.Font = new Font("Segoe UI", 8F);
        hint.BackColor = Color.Transparent;
        card.Controls.Add(hint);

        backLink.Name = "adminReturnToTaskTap";
        backLink.Text = "← Return to TaskTap";
        backLink.AutoSize = true;
        backLink.FlatStyle = FlatStyle.Flat;
        backLink.BackColor = Color.Transparent;
        backLink.ForeColor = Blue;
        backLink.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        backLink.Cursor = Cursors.Hand;
        backLink.UseVisualStyleBackColor = false;
        backLink.FlatAppearance.BorderSize = 0;
        backLink.Click += (_, _) => Close();
        card.Controls.Add(backLink);

        AcceptButton = signInButton;
    }

    private void LayoutPage()
    {
        if (body.ClientSize.Width <= 0 || body.ClientSize.Height <= 0)
            return;

        var width = Math.Min(920, body.ClientSize.Width - 48);
        var height = Math.Min(540, body.ClientSize.Height - 48);
        shell.Bounds = new Rectangle(
            (body.ClientSize.Width - width) / 2,
            (body.ClientSize.Height - height) / 2,
            width,
            height);

        var brandWidth = (int)(width * 0.42);
        brandPanel.Bounds = new Rectangle(0, 0, brandWidth, height);
        card.Bounds = new Rectangle(brandWidth, 0, width - brandWidth, height);

        var inner = card.Width - 72;
        const int left = 36;
        emailBox.Bounds = new Rectangle(left, 160, inner, 34);
        passwordBox.Bounds = new Rectangle(left, 224, inner - 34, 34);
        rememberBox.Location = new Point(left, 270);
        forgotButton.Location = new Point(left + inner - forgotButton.Width, 268);
        feedback.Bounds = new Rectangle(left, 296, inner, 18);
        signInButton.Bounds = new Rectangle(left, 320, inner, 44);
        hint.Location = new Point(left, 376);
        backLink.Location = new Point(left, 404);
    }

    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(emailBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Text))
        {
            feedback.Text = "Enter your admin email and password to continue.";
            return;
        }

        signInButton.Enabled = false;
        var result = Backend.Authenticate(emailBox.Text, passwordBox.Text, adminOnly: true);
        signInButton.Enabled = true;
        if (!result.Success || result.User is null)
        {
            feedback.Text = result.Error;
            passwordBox.Clear();
            return;
        }

        feedback.Text = string.Empty;
        passwordBox.Clear();
        AppSession.SignInAdminAs(result.User);

        Hide();
        var shellForm = new AdminShellForm();
        shellForm.FormClosed += (_, _) =>
        {
            if (IsDisposed)
                return;
            Show();
            Activate();
        };
        shellForm.Show();
    }
}
