using PCDynasty.Club.Facilities;

namespace PCDynasty.Club.Academy;

public sealed class AcademyState
{
    public AcademyState(
        string academyId,
        string ownerClubId,
        string regionId,
        int asOfDay,
        int recruitment,
        int coaching,
        FacilityState facility)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(asOfDay);
        AcademyId = AcademyValue.Required(academyId, nameof(academyId));
        OwnerClubId = AcademyValue.Required(ownerClubId, nameof(ownerClubId));
        RegionId = AcademyValue.Required(regionId, nameof(regionId));
        Recruitment = AcademyValue.Score(recruitment, nameof(recruitment));
        Coaching = AcademyValue.Score(coaching, nameof(coaching));
        Facility = facility ?? throw new ArgumentNullException(nameof(facility));
        if (!string.Equals(facility.OwnerClubId, OwnerClubId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Academy facility must belong to the same club.", nameof(facility));
        }

        if (facility.AsOfDay > asOfDay)
        {
            throw new ArgumentException("Academy cannot precede its facility state.", nameof(facility));
        }

        AsOfDay = asOfDay;
    }

    public string AcademyId { get; }
    public string OwnerClubId { get; }
    public string RegionId { get; }
    public int AsOfDay { get; }
    public int Recruitment { get; }
    public int Coaching { get; }
    public FacilityState Facility { get; }

    internal AcademyState AtDay(int targetDay) =>
        new(AcademyId, OwnerClubId, RegionId, targetDay, Recruitment, Coaching, Facility);
}

internal static class AcademyValue
{
    public const int Maximum = 1_000;

    public static int Score(int value, string parameterName) =>
        value is >= 0 and <= Maximum
            ? value
            : throw new ArgumentOutOfRangeException(parameterName);

    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
