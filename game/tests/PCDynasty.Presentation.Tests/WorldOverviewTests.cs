using System.IO;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class WorldOverviewTests
{
    private static readonly string ContentRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    [Fact]
    public async Task ListAllCompetitionsReturnsEveryShippedCompetition()
    {
        var service = WorldService.Initialize(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListAllCompetitions();

        Assert.NotEmpty(competitions);
        Assert.True(
            competitions.Count >= 4,
            $"Expected at least 4 starter-world competitions, got {competitions.Count}.");

        // The starter-world ships the Boreal Series, Ember League, Horizon
        // Cup and at least one more. They are fictional.
        Assert.Contains(competitions, c => c.DisplayName.Contains("Boreal", StringComparison.Ordinal));
        Assert.Contains(competitions, c => c.DisplayName.Contains("Ember", StringComparison.Ordinal));
        Assert.Contains(competitions, c => c.DisplayName.Contains("Horizon", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ListAllCompetitionsAreOrderedByDisplayName()
    {
        var service = WorldService.Initialize(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListAllCompetitions();

        for (int i = 1; i < competitions.Count; i++)
        {
            Assert.True(
                string.Compare(
                    competitions[i - 1].DisplayName,
                    competitions[i].DisplayName,
                    StringComparison.Ordinal) <= 0,
                $"Competitions not ordered: {competitions[i - 1].DisplayName} should come before {competitions[i].DisplayName}.");
        }
    }

    [Fact]
    public async Task ListAllCompetitionsExposesRivalsForEach()
    {
        var service = WorldService.Initialize(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListAllCompetitions();

        Assert.All(competitions, c => Assert.NotEmpty(c.Rivals));
    }
}