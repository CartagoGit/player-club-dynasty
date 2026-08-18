using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

public sealed class SqliteCareerTransferStore : ICareerTransferStore
{
    private readonly object _gate = new();
    private bool _schemaInitialized;

    public SqliteCareerTransferStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public IReadOnlyList<CareerTransferRecord> ListForClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        lock (_gate)
        {
            if (!File.Exists(DatabasePath))
            {
                return ImmutableArray<CareerTransferRecord>.Empty;
            }

            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            return SqliteCareerTransferReader.ListForClub(connection, clubExternalId);
        }
    }

    public bool TrySettle(
        CareerTransferRecord transfer,
        CareerFinanceState expectedFinance,
        CareerFinanceState updatedFinance)
    {
        ValidateSettlement(transfer, expectedFinance, updatedFinance);
        lock (_gate)
        {
            EnsureParentDirectory();
            using SqliteConnection connection = Open();
            EnsureSchema(connection);
            using SqliteTransaction transaction = connection.BeginTransaction();
            if (!SqliteCareerFinanceWriter.TryReplaceExpected(
                    connection,
                    transaction,
                    expectedFinance,
                    updatedFinance) ||
                !SqliteCareerTransferWriter.TryInsert(connection, transaction, transfer))
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
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
            SqliteCareerTransferWriter.Clear(connection, transaction);
            transaction.Commit();
        }
    }

    private static void ValidateSettlement(
        CareerTransferRecord transfer,
        CareerFinanceState expectedFinance,
        CareerFinanceState updatedFinance)
    {
        ArgumentNullException.ThrowIfNull(transfer);
        ArgumentNullException.ThrowIfNull(expectedFinance);
        ArgumentNullException.ThrowIfNull(updatedFinance);
        if (!string.Equals(transfer.BuyerClubExternalId, expectedFinance.ClubExternalId, StringComparison.Ordinal) ||
            !string.Equals(expectedFinance.ClubExternalId, updatedFinance.ClubExternalId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Transfer and finance snapshots must belong to the same buyer club.");
        }

        if (!string.Equals(transfer.Currency, expectedFinance.Currency, StringComparison.Ordinal) ||
            !string.Equals(expectedFinance.Currency, updatedFinance.Currency, StringComparison.Ordinal))
        {
            throw new ArgumentException("Transfer and finance snapshots must use the same currency.");
        }

        bool unchangedAnnualValues = expectedFinance.SeasonYear == updatedFinance.SeasonYear &&
            expectedFinance.Revenue == updatedFinance.Revenue &&
            expectedFinance.Costs == updatedFinance.Costs &&
            expectedFinance.Debt == updatedFinance.Debt &&
            expectedFinance.WageBudget == updatedFinance.WageBudget;
        bool validDebit = expectedFinance.Cash >= transfer.Amount &&
            expectedFinance.TransferBudget >= transfer.Amount &&
            updatedFinance.Cash == checked(expectedFinance.Cash - transfer.Amount) &&
            updatedFinance.TransferBudget == checked(expectedFinance.TransferBudget - transfer.Amount);
        if (!unchangedAnnualValues || !validDebit || updatedFinance.UpdatedAtUtc <= expectedFinance.UpdatedAtUtc)
        {
            throw new ArgumentException("Updated finance must represent exactly the completed transfer debit.", nameof(updatedFinance));
        }
    }

    private void EnsureSchema(SqliteConnection connection)
    {
        if (_schemaInitialized)
        {
            return;
        }

        SqliteCareerFinanceSchema.EnsureCreated(connection);
        SqliteCareerTransferSchema.EnsureCreated(connection);
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
