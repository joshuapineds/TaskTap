namespace TaskTap;

/// <summary>
/// Barangay Management: add, edit and pause the service areas the user app reads for
/// its address dropdowns and filters.
/// </summary>
public sealed class BarangayManagementPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private int _lastListWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Barangays;

    public BarangayManagementPage()
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

        var eyebrow = AdminUi.MakeLabel("SERVICE AREAS", 8, AdminUi.Blue, true);
        eyebrow.Name = "barangaysEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Barangay Management", 26, AdminUi.Navy, true);
        title.Name = "barangaysTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("Barangays power address dropdowns, filters and locality counts in the user app.", 9, AdminUi.Muted);
        subtitle.Name = "barangaysSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var add = AdminUi.MakePrimaryButton("+ Add barangay", "barangaysAddButton", 170);
        add.Click += (_, _) => AddBarangay();
        page.Controls.Add(add);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var header = AdminUi.MakeLabel("Barangays", 11, AdminUi.Navy, true);
        header.Location = new Point(18, 14);
        listCard.Controls.Add(header);

        listBody.BackColor = Color.White;
        listCard.Controls.Add(listBody);

        RefreshList();
        LayoutPage();
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        const int left = 32;

        var eyebrow = AdminUi.Find<Label>(page, "barangaysEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "barangaysTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "barangaysSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var add = AdminUi.Find<Button>(page, "barangaysAddButton");
        if (add is not null) add.Location = new Point(left + contentWidth - 170, 40);

        listCard.Bounds = new Rectangle(left, 132, contentWidth, 560);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);
        page.AutoScrollMinSize = new Size(0, 730);

        if (listBody.Width != _lastListWidth)
        {
            _lastListWidth = listBody.Width;
            RefreshList();
        }
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(400, listBody.ClientSize.Width - 2);
        var barangays = DemoDataStore.Barangays.OrderBy(barangay => barangay.Id).ToList();
        var y = 0;
        foreach (var barangay in barangays)
        {
            var row = BuildRow(barangay, width);
            row.SetBounds(0, y, width, 76);
            listBody.Controls.Add(row);
            y += 82;
        }

        if (barangays.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No service areas yet. Add one to get started.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(DemoBarangay barangay, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var name = AdminUi.MakeLabel(barangay.Name, 10, AdminUi.Navy, true);
        name.Location = new Point(14, 14);
        row.Controls.Add(name);

        var number = barangay.Name.Replace("Brgy. ", string.Empty).Trim();
        var taskCount = DemoDataStore.PostedTasks.Count(task => task.Barangay == number)
                      + DemoDataStore.AvailableTasks.Count(task => task.Barangay == number);
        var userCount = DemoDataStore.Users.Count(user => user.Barangay == number);

        var meta = AdminUi.MakeLabel($"{barangay.City} · {taskCount} task{(taskCount == 1 ? "" : "s")} · {userCount} member{(userCount == 1 ? "" : "s")}", 8, AdminUi.Muted);
        meta.Name = "barangayRowMeta";
        meta.MaximumSize = new Size(Math.Max(200, width - 480), 18);
        meta.Location = new Point(14, 36);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(
            barangay.Active ? "Active" : "Paused",
            barangay.Active ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 247, 224),
            barangay.Active ? AdminUi.Good : Color.FromArgb(180, 130, 20));
        status.Location = new Point(width - 430, 20);
        row.Controls.Add(status);

        var edit = AdminUi.MakeLinkButton("Edit", $"barangayEdit{barangay.Id}");
        edit.Location = new Point(width - 250, 24);
        edit.Click += (_, _) => EditBarangay(barangay);
        row.Controls.Add(edit);

        var toggle = AdminUi.MakeLinkButton(barangay.Active ? "Deactivate" : "Activate", $"barangayToggle{barangay.Id}");
        toggle.ForeColor = barangay.Active ? AdminUi.Bad : AdminUi.Good;
        toggle.Location = new Point(width - 186, 24);
        toggle.Click += (_, _) =>
        {
            if (barangay.Active)
                DemoDataStore.DeactivateBarangay(barangay);
            else
                DemoDataStore.ActivateBarangay(barangay);
            RefreshList();
        };
        row.Controls.Add(toggle);

        return row;
    }

    private void AddBarangay()
    {
        using var dialog = new AdminBarangayDialog(null);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        if (DemoDataStore.Barangays.Any(barangay => barangay.Name.Equals(dialog.BarangayName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "That barangay already exists.", "Barangay", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var number = dialog.BarangayName.Replace("Brgy. ", string.Empty).Trim();
        var id = number.Length > 0 && number.All(char.IsDigit)
            ? $"BRY-{number}"
            : $"BRY-{DemoDataStore.Barangays.Count + 1:000}";

        var created = new DemoBarangay
        {
            Id = id,
            Name = dialog.BarangayName,
            City = dialog.BarangayCity,
            Active = dialog.BarangayActive
        };
        DemoDataStore.AddBarangay(created);
        RefreshList();
        Toast($"{created.Name} added.");
    }

    private void EditBarangay(DemoBarangay barangay)
    {
        using var dialog = new AdminBarangayDialog(barangay);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        barangay.Name = dialog.BarangayName;
        barangay.City = dialog.BarangayCity;
        barangay.Active = dialog.BarangayActive;
        DemoDataStore.UpdateBarangay(barangay);
        RefreshList();
        Toast($"{barangay.Name} updated.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
