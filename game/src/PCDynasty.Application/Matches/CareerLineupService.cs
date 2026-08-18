using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Matches;

public sealed class CareerLineupService : ICareerLineupService
{
    private readonly ICareerLineupStore _store;
    private readonly TimeProvider _timeProvider;

    public CareerLineupService(ICareerLineupStore store, TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public LineupPlan Submit(SubmitLineupCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plan = new LineupPlan(
            command.ControlledEntityId,
            command.ContestId,
            _timeProvider.GetUtcNow(),
            command.Selections.Select(selection => new LineupAssignment(
                selection.ParticipantId,
                selection.AssignmentId,
                selection.Order)),
            command.Options);
        _store.Save(plan);
        return plan;
    }

    public bool TryGetLatest(string clubExternalId, out LineupPlan? plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return _store.TryLoad(clubExternalId, out plan);
    }
}
