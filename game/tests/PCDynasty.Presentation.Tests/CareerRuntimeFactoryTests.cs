using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class CareerRuntimeFactoryTests
{
    [Fact]
    public void CompositionCreatesCareerScopedFinanceServiceAgainstExternalDatabase()
    {
        string careersRoot = Path.Combine(FindGameRoot(), "godot", "Scripts", "Careers");
        string factory = File.ReadAllText(Path.Combine(careersRoot, "CareerRuntimeFactory.cs"));
        string runtime = File.ReadAllText(Path.Combine(careersRoot, "CareerRuntime.cs"));

        Assert.Contains("new SqliteCareerFinanceStore(_databasePath)", factory, StringComparison.Ordinal);
        Assert.Contains("new CareerFinanceService(financeStore, _timeProvider)", factory, StringComparison.Ordinal);
        Assert.Contains("CareerFinanceMode.New", factory, StringComparison.Ordinal);
        Assert.Contains("CareerFinanceMode.Continue", factory, StringComparison.Ordinal);
        Assert.Contains("ICareerFinanceService Finances", runtime, StringComparison.Ordinal);
        Assert.DoesNotContain("static CareerFinanceService", factory, StringComparison.Ordinal);
    }

    [Fact]
    public void BootstrapClampsLastPlayedWhenSystemClockMovesBehindCareerStart()
    {
        string source = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "PresentationBootstrap.cs"));

        Assert.Contains("if (lastPlayedAt < startedAt)", source, StringComparison.Ordinal);
        Assert.Contains("lastPlayedAt = startedAt", source, StringComparison.Ordinal);
        Assert.Contains("startedAt, lastPlayedAt", source, StringComparison.Ordinal);
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

        throw new DirectoryNotFoundException("Could not locate the game workspace.");
    }
}
