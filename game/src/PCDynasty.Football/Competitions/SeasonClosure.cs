using System.Collections.Immutable;
using PCDynasty.Competition.Outcomes;

namespace PCDynasty.Football.Competitions;

public sealed class SeasonClosure
{
    internal SeasonClosure(
        ImmutableArray<QualificationOutcome> promoted,
        ImmutableArray<QualificationOutcome> remaining,
        ImmutableArray<QualificationOutcome> relegated)
    {
        Promoted = promoted;
        Remaining = remaining;
        Relegated = relegated;
    }

    public ImmutableArray<QualificationOutcome> Promoted { get; }
    public ImmutableArray<QualificationOutcome> Remaining { get; }
    public ImmutableArray<QualificationOutcome> Relegated { get; }
}
