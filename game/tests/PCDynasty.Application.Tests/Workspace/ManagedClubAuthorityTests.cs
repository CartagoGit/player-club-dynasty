using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Finances;
using PCDynasty.Application.Matches;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Application.Tests.Workspace;

/// <summary>
/// Audit a00001 found command authority was implicit: screens handed services
/// whichever club the user was browsing, so a rival could be fielded, financed
/// or played. Commands now demand the managed context and reject anything else
/// without touching a store.
/// </summary>
public sealed class ManagedClubAuthorityTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly ManagedClubContext Managed = new("club:managed");
    private static readonly ClubRow Rival = Club("club:rival", "Rival Town");
    private static readonly ClubRow Other = Club("club:other", "Other City");

    [Fact]
    public void ManagedContextRejectsAnEmptyClub()
    {
        Assert.Throws<ArgumentException>(() => new ManagedClubContext("  "));
    }

    [Fact]
    public void ManagedContextOwnsOnlyItsOwnClub()
    {
        Assert.True(Managed.Owns("club:managed"));
        Assert.False(Managed.Owns("club:Managed"));
        Assert.False(Managed.Owns("club:rival"));
    }

    [Fact]
    public void BrowsedClubKnowsWhetherItIsTheManagedOne()
    {
        Assert.True(new BrowsedClubContext("club:managed").IsManaged(Managed));
        Assert.False(new BrowsedClubContext("club:rival").IsManaged(Managed));
    }

    [Fact]
    public void LineupForARivalIsRejectedAndNothingIsSaved()
    {
        var store = new RecordingLineupStore();
        var service = new CareerLineupService(store, new FixedTimeProvider(FixedNow));
        var command = new SubmitLineupCommand(
            "contest:next",
            Rival.ExternalId,
            [new LineupParticipantSelection("person:a", "starter", 0)],
            []);

        ClubAuthorityException failure = Assert.Throws<ClubAuthorityException>(
            () => service.Submit(Managed, command));

        Assert.Equal("club:managed", failure.ManagedClubExternalId);
        Assert.Equal("club:rival", failure.TargetClubExternalId);
        Assert.Equal(0, store.Saves);
    }

    [Fact]
    public void OpeningTheFinancesOfARivalIsRejectedAndNothingIsPersisted()
    {
        var store = new RecordingFinanceStore();
        var service = new CareerFinanceService(store, new FixedTimeProvider(FixedNow));

        Assert.Throws<ClubAuthorityException>(
            () => service.Open(Managed, Rival, CareerFinanceMode.New));

        Assert.Equal(0, store.Saves);
        Assert.Equal(0, store.Clears);
    }

    [Fact]
    public void PlayingAMatchBetweenTwoRivalsIsRejected()
    {
        var service = new CompetitionSimulationService(new FixedTimeProvider(FixedNow));

        Assert.Throws<ClubAuthorityException>(
            () => service.PlayMatch(Managed, "Test Cup", Rival, Other));
    }

    [Fact]
    public void PlayingASeasonForARivalIsRejected()
    {
        var service = new CompetitionSimulationService(new FixedTimeProvider(FixedNow));

        Assert.Throws<ClubAuthorityException>(() => service.PlaySeason(
            Managed,
            "competition:test",
            "Test League",
            Rival,
            [Rival, Other]));
    }

    private static ClubRow Club(string id, string name) =>
        new(id, name, name, "City", "Country", "1900", 50, 50, 1);

    private sealed class RecordingLineupStore : ICareerLineupStore
    {
        internal int Saves { get; private set; }

        public void Save(LineupPlan plan) => Saves++;

        public void Clear()
        {
        }

        public bool TryLoad(string clubExternalId, out LineupPlan? plan)
        {
            plan = null;
            return false;
        }
    }

    private sealed class RecordingFinanceStore : ICareerFinanceStore
    {
        internal int Saves { get; private set; }

        internal int Clears { get; private set; }

        public void Save(CareerFinanceState state) => Saves++;

        public void Clear() => Clears++;

        public bool TryLoad(string clubExternalId, out CareerFinanceState? state)
        {
            state = null;
            return false;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
