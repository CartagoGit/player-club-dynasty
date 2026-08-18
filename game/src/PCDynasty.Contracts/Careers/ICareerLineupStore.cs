namespace PCDynasty.Contracts.Careers;

public interface ICareerLineupStore
{
    void Save(LineupPlan plan);

    bool TryLoad(string clubExternalId, out LineupPlan? plan);

    void Clear();
}
