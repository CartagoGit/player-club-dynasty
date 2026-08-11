using System.Globalization;

namespace PCDynasty.Contracts;

public readonly record struct ContractVersion
{
    public ContractVersion(int major, int minor)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        Major = major;
        Minor = minor;
    }

    public int Major { get; }
    public int Minor { get; }

    public bool IsWireCompatibleWith(ContractVersion other) => Major == other.Major;

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Major}.{Minor}");
}

internal static class ContractIdentifier
{
    public static string Normalize(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }

    public static string? NormalizeOptional(string? value, string parameterName) =>
        value is null ? null : Normalize(value, parameterName);
}
