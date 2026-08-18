using System.Text.RegularExpressions;
using Xunit;

namespace PCDynasty.Architecture.Tests;

public sealed partial class PresentationStructureTests
{
    private const int MaximumLinesPerPresentationFile = 500;
    private const int MaximumLinesPerWorldServiceFile = 180;

    [Fact]
    public void WorldServiceCollaboratorsStayFocusedAndInternal()
    {
        string root = Path.Combine(FindGameRoot(), "src", "PCDynasty.Presentation.Core");
        string[] files =
        [
            "WorldService.cs",
            "WorldContentCatalog.cs",
            "WorldClubProjection.cs",
            "WorldPeopleProjection.cs",
            "WorldCompetitionProjection.cs",
            "WorldCountryNames.cs",
        ];

        foreach (string file in files)
        {
            int lines = File.ReadLines(Path.Combine(root, file)).Count();
            Assert.True(lines <= MaximumLinesPerWorldServiceFile, $"{file} has {lines} lines.");
        }

        Assert.True(typeof(PCDynasty.Presentation.Core.WorldService).IsPublic);
        Type[] internalCollaborators = typeof(PCDynasty.Presentation.Core.WorldService).Assembly
            .GetTypes()
            .Where(type => type.Name is "WorldContentCatalog"
                or "WorldClubProjection"
                or "WorldPeopleProjection"
                or "WorldCompetitionProjection"
                or "WorldCountryNames")
            .ToArray();
        Assert.Equal(5, internalCollaborators.Length);
        Assert.All(internalCollaborators, type => Assert.False(type.IsPublic));
    }

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

    [Fact]
    public void WorkspaceDoesNotUseHistoryServiceLocators()
    {
        string workspaceRoot = Path.Combine(FindGameRoot(), "godot", "Scripts", "Workspace");
        string[] forbiddenAccess = ["MatchHistory.Instance", "SeasonHistory.Instance", "WorldService.Instance"];

        foreach (string path in Directory.EnumerateFiles(workspaceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string source = File.ReadAllText(path);
            foreach (string forbidden in forbiddenAccess)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void BootstrapCreatesCareerScopedRuntimeForEveryOpening()
    {
        string source = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "PresentationBootstrap.cs"));

        Assert.Contains("CareerRuntimeFactory", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_careerSession", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_historyPersistence", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_lineupService", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PresentationCoreDoesNotOwnSqliteInfrastructure()
    {
        string presentationRoot = Path.Combine(FindGameRoot(), "src", "PCDynasty.Presentation.Core");
        string sqliteAdapterProject = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "src",
            "PCDynasty.Persistence.Sqlite",
            "PCDynasty.Persistence.Sqlite.csproj"));
        string projectSource = File.ReadAllText(Path.Combine(
            presentationRoot,
            "PCDynasty.Presentation.Core.csproj"));
        string[] sqliteSources = Directory
            .EnumerateFiles(presentationRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("Microsoft.Data.Sqlite", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(presentationRoot, path))
            .ToArray();

        Assert.DoesNotContain("Microsoft.Data.Sqlite", projectSource, StringComparison.Ordinal);
        Assert.Empty(sqliteSources);
        Assert.DoesNotContain("PCDynasty.Presentation.Core", sqliteAdapterProject, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspacePersistenceDependsOnTheCareerStorePort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "Workspace",
            "Persistence",
            "CareerHistoryPersistence.cs"));

        Assert.Contains("ICareerHistoryStore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SqliteCareerHistoryStore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSettings", source, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspaceDoesNotOwnCompetitionEnginePolicy()
    {
        string workspaceRoot = Path.Combine(FindGameRoot(), "godot", "Scripts", "Workspace");
        string[] forbidden =
        [
            "DateTime.UtcNow",
            "CreateSeed",
            "MatchSimulator.",
            "SeasonSimulator.",
            "new CompetitionSimulationService",
        ];

        foreach (string path in Directory.EnumerateFiles(workspaceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string source = File.ReadAllText(path);
            foreach (string pattern in forbidden)
            {
                Assert.DoesNotContain(pattern, source, StringComparison.Ordinal);
            }
        }

        string competitionScreen = File.ReadAllText(Path.Combine(
            workspaceRoot,
            "Screens",
            "CompetitionsScreen.cs"));
        Assert.Contains("ICompetitionSimulationService", competitionScreen, StringComparison.Ordinal);
    }

    [Fact]
    public void PresentationCoreDoesNotOwnCompetitionEngines()
    {
        string presentationRoot = Path.Combine(FindGameRoot(), "src", "PCDynasty.Presentation.Core");
        string projectSource = File.ReadAllText(Path.Combine(
            presentationRoot,
            "PCDynasty.Presentation.Core.csproj"));

        Assert.False(File.Exists(Path.Combine(presentationRoot, "MatchSimulator.cs")));
        Assert.False(File.Exists(Path.Combine(presentationRoot, "SeasonSimulator.cs")));
        Assert.DoesNotContain("PCDynasty.Simulation", projectSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspaceDoesNotOwnSqliteCareerSchemas()
    {
        string workspaceRoot = Path.Combine(FindGameRoot(), "godot", "Scripts", "Workspace");
        string[] forbidden = ["Microsoft.Data.Sqlite", "SqliteCareer", "CREATE TABLE"];

        foreach (string path in Directory.EnumerateFiles(workspaceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string source = File.ReadAllText(path);
            foreach (string pattern in forbidden)
            {
                Assert.DoesNotContain(pattern, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void FinanceScreenDependsOnApplicationPortNotSqlite()
    {
        string source = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "Workspace",
            "Screens",
            "FinancesScreen.cs"));

        Assert.Contains("ICareerFinanceService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SqliteCareerFinanceStore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("PCDynasty.Economy", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MarketAndSquadScreensUseNarrowApplicationPorts()
    {
        string screensRoot = Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "Workspace",
            "Screens");
        string market = File.ReadAllText(Path.Combine(screensRoot, "MarketScreen.cs"));
        string squad = File.ReadAllText(Path.Combine(screensRoot, "SquadScreen.cs"));

        Assert.Contains("ICareerTransferMarketService", market, StringComparison.Ordinal);
        Assert.DoesNotContain("SqliteCareerTransferStore", market, StringComparison.Ordinal);
        Assert.Contains("ICareerSquadProjection", squad, StringComparison.Ordinal);
        Assert.DoesNotContain("ICareerTransferMarketService", squad, StringComparison.Ordinal);
        Assert.DoesNotContain("PCDynasty.Persistence", market, StringComparison.Ordinal);
        Assert.DoesNotContain("PCDynasty.Persistence", squad, StringComparison.Ordinal);
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
