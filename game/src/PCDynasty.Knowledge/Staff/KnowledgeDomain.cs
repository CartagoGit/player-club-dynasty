namespace PCDynasty.Knowledge.Staff;

public sealed class KnowledgeDomain
{
    public KnowledgeDomain(string domainId, string regionId)
    {
        DomainId = StaffKnowledgeValue.Required(domainId, nameof(domainId));
        RegionId = StaffKnowledgeValue.Required(regionId, nameof(regionId));
    }

    public string DomainId { get; }
    public string RegionId { get; }
}

public sealed class KnowledgeCapability
{
    public KnowledgeCapability(
        KnowledgeDomain domain,
        int expertise,
        int confidence)
    {
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Expertise = StaffKnowledgeValue.Score(expertise, nameof(expertise));
        Confidence = StaffKnowledgeValue.Score(confidence, nameof(confidence));
    }

    public KnowledgeDomain Domain { get; }
    public int Expertise { get; }
    public int Confidence { get; }
}

internal static class StaffKnowledgeValue
{
    public static int Score(int value, string parameterName) =>
        value is >= 0 and <= 100
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
