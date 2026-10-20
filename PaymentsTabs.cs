namespace TaskTap;

/// <summary>Segmented History / Earnings switcher shared by the two payments pages.</summary>
public static class PaymentsTabs
{
    public static Panel Build(bool historyActive)
    {
        var host = new Panel
        {
            Name = "paymentsTabs",
            BackColor = Color.FromArgb(236, 242, 249),
            Size = new Size(196, 36)
        };
        host.Controls.Add(MakeTab("History", "paymentsTabHistory", historyActive, 2, () => NavigationService.Current.Swap(Route.PaymentHistory)));
        host.Controls.Add(MakeTab("Earnings", "paymentsTabEarnings", !historyActive, 100, () => NavigationService.Current.Swap(Route.Earnings)));
        return host;
    }

    private static Button MakeTab(string text, string name, bool active, int x, Action open)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            Size = new Size(94, 32),
            Location = new Point(x, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? Color.White : Color.Transparent,
            ForeColor = active ? Color.FromArgb(8, 38, 87) : Color.FromArgb(100, 119, 143),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => open();
        return button;
    }
}
