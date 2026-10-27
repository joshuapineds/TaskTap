using System.Diagnostics;
using TaskTap.Data;

namespace TaskTap;

/// <summary>Picking, validating and opening the PDF (CV / achievements) an applicant attaches to a request.</summary>
internal static class CvTools
{
    private static bool _warned;

    public static string FormatSize(int bytes) =>
        bytes >= 1024 * 1024 ? $"{bytes / 1048576.0:0.0} MB" : $"{Math.Max(1, bytes / 1024)} KB";

    public static string Describe(string name, int size) =>
        string.IsNullOrWhiteSpace(name) ? "No CV attached" : $"📎 {name}  ·  {FormatSize(size)}";

    /// <summary>Lets the member choose a PDF and checks it. Returns null when cancelled or invalid.</summary>
    public static (string Name, byte[] Bytes)? Pick(IWin32Window owner)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choose your CV, resume or achievements (PDF)",
            Filter = "PDF documents (*.pdf)|*.pdf",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return null;

        try
        {
            if (new FileInfo(dialog.FileName).Length > Backend.MaxCvBytes)
            {
                MessageBox.Show(owner, $"The PDF is too large. The limit is {Backend.MaxCvBytes / (1024 * 1024)} MB.", "File too large", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var bytes = File.ReadAllBytes(dialog.FileName);
            var error = Backend.ValidateCv(dialog.FileName, bytes);
            if (error is not null)
            {
                MessageBox.Show(owner, error, "Cannot use this file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return (Path.GetFileName(dialog.FileName), bytes);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, "The file could not be read: " + ex.Message, "Cannot use this file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
    }

    /// <summary>Opens an applicant's CV in the default PDF viewer (only the poster and the applicant can fetch it).</summary>
    public static void Open(IWin32Window owner, int requestId)
    {
        (string Name, byte[] Bytes)? file = null;
        if (!Db.Try(() => file = Backend.GetAttachment(requestId, AppSession.CurrentUser.Id)))
            return;
        if (file is null)
        {
            MessageBox.Show(owner, "No CV was attached to this request.", "CV not available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!_warned)
        {
            var answer = MessageBox.Show(owner,
                "This PDF was uploaded by another member. Only open it if you trust it, and keep your PDF reader up to date.",
                "Open PDF", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (answer != DialogResult.OK)
                return;
            _warned = true;
        }

        try
        {
            var folder = Path.Combine(Path.GetTempPath(), "TaskTap", "cv");
            Directory.CreateDirectory(folder);
            var safeName = string.Concat(Path.GetFileName(file.Value.Name).Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            var path = Path.Combine(folder, $"{requestId}-{safeName}");
            File.WriteAllBytes(path, file.Value.Bytes);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, "The PDF could not be opened: " + ex.Message, "Cannot open PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>After acceptance each side can see how to reach the other, since TaskTap has no chat.</summary>
    public static void ShowContact(IWin32Window owner, int requestId, string otherRole)
    {
        (string Name, string Phone, string Email)? contact = null;
        if (!Db.Try(() => contact = Backend.GetAcceptedContact(requestId, AppSession.CurrentUser.Id)))
            return;
        if (contact is null)
        {
            MessageBox.Show(owner, "Contact details are shared only after a request is accepted.", "Contact", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var phone = string.IsNullOrWhiteSpace(contact.Value.Phone) ? "not provided" : contact.Value.Phone;
        MessageBox.Show(owner,
            $"{otherRole}: {contact.Value.Name}\r\nPhone: {phone}\r\nEmail: {contact.Value.Email}\r\n\r\nUse these only to arrange this task.",
            "Contact details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
