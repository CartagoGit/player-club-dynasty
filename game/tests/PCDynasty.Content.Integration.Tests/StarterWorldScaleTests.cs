using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;
using PCDynasty.Content.Worlds;
using Xunit;

namespace PCDynasty.Content.Integration.Tests;

/// <summary>
/// The shipped world must be a league someone can actually play: two divisions
/// of twenty clubs with real squads. Six clubs and eleven players never could
/// be (audit a00001, stage 4).
/// </summary>
public sealed class StarterWorldScaleTests
{
    private const string FirstDivision = "competition:boreal-series";
    private const string SecondDivision = "competition:cinder-tier";
    private const int DivisionSize = 20;

    private static readonly string ContentRoot =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "starter-world");

    private static readonly string[] NumericKeys =
    [
        "prestige", "fanbase", "birth-year", "technique", "form", "physical", "mental", "reputation",
    ];

    private static Task<WorldPackLoadResult> LoadAsync() =>
        new WorldPackLoader(new JsonContentReader()).LoadAsync(
            ContentSourceDescriptor.Integrated(ContentRoot, "pcdynasty.starter-world").ToPackSource(NumericKeys));

    [Fact]
    public async Task TwoDivisionsOfTwentyClubsAreDeclared()
    {
        WorldPackLoadResult world = await LoadAsync();
        Assert.True(world.IsSuccess, world.DescribeErrors());

        ImmutableArray<string> first = Participants(world.Pack!, FirstDivision);
        ImmutableArray<string> second = Participants(world.Pack!, SecondDivision);

        Assert.Equal(DivisionSize, first.Length);
        Assert.Equal(DivisionSize, second.Length);
        Assert.Empty(first.Intersect(second, StringComparer.Ordinal));
        Assert.Equal(DivisionSize * 2, first.Concat(second).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task EveryDivisionClubFieldsAValidSquad()
    {
        WorldPackLoadResult world = await LoadAsync();
        CompiledContentPack pack = world.Pack!;

        foreach (string club in Participants(pack, FirstDivision).Concat(Participants(pack, SecondDivision)))
        {
            NormalizedContentEntity[] squad = People(pack, club, "player").ToArray();

            Assert.True(squad.Length >= 22, $"{club} fields only {squad.Length} players.");
            Assert.All(squad, player =>
            {
                Assert.True(player.Values.ContainsKey("position"), $"{player.ExternalId} has no position.");
                Assert.True(player.Values.ContainsKey("contract-until"), $"{player.ExternalId} has no contract.");
                Assert.True(player.Values.ContainsKey("birth-date"), $"{player.ExternalId} has no birth date.");
            });
            Assert.Contains(squad, player => player.Values["position"] == "goalkeeper");
            Assert.NotEmpty(People(pack, club, "manager"));
        }
    }

    [Fact]
    public async Task ThePopulationIsWithinThePlayableRange()
    {
        WorldPackLoadResult world = await LoadAsync();
        ImmutableArray<NormalizedContentEntity> people = world.Pack!.EntitiesByKind["person"];

        int players = people.Count(person => Occupation(person) == "player");
        int managers = people.Count(person => Occupation(person) == "manager");
        int staff = people.Count(person => Occupation(person) is not ("player" or "manager"));

        Assert.InRange(players, 880, 1000);
        Assert.True(managers >= 44, $"Only {managers} managers for {world.Pack!.EntitiesByKind["club"].Length} clubs.");
        Assert.True(staff >= 44, $"Only {staff} staff members.");
        Assert.Equal(players + managers + staff, people.Length);
    }

    [Fact]
    public async Task ADanglingReferenceIsRejected()
    {
        string workspace = Directory.CreateTempSubdirectory("pcdynasty-scale-").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(workspace, "data"));
            foreach (string file in Directory.EnumerateFiles(Path.Combine(ContentRoot, "data")))
            {
                File.Copy(file, Path.Combine(workspace, "data", Path.GetFileName(file)));
            }

            string clubs = Path.Combine(workspace, "data", "clubs.json");
            File.WriteAllText(
                clubs,
                File.ReadAllText(clubs).Replace("country:norvane", "country:nowhere", StringComparison.Ordinal));

            WorldPackLoadResult mutated = await new WorldPackLoader(new JsonContentReader()).LoadAsync(
                ContentSourceDescriptor.Integrated(workspace, "pcdynasty.starter-world").ToPackSource(NumericKeys));

            Assert.False(mutated.IsSuccess);
            Assert.Contains("country:nowhere", mutated.DescribeErrors(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    private static string Occupation(NormalizedContentEntity person) =>
        person.Values.TryGetValue("occupation", out string? occupation) ? occupation : string.Empty;

    private static IEnumerable<NormalizedContentEntity> People(
        CompiledContentPack pack,
        string clubExternalId,
        string occupation) =>
        pack.EntitiesByKind["person"]
            .Where(person => Occupation(person) == occupation)
            .Where(person => person.References.Any(reference =>
                reference.RelationId == "club" && reference.TargetExternalId == clubExternalId));

    private static ImmutableArray<string> Participants(CompiledContentPack pack, string competitionExternalId)
    {
        Assert.True(pack.TryGetEntity(competitionExternalId, out NormalizedContentEntity? competition));
        return competition!.References
            .Where(reference => reference.RelationId == "participant")
            .Select(reference => reference.TargetExternalId)
            .ToImmutableArray();
    }
}
