using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

public sealed class SqliteCareerLineupStore : ICareerLineupStore
{
    private readonly object _gate = new();
    private bool _schemaInitialized;

    public SqliteCareerLineupStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public void Save(LineupPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        lock (_gate)
        {
            EnsureParentDirectory();
            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            using SqliteTransaction transaction = connection.BeginTransaction();
            SqliteCareerLineupWriter.Save(connection, transaction, plan);
            transaction.Commit();
        }
    }

    public bool TryLoad(string clubExternalId, out LineupPlan? plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        lock (_gate)
        {
            if (!File.Exists(DatabasePath))
            {
                plan = null;
                return false;
            }

            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            return SqliteCareerLineupReader.TryLoad(connection, clubExternalId, out plan);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            if (!File.Exists(DatabasePath))
            {
                return;
            }

            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            using SqliteTransaction transaction = connection.BeginTransaction();
            SqliteCareerLineupWriter.Clear(connection, transaction);
            transaction.Commit();
        }
    }

    private void EnsureSchema(SqliteConnection connection)
    {
        if (_schemaInitialized)
        {
            return;
        }

        SqliteCareerLineupSchema.EnsureCreated(connection);
        _schemaInitialized = true;
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
        }.ToString());
        connection.Open();
        return connection;
    }

    private void EnsureParentDirectory()
    {
        string? directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
