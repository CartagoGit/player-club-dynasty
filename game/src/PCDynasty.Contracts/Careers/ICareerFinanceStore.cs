namespace PCDynasty.Contracts.Careers;

public interface ICareerFinanceStore
{
    void Save(CareerFinanceState state);

    bool TryLoad(string clubExternalId, out CareerFinanceState? state);

    void Clear();
}
