namespace PCDynasty.Application.Market;

public interface ICareerTransferMarketService
{
    IReadOnlyList<TransferCandidate> ListCandidates(string managedClubExternalId);

    TransferOfferResult SubmitOffer(
        string managedClubExternalId,
        string playerExternalId,
        long offeredMinorUnits);
}
