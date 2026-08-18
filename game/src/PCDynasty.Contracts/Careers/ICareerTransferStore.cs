namespace PCDynasty.Contracts.Careers;

public interface ICareerTransferStore
{
    IReadOnlyList<CareerTransferRecord> ListForClub(string clubExternalId);

    bool TrySettle(
        CareerTransferRecord transfer,
        CareerFinanceState expectedFinance,
        CareerFinanceState updatedFinance);

    void Clear();
}
