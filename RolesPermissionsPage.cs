namespace TaskTap;

/// <summary>
/// Roles &amp; Permissions: promote/demote administrators, assign a sub-role and edit
/// the role × permission matrix. Changes are logged to the shared activity feed.
/// </summary>
public sealed class RolesPermissionsPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel addCard = new();
    private readonly Panel adminsCard = new();
    private readonly Panel matrixCard = new();
    private readonly Panel adminsBody = new();
    private readonly Panel matrixBody = new();
    private readonly ComboBox userCombo = new();
    private readonly ComboBox roleCombo = new();
    private int _lastAdminsWidth = -1;
    private int _lastMatrixWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.RolesPermissions;

    public RolesPermissionsPage()
    {
        BuildContent();
    }

    private void BuildContent()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = AdminUi.Page;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var crumb = AdminUi.MakeLinkButton("← Settings", "rolesCrumb");
        crumb.Click += (_, _) => Nav.Navigate(AdminRoute.Settings);
        page.Controls.Add(crumb);

        var eyebrow = AdminUi.MakeLabel("ACCESS CONTROL", 8, AdminUi.Blue, true);
        eyebrow.Name = "rolesEyebrow";
        eyebrow.Location = new Point(32, 48);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Roles & Permissions", 26, AdminUi.Navy, true);
        title.Name = "rolesTitle";
        title.Location = new Point(32, 66);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Decide who can see and change each part of the admin portal.", 9, AdminUi.Muted);
        subtitle.Name = "rolesSubtitle";
        subtitle.Location = new Point(32, 118);
        page.Controls.Add(subtitle);

        BuildAddCard();
        BuildAdminsCard();
        BuildMatrixCard();

        RefreshUserCombo();
        RefreshAdmins();
        RefreshMatrix();
        LayoutPage();
    }

    private void BuildAddCard()
    {
        addCard.BackColor = Color.White;
        addCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(addCard);

        var title = AdminUi.MakeLabel("Add administrator", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        addCard.Controls.Add(title);

        var memberLabel = AdminUi.MakeLabel("Member", 8, AdminUi.Muted, true);
        memberLabel.Location = new Point(18, 46);
        addCard.Controls.Add(memberLabel);

        userCombo.Name = "rolesUserCombo";
        userCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        userCombo.Font = new Font("Segoe UI", 9F);
        userCombo.FlatStyle = FlatStyle.Flat;
        addCard.Controls.Add(userCombo);

        var roleLabel = AdminUi.MakeLabel("Role", 8, AdminUi.Muted, true);
        roleLabel.Location = new Point(248, 46);
        addCard.Controls.Add(roleLabel);

        roleCombo.Name = "rolesRoleCombo";
        roleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        roleCombo.Font = new Font("Segoe UI", 9F);
        roleCombo.FlatStyle = FlatStyle.Flat;
        roleCombo.Items.AddRange(DemoDataStore.AdminRoles);
        roleCombo.SelectedIndex = 1;
        addCard.Controls.Add(roleCombo);

        var add = AdminUi.MakePrimaryButton("Add administrator", "rolesAddButton", 150);
        add.Click += (_, _) => AddAdmin();
        addCard.Controls.Add(add);
    }

    private void BuildAdminsCard()
    {
        adminsCard.BackColor = Color.White;
        adminsCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(adminsCard);

        var title = AdminUi.MakeLabel("Administrators", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        adminsCard.Controls.Add(title);

        adminsBody.BackColor = Color.White;
        adminsCard.Controls.Add(adminsBody);
    }

    private void BuildMatrixCard()
    {
        matrixCard.BackColor = Color.White;
        matrixCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(matrixCard);

        var title = AdminUi.MakeLabel("Role permissions", 11, AdminUi.Navy, true);
        title.Location = new Point(18, 14);
        matrixCard.Controls.Add(title);

        matrixBody.BackColor = Color.White;
        matrixCard.Controls.Add(matrixBody);
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var crumb = AdminUi.Find<Button>(page, "rolesCrumb");
        if (crumb is not null) crumb.Location = new Point(left, 16);
        var eyebrow = AdminUi.Find<Label>(page, "rolesEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 46);
        var title = AdminUi.Find<Label>(page, "rolesTitle");
        if (title is not null) title.Location = new Point(left, 64);
        var subtitle = AdminUi.Find<Label>(page, "rolesSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 116);

        var leftWidth = (int)(contentWidth * 0.5);
        var rightWidth = contentWidth - leftWidth - 18;

        addCard.Bounds = new Rectangle(left, 154, leftWidth, 132);
        userCombo.Bounds = new Rectangle(18, 66, 220, 30);
        roleCombo.Bounds = new Rectangle(248, 66, 150, 30);
        var add = AdminUi.Find<Button>(addCard, "rolesAddButton");
        if (add is not null) add.Location = new Point(408, 66);

        adminsCard.Bounds = new Rectangle(left, 300, leftWidth, 400);
        adminsBody.Bounds = new Rectangle(14, 44, adminsCard.Width - 28, adminsCard.Height - 56);

        matrixCard.Bounds = new Rectangle(left + leftWidth + 18, 154, rightWidth, 546);
        matrixBody.Bounds = new Rectangle(14, 46, matrixCard.Width - 28, matrixCard.Height - 58);

        page.AutoScrollMinSize = new Size(0, 740);

        if (adminsBody.Width != _lastAdminsWidth)
        {
            _lastAdminsWidth = adminsBody.Width;
            RefreshAdmins();
        }

        if (matrixBody.Width != _lastMatrixWidth)
        {
            _lastMatrixWidth = matrixBody.Width;
            RefreshMatrix();
        }
    }

    private void RefreshUserCombo()
    {
        userCombo.Items.Clear();
        foreach (var user in DemoDataStore.Users.Where(user => !user.IsAdministrator).OrderBy(user => user.FullName))
            userCombo.Items.Add(user.FullName);
        if (userCombo.Items.Count > 0)
            userCombo.SelectedIndex = 0;
    }

    private void RefreshAdmins()
    {
        if (adminsBody.Width <= 0)
            return;

        adminsBody.SuspendLayout();
        adminsBody.Controls.Clear();

        var width = Math.Max(300, adminsBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var admin in DemoDataStore.Users.Where(user => user.IsAdministrator).OrderBy(user => user.FullName))
        {
            var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            row.SetBounds(0, y, width, 76);

            var avatar = new Label
            {
                Text = admin.Initials,
                Size = new Size(36, 36),
                Location = new Point(14, 18),
                BackColor = admin.AvatarColor,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            row.Controls.Add(avatar);

            var name = AdminUi.MakeLabel(admin.FullName, 10, AdminUi.Navy, true);
            name.Location = new Point(60, 12);
            row.Controls.Add(name);

            var email = AdminUi.MakeLabel(admin.Email, 8, AdminUi.Muted);
            email.Location = new Point(60, 32);
            row.Controls.Add(email);

            var lastActive = AdminUi.MakeLabel($"Last active {admin.LastActive}", 7.5F, AdminUi.Muted);
            lastActive.Location = new Point(60, 50);
            row.Controls.Add(lastActive);

            var roleSelect = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8F),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 28),
                Location = new Point(Math.Max(180, width - 200), 22)
            };
            roleSelect.Items.AddRange(DemoDataStore.AdminRoles);
            roleSelect.SelectedItem = admin.AdminRole;
            roleSelect.SelectedIndexChanged += (_, _) =>
            {
                if (roleSelect.SelectedItem is string newRole && newRole != admin.AdminRole)
                {
                    DemoDataStore.SetAdminRole(admin, newRole);
                    Toast($"{admin.FullName} is now {newRole}.");
                }
            };
            row.Controls.Add(roleSelect);

            var remove = AdminUi.MakeLinkButton("Remove", $"rolesRemove{admin.Id}");
            remove.ForeColor = AdminUi.Bad;
            remove.Location = new Point(Math.Max(320, width - 62), 24);
            remove.Click += (_, _) =>
            {
                DemoDataStore.DemoteAdmin(admin);
                RefreshUserCombo();
                RefreshAdmins();
                RefreshMatrix();
            };
            row.Controls.Add(remove);

            adminsBody.Controls.Add(row);
            y += 82;
        }

        if (DemoDataStore.Users.Count(user => user.IsAdministrator) <= 1)
        {
            var note = AdminUi.MakeLabel("At least one administrator is required.", 8, AdminUi.Muted);
            note.Location = new Point(4, y + 4);
            adminsBody.Controls.Add(note);
        }

        adminsBody.ResumeLayout();
    }

    private void RefreshMatrix()
    {
        if (matrixBody.Width <= 0)
            return;

        matrixBody.SuspendLayout();
        matrixBody.Controls.Clear();

        var roles = DemoDataStore.AdminRoles;
        var keys = DemoDataStore.AdminPermissionKeys;

        var permissionHeader = AdminUi.MakeLabel("Permission", 8, AdminUi.Muted, true);
        permissionHeader.Location = new Point(0, 0);
        matrixBody.Controls.Add(permissionHeader);

        for (var index = 0; index < roles.Length; index++)
        {
            var header = AdminUi.MakeLabel(roles[index], 8, AdminUi.Navy, true);
            header.Location = new Point(200 + index * 92, 0);
            matrixBody.Controls.Add(header);
        }

        var y = 28;
        foreach (var permission in keys)
        {
            var label = AdminUi.MakeLabel(permission, 9, AdminUi.Navy);
            label.Location = new Point(0, y);
            matrixBody.Controls.Add(label);

            for (var index = 0; index < roles.Length; index++)
            {
                var role = roles[index];
                var granted = DemoDataStore.AdminPermissions[role].Contains(permission);
                var check = new CheckBox
                {
                    Checked = granted,
                    Size = new Size(18, 18),
                    Location = new Point(200 + index * 92 + 30, y),
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                var capturedRole = role;
                var capturedPermission = permission;
                check.CheckedChanged += (_, _) =>
                {
                    DemoDataStore.SetRolePermission(capturedRole, capturedPermission, check.Checked);
                };
                matrixBody.Controls.Add(check);
            }

            y += 34;
        }

        var note = AdminUi.MakeLabel("Changes are saved automatically.", 8, AdminUi.Muted);
        note.Location = new Point(0, y + 6);
        matrixBody.Controls.Add(note);

        matrixBody.ResumeLayout();
    }

    private void AddAdmin()
    {
        var user = DemoDataStore.Users.FirstOrDefault(candidate => candidate.FullName == userCombo.SelectedItem?.ToString());
        if (user is null)
            return;
        var role = roleCombo.SelectedItem?.ToString() ?? "Moderator";
        DemoDataStore.PromoteToAdmin(user, role);
        RefreshUserCombo();
        RefreshAdmins();
        RefreshMatrix();
        Toast($"{user.FullName} is now {role}.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo()
    {
        RefreshUserCombo();
        RefreshAdmins();
        RefreshMatrix();
    }
}
