using System.Drawing;

namespace TaskTap
{
    public sealed class TaskDetailsForm : Form
    {
        private readonly string taskTitle;
        private readonly string category;
        private readonly int price;
        private readonly string barangay;
        private readonly int days;
        private readonly string deadline;
        private readonly string icon;
        private readonly string description;
        private readonly Color iconBack;

        public TaskDetailsForm(string taskTitle, string category, int price, string barangay, int days, string deadline, string icon, string description, Color iconBack)
        {
            this.taskTitle = taskTitle;
            this.category = category;
            this.price = price;
            this.barangay = barangay;
            this.days = days;
            this.deadline = deadline;
            this.icon = icon;
            this.description = description;
            this.iconBack = iconBack;

            BuildPage();
        }

        private void BuildPage()
        {
            Text = $"TaskTap — {taskTitle}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 520);
            ClientSize = new Size(900, 620);
            BackColor = Color.FromArgb(247, 251, 255);
            Font = new Font("Segoe UI", 10F);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32),
                ColumnCount = 1,
                RowCount = 7,
                BackColor = BackColor
            };
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var title = new Label
            {
                AutoSize = true,
                Text = $"{icon}  {taskTitle}",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(7, 30, 70),
                Margin = new Padding(0, 0, 0, 8)
            };
            var categoryLabel = CreateInfoLabel($"Category: {category}");
            var priceLabel = CreateInfoLabel($"Budget: ₱{price:N0}");
            var locationLabel = CreateInfoLabel($"Location: Brgy. {barangay}   •   Deadline: {deadline}   •   {days} day(s)");
            var descriptionLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Text = description,
                ForeColor = Color.FromArgb(100, 116, 139),
                MaximumSize = new Size(820, 0),
                Margin = new Padding(0, 18, 0, 18)
            };
            var closeButton = new Button
            {
                Text = "Close",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(13, 63, 134),
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(16, 7, 16, 7)
            };
            closeButton.FlatAppearance.BorderColor = Color.FromArgb(189, 208, 230);

            content.Controls.Add(title, 0, 0);
            content.Controls.Add(categoryLabel, 0, 1);
            content.Controls.Add(priceLabel, 0, 2);
            content.Controls.Add(locationLabel, 0, 3);
            content.Controls.Add(descriptionLabel, 0, 4);
            content.Controls.Add(closeButton, 0, 5);
            Controls.Add(content);
            CancelButton = closeButton;
        }

        private static Label CreateInfoLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                ForeColor = Color.FromArgb(51, 71, 101),
                Margin = new Padding(0, 3, 0, 3)
            };
        }
    }
}