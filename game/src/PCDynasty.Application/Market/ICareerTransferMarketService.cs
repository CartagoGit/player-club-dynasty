using PCDynasty.Application.Workspace;

namespace PCDynasty.Application.Market;

public interface ICareerTransferMarketService
{
    IReadOnlyList<TransferCandidate> ListCandidates(ManagedClubContext managed);

    TransferOfferResult SubmitOffer(
        ManagedClubContext managed,
        string playerExternalId,
        long offeredMinorUnits);
}
