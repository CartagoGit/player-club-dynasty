using System.Reflection;
using PCDynasty.Packs.Abstractions.Sports;
using PCDynasty.TestSport;
using Xunit;

namespace PCDynasty.TestSport.Tests;

public sealed class TestSportPackTests
{
    [Fact]
    public void DefinitionRepresentsOneVersusOneSportWithThreeCompiledAttributes()
    {
        ISportPack pack = new TestSportPack();

        Assert.Equal(2, pack.Definition.MinimumParticipants);
        Assert.Equal(3, pack.Definition.AttributeIds.Length);
        Assert.Equal(
            3,
            pack.Definition.AttributeIds
                .Select(attributeId => attributeId.Value)
                .Distinct()
                .Count());
    }

    [Fact]
    public void PublicContractRemainsFreeOfFootballSpecificConcepts()
    {
        Assembly assembly = typeof(TestSportPack).Assembly;
        string[] forbiddenConcepts = ["Club", "Position", "Goal", "League", "Bench"];

        string[] publicContractNames = assembly
            .GetExportedTypes()
            .SelectMany(GetPublicContractNames)
            .ToArray();

        foreach (string forbiddenConcept in forbiddenConcepts)
        {
            Assert.DoesNotContain(
                publicContractNames,
                contractName => contractName.Contains(forbiddenConcept, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static IEnumerable<string> GetPublicContractNames(Type exportedType)
    {
        yield return exportedType.Name;

        foreach (MemberInfo member in exportedType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
        {
            yield return member.Name;
        }
    }
}
