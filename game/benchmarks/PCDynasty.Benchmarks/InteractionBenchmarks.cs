using BenchmarkDotNet.Attributes;
using System.Globalization;
using PCDynasty.Application.Competitions;
using PCDynasty.Application.History;
using PCDynasty.Application.Players;
using PCDynasty.Competition.Outcomes;
using PCDynasty.Competition.Rankings;
using PCDynasty.Core.Domain;
using PCDynasty.Core.People;
using PCDynasty.Core.Performance;
using PCDynasty.History.Events;
using PCDynasty.Knowledge.Observations;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class InteractionBenchmarks
{
    private const int SelectedPersonId = 10_000;
    private const string SelectedClubId = "benchmark:club:0042";

    private readonly Dictionary<int, Person> people = CreatePeople();
    private readonly Observation[] playerObservations = CreateObservations();
    private readonly HistoricalEvent[] clubEvents = CreateClubEvents();
    private readonly RankingTable competitionTable = CreateCompetitionTable();
    private readonly QualificationOutcome[] competitionOutcomes = CreateCompetitionOutcomes();

    [Benchmark]
    public long OpenPlayerView()
    {
        Person person = people[SelectedPersonId];
        PlayerSummaryView view = PlayerSummaryQuery.Execute(
            person,
            playerObservations[0],
            playerObservations[1],
            playerObservations[2],
            playerObservations[3],
            new CurrentPerformanceWeights(1, 1, 1, 1));
        return view.PersonId + view.Overall.Estimate;
    }

    [Benchmark]
    public long OpenClubView()
    {
        HistoryView view = HistoryQuery.ExecuteCareer(SelectedClubId, clubEvents);
        return view.SubjectId.Length + view.Entries.Length;
    }

    [Benchmark]
    public long OpenCompetitionView()
    {
        SeasonTableView view = SeasonTableQuery.Execute(
            competitionTable,
            competitionOutcomes);
        return view.Entries.Length + view.Entries[0].Rank;
    }

    private static Dictionary<int, Person> CreatePeople()
    {
        return Enumerable.Range(1, SelectedPersonId)
            .ToDictionary(
                id => id,
                id => new Person(
                    new PersonId(id),
                    new PersonProfile(
                        $"Name{id:D5}",
                        $"Family{id:D5}",
                        new DateOnly(1990 + (id % 15), 1 + (id % 12), 1 + (id % 27)),
                        $"benchmark:country:{id % 3}")));
    }

    private static Observation[] CreateObservations()
    {
        var subjectId = new PersonId(SelectedPersonId);
        return Enumerable.Range(0, 4)
            .Select(attribute => new Observation(
                subjectId,
                new AttributeId(attribute),
                minimumEstimate: 60 + attribute,
                maximumEstimate: 75 + attribute,
                confidence: 80,
                $"benchmark:source:{attribute}",
                new DateOnly(2026, 8, 11)))
            .ToArray();
    }

    private static HistoricalEvent[] CreateClubEvents()
    {
        return Enumerable.Range(1, 1_000)
            .Select(sequence => new HistoricalEvent(
                sequence,
                occurredOnDay: 5_000 + sequence,
                typeId: "history:club-event",
                schemaVersion: 1,
                subjectIds:
                [
                    sequence % 33 == 0
                        ? SelectedClubId
                        : $"benchmark:club:{sequence % 1_000:D4}",
                ],
                values: [KeyValuePair.Create(
                    "sequence",
                    sequence.ToString(CultureInfo.InvariantCulture))]))
            .ToArray();
    }

    private static RankingTable CreateCompetitionTable()
    {
        return new RankingTable(
            Enumerable.Range(0, 64).Select(index => new RankingCandidate(
                $"benchmark:participant:{index:D2}",
                [120 - index, 80 - (index % 20)])),
            [RankingCriterionDirection.Descending, RankingCriterionDirection.Descending]);
    }

    private static QualificationOutcome[] CreateCompetitionOutcomes()
    {
        return Enumerable.Range(0, 64)
            .Select(index => new QualificationOutcome(
                $"benchmark:participant:{index:D2}",
                index < 8 ? "benchmark:qualified" : "benchmark:retained",
                index < 8 ? "benchmark:stage:next" : null))
            .ToArray();
    }
}
