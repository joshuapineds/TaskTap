using System.Runtime.InteropServices;

namespace TaskTap;

/// <summary>Adds a small "eye" button inside a password TextBox that shows or hides what was typed.</summary>
internal static class PasswordReveal
{
    private const int EM_SETMARGINS = 0xD3;
    private const int EC_RIGHTMARGIN = 2;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

    public static void Attach(TextBox box)
    {
        var eye = new Button
        {
            Name = (string.IsNullOrEmpty(box.Name) ? "password" : box.Name) + "RevealButton",
            Text = "👁",
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Emoji", 9F),
            ForeColor = Color.FromArgb(91, 111, 134),
            BackColor = Color.White,
            UseVisualStyleBackColor = false,
            Size = new Size(30, 24)
        };
        eye.FlatAppearance.BorderSize = 0;
        eye.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 243, 248);
        eye.AccessibleName = "Show or hide password";

        eye.Click += (_, _) =>
        {
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            eye.Text = box.UseSystemPasswordChar ? "👁" : "🙈";
            box.Focus();
        };

        void Place()
        {
            if (box.Parent is null)
                return;
            if (!ReferenceEquals(eye.Parent, box.Parent))
            {
                eye.Parent?.Controls.Remove(eye);
                box.Parent.Controls.Add(eye);
            }
            eye.Height = Math.Max(20, box.Height - 4);
            eye.Location = new Point(box.Right - eye.Width - 3, box.Top + 2);
            eye.Visible = box.Visible;
            eye.BringToFront();
        }

        void ReserveSpace()
        {
            if (box.IsHandleCreated)
                SendMessage(box.Handle, EM_SETMARGINS, new IntPtr(EC_RIGHTMARGIN), new IntPtr((eye.Width + 4) << 16));
        }

        box.ParentChanged += (_, _) => Place();
        box.LocationChanged += (_, _) => Place();
        box.SizeChanged += (_, _) => Place();
        box.VisibleChanged += (_, _) => Place();
        box.HandleCreated += (_, _) => ReserveSpace();
        box.Disposed += (_, _) => eye.Dispose();
        Place();
        ReserveSpace();
    }
}
