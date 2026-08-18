using System.Collections.Immutable;

namespace PCDynasty.Club.Academy;

public sealed record YouthCandidate(
    string CandidateId,
    string RegionId,
    int BirthDay,
    int LearningAffinity,
    int Adaptability);

public sealed class YouthGenerationResult
{
    public YouthGenerationResult(
        AcademyState state,
        IEnumerable<YouthCandidate> candidates)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        ArgumentNullException.ThrowIfNull(candidates);
        Candidates = candidates.ToImmutableArray();
    }

    public AcademyState State { get; }
    public ImmutableArray<YouthCandidate> Candidates { get; }
}

public static class YouthGenerator
{
    private const int MinimumAgeDays = 12 * 365;
    private const int MaximumAgeDays = 18 * 365;

    public static YouthGenerationResult Generate(
        AcademyState academy,
        int targetDay,
        ulong seed,
        int candidateCount)
    {
        ArgumentNullException.ThrowIfNull(academy);
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, academy.AsOfDay);
        if (candidateCount is < 0 or > 10_000)
        {
            throw new ArgumentOutOfRangeException(nameof(candidateCount));
        }

        ulong derivedSeed = seed ^ StableHash(academy.AcademyId) ^
            RotateLeft(StableHash(academy.RegionId), 17) ^ (uint)targetDay;
        var random = new SplitMix64(derivedSeed);
        var candidates = ImmutableArray.CreateBuilder<YouthCandidate>(candidateCount);
        for (int index = 0; index < candidateCount; index++)
        {
            int ageDays = random.NextInclusive(MinimumAgeDays, MaximumAgeDays);
            int learningDraw = random.NextInclusive(0, AcademyValue.Maximum);
            int adaptabilityDraw = random.NextInclusive(0, AcademyValue.Maximum);
            int learning = Weighted(
                learningDraw,
                academy.Recruitment,
                academy.Coaching,
                academy.Facility.Quality);
            int adaptability = Math.Clamp(
                ((adaptabilityDraw * 700) +
                 (academy.Recruitment * 150) +
                 (academy.Facility.Modernization * 150)) / AcademyValue.Maximum,
                0,
                AcademyValue.Maximum);
            candidates.Add(new YouthCandidate(
                $"{academy.AcademyId}:candidate:{targetDay}:{index}",
                academy.RegionId,
                targetDay - ageDays,
                learning,
                adaptability));
        }

        return new YouthGenerationResult(academy.AtDay(targetDay), candidates);
    }

    private static int Weighted(int draw, int recruitment, int coaching, int facilityQuality) =>
        Math.Clamp(
            ((draw * 500) + (recruitment * 150) + (coaching * 150) +
             (facilityQuality * 200)) / AcademyValue.Maximum,
            0,
            AcademyValue.Maximum);

    private static ulong StableHash(string value)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;
        foreach (char character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }

    private static ulong RotateLeft(ulong value, int count) =>
        (value << count) | (value >> (64 - count));

    private struct SplitMix64
    {
        private ulong _state;

        public SplitMix64(ulong seed)
        {
            _state = seed;
        }

        public int NextInclusive(int minimum, int maximum)
        {
            ulong range = (ulong)(maximum - minimum + 1);
            return minimum + (int)(Next() % range);
        }

        private ulong Next()
        {
            ulong value = _state += 0x9E3779B97F4A7C15UL;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
