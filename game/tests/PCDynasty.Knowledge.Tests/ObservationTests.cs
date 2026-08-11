using PCDynasty.Core.Domain;
using PCDynasty.Knowledge.Observations;
using Xunit;

namespace PCDynasty.Knowledge.Tests;

public sealed class ObservationTests
{
    [Fact]
    public void RecordsAConfidenceAwareEstimateWithoutHiddenTruth()
    {
        var observation = new Observation(
            new PersonId(41),
            new AttributeId(2),
            minimumEstimate: 58,
            maximumEstimate: 74,
            confidence: 67,
            sourceId: "scout:regional:7",
            observedOn: new DateOnly(2026, 8, 11));

        Assert.Equal(41, observation.SubjectId.Value);
        Assert.Equal(2, observation.AttributeId.Value);
        Assert.Equal(58, observation.MinimumEstimate);
        Assert.Equal(74, observation.MaximumEstimate);
        Assert.Equal(67, observation.Confidence);
        Assert.Equal("scout:regional:7", observation.SourceId);
        Assert.Equal(new DateOnly(2026, 8, 11), observation.ObservedOn);

        string[] propertyNames = typeof(Observation)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(propertyNames, name =>
            name.Contains("True", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Actual", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Exact", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RejectsInvalidEstimateBoundaries()
    {
        Assert.Throws<ArgumentException>(() => CreateObservation(75, 60, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateObservation(-1, 60, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateObservation(60, 101, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateObservation(60, 75, 101));
    }

    [Fact]
    public void RejectsAnUnknownSourceOrDate()
    {
        Assert.Throws<ArgumentException>(() => new Observation(
            new PersonId(41),
            new AttributeId(2),
            58,
            74,
            67,
            " ",
            new DateOnly(2026, 8, 11)));

        Assert.Throws<ArgumentOutOfRangeException>(() => new Observation(
            new PersonId(41),
            new AttributeId(2),
            58,
            74,
            67,
            "scout:regional:7",
            default));
    }

    private static Observation CreateObservation(
        int minimumEstimate,
        int maximumEstimate,
        int confidence)
    {
        return new Observation(
            new PersonId(41),
            new AttributeId(2),
            minimumEstimate,
            maximumEstimate,
            confidence,
            "scout:regional:7",
            new DateOnly(2026, 8, 11));
    }
}
