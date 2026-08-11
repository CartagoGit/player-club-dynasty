using PCDynasty.Application.Players;
using PCDynasty.Core.Domain;
using PCDynasty.Core.People;
using PCDynasty.Core.Performance;
using PCDynasty.Knowledge.Observations;
using Xunit;

namespace PCDynasty.Application.Tests;

public sealed class PlayerSummaryQueryTests
{
    [Fact]
    public void ProducesExplainableIndicatorsFromKnowledgeOnly()
    {
        Person person = CreatePerson(41);

        PlayerSummaryView view = PlayerSummaryQuery.Execute(
            person,
            CreateObservation(person.Id, 0, 70, 80, 60, "scout:technical"),
            CreateObservation(person.Id, 1, 50, 70, 80, "analyst:form"),
            CreateObservation(person.Id, 2, 60, 80, 70, "medical:physical"),
            CreateObservation(person.Id, 3, 40, 60, 50, "coach:mental"),
            new CurrentPerformanceWeights(1, 1, 1, 1));

        Assert.Equal(41, view.PersonId);
        Assert.Equal("Iria Valdés", view.DisplayName);
        Assert.Equal(75, view.Technique.Estimate);
        Assert.Equal(70, view.Technique.MinimumEstimate);
        Assert.Equal(80, view.Technique.MaximumEstimate);
        Assert.Equal(60, view.Technique.Confidence);
        Assert.Equal("scout:technical", view.Technique.SourceId);
        Assert.Equal(64, view.Overall.Estimate);
        Assert.Equal(55, view.Overall.MinimumEstimate);
        Assert.Equal(73, view.Overall.MaximumEstimate);
        Assert.Equal(65, view.Overall.Confidence);
        Assert.Equal("knowledge.weighted-observed-indicators", view.Overall.ExplanationKey);
    }

    [Fact]
    public void RejectsKnowledgeAboutAnotherPerson()
    {
        Person person = CreatePerson(41);
        Observation otherPerson = CreateObservation(
            new PersonId(99),
            0,
            70,
            80,
            60,
            "scout:technical");

        Assert.Throws<ArgumentException>(() => PlayerSummaryQuery.Execute(
            person,
            otherPerson,
            CreateObservation(person.Id, 1, 50, 70, 80, "analyst:form"),
            CreateObservation(person.Id, 2, 60, 80, 70, "medical:physical"),
            CreateObservation(person.Id, 3, 40, 60, 50, "coach:mental"),
            new CurrentPerformanceWeights(1, 1, 1, 1)));
    }

    [Fact]
    public void ApplicationDoesNotReferenceGodotOrAcceptHiddenAttributeVectors()
    {
        string[] assemblyReferences = typeof(PlayerSummaryQuery).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Type[] queryParameterTypes = typeof(PlayerSummaryQuery)
            .GetMethods()
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        Assert.DoesNotContain(assemblyReferences, name =>
            name.StartsWith("Godot", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(AttributeVector), queryParameterTypes);
        Assert.DoesNotContain(typeof(CapabilityProfile), queryParameterTypes);
    }

    private static Person CreatePerson(int id)
    {
        return new Person(
            new PersonId(id),
            new PersonProfile(
                "Iria",
                "Valdés",
                new DateOnly(2001, 3, 17),
                "fictional:country:monteluz"));
    }

    private static Observation CreateObservation(
        PersonId personId,
        int attributeId,
        int minimum,
        int maximum,
        int confidence,
        string sourceId)
    {
        return new Observation(
            personId,
            new AttributeId(attributeId),
            minimum,
            maximum,
            confidence,
            sourceId,
            new DateOnly(2026, 8, 11));
    }
}
