using System.Globalization;

namespace TaskTap;

/// <summary>Shared pieces of the Post Task and Edit Task forms.</summary>
internal static class TaskFormHelpers
{
    /// <summary>Methods the poster and performer agree on. TaskTap does not process payments; it records them.</summary>
    public static readonly string[] PaymentMethods = ["Select a method", "Cash", "GCash", "Maya", "Bank transfer"];

    /// <summary>Time picker with an "optional" check box, shown as 9:00 AM (not the current time).</summary>
    public static void ConfigureStartTime(DateTimePicker picker, string? saved)
    {
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = "h:mm tt";
        picker.ShowUpDown = true;
        picker.ShowCheckBox = true;
        if (DateTime.TryParseExact(saved ?? "", "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            picker.Value = DateTime.Today.Add(time.TimeOfDay);
            picker.Checked = true;
        }
        else
        {
            picker.Value = DateTime.Today.AddHours(9);
            picker.Checked = false;
        }
    }

    /// <summary>The chosen start time, or an empty string when the poster left it unchecked.</summary>
    public static string StartTimeText(DateTimePicker picker) =>
        picker.Checked ? picker.Value.ToString("h:mm tt", CultureInfo.InvariantCulture) : "";

    /// <summary>The area always comes from the poster's profile, so it cannot be mistyped or faked.</summary>
    public static void LockBarangay(ComboBox combo, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        combo.Items.Clear();
        combo.Items.Add("Select barangay");
        combo.Items.Add(value);
        combo.SelectedIndex = 1;
        combo.Enabled = false;
    }
}
