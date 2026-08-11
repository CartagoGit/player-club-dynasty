namespace PCDynasty.Application.Performance;

public enum BackgroundWorkState
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed,
}

public sealed class BackgroundWorkStatus
{
    private BackgroundWorkStatus(
        string workId,
        string cause,
        int totalUnits,
        int completedUnits,
        long requestedFromVersion,
        long checkpointVersion,
        BackgroundWorkState state,
        string? failureReason)
    {
        WorkId = workId;
        Cause = cause;
        TotalUnits = totalUnits;
        CompletedUnits = completedUnits;
        RequestedFromVersion = requestedFromVersion;
        CheckpointVersion = checkpointVersion;
        State = state;
        FailureReason = failureReason;
    }

    public string WorkId { get; }

    public string Cause { get; }

    public int TotalUnits { get; }

    public int CompletedUnits { get; }

    public long RequestedFromVersion { get; }

    public long CheckpointVersion { get; }

    public BackgroundWorkState State { get; }

    public string? FailureReason { get; }

    public double Progress => (double)CompletedUnits / TotalUnits;

    public bool IsFinal => State == BackgroundWorkState.Completed;

    public bool IsTerminal => State is
        BackgroundWorkState.Completed or
        BackgroundWorkState.Cancelled or
        BackgroundWorkState.Failed;

    public static BackgroundWorkStatus Start(
        string workId,
        string cause,
        int totalUnits,
        long requestedFromVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalUnits);
        ArgumentOutOfRangeException.ThrowIfNegative(requestedFromVersion);
        return new BackgroundWorkStatus(
            Normalize(workId, nameof(workId)),
            Normalize(cause, nameof(cause)),
            totalUnits,
            completedUnits: 0,
            requestedFromVersion,
            checkpointVersion: requestedFromVersion,
            BackgroundWorkState.Pending,
            failureReason: null);
    }

    public BackgroundWorkStatus ReportCheckpoint(
        int completedUnits,
        long checkpointVersion)
    {
        EnsureActive();
        ArgumentOutOfRangeException.ThrowIfLessThan(completedUnits, CompletedUnits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(completedUnits, TotalUnits);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            checkpointVersion,
            CheckpointVersion);

        return Copy(
            completedUnits,
            checkpointVersion,
            BackgroundWorkState.Running,
            failureReason: null);
    }

    public BackgroundWorkStatus Complete(long checkpointVersion)
    {
        EnsureActive();
        if (CompletedUnits != TotalUnits)
        {
            throw new InvalidOperationException(
                "Background work cannot be final before every unit has a checkpoint.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(
            checkpointVersion,
            CheckpointVersion);

        return Copy(
            CompletedUnits,
            checkpointVersion,
            BackgroundWorkState.Completed,
            failureReason: null);
    }

    public BackgroundWorkStatus Cancel()
    {
        EnsureActive();
        return Copy(
            CompletedUnits,
            CheckpointVersion,
            BackgroundWorkState.Cancelled,
            failureReason: null);
    }

    public BackgroundWorkStatus Fail(string reason)
    {
        EnsureActive();
        return Copy(
            CompletedUnits,
            CheckpointVersion,
            BackgroundWorkState.Failed,
            Normalize(reason, nameof(reason)));
    }

    public bool CanPublishFinalResult(long visibleVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(visibleVersion);
        return IsFinal && visibleVersion == RequestedFromVersion;
    }

    private BackgroundWorkStatus Copy(
        int completedUnits,
        long checkpointVersion,
        BackgroundWorkState state,
        string? failureReason)
    {
        return new BackgroundWorkStatus(
            WorkId,
            Cause,
            TotalUnits,
            completedUnits,
            RequestedFromVersion,
            checkpointVersion,
            state,
            failureReason);
    }

    private void EnsureActive()
    {
        if (IsTerminal)
        {
            throw new InvalidOperationException("Terminal background work cannot change state.");
        }
    }

    private static string Normalize(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
