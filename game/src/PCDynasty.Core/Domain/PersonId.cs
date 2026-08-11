namespace PCDynasty.Core.Domain;

public readonly record struct PersonId
{
    public PersonId(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "A person ID must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
