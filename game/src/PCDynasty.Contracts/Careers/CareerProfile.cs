namespace PCDynasty.Contracts.Careers;

public sealed record CareerProfile
{
    public CareerProfile(
        string clubExternalId,
        string clubDisplayName,
        DateTimeOffset startedAtUtc,
        DateTimeOffset lastPlayedAtUtc)
    {
        ClubExternalId = CareerContractValue.Required(clubExternalId, nameof(clubExternalId));
        ClubDisplayName = CareerContractValue.Required(clubDisplayName, nameof(clubDisplayName));
        if (lastPlayedAtUtc < startedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastPlayedAtUtc),
                "Last-played time cannot precede career start time.");
        }

        StartedAtUtc = startedAtUtc.ToUniversalTime();
        LastPlayedAtUtc = lastPlayedAtUtc.ToUniversalTime();
    }

    public string ClubExternalId { get; }

    public string ClubDisplayName { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset LastPlayedAtUtc { get; }
}

internal static class CareerContractValue
{
    internal static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
