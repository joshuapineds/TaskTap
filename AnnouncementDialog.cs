namespace TaskTap;

/// <summary>Dialog used to publish a new announcement.</summary>
public sealed class AnnouncementDialog : Form
{
    private readonly TextBox titleBox = new();
    private readonly TextBox bodyBox = new();
    private readonly ComboBox audienceBox = new();
    private readonly CheckBox activeBox = new();

    public string AnnouncementTitle => titleBox.Text.Trim();
    public string AnnouncementBody => bodyBox.Text.Trim();
    public string AnnouncementAudience => audienceBox.SelectedItem?.ToString() ?? "All members";
    public bool AnnouncementActive => activeBox.Checked;

    public AnnouncementDialog()
    {
        Text = "New announcement";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 356);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        var titleLabel = new Label
        {
            Text = "Title",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 16)
        };
        Controls.Add(titleLabel);

        titleBox.Name = "announcementTitleBox";
        titleBox.BorderStyle = BorderStyle.FixedSingle;
        titleBox.Font = new Font("Segoe UI", 10F);
        titleBox.Location = new Point(20, 36);
        titleBox.Size = new Size(440, 30);
        Controls.Add(titleBox);

        var audienceLabel = new Label
        {
            Text = "Audience",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 80)
        };
        Controls.Add(audienceLabel);

        audienceBox.Name = "announcementAudienceBox";
        audienceBox.DropDownStyle = ComboBoxStyle.DropDownList;
        audienceBox.Font = new Font("Segoe UI", 9F);
        audienceBox.FlatStyle = FlatStyle.Flat;
        audienceBox.Items.AddRange(["All members", "Unverified members", "Task doers", "Task posters"]);
        audienceBox.SelectedIndex = 0;
        audienceBox.Location = new Point(20, 100);
        audienceBox.Size = new Size(220, 30);
        Controls.Add(audienceBox);

        activeBox.Name = "announcementActiveBox";
        activeBox.Text = "Publish immediately";
        activeBox.AutoSize = true;
        activeBox.Checked = true;
        activeBox.ForeColor = AdminUi.Navy;
        activeBox.Font = new Font("Segoe UI", 9F);
        activeBox.Location = new Point(260, 104);
        Controls.Add(activeBox);

        var bodyLabel = new Label
        {
            Text = "Message",
            AutoSize = true,
            ForeColor = AdminUi.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(20, 144)
        };
        Controls.Add(bodyLabel);

        bodyBox.Name = "announcementBodyBox";
        bodyBox.BorderStyle = BorderStyle.FixedSingle;
        bodyBox.Font = new Font("Segoe UI", 9F);
        bodyBox.Multiline = true;
        bodyBox.ScrollBars = ScrollBars.Vertical;
        bodyBox.Location = new Point(20, 164);
        bodyBox.Size = new Size(440, 108);
        Controls.Add(bodyBox);

        var cancel = AdminUi.MakeSecondaryButton("Cancel", "AnnouncementCancelButton", 100);
        cancel.Location = new Point(244, 296);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(cancel);

        var confirm = AdminUi.MakePrimaryButton("Publish", "AnnouncementConfirmButton", 120);
        confirm.Location = new Point(352, 296);
        confirm.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(titleBox.Text) || string.IsNullOrWhiteSpace(bodyBox.Text))
            {
                MessageBox.Show(this, "Enter a title and a message.", "Announcement", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
