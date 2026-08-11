using PCDynasty.Core.Domain;

namespace PCDynasty.Core.People;

public sealed class Person
{
    public Person(PersonId id, PersonProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        Id = id;
        Profile = profile;
    }

    public PersonId Id { get; }

    public PersonProfile Profile { get; }
}
