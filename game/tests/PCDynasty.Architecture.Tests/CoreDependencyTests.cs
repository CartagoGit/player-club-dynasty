using System.Reflection;
using PCDynasty.Core.Domain;
using Xunit;

namespace PCDynasty.Architecture.Tests;

public sealed class CoreDependencyTests
{
    private static readonly string[] ForbiddenAssemblyPrefixes =
    [
        "Godot",
        "PCDynasty.Football",
        "PCDynasty.Persistence",
        "PCDynasty.Presentation",
    ];

    private static readonly string[] FootballVocabulary =
    [
        "Football",
        "Goalkeeper",
        "Offside",
        "CornerKick",
        "PenaltyKick",
        "YellowCard",
        "ChampionsLeague",
    ];

    [Fact]
    public void CoreDoesNotReferencePresentationPersistenceOrFootballAssemblies()
    {
        Assembly coreAssembly = typeof(AttributeId).Assembly;
        string[] references = coreAssembly
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
    public void CorePublicContractDoesNotExposeFootballVocabulary()
    {
        Assembly coreAssembly = typeof(AttributeId).Assembly;
        string[] publicTypeNames = coreAssembly
            .GetExportedTypes()
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        foreach (string footballTerm in FootballVocabulary)
        {
            Assert.DoesNotContain(
                publicTypeNames,
                typeName => typeName.Contains(footballTerm, StringComparison.OrdinalIgnoreCase));
        }
    }
}
