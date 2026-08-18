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

    [Fact]
    public void ConstructorSnapshotsMutableInputs()
    {
        var stages = new List<EditableCompetitionStage>
        {
            new("stage:start", 4),
        };
        var transitions = new List<EditableCompetitionTransition>();
        var editor = new CompetitionGraphEditor(
            "competition:cup",
            "stage:start",
            stages,
            transitions);

        stages.Add(new("stage:late", 2));
        transitions.Add(new("transition:late", "stage:start", "stage:late", "outcome:advance"));

        Assert.Single(editor.Stages);
        Assert.Empty(editor.Transitions);
    }

    [Fact]
    public void CompetitionEditorProductionFilesStayFocused()
    {
        string root = FindCompetitionSourceRoot();
        string[] files =
        [
            "CompetitionGraphEditor.cs",
            "EditableCompetitionStage.cs",
            "EditableCompetitionTransition.cs",
            "CompetitionGraphValidator.cs",
            "CompetitionPackBuilder.cs",
        ];

        foreach (string file in files)
        {
            int lines = File.ReadLines(Path.Combine(root, file)).Count();
            Assert.True(lines <= 180, $"{file} has {lines} lines.");
        }

        string editorSource = File.ReadAllText(Path.Combine(root, "CompetitionGraphEditor.cs"));
        Assert.DoesNotContain("new PackEntityDocument", editorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("editor.competition.cycle", editorSource, StringComparison.Ordinal);
    }

    private static string FindCompetitionSourceRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(
                current.FullName,
                "src",
                "PCDynasty.Editor",
                "Competitions");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate competition editor sources.");
    }
}
