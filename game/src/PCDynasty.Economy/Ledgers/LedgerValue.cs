namespace PCDynasty.Economy.Ledgers;

internal static class LedgerValue
{
    internal static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }

    internal static string Currency(string value) =>
        Required(value, nameof(value)).ToUpperInvariant();
}
