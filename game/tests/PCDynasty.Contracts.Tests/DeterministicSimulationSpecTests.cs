using PCDynasty.Contracts.Simulation;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class DeterministicSimulationSpecTests
{
    [Fact]
    public void VersionsEverySourceOfCrossRuntimeDeterminism()
    {
        var spec = new DeterministicSimulationSpec(
            protocolVersion: 1,
            prngAlgorithmId: "pcg32",
            prngVersion: 1,
            canonicalSerializationId: "pcdynasty-binary",
            canonicalSerializationVersion: 2,
            ticksPerDay: 1_440,
            numericScale: 1_000_000);

        Assert.Equal(1, spec.ProtocolVersion);
        Assert.Equal("pcg32", spec.PrngAlgorithmId);
        Assert.Equal(1, spec.PrngVersion);
        Assert.Equal("pcdynasty-binary", spec.CanonicalSerializationId);
        Assert.Equal(2, spec.CanonicalSerializationVersion);
        Assert.Equal(1_440, spec.TicksPerDay);
        Assert.Equal(1_000_000, spec.NumericScale);
    }

    [Fact]
    public void ProvidesATotalOrdinalEventOrder()
    {
        EventSequence[] sequences =
        [
            new EventSequence(42, phase: 2, "emitter:b", localSequence: 0),
            new EventSequence(42, phase: 1, "emitter:z", localSequence: 0),
            new EventSequence(42, phase: 1, "emitter:a", localSequence: 1),
            new EventSequence(41, phase: 9, "emitter:z", localSequence: 9),
            new EventSequence(42, phase: 1, "emitter:a", localSequence: 0),
        ];

        Array.Sort(sequences);

        Assert.Equal(41, sequences[0].WorldTick);
        Assert.Equal("emitter:a", sequences[1].EmitterId);
        Assert.Equal(0, sequences[1].LocalSequence);
        Assert.Equal(1, sequences[2].LocalSequence);
        Assert.Equal("emitter:z", sequences[3].EmitterId);
        Assert.Equal(2, sequences[4].Phase);
    }

    [Fact]
    public void RejectsAmbiguousOrUnversionedProtocolValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DeterministicSimulationSpec(
            0,
            "pcg32",
            1,
            "binary",
            1,
            1_440,
            1_000));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EventSequence(-1, phase: 0, "emitter:a", localSequence: 0));
        Assert.Throws<ArgumentException>(() =>
            new EventSequence(1, phase: 0, " ", localSequence: 0));
    }
}
