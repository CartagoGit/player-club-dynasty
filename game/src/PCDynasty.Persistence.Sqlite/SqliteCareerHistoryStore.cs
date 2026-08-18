using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

/// <summary>
/// SQLite adapter for the engine-neutral career history contract.
/// The caller chooses the database file, so saves can be imported, backed up
/// or replaced without changing the manager or presentation layers.
/// </summary>
public sealed class SqliteCareerHistoryStore : ICareerHistoryStore
{
    private readonly object _gate = new();
    private bool _schemaInitialized;

    public SqliteCareerHistoryStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public void SaveMatch(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Execute(connection => SqliteCareerHistoryWriter.SaveMatch(connection, entry));
    }

    public void SaveSeason(SeasonHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Execute(connection => SqliteCareerHistoryWriter.SaveSeason(connection, entry));
    }

    public IReadOnlyList<HistoryEntry> LoadMatches()
    {
        lock (_gate)
        {
            EnsureSchema();
            using SqliteConnection connection = Open();
            return SqliteCareerHistoryReader.LoadMatches(connection);
        }
    }

    public IReadOnlyList<SeasonHistoryEntry> LoadSeasons()
    {
        lock (_gate)
        {
            EnsureSchema();
            using SqliteConnection connection = Open();
            return SqliteCareerHistoryReader.LoadSeasons(connection);
        }
    }

    public void Clear() => Execute(SqliteCareerHistoryWriter.Clear);

    private void Execute(Action<SqliteConnection> operation)
    {
        lock (_gate)
        {
            EnsureSchema();
            using SqliteConnection connection = Open();
            operation(connection);
        }
    }

    private void EnsureSchema()
    {
        if (_schemaInitialized)
        {
            return;
        }

        string? directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using SqliteConnection connection = Open();
        SqliteCareerHistorySchema.EnsureCreated(connection);
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
}
