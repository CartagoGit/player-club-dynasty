using System.Text.RegularExpressions;
using Xunit;

namespace PCDynasty.Architecture.Tests;

public sealed partial class PresentationStructureTests
{
    private const int MaximumLinesPerPresentationFile = 500;

    [Fact]
    public void GodotPresentationFilesStayFocused()
    {
        string scriptsRoot = Path.Combine(FindGameRoot(), "godot", "Scripts");
        string[] oversizedFiles = Directory
            .EnumerateFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => new
            {
                Path = Path.GetRelativePath(scriptsRoot, path),
                Lines = File.ReadLines(path).Count(),
            })
            .Where(file => file.Lines > MaximumLinesPerPresentationFile)
            .Select(file => $"{file.Path} ({file.Lines} lines)")
            .ToArray();

        Assert.True(
            oversizedFiles.Length == 0,
            $"Presentation files must stay below {MaximumLinesPerPresentationFile} lines: {string.Join(", ", oversizedFiles)}");
    }

    [Fact]
    public void WorkspaceOrchestratorDoesNotOwnRenderingSimulationOrPersistence()
    {
        string controllerPath = Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "Workspace",
            "ManagerWorkspaceController.cs");
        string source = File.ReadAllText(controllerPath);
        string[] forbiddenDependencies =
        [
            "CareerStore",
            "DateTime",
            "MatchHistory",
            "MatchSimulator",
            "SeasonHistory",
            "SeasonSimulator",
            "new Button",
            "new Label",
            "new PanelContainer",
        ];

        foreach (string forbiddenDependency in forbiddenDependencies)
        {
            Assert.DoesNotContain(forbiddenDependency, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void WorkspaceScreensDoNotUseMutableStaticState()
    {
        string workspaceRoot = Path.Combine(FindGameRoot(), "godot", "Scripts", "Workspace");
        string[] offenders = Directory
            .EnumerateFiles(workspaceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => MutableStaticField().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(workspaceRoot, path))
            .ToArray();

        Assert.Empty(offenders);
    }

    private static string FindGameRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "godot", "project.godot")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the game workspace from the test output directory.");
    }

    [GeneratedRegex(
        @"^\s*private\s+static\s+(?!readonly\b)[^(\r\n]+(?:=|;)",
        RegexOptions.CultureInvariant | RegexOptions.Multiline)]
    private static partial Regex MutableStaticField();
}
