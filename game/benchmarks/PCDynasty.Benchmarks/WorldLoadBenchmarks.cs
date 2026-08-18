using System.Collections.Immutable;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Benchmarks;

/// <summary>
/// Measures the world the game actually reads: the same <see cref="WorldService"/>
/// the Godot composition root builds, over a world at target scale.
/// </summary>
/// <remarks>
/// Audit a00001 measured the previous path as roughly O(clubs × people) and
/// noted that the benchmark named for opening a club view measured something
/// else entirely. These workloads exercise the canonical loader and the
/// indexed snapshot instead.
/// </remarks>
[MemoryDiagnoser]
public class WorldLoadBenchmarks
{
    public const int ClubCount = 6_400;
    public const int PlayersPerClub = 25;
    public const string SelectedClubId = "club:03200";

    private static readonly ImmutableArray<NormalizedContentEntity> LargeWorld = BuildWorld();
    private readonly ImmutableArray<NormalizedContentEntity> entities = LargeWorld;
    private readonly WorldService world = CreateLoadedWorld();

    [Benchmark]
    public int CompileLargeWorld()
    {
        var service = new WorldService(new InMemoryContentReader(entities), "benchmark://world");
        service.ReloadAsync().GetAwaiter().GetResult();
        return service.ListClubs().Count;
    }

    [Benchmark]
    public int OpenSquadView() => world.ListPlayers(SelectedClubId).Count;

    [Benchmark]
    public int FindClubById() => world.FindClub(SelectedClubId) is null ? 0 : 1;

    /// <summary>The loaded target-scale world, reused by the fast latency gate.</summary>
    public static WorldService CreateLoadedWorld()
    {
        var service = new WorldService(new InMemoryContentReader(LargeWorld), "benchmark://world");
        service.ReloadAsync().GetAwaiter().GetResult();
        return service;
    }

    /// <summary>
    /// One country, <see cref="ClubCount"/> clubs and 25 players per club:
    /// 160,000 people, the scale the master plan requires queries to hold.
    /// </summary>
    private static ImmutableArray<NormalizedContentEntity> BuildWorld()
    {
        var entities = ImmutableArray.CreateBuilder<NormalizedContentEntity>(
            1 + ClubCount + (ClubCount * PlayersPerClub));
        entities.Add(new NormalizedContentEntity(
            "country:benchmark",
            "country",
            [KeyValuePair.Create("short-name", "BEN")],
            []));

        for (int club = 0; club < ClubCount; club++)
        {
            string clubId = ClubId(club);
            entities.Add(new NormalizedContentEntity(
                clubId,
                "club",
                [
                    KeyValuePair.Create("display-name", $"Benchmark Club {club:00000}"),
                    KeyValuePair.Create("short-name", $"BC{club:00000}"),
                    KeyValuePair.Create("city", "Benchmark City"),
                    KeyValuePair.Create("founded-year", "1900"),
                    KeyValuePair.Create("prestige", Bounded(club, 20, 90)),
                    KeyValuePair.Create("fanbase", Bounded(club, 10, 95)),
                ],
                [new ContentReference("country", "country:benchmark", required: true)]));

            for (int player = 0; player < PlayersPerClub; player++)
            {
                int seed = (club * PlayersPerClub) + player;
                entities.Add(new NormalizedContentEntity(
                    $"person:{seed:0000000}",
                    "person",
                    [
                        KeyValuePair.Create("display-name", $"Player {seed:0000000}"),
                        KeyValuePair.Create("occupation", "player"),
                        KeyValuePair.Create("birth-year", Bounded(seed, 1988, 2008)),
                        KeyValuePair.Create("technique", Bounded(seed, 30, 92)),
                        KeyValuePair.Create("form", Bounded(seed + 1, 30, 92)),
                        KeyValuePair.Create("physical", Bounded(seed + 2, 30, 92)),
                        KeyValuePair.Create("mental", Bounded(seed + 3, 30, 92)),
                    ],
                    [
                        new ContentReference("club", clubId, required: true),
                        new ContentReference("country", "country:benchmark", required: true),
                    ]));
            }
        }

        return entities.ToImmutable();
    }

    private static string ClubId(int index) => $"club:{index:00000}";

    private static string Bounded(int seed, int minimum, int maximum) =>
        (minimum + (seed % (maximum - minimum + 1))).ToString(CultureInfo.InvariantCulture);

    /// <summary>Serves the same world for every requested document.</summary>
    private sealed class InMemoryContentReader(ImmutableArray<NormalizedContentEntity> entities)
        : IContentReader
    {
        private int _served;

        public string FormatId => "memory";

        public ValueTask<ContentReadResult> ReadAsync(
            ContentReadRequest request,
            CancellationToken cancellationToken = default)
        {
            // The catalog asks for five documents; the whole world travels in
            // the first one and the rest are empty, which keeps the merge and
            // the compile identical to a single-document source.
            ImmutableArray<NormalizedContentEntity> payload =
                Interlocked.Increment(ref _served) == 1 ? entities : [];
            return ValueTask.FromResult(new ContentReadResult(
                new NormalizedContentModel(
                    request.ProviderId,
                    request.ExpectedSchemaVersion,
                    payload),
                []));
        }
    }
}
