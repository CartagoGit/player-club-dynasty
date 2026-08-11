using PCDynasty.Knowledge.Staff;
using Xunit;

namespace PCDynasty.Knowledge.Tests;

public sealed class KnowledgeDomainTests
{
    [Fact]
    public void ProfileKeepsRegionalDomainCapabilitiesSeparate()
    {
        var north = new KnowledgeCapability(
            new KnowledgeDomain("domain:participants", "region:north"),
            expertise: 82,
            confidence: 75);
        var south = new KnowledgeCapability(
            new KnowledgeDomain("domain:participants", "region:south"),
            expertise: 61,
            confidence: 90);
        var profile = new StaffKnowledgeProfile("staff:7", [south, north]);

        Assert.Equal(82, profile.Get("domain:participants", "region:north").Expertise);
        Assert.Equal(61, profile.Get("domain:participants", "region:south").Expertise);
        Assert.Equal("region:north", profile.Capabilities[0].Domain.RegionId);
    }

    [Fact]
    public void KnowledgeContractsDoNotContainPersonTruth()
    {
        string[] publicNames = new[]
        {
            typeof(KnowledgeDomain),
            typeof(KnowledgeCapability),
            typeof(StaffKnowledgeProfile),
        }.SelectMany(type => type.GetMembers().Select(member => member.Name).Append(type.Name))
         .ToArray();

        Assert.DoesNotContain(
            publicNames,
            name => name.Contains("True", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Actual", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("HiddenAttribute", StringComparison.OrdinalIgnoreCase));
        Assert.Throws<ArgumentException>(() => new StaffKnowledgeProfile(
            "staff:7",
            [
                new KnowledgeCapability(new KnowledgeDomain("domain:a", "region:a"), 50, 50),
                new KnowledgeCapability(new KnowledgeDomain("domain:a", "region:a"), 60, 60),
            ]));
    }
}
