using System.Text.RegularExpressions;
using MySqlConnector;

namespace TaskTap.Data;

/// <summary>
/// Thin MySQL access helper. Every call borrows a pooled connection, so repositories
/// stay stateless. All values are passed as parameters (never concatenated into SQL).
/// </summary>
public static class Db
{
    private static string _connectionString = "";
    private static DateTime _lastErrorShown = DateTime.MinValue;

    public static bool IsReady { get; private set; }
    public static string DatabaseName { get; private set; } = "";
    public static string LastError { get; private set; } = "";

    /// <summary>Creates the database and tables if needed, then seeds lookup data and the first admin.</summary>
    public static bool Initialize(out string error)
    {
        try
        {
            var config = AppConfig.Load();
            var settings = config.MySql;
            if (!Regex.IsMatch(settings.Database, "^[A-Za-z0-9_]{1,64}$"))
                throw new InvalidOperationException("MySql:Database in appsettings.json may only contain letters, digits and underscores.");

            var builder = new MySqlConnectionStringBuilder
            {
                Server = settings.Server,
                Port = (uint)settings.Port,
                UserID = settings.User,
                Password = settings.Password,
                CharacterSet = "utf8mb4",
                ConnectionTimeout = 8,
                DefaultCommandTimeout = 30
            };

            using (var serverConnection = new MySqlConnection(builder.ConnectionString))
            {
                serverConnection.Open();
                using var create = serverConnection.CreateCommand();
                create.CommandText = $"CREATE DATABASE IF NOT EXISTS `{settings.Database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                create.ExecuteNonQuery();
            }

            builder.Database = settings.Database;
            _connectionString = builder.ConnectionString;
            DatabaseName = settings.Database;

            foreach (var statement in Schema.Statements)
                NonQuery(statement);

            Seeder.Run(config);
            IsReady = true;
            LastError = "";
            error = "";
            return true;
        }
        catch (Exception ex)
        {
            IsReady = false;
            LastError = ex.Message;
            error = ex.Message;
            return false;
        }
    }

    public static MySqlConnection Open()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static void Bind(MySqlCommand command, (string Name, object? Value)[] parameters)
    {
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    public static int NonQuery(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        Bind(command, parameters);
        return command.ExecuteNonQuery();
    }

    /// <summary>Runs an INSERT and returns the generated AUTO_INCREMENT id.</summary>
    public static long Insert(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        Bind(command, parameters);
        command.ExecuteNonQuery();
        return command.LastInsertedId;
    }

    public static object? Scalar(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        Bind(command, parameters);
        var value = command.ExecuteScalar();
        return value is DBNull ? null : value;
    }

    public static List<T> Query<T>(string sql, Func<MySqlDataReader, T> map, params (string Name, object? Value)[] parameters)
    {
        var rows = new List<T>();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        Bind(command, parameters);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            rows.Add(map(reader));
        return rows;
    }

    /// <summary>Statement runner handed to <see cref="Db.Transaction"/> callbacks.</summary>
    public sealed class Tx
    {
        private readonly MySqlConnection _connection;
        private readonly MySqlTransaction _transaction;

        internal Tx(MySqlConnection connection, MySqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public int Run(string sql, params (string Name, object? Value)[] parameters)
        {
            using var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = sql;
            Bind(command, parameters);
            return command.ExecuteNonQuery();
        }
    }

    /// <summary>Runs several statements atomically on one connection.</summary>
    public static void Transaction(Action<Tx> work)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            work(new Tx(connection, transaction));
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Runs a write-through action. A database failure is reported once (rate limited) instead
    /// of crashing the page that triggered it; the in-memory state stays usable.
    /// </summary>
    public static bool Try(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            if (DateTime.Now - _lastErrorShown > TimeSpan.FromSeconds(8))
            {
                _lastErrorShown = DateTime.Now;
                MessageBox.Show(
                    $"TaskTap could not reach the database, so your last change may not have been saved.\r\n\r\n{ex.Message}",
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            return false;
        }
    }

    // ----- Reader helpers ----------------------------------------------------

    public static string Str(this MySqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
    }

    public static int Int(this MySqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    public static bool Bool(this MySqlDataReader reader, string column) => reader.Int(column) != 0;

    public static DateTime Date(this MySqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
    }

    public static DateTime? DateOrNull(this MySqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
