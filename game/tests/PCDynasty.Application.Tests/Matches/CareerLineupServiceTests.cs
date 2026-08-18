using PCDynasty.Application.Matches;
using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Application.Tests.Matches;

public sealed class CareerLineupServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SubmitMapsCommandPersistsAndExposesTheLatestPlan()
    {
        var store = new MemoryStore();
        var service = new CareerLineupService(store, new FixedTimeProvider(FixedNow));
        var command = new SubmitLineupCommand(
            "contest:next",
            "club:test",
            [new LineupParticipantSelection("person:a", "starter", 0)],
            [KeyValuePair.Create("approach", "balanced")]);

        LineupPlan saved = service.Submit(command);

        Assert.Same(saved, store.Saved);
        Assert.Equal(FixedNow, saved.SubmittedAtUtc);
        Assert.True(service.TryGetLatest("club:test", out LineupPlan? loaded));
        Assert.Same(saved, loaded);
    }

    private sealed class MemoryStore : ICareerLineupStore
    {
        internal LineupPlan? Saved { get; private set; }

        public void Save(LineupPlan plan) => Saved = plan;

        public void Clear() => Saved = null;

        public bool TryLoad(string clubExternalId, out LineupPlan? plan)
        {
            plan = Saved?.ClubExternalId == clubExternalId ? Saved : null;
            return plan is not null;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
