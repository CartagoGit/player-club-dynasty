using PCDynasty.Editor.Competitions;
using PCDynasty.Editor.Documents;
using Xunit;

namespace PCDynasty.Editor.Tests;

public sealed class CompetitionGraphEditorTests
{
    [Fact]
    public void InvalidGraphReportsErrorsAndCannotProduceAPack()
    {
        var editor = new CompetitionGraphEditor(
            "competition:cup",
            "stage:start",
            [
                new EditableCompetitionStage("stage:start", 8),
                new EditableCompetitionStage("stage:orphan", 2),
            ],
            [
                new EditableCompetitionTransition(
                    "transition:cycle",
                    "stage:start",
                    "stage:start",
                    "outcome:repeat"),
            ]);

        CompetitionValidationReport report = editor.ValidateAndBuild(
            "community:test",
            schemaVersion: 1);

        Assert.False(report.IsValid);
        Assert.Null(report.Pack);
        Assert.Contains(report.Diagnostics, item => item.Code == "editor.competition.cycle");
        Assert.Contains(report.Diagnostics, item => item.Code == "editor.competition.unreachable-stage");
    }

    [Fact]
    public void ValidGraphProducesADeclarativePackThatPassesDocumentValidation()
    {
        var editor = new CompetitionGraphEditor(
            "competition:cup",
            "stage:qualification",
            [
                new EditableCompetitionStage("stage:qualification", 8),
                new EditableCompetitionStage("stage:final", 2),
            ],
            [
                new EditableCompetitionTransition(
                    "transition:advance",
                    "stage:qualification",
                    "stage:final",
                    "outcome:advance"),
            ]);

        CompetitionValidationReport report = editor.ValidateAndBuild(
            "community:test",
            1);

        Assert.True(report.IsValid);
        PackDocument pack = Assert.IsType<PackDocument>(report.Pack);
        Assert.Equal(4, pack.Entities.Length);
        Assert.True(new PackDocumentService().ValidateAndNormalize(pack).IsValid);
        Assert.Contains(pack.Entities, entity => entity.KindId == "competition-transition");
    }

    [Fact]
    public void EditingReturnsANewGraphDraft()
    {
        var original = new CompetitionGraphEditor(
            "competition:cup",
            "stage:start",
            [new EditableCompetitionStage("stage:start", 4)],
            []);

        CompetitionGraphEditor edited = original.UpsertStage(
            new EditableCompetitionStage("stage:final", 2));

        Assert.Single(original.Stages);
        Assert.Equal(2, edited.Stages.Length);
    }
}
