namespace PCDynasty.Contracts.Careers;

public interface ICareerHistoryStore
{
    void SaveMatch(HistoryEntry entry);

    void SaveSeason(SeasonHistoryEntry entry);

    IReadOnlyList<HistoryEntry> LoadMatches();

    IReadOnlyList<SeasonHistoryEntry> LoadSeasons();

    void Clear();
}
