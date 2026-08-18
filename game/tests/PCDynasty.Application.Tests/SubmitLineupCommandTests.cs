using PCDynasty.Application.Matches;
using Xunit;

namespace PCDynasty.Application.Tests;

public sealed class SubmitLineupCommandTests
{
    [Fact]
    public void CopiesOrderedSelectionsAndOpaqueOptions()
    {
        LineupParticipantSelection[] selections =
        [
            new("participant:2", "assignment:right", order: 2),
            new("participant:1", "assignment:left", order: 1),
        ];
        var options = new Dictionary<string, string>
        {
            ["tempo"] = "measured",
        };

        var command = new SubmitLineupCommand(
            "contest:7",
            "entity:home",
            selections,
            options);
        selections[0] = new LineupParticipantSelection("changed", "changed", 0);
        options["tempo"] = "changed";

        Assert.Equal("participant:1", command.Selections[0].ParticipantId);
        Assert.Equal("participant:2", command.Selections[1].ParticipantId);
        Assert.Equal("measured", command.Options["tempo"]);
        Assert.Throws<ArgumentException>(() => new SubmitLineupCommand(
            "contest:7",
            "entity:home",
            [
                new LineupParticipantSelection("participant:1", "assignment:a", 0),
                new LineupParticipantSelection("participant:1", "assignment:b", 1),
            ],
            []));
    }

    [Fact]
    public void PublicContractIsSportNeutralAndDoesNotReferenceGodot()
    {
        string[] publicNames = typeof(SubmitLineupCommand).Assembly
            .GetExportedTypes()
            .Where(type => type.Namespace == typeof(SubmitLineupCommand).Namespace)
            .SelectMany(type => type.GetMembers().Select(member => member.Name).Append(type.Name))
            .ToArray();
        string[] forbidden = ["Football", "Player", "Position", "Bench", "Goal"];

        Assert.DoesNotContain(
            publicNames,
            name => forbidden.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(
            typeof(SubmitLineupCommand).Assembly.GetReferencedAssemblies(),
            reference => reference.Name?.StartsWith("Godot", StringComparison.Ordinal) == true);
        Assert.All(
            typeof(SubmitLineupCommand).GetProperties()
                .Concat(typeof(LineupParticipantSelection).GetProperties()),
            property => Assert.False(property.CanWrite));
    }
}
