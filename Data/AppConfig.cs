using System.Text.Json;

namespace TaskTap.Data;

internal sealed class DatabaseSettings
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "tasktap";
    public string User { get; set; } = "root";
    public string Password { get; set; } = "";
}

/// <summary>Credentials for the first administrator, created only when the database has none.</summary>
internal sealed class AdminBootstrap
{
    public string FirstName { get; set; } = "TaskTap";
    public string LastName { get; set; } = "Admin";
    public string Email { get; set; } = "admin@tasktap.local";
    public string Password { get; set; } = "ChangeMe#2026";
}

/// <summary>Reads appsettings.json from the application folder (falls back to local defaults).</summary>
internal sealed class AppConfig
{
    public DatabaseSettings MySql { get; set; } = new();
    public AdminBootstrap Admin { get; set; } = new();

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return new AppConfig();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path), options) ?? new AppConfig();
    }
}
