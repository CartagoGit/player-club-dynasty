using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal sealed class SqliteWorldConnectionFactory
{
    private readonly string _connectionString;

    internal SqliteWorldConnectionFactory(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(databasePath),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        }.ToString();
    }

    internal async ValueTask<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await SqliteWorldSchema.EnsureCreatedAsync(connection, cancellationToken);
        return connection;
    }
}
