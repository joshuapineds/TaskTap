using TaskTap.Data;

namespace TaskTap
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            string error;
            while (!Db.Initialize(out error))
            {
                var choice = MessageBox.Show(
                    "TaskTap could not connect to MySQL.\r\n\r\n" + error +
                    "\r\n\r\nCheck that MySQL is running and that appsettings.json (next to TaskTap.exe) has the right server, user and password.",
                    "TaskTap - database unavailable",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);
                if (choice != DialogResult.Retry)
                    return;
            }

            if (Seeder.AdminCreatedFor is { } adminEmail)
            {
                MessageBox.Show(
                    "First-time setup finished. An administrator account was created:\r\n\r\n" + adminEmail +
                    "\r\n\r\nThe password is the one set under \"Admin\" in appsettings.json. Change it after signing in.",
                    "TaskTap - setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            DemoDataStore.LoadPublicData();
            Application.Run(new Form1());
        }
    }
}
