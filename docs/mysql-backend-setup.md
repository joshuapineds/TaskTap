# TaskTap – MySQL backend setup

TaskTap now stores everything in MySQL (or MariaDB, e.g. the one bundled with XAMPP).

## 1. Start MySQL
- XAMPP: start **MySQL** in the control panel. Default user `root`, empty password.
- Any MySQL 8 / MariaDB 10.4+ server works.

## 2. Configure the connection
Edit `appsettings.json` (it is copied next to `TaskTap.exe` on every build):

```json
{
  "MySql": { "Server": "localhost", "Port": 3306, "Database": "tasktap", "User": "root", "Password": "" },
  "Admin": { "FirstName": "TaskTap", "LastName": "Admin", "Email": "admin@tasktap.local", "Password": "ChangeMe#2026" }
}
```

## 3. Run
`dotnet restore` will download the `MySqlConnector` package, then run the app.
On first start TaskTap automatically:
- creates the `tasktap` database and all tables (same SQL as `database/schema.sql`),
- inserts the default categories, barangays, role permissions and settings,
- creates the first administrator from the `Admin` section (only if no admin exists).

Sign in to the admin portal with that email/password and change the password under **Settings → Change password**.
Members create their own accounts from **Register**.

## What is stored
users (PBKDF2-SHA256 salted password hashes), tasks, task_requests (shortlist/accept/decline),
saved_tasks, conversations + messages, notifications, payments, reviews, reports, disputes,
support_tickets, announcements, activity_log, categories, barangays, role_permissions, app_settings.

## Notes
- Pages that show database data are rebuilt on every visit, and the data is refreshed from MySQL
  when it is more than 10 seconds old, so changes from other members show up as you navigate.
- Five wrong sign-in attempts for the same email lock that identity for one minute.
- The app connects to MySQL directly with the credentials in `appsettings.json`. That is fine for a
  school/local deployment; for a public release put an API in front of the database.
