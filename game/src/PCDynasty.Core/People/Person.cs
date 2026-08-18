using PCDynasty.Core.Domain;

namespace PCDynasty.Core.People;

public sealed class Person
{
    public Person(PersonId id, PersonProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (id.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                id,
                "A person must have a valid identity.");
        }

        Id = id;
        Profile = profile;
    }

    public PersonId Id { get; }

    public PersonProfile Profile { get; }
}
