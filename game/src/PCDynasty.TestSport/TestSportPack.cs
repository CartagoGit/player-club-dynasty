using PCDynasty.Core.Domain;
using PCDynasty.Packs.Abstractions.Sports;

namespace PCDynasty.TestSport;

public sealed class TestSportPack : ISportPack
{
    private const int RequiredParticipants = 2;

    private static readonly SportDefinition CompiledDefinition = new(
        new SportId(10_000),
        RequiredParticipants,
        [new AttributeId(10_001), new AttributeId(10_002), new AttributeId(10_003)]);

    public SportDefinition Definition => CompiledDefinition;
}
