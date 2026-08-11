using System.Collections.Immutable;
using PCDynasty.Core.Domain;

namespace PCDynasty.Packs.Abstractions.Sports;

public readonly record struct SportId
{
    public SportId(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "A sport ID must be zero or greater.");
        }

        Value = value;
    }

    public int Value { get; }
}

public sealed class SportDefinition
{
    public SportDefinition(
        SportId id,
        int minimumParticipants,
        IEnumerable<AttributeId> attributeIds)
    {
        ArgumentNullException.ThrowIfNull(attributeIds);

        if (minimumParticipants <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumParticipants),
                minimumParticipants,
                "The minimum participant count must be greater than zero.");
        }

        ImmutableArray<AttributeId> compiledAttributeIds = attributeIds.ToImmutableArray();
        if (compiledAttributeIds.IsEmpty)
        {
            throw new ArgumentException("A sport must define at least one compiled attribute ID.", nameof(attributeIds));
        }

        if (compiledAttributeIds.Distinct().Count() != compiledAttributeIds.Length)
        {
            throw new ArgumentException("Compiled attribute IDs must be unique.", nameof(attributeIds));
        }

        Id = id;
        MinimumParticipants = minimumParticipants;
        AttributeIds = compiledAttributeIds;
    }

    public SportId Id { get; }

    public int MinimumParticipants { get; }

    public ImmutableArray<AttributeId> AttributeIds { get; }
}
