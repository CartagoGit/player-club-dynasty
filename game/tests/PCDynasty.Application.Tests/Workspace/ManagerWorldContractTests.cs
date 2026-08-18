using System.Reflection;
using PCDynasty.Application.Workspace;
using Xunit;

namespace PCDynasty.Application.Tests.Workspace;

public sealed class ManagerWorldContractTests
{
    private static readonly string[] ForbiddenAssemblyPrefixes =
    [
        "Godot",
        "Microsoft.Data.Sqlite",
        "PCDynasty.Content",
        "PCDynasty.Persistence",
        "PCDynasty.Presentation",
    ];

    [Fact]
    public void ManagerWorldContractHasNoAdapterDependencies()
    {
        Assembly application = typeof(IManagerWorld).Assembly;
        string[] references = application
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        foreach (string forbiddenPrefix in ForbiddenAssemblyPrefixes)
        {
            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(forbiddenPrefix, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ManagerWorldRowsAreSealedRecords()
    {
        Type[] rowTypes =
        [
            typeof(ClubRow),
            typeof(PlayerRow),
            typeof(ManagerRow),
            typeof(StaffRow),
            typeof(CompetitionRow),
        ];

        foreach (Type rowType in rowTypes)
        {
            Assert.True(rowType.IsSealed, $"{rowType.Name} must be sealed.");
            Assert.NotNull(rowType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static));
        }
    }

    [Fact]
    public void QueryPortExposesOnlyImmutableReadModels()
    {
        MethodInfo[] methods = typeof(IManagerWorld).GetMethods();

        // Clubs, players, managers, staff, competitions per club, all
        // competitions and a single club lookup: seven queries, no commands.
        Assert.Equal(7, methods.Length);
        Assert.All(methods, method => Assert.DoesNotContain("Task", method.ReturnType.Name, StringComparison.Ordinal));
        Assert.DoesNotContain(methods, method => method.Name.StartsWith("Save", StringComparison.Ordinal));
        Assert.DoesNotContain(methods, method => method.Name.StartsWith("Update", StringComparison.Ordinal));
    }

    [Fact]
    public void CompetitionRowDefensivelyCopiesRivals()
    {
        var source = new List<string> { "Northglass" };
        var row = new CompetitionRow(
            "competition:boreal-series",
            "Boreal Series",
            "round-robin",
            "annual",
            source);

        source.Add("Cinder Valley");

        Assert.Equal(["Northglass"], row.Rivals.ToArray());
    }
}
