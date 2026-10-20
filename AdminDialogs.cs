namespace TaskTap;

/// <summary>
/// Small modal used to capture a reason (reject verification, resolve a dispute, etc.).
/// </summary>
public sealed class ReasonDialog : Form
{
    private readonly ComboBox reasonBox = new();
    private readonly TextBox noteBox = new();

    public string? SelectedReason { get; private set; }

    public ReasonDialog(string title, string prompt, string[] reasons)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(440, 232);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        var promptLabel = new Label
        {
            Text = prompt,
            AutoSize = true,
            MaximumSize = new Size(400, 0),
            ForeColor = AdminUi.Navy,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Location = new Point(20, 18)
        };
        Controls.Add(promptLabel);

        reasonBox.Name = "reasonComboBox";
        reasonBox.DropDownStyle = ComboBoxStyle.DropDownList;
        reasonBox.Font = new Font("Segoe UI", 9F);
        reasonBox.Items.AddRange(reasons);
        reasonBox.SelectedIndex = 0;
        reasonBox.Location = new Point(20, 56);
        reasonBox.Size = new Size(400, 30);
        Controls.Add(reasonBox);

        var noteLabel = new Label
        {
            Text = "Note (optional)",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F),
            Location = new Point(20, 94)
        };
        Controls.Add(noteLabel);

        noteBox.Name = "reasonNoteBox";
        noteBox.Multiline = true;
        noteBox.BorderStyle = BorderStyle.FixedSingle;
        noteBox.Font = new Font("Segoe UI", 9F);
        noteBox.Location = new Point(20, 116);
        noteBox.Size = new Size(400, 52);
        Controls.Add(noteBox);

        var cancel = AdminUi.MakeSecondaryButton("Cancel", "ReasonCancelButton", 100);
        cancel.Location = new Point(204, 182);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(cancel);

        var confirm = AdminUi.MakePrimaryButton("Confirm", "ReasonConfirmButton", 116);
        confirm.Location = new Point(312, 182);
        confirm.Click += (_, _) =>
        {
            var note = noteBox.Text.Trim();
            SelectedReason = string.IsNullOrEmpty(note)
                ? reasonBox.SelectedItem?.ToString()
                : $"{reasonBox.SelectedItem}: {note}";
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(confirm);

        AcceptButton = confirm;
        CancelButton = cancel;
    }
}

/// <summary>Add/edit dialog for a task category.</summary>
public sealed class AdminCategoryDialog : Form
{
    private readonly TextBox nameBox = new();
    private readonly TextBox iconBox = new();
    private readonly CheckBox activeBox = new();

    public string CategoryName => nameBox.Text.Trim();
    public string CategoryIcon => string.IsNullOrWhiteSpace(iconBox.Text) ? "✦" : iconBox.Text.Trim();
    public bool CategoryActive => activeBox.Checked;

    public AdminCategoryDialog(DemoCategory? existing)
    {
        Text = existing is null ? "Add category" : "Edit category";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 236);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        var nameLabel = new Label
        {
            Text = "Category name",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 18)
        };
        Controls.Add(nameLabel);

        nameBox.Name = "categoryNameBox";
        nameBox.Text = existing?.Name ?? string.Empty;
        nameBox.BorderStyle = BorderStyle.FixedSingle;
        nameBox.Font = new Font("Segoe UI", 10F);
        nameBox.Location = new Point(20, 38);
        nameBox.Size = new Size(250, 30);
        Controls.Add(nameBox);

        var iconLabel = new Label
        {
            Text = "Icon",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(290, 18)
        };
        Controls.Add(iconLabel);

        iconBox.Name = "categoryIconBox";
        iconBox.Text = existing?.Icon ?? "✦";
        iconBox.BorderStyle = BorderStyle.FixedSingle;
        iconBox.Font = new Font("Segoe UI", 10F);
        iconBox.Location = new Point(290, 38);
        iconBox.Size = new Size(120, 30);
        Controls.Add(iconBox);

        activeBox.Name = "categoryActiveBox";
        activeBox.Text = "Visible in the user app";
        activeBox.AutoSize = true;
        activeBox.Checked = existing?.Active ?? true;
        activeBox.ForeColor = AdminUi.Navy;
        activeBox.Font = new Font("Segoe UI", 9F);
        activeBox.Location = new Point(20, 84);
        Controls.Add(activeBox);

        var cancel = AdminUi.MakeSecondaryButton("Cancel", "CategoryCancelButton", 100);
        cancel.Location = new Point(194, 184);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(cancel);

        var confirm = AdminUi.MakePrimaryButton(existing is null ? "Add category" : "Save changes", "CategoryConfirmButton", 130);
        confirm.Location = new Point(302, 184);
        confirm.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show(this, "Enter a category name.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(confirm);

        AcceptButton = confirm;
        CancelButton = cancel;
    }
}

/// <summary>Add/edit dialog for a barangay.</summary>
public sealed class AdminBarangayDialog : Form
{
    private readonly TextBox nameBox = new();
    private readonly TextBox cityBox = new();
    private readonly CheckBox activeBox = new();

    public string BarangayName => nameBox.Text.Trim();
    public string BarangayCity => string.IsNullOrWhiteSpace(cityBox.Text) ? "Caloocan City" : cityBox.Text.Trim();
    public bool BarangayActive => activeBox.Checked;

    public AdminBarangayDialog(DemoBarangay? existing)
    {
        Text = existing is null ? "Add barangay" : "Edit barangay";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 236);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        var nameLabel = new Label
        {
            Text = "Barangay name",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 18)
        };
        Controls.Add(nameLabel);

        nameBox.Name = "barangayNameBox";
        nameBox.Text = existing?.Name ?? string.Empty;
        nameBox.BorderStyle = BorderStyle.FixedSingle;
        nameBox.Font = new Font("Segoe UI", 10F);
        nameBox.Location = new Point(20, 38);
        nameBox.Size = new Size(390, 30);
        Controls.Add(nameBox);

        var cityLabel = new Label
        {
            Text = "City",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 82)
        };
        Controls.Add(cityLabel);

        cityBox.Name = "barangayCityBox";
        cityBox.Text = existing?.City ?? "Caloocan City";
        cityBox.BorderStyle = BorderStyle.FixedSingle;
        cityBox.Font = new Font("Segoe UI", 10F);
        cityBox.Location = new Point(20, 102);
        cityBox.Size = new Size(390, 30);
        Controls.Add(cityBox);

        activeBox.Name = "barangayActiveBox";
        activeBox.Text = "Active service area";
        activeBox.AutoSize = true;
        activeBox.Checked = existing?.Active ?? true;
        activeBox.ForeColor = AdminUi.Navy;
        activeBox.Font = new Font("Segoe UI", 9F);
        activeBox.Location = new Point(20, 146);
        Controls.Add(activeBox);

        var cancel = AdminUi.MakeSecondaryButton("Cancel", "BarangayCancelButton", 100);
        cancel.Location = new Point(194, 184);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(cancel);

        var confirm = AdminUi.MakePrimaryButton(existing is null ? "Add barangay" : "Save changes", "BarangayConfirmButton", 130);
        confirm.Location = new Point(302, 184);
        confirm.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show(this, "Enter a barangay name.", "Barangay", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(confirm);

        AcceptButton = confirm;
        CancelButton = cancel;
    }
}
