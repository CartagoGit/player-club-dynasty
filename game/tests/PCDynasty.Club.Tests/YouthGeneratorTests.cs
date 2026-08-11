using PCDynasty.Club.Academy;
using PCDynasty.Club.Facilities;
using Xunit;

namespace PCDynasty.Club.Tests;

public sealed class YouthGeneratorTests
{
    [Fact]
    public void GenerationIsReplayableAndCarriesFacilityRegionAndTimeContext()
    {
        var academy = new AcademyState(
            "academy:7",
            "club:7",
            "region:north",
            asOfDay: 10_000,
            recruitment: 720,
            coaching: 680,
            new FacilityState(
                "facility:academy",
                "club:7",
                asOfDay: 10_000,
                quality: 800,
                condition: 700,
                modernization: 600));

        YouthGenerationResult first = YouthGenerator.Generate(
            academy,
            targetDay: 10_365,
            seed: 42,
            candidateCount: 8);
        YouthGenerationResult replay = YouthGenerator.Generate(
            academy,
            targetDay: 10_365,
            seed: 42,
            candidateCount: 8);

        Assert.Equal(Fingerprint(first), Fingerprint(replay));
        Assert.Equal(10_365, first.State.AsOfDay);
        Assert.All(first.Candidates, candidate =>
        {
            Assert.Equal("region:north", candidate.RegionId);
            Assert.InRange(candidate.LearningAffinity, 0, 1_000);
            Assert.InRange(candidate.Adaptability, 0, 1_000);
            Assert.InRange(candidate.BirthDay, 3_000, 7_000);
        });
        Assert.NotEqual(
            Fingerprint(first),
            Fingerprint(YouthGenerator.Generate(academy, 10_365, seed: 43, candidateCount: 8)));
    }

    [Fact]
    public void FacilityQualityChangesOutputWithoutChangingRandomDrawOrder()
    {
        AcademyState low = Academy(quality: 200);
        AcademyState high = Academy(quality: 900);

        YouthGenerationResult lowResult = YouthGenerator.Generate(low, 10_365, 99, 32);
        YouthGenerationResult highResult = YouthGenerator.Generate(high, 10_365, 99, 32);

        Assert.True(
            highResult.Candidates.Average(candidate => candidate.LearningAffinity) >
            lowResult.Candidates.Average(candidate => candidate.LearningAffinity));
        Assert.Equal(
            lowResult.Candidates.Select(candidate => candidate.CandidateId),
            highResult.Candidates.Select(candidate => candidate.CandidateId));
    }

    private static AcademyState Academy(int quality) =>
        new(
            "academy:7",
            "club:7",
            "region:test",
            asOfDay: 10_000,
            recruitment: 500,
            coaching: 500,
            new FacilityState("facility:academy", "club:7", 10_000, quality, 700, 500));

    private static string Fingerprint(YouthGenerationResult result) => string.Join(
        ';',
        result.Candidates.Select(candidate => string.Join(
            '|',
            candidate.CandidateId,
            candidate.BirthDay,
            candidate.LearningAffinity,
            candidate.Adaptability)));
}
