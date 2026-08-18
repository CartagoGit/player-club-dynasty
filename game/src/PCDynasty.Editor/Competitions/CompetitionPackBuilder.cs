using System.Collections.Immutable;
using System.Globalization;
using PCDynasty.Editor.Documents;

namespace PCDynasty.Editor.Competitions;

internal static class CompetitionPackBuilder
{
    internal static PackDocument Build(
        CompetitionGraphEditor graph,
        string providerId,
        int schemaVersion)
    {
        var entities = ImmutableArray.CreateBuilder<PackEntityDocument>();
        entities.Add(new PackEntityDocument(
            graph.CompetitionId.Trim(),
            "competition",
            [new PackValueDocument("entry-stage", graph.EntryStageId.Trim())],
            [new PackReferenceDocument("entry-stage", graph.EntryStageId.Trim(), true)]));
        entities.AddRange(graph.Stages.Select(stage => new PackEntityDocument(
            stage.StageId.Trim(),
            "competition-stage",
            [new PackValueDocument(
                "minimum-participants",
                stage.MinimumParticipants.ToString(CultureInfo.InvariantCulture))],
            [new PackReferenceDocument("dependency:competition", graph.CompetitionId.Trim(), true)])));
        entities.AddRange(graph.Transitions.Select(transition => new PackEntityDocument(
            transition.TransitionId.Trim(),
            "competition-transition",
            [new PackValueDocument("outcome-id", transition.OutcomeId.Trim())],
            [
                new PackReferenceDocument("from-stage", transition.FromStageId.Trim(), true),
                new PackReferenceDocument("to-stage", transition.ToStageId.Trim(), true),
            ])));
        return new PackDocument(providerId, schemaVersion, entities);
    }
}
