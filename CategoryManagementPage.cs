namespace TaskTap;

/// <summary>
/// Category Management: add, edit, reorder, pause and delete the categories the user
/// app reads for tiles, filters and the Post Task picker.
/// </summary>
public sealed class CategoryManagementPage : AdminPage
{
    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private int _lastListWidth = -1;

    public override AdminRoute AdminRouteName => AdminRoute.Categories;

    public CategoryManagementPage()
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

        var eyebrow = AdminUi.MakeLabel("CATALOG", 8, AdminUi.Blue, true);
        eyebrow.Name = "categoriesEyebrow";
        eyebrow.Location = new Point(32, 24);
        page.Controls.Add(eyebrow);

        var title = AdminUi.MakeLabel("Category Management", 26, AdminUi.Navy, true);
        title.Name = "categoriesTitle";
        title.Location = new Point(32, 42);
        page.Controls.Add(title);

        var subtitle = AdminUi.MakeLabel("The categories shown in the user app's tiles, filters and task pickers.", 9, AdminUi.Muted);
        subtitle.Name = "categoriesSubtitle";
        subtitle.Location = new Point(32, 94);
        page.Controls.Add(subtitle);

        var add = AdminUi.MakePrimaryButton("+ Add category", "categoriesAddButton", 160);
        add.Click += (_, _) => AddCategory();
        page.Controls.Add(add);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var header = AdminUi.MakeLabel("Categories", 11, AdminUi.Navy, true);
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

        var eyebrow = AdminUi.Find<Label>(page, "categoriesEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = AdminUi.Find<Label>(page, "categoriesTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = AdminUi.Find<Label>(page, "categoriesSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);
        var add = AdminUi.Find<Button>(page, "categoriesAddButton");
        if (add is not null) add.Location = new Point(left + contentWidth - 160, 40);

        listCard.Bounds = new Rectangle(left, 132, contentWidth, 620);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);
        page.AutoScrollMinSize = new Size(0, 790);

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
        var categories = DemoDataStore.Categories.OrderBy(category => category.SortOrder).ToList();
        var y = 0;
        foreach (var category in categories)
        {
            var row = BuildRow(category, width);
            row.SetBounds(0, y, width, 72);
            listBody.Controls.Add(row);
            y += 78;
        }

        if (categories.Count == 0)
        {
            var empty = AdminUi.MakeLabel("No categories yet. Add one to get started.", 9, AdminUi.Muted);
            empty.Location = new Point(4, 6);
            listBody.Controls.Add(empty);
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(DemoCategory category, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var icon = new Label
        {
            Text = category.Icon,
            Size = new Size(38, 38),
            Location = new Point(14, 16),
            BackColor = category.Color,
            ForeColor = category.Accent,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold)
        };
        row.Controls.Add(icon);

        var name = AdminUi.MakeLabel(category.Name, 10, AdminUi.Navy, true);
        name.Location = new Point(64, 14);
        row.Controls.Add(name);

        var taskCount = DemoDataStore.PostedTasks.Count(task => task.Category == category.Name)
                      + DemoDataStore.AvailableTasks.Count(task => task.Category == category.Name);
        var meta = AdminUi.MakeLabel($"{taskCount} task{(taskCount == 1 ? "" : "s")} · order #{category.SortOrder}", 8, AdminUi.Muted);
        meta.Name = "categoryRowMeta";
        meta.MaximumSize = new Size(Math.Max(200, width - 520), 18);
        meta.Location = new Point(64, 34);
        row.Controls.Add(meta);

        var status = AdminUi.MakePill(
            category.Active ? "Active" : "Paused",
            category.Active ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 247, 224),
            category.Active ? AdminUi.Good : Color.FromArgb(180, 130, 20));
        status.Cursor = Cursors.Hand;
        status.Location = new Point(width - 430, 18);
        status.Click += (_, _) =>
        {
            DemoDataStore.ToggleCategory(category);
            RefreshList();
        };
        row.Controls.Add(status);

        var up = AdminUi.MakeSecondaryButton("↑", $"categoryUp{category.Id}", 34);
        up.Size = new Size(34, 28);
        up.Location = new Point(width - 340, 18);
        up.Click += (_, _) =>
        {
            DemoDataStore.MoveCategory(category, -1);
            RefreshList();
        };
        row.Controls.Add(up);

        var down = AdminUi.MakeSecondaryButton("↓", $"categoryDown{category.Id}", 34);
        down.Size = new Size(34, 28);
        down.Location = new Point(width - 300, 18);
        down.Click += (_, _) =>
        {
            DemoDataStore.MoveCategory(category, 1);
            RefreshList();
        };
        row.Controls.Add(down);

        var edit = AdminUi.MakeLinkButton("Edit", $"categoryEdit{category.Id}");
        edit.Location = new Point(width - 250, 22);
        edit.Click += (_, _) => EditCategory(category);
        row.Controls.Add(edit);

        var delete = AdminUi.MakeLinkButton("Delete", $"categoryDelete{category.Id}");
        delete.ForeColor = AdminUi.Bad;
        delete.Location = new Point(width - 176, 22);
        delete.Click += (_, _) => DeleteCategory(category);
        row.Controls.Add(delete);

        return row;
    }

    private void AddCategory()
    {
        using var dialog = new AdminCategoryDialog(null);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        if (DemoDataStore.Categories.Any(category => category.Name.Equals(dialog.CategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "A category with that name already exists.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var nextId = DemoDataStore.Categories
            .Select(category => int.TryParse(category.Id.Replace("CAT-", string.Empty), out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var created = new DemoCategory
        {
            Id = $"CAT-{nextId:00}",
            Name = dialog.CategoryName,
            Icon = dialog.CategoryIcon,
            Color = Color.FromArgb(238, 245, 255),
            Accent = AdminUi.Blue,
            Active = dialog.CategoryActive
        };
        DemoDataStore.AddCategory(created);
        RefreshList();
        Toast($"{created.Name} added.");
    }

    private void EditCategory(DemoCategory category)
    {
        using var dialog = new AdminCategoryDialog(category);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        category.Name = dialog.CategoryName;
        category.Icon = dialog.CategoryIcon;
        category.Active = dialog.CategoryActive;
        DemoDataStore.UpdateCategory(category);
        RefreshList();
        Toast($"{category.Name} updated.");
    }

    private void DeleteCategory(DemoCategory category)
    {
        var answer = MessageBox.Show(
            this,
            $"Delete the \"{category.Name}\" category?\n\nTasks keep their category label, but the category disappears from the user app.",
            "Delete category",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        DemoDataStore.DeleteCategory(category);
        RefreshList();
        Toast($"{category.Name} deleted.");
    }

    private static void Toast(string message) => AdminNavigationService.Current.Shell?.ShowToast(message);

    public override void OnNavigatedTo() => RefreshList();
}
