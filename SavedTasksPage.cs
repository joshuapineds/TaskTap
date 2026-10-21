namespace TaskTap;

/// <summary>Signed-in list of tasks the user saved with the ♡ button.</summary>
public sealed class SavedTasksPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(100, 119, 143);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);

    private readonly Panel page = new();
    private readonly Panel listCard = new();
    private readonly Panel listBody = new();
    private readonly Label countLabel = new();
    private readonly Label emptyLabel = new();
    private int _lastWidth = -1;

    public override Route RouteName => Route.SavedTasks;

    public SavedTasksPage()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = PageColor;
        page.AutoScroll = true;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);

        var eyebrow = MakeLabel("YOUR LIST", 8, Blue, true);
        eyebrow.Name = "savedEyebrow";
        page.Controls.Add(eyebrow);

        var title = MakeLabel("Saved Tasks", 26, Navy, true);
        title.Name = "savedTitle";
        page.Controls.Add(title);

        var subtitle = MakeLabel("Tasks you saved to come back to.", 9, Muted);
        subtitle.Name = "savedSubtitle";
        page.Controls.Add(subtitle);

        listCard.BackColor = Color.White;
        listCard.BorderStyle = BorderStyle.FixedSingle;
        page.Controls.Add(listCard);

        var heading = MakeLabel("Your saved tasks", 11, Navy, true);
        heading.Location = new Point(18, 14);
        listCard.Controls.Add(heading);

        countLabel.AutoSize = true;
        countLabel.ForeColor = Muted;
        countLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        countLabel.Location = new Point(164, 16);
        listCard.Controls.Add(countLabel);

        emptyLabel.AutoSize = true;
        emptyLabel.ForeColor = Muted;
        emptyLabel.Font = new Font("Segoe UI", 9F);
        emptyLabel.Text = "No saved tasks yet. Tap ♡ on a task to save it here.";
        emptyLabel.Location = new Point(18, 56);
        listCard.Controls.Add(emptyLabel);

        listBody.BackColor = Color.White;
        listBody.AutoScroll = true;
        listCard.Controls.Add(listBody);

        RefreshList();
        LayoutPage();

        DemoDataStore.DataChanged += OnDataChanged;
    }

    private void LayoutPage()
    {
        if (page.ClientSize.Width <= 0)
            return;

        var contentWidth = Math.Max(880, page.ClientSize.Width - 64);
        if (contentWidth == _lastWidth)
            return;
        _lastWidth = contentWidth;

        const int left = 32;
        var eyebrow = FindLabel("savedEyebrow");
        if (eyebrow is not null) eyebrow.Location = new Point(left, 24);
        var title = FindLabel("savedTitle");
        if (title is not null) title.Location = new Point(left, 42);
        var subtitle = FindLabel("savedSubtitle");
        if (subtitle is not null) subtitle.Location = new Point(left, 94);

        listCard.Bounds = new Rectangle(left, 132, contentWidth, 560);
        listBody.Bounds = new Rectangle(14, 46, listCard.Width - 28, listCard.Height - 58);

        page.AutoScrollMinSize = new Size(0, 730);
        RefreshList();
    }

    private void RefreshList()
    {
        if (listBody.Width <= 0)
            return;

        countLabel.Text = $"{DemoDataStore.SavedTasks.Count} saved";
        emptyLabel.Visible = DemoDataStore.SavedTasks.Count == 0;

        listBody.SuspendLayout();
        listBody.Controls.Clear();

        var width = Math.Max(420, listBody.ClientSize.Width - 2);
        var y = 0;
        foreach (var task in DemoDataStore.SavedTasks.ToList())
        {
            var row = BuildRow(task, width);
            row.SetBounds(0, y, width, 84);
            listBody.Controls.Add(row);
            y += 90;
        }

        listBody.ResumeLayout();
    }

    private Panel BuildRow(DemoTask task, int width)
    {
        var row = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var icon = new Label
        {
            Text = task.Icon,
            Size = new Size(44, 44),
            Location = new Point(14, 16),
            BackColor = task.IconBack,
            ForeColor = Navy,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Emoji", 16F)
        };
        row.Controls.Add(icon);

        var title = MakeLabel(task.Title, 10, Navy, true);
        title.Location = new Point(70, 12);
        row.Controls.Add(title);

        var meta = MakeLabel($"{task.Category} · Brgy. {task.Barangay} · {task.Deadline} · ₱{task.Price:N0}", 8, Muted);
        meta.Location = new Point(70, 34);
        row.Controls.Add(meta);

        var view = MakeLink("View", $"SavedView{task.Title.Replace(" ", string.Empty)}");
        view.Location = new Point(width - 190, 28);
        view.Click += (_, _) => Nav.Navigate(new UserTaskDetailsPage(
            task.Title,
            task.Category,
            task.Price,
            task.Barangay,
            task.Days,
            task.Deadline,
            task.Icon,
            task.Description,
            task.IconBack));
        row.Controls.Add(view);

        var remove = MakeLink("Remove", $"SavedRemove{task.Title.Replace(" ", string.Empty)}");
        remove.ForeColor = Color.FromArgb(185, 55, 67);
        remove.Location = new Point(width - 110, 28);
        remove.Click += (_, _) =>
        {
            DemoDataStore.ToggleSaved(task);
            RefreshList();
        };
        row.Controls.Add(remove);

        return row;
    }

    private System.Windows.Forms.Label? FindLabel(string name) =>
        page.Controls.Find(name, false).OfType<System.Windows.Forms.Label>().FirstOrDefault();

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
    };

    private static Button MakeLink(string text, string name)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Blue,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void OnDataChanged()
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
        {
            BeginInvoke(OnDataChanged);
            return;
        }

        RefreshList();
    }

    public override void OnNavigatedTo() => RefreshList();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DemoDataStore.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }
}
