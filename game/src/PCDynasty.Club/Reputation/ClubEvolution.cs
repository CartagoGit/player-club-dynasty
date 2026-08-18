using PCDynasty.Club.Staff;

namespace PCDynasty.Club.Reputation;

public sealed class ClubEvolutionState
{
    public ClubEvolutionState(string clubId, int asOfDay, int prestige, int fanbase)
    {
        ClubId = EffectValue.Required(clubId, nameof(clubId));
        AsOfDay = NonNegativeDay(asOfDay);
        Prestige = EffectValue.Score(prestige, nameof(prestige));
        Fanbase = EffectValue.Score(fanbase, nameof(fanbase));
    }

    public string ClubId { get; }
    public int AsOfDay { get; }
    public int Prestige { get; }
    public int Fanbase { get; }

    private static int NonNegativeDay(int day)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(day);
        return day;
    }
}

public sealed class ClubEvolutionPolicy
{
    public ClubEvolutionPolicy(
        int performanceWeight,
        int staffWeight,
        int communityWeight,
        int prestigeFanbaseWeight,
        int communityFanbaseWeight)
    {
        PerformanceWeight = EffectValue.Score(performanceWeight, nameof(performanceWeight));
        StaffWeight = EffectValue.Score(staffWeight, nameof(staffWeight));
        CommunityWeight = EffectValue.Score(communityWeight, nameof(communityWeight));
        PrestigeFanbaseWeight = EffectValue.Score(
            prestigeFanbaseWeight,
            nameof(prestigeFanbaseWeight));
        CommunityFanbaseWeight = EffectValue.Score(
            communityFanbaseWeight,
            nameof(communityFanbaseWeight));
        ReputationTotalWeight = PerformanceWeight + StaffWeight + CommunityWeight;
        FanbaseTotalWeight = PrestigeFanbaseWeight + CommunityFanbaseWeight;
        ArgumentOutOfRangeException.ThrowIfZero(ReputationTotalWeight);
        ArgumentOutOfRangeException.ThrowIfZero(FanbaseTotalWeight);
    }

    public int PerformanceWeight { get; }
    public int StaffWeight { get; }
    public int CommunityWeight { get; }
    public int PrestigeFanbaseWeight { get; }
    public int CommunityFanbaseWeight { get; }
    internal int ReputationTotalWeight { get; }
    internal int FanbaseTotalWeight { get; }
}

public sealed class ClubInfluencePolicy
{
    public ClubInfluencePolicy(int prestigeWeight, int fanbaseWeight)
    {
        PrestigeWeight = EffectValue.Score(prestigeWeight, nameof(prestigeWeight));
        FanbaseWeight = EffectValue.Score(fanbaseWeight, nameof(fanbaseWeight));
        TotalWeight = PrestigeWeight + FanbaseWeight;
        ArgumentOutOfRangeException.ThrowIfZero(TotalWeight);
    }

    public int PrestigeWeight { get; }
    public int FanbaseWeight { get; }
    internal int TotalWeight { get; }
}

public static class ClubEvolution
{
    public static ClubEvolutionState Advance(
        ClubEvolutionState current,
        int targetDay,
        int performance,
        int staffEffect,
        int communityEngagement,
        ClubEvolutionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, current.AsOfDay);
        EffectValue.Score(performance, nameof(performance));
        EffectValue.Score(staffEffect, nameof(staffEffect));
        EffectValue.Score(communityEngagement, nameof(communityEngagement));
        int elapsedDays = targetDay - current.AsOfDay;
        long reputationSignal =
            (((long)performance * policy.PerformanceWeight) +
             ((long)staffEffect * policy.StaffWeight) +
             ((long)communityEngagement * policy.CommunityWeight)) /
            policy.ReputationTotalWeight;
        long prestigeDelta = ((reputationSignal - 500L) * elapsedDays) / 3_650L;
        int prestige = (int)Math.Clamp(
            current.Prestige + prestigeDelta,
            0,
            EffectValue.Maximum);
        long fanbaseSignal =
            (((long)prestige * policy.PrestigeFanbaseWeight) +
             ((long)communityEngagement * policy.CommunityFanbaseWeight)) /
            policy.FanbaseTotalWeight;
        long fanbaseDelta = ((fanbaseSignal - 500L) * elapsedDays) / 3_650L;
        int fanbase = (int)Math.Clamp(
            current.Fanbase + fanbaseDelta,
            0,
            EffectValue.Maximum);
        return new ClubEvolutionState(current.ClubId, targetDay, prestige, fanbase);
    }

    public static int ResolveInfluence(
        ClubEvolutionState state,
        ClubInfluencePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(policy);
        return (int)((((long)state.Prestige * policy.PrestigeWeight) +
                      ((long)state.Fanbase * policy.FanbaseWeight)) /
                     policy.TotalWeight);
    }
}
