using System.Collections.Immutable;
using PCDynasty.Core.Domain;
using PCDynasty.Football.Attributes;

namespace PCDynasty.Football.Positions;

public readonly record struct AttributeRelevance
{
    public AttributeRelevance(AttributeId attributeId, int relevance)
    {
        AttributeId = attributeId;
        Relevance = FootballDomainValue.EnsureScore(relevance, nameof(relevance));
    }

    public AttributeId AttributeId { get; }

    public int Relevance { get; }
}

public sealed class RoleRelevance
{
    public RoleRelevance(string roleId, IEnumerable<AttributeRelevance> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        ImmutableArray<AttributeRelevance> copy = attributes.ToImmutableArray();
        ValidateCompleteCatalogue(copy);

        RoleId = FootballDomainValue.NormalizeIdentifier(roleId, nameof(roleId));
        OrderedAttributes = copy
            .OrderByDescending(attribute => attribute.Relevance)
            .ThenBy(attribute => attribute.AttributeId.Value)
            .ToImmutableArray();
    }

    public string RoleId { get; }

    public ImmutableArray<AttributeRelevance> OrderedAttributes { get; }

    private static void ValidateCompleteCatalogue(ImmutableArray<AttributeRelevance> attributes)
    {
        if (attributes.Length != FootballAttributeCatalog.Count)
        {
            throw new ArgumentException(
                $"Role relevance must cover exactly {FootballAttributeCatalog.Count} football attributes.",
                nameof(attributes));
        }

        var seenIds = new HashSet<int>();
        foreach (AttributeRelevance attribute in attributes)
        {
            int id = attribute.AttributeId.Value;
            if ((uint)id >= (uint)FootballAttributeCatalog.Count || !seenIds.Add(id))
            {
                throw new ArgumentException(
                    "Role relevance must contain each compiled football attribute exactly once.",
                    nameof(attributes));
            }
        }
    }
}
