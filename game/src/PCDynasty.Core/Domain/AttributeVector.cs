namespace PCDynasty.Core.Domain;

public sealed class AttributeVector
{
    private readonly int[] values;

    public AttributeVector(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = values.ToArray();
    }

    public AttributeVector(ReadOnlySpan<int> values)
    {
        this.values = values.ToArray();
    }

    public int Count => values.Length;

    public int this[AttributeId attributeId] => values[GetIndex(attributeId)];

    public bool TryGetValue(AttributeId attributeId, out int value)
    {
        if ((uint)attributeId.Value >= (uint)values.Length)
        {
            value = default;
            return false;
        }

        value = values[attributeId.Value];
        return true;
    }

    private int GetIndex(AttributeId attributeId)
    {
        if ((uint)attributeId.Value >= (uint)values.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(attributeId), attributeId, "The attribute ID is outside this vector.");
        }

        return attributeId.Value;
    }
}
