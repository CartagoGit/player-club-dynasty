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
        Assert.Contains("new SqliteCareerTransferStore(_databasePath)", factory, StringComparison.Ordinal);
        Assert.Contains("new CareerTransferMarketService", factory, StringComparison.Ordinal);
        Assert.Contains("transferStore.Clear()", factory, StringComparison.Ordinal);
        Assert.Contains("CareerFinanceMode.New", factory, StringComparison.Ordinal);
        Assert.Contains("CareerFinanceMode.Continue", factory, StringComparison.Ordinal);
        Assert.Contains("ICareerFinanceService Finances", runtime, StringComparison.Ordinal);
        Assert.Contains("ICareerTransferMarketService TransferMarket", runtime, StringComparison.Ordinal);
        Assert.Contains("ICareerSquadProjection Squad", runtime, StringComparison.Ordinal);
        Assert.DoesNotContain("static CareerFinanceService", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("static CareerTransferMarketService", factory, StringComparison.Ordinal);
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

    [Fact]
    public void BootstrapAcceptsAnExternallySuppliedCareerDatabase()
    {
        string source = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Scripts",
            "PresentationBootstrap.cs"));

        Assert.Contains("PCDYNASTY_CAREER_DB", source, StringComparison.Ordinal);
        Assert.Contains("Path.GetFullPath(externalPath)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void GodotSmokePinsTransferScopeRestoreAndNewCareerReset()
    {
        string smoke = File.ReadAllText(Path.Combine(
            FindGameRoot(),
            "godot",
            "Tests",
            "SmokeBoot.gd"));

        Assert.Contains("Market must remain scoped to the managed career club", smoke, StringComparison.Ordinal);
        Assert.Contains("Continuing must keep an acquired player out of the market", smoke, StringComparison.Ordinal);
        Assert.Contains("A new career must clear prior transfers", smoke, StringComparison.Ordinal);
        Assert.Contains("The squad header must count acquired players", smoke, StringComparison.Ordinal);
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
