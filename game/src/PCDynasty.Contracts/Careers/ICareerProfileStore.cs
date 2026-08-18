namespace PCDynasty.Contracts.Careers;

public interface ICareerProfileStore
{
    void Save(CareerProfile profile);

    bool TryLoad(out CareerProfile? profile);
}
