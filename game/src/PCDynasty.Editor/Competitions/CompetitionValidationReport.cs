using System.Collections.Immutable;
using PCDynasty.Editor.Documents;

namespace PCDynasty.Editor.Competitions;

public sealed record CompetitionEditorDiagnostic(
    string Code,
    string Message,
    string? ElementId = null);

public sealed class CompetitionValidationReport
{
    internal CompetitionValidationReport(
        PackDocument? pack,
        ImmutableArray<CompetitionEditorDiagnostic> diagnostics)
    {
        Pack = pack;
        Diagnostics = diagnostics;
    }

    public PackDocument? Pack { get; }

    public ImmutableArray<CompetitionEditorDiagnostic> Diagnostics { get; }

    public bool IsValid => Pack is not null && Diagnostics.IsEmpty;
}
