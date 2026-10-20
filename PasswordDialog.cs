using TaskTap.Data;

namespace TaskTap;

/// <summary>Modal "change password" dialog shared by the member and admin portals.</summary>
public sealed class PasswordDialog : Form
{
    private readonly TextBox current = new() { UseSystemPasswordChar = true };
    private readonly TextBox next = new() { UseSystemPasswordChar = true };
    private readonly TextBox confirm = new() { UseSystemPasswordChar = true };
    private readonly Label message = new() { AutoSize = false, ForeColor = Color.FromArgb(190, 60, 60) };
    private readonly int userId;

    private PasswordDialog(int userId)
    {
        this.userId = userId;
        Text = "Change password";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(380, 300);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        AddRow("Current password", current, 16);
        AddRow("New password (min. 8 characters)", next, 76);
        AddRow("Confirm new password", confirm, 136);
        message.Bounds = new Rectangle(20, 200, 340, 36);
        Controls.Add(message);

        var cancel = new Button { Text = "Cancel", Bounds = new Rectangle(160, 248, 90, 34), DialogResult = DialogResult.Cancel };
        var save = new Button { Text = "Save", Bounds = new Rectangle(262, 248, 98, 34), BackColor = Color.FromArgb(8, 117, 232), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        save.FlatAppearance.BorderSize = 0;
        save.Click += (_, _) => Save();
        Controls.Add(cancel);
        Controls.Add(save);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void AddRow(string label, TextBox box, int top)
    {
        Controls.Add(new Label { Text = label, Location = new Point(20, top), AutoSize = true, ForeColor = Color.FromArgb(8, 38, 87), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) });
        box.Bounds = new Rectangle(20, top + 22, 340, 28);
        Controls.Add(box);
    }

    private void Save()
    {
        if (next.Text != confirm.Text)
        {
            message.Text = "The new passwords do not match.";
            return;
        }

        var (success, error) = Backend.ChangePassword(userId, current.Text, next.Text);
        if (!success)
        {
            message.Text = error;
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>Shows the dialog; returns true when the password was changed.</summary>
    public static bool ShowFor(IWin32Window owner, int userId)
    {
        using var dialog = new PasswordDialog(userId);
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }
}
