using System.Collections.Immutable;

namespace PCDynasty.Core.People;

public sealed class PersonProfile
{
    public PersonProfile(
        string givenName,
        string familyName,
        DateOnly birthDate,
        string nationalityId,
        string? regionId = null,
        IEnumerable<string>? languageIds = null)
    {
        GivenName = NormalizeRequired(givenName, nameof(givenName));
        FamilyName = NormalizeRequired(familyName, nameof(familyName));
        BirthDate = birthDate;
        NationalityId = NormalizeRequired(nationalityId, nameof(nationalityId));
        RegionId = NormalizeOptional(regionId, nameof(regionId));
        LanguageIds = CopyUniqueIdentifiers(languageIds, nameof(languageIds));
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public DateOnly BirthDate { get; }

    public string NationalityId { get; }

    public string? RegionId { get; }

    public ImmutableArray<string> LanguageIds { get; }

    private static string NormalizeRequired(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        string normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("The value cannot be empty or whitespace.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string parameterName)
    {
        return value is null ? null : NormalizeRequired(value, parameterName);
    }

    private static ImmutableArray<string> CopyUniqueIdentifiers(
        IEnumerable<string>? identifiers,
        string parameterName)
    {
        if (identifiers is null)
        {
            return [];
        }

        var uniqueIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var copy = ImmutableArray.CreateBuilder<string>();

        foreach (string identifier in identifiers)
        {
            string normalized = NormalizeRequired(identifier, parameterName);
            if (!uniqueIdentifiers.Add(normalized))
            {
                throw new ArgumentException("Identifiers must be unique.", parameterName);
            }

            copy.Add(normalized);
        }

        return copy.ToImmutable();
    }
}
