using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

public sealed class SqliteCareerFinanceStore : ICareerFinanceStore
{
    private readonly object _gate = new();
    private bool _schemaInitialized;

    public SqliteCareerFinanceStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public void Save(CareerFinanceState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        lock (_gate)
        {
            EnsureParentDirectory();
            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            using SqliteTransaction transaction = connection.BeginTransaction();
            SqliteCareerFinanceWriter.Save(connection, transaction, state);
            transaction.Commit();
        }
    }

    public bool TryLoad(string clubExternalId, out CareerFinanceState? state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        lock (_gate)
        {
            if (!File.Exists(DatabasePath))
            {
                state = null;
                return false;
            }

            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            return SqliteCareerFinanceReader.TryLoad(connection, clubExternalId, out state);
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
            SqliteCareerFinanceWriter.Clear(connection, transaction);
            transaction.Commit();
        }
    }

    private void EnsureSchema(SqliteConnection connection)
    {
        if (_schemaInitialized)
        {
            return;
        }

        SqliteCareerFinanceSchema.EnsureCreated(connection);
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
