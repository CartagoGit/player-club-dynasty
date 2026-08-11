using System.Reflection;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Architecture.Tests;

/// <summary>
/// Architectural invariants for the engine-neutral presentation layer.
/// These tests pin the contract that <c>PCDynasty.Presentation.Core</c>
/// never reaches into Godot: that is what keeps the domain engine-agnostic
/// and lets us test the bridge on a non-Windows CI without Godot.
/// </summary>
public sealed class PresentationCoreDependencyTests
{
    [Fact]
    public void PresentationCoreDoesNotReferenceGodotAssemblies()
    {
        Assembly presentationCore = typeof(WorldService).Assembly;
        string[] references = presentationCore
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            references,
            reference => reference.StartsWith("Godot", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PresentationCoreDoesNotReferenceTheGodotBoundPresentationAssembly()
    {
        Assembly presentationCore = typeof(WorldService).Assembly;
        string[] references = presentationCore
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        // The Godot-bound Presentation must depend on Presentation.Core,
        // never the other way around (ADR 0001: presentation consumes
        // read models, never the other way).
        Assert.DoesNotContain(
            references,
            reference => reference.StartsWith("PCDynasty.Presentation", StringComparison.Ordinal) && reference != "PCDynasty.Presentation.Core");
    }
}