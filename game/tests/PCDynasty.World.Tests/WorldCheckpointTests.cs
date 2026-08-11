using PCDynasty.World.State;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class WorldCheckpointTests
{
    [Fact]
    public void CapturesVersionedDateAndCompactOpaqueClubMetrics()
    {
        var metrics = new Dictionary<string, long>
        {
            ["competition:points"] = 74,
            ["finance:balance"] = 25_000_000,
        };
        var state = new ClubSeasonState(
            "club:7",
            "season:2042",
            "competition:premier",
            metrics);
        ClubSeasonState[] states = [state];
        var checkpoint = new WorldCheckpoint(
            schemaVersion: 3,
            sequence: 9_000,
            new WorldDate(2042, 7, 1),
            states);
        metrics["competition:points"] = 0;
        states[0] = new ClubSeasonState(
            "changed",
            "changed",
            "changed",
            new Dictionary<string, long>());

        Assert.Equal(3, checkpoint.SchemaVersion);
        Assert.Equal(9_000, checkpoint.Sequence);
        Assert.Equal(new WorldDate(2042, 7, 1), checkpoint.Date);
        Assert.Same(state, Assert.Single(checkpoint.ClubSeasons));
        Assert.Equal(74, state.Metrics["competition:points"]);
    }

    [Fact]
    public void RejectsDuplicateClubSeasonCompetitionKeys()
    {
        var state = new ClubSeasonState(
            "club:7",
            "season:2042",
            "competition:premier",
            new Dictionary<string, long>());

        Assert.Throws<ArgumentException>(() => new WorldCheckpoint(
            1,
            1,
            new WorldDate(2042, 1, 1),
            [state, state]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldDate(2042, 2, 30));
    }

    [Fact]
    public void MacroWorldStateRemainsSportNeutral()
    {
        string[] references = typeof(WorldCheckpoint).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("PCDynasty.Football", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Godot", StringComparison.Ordinal));
    }
}
