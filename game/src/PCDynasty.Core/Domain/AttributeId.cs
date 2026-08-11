namespace PCDynasty.Core.Domain;

public readonly record struct AttributeId
{
    public AttributeId(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "An attribute ID must be zero or greater.");
        }

        Value = value;
    }

    public int Value { get; }
}
