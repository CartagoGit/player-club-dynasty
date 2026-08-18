namespace PCDynasty.Persistence.Sqlite;

public sealed class WorldStoreConcurrencyException : InvalidOperationException
{
    public WorldStoreConcurrencyException(string message)
        : base(message)
    {
    }
}
