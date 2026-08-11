namespace PCDynasty.Rules.Compilation;

public sealed class RuleComplexityLimits
{
    public RuleComplexityLimits(
        int maximumInstructions,
        int maximumStackDepth,
        int maximumOperandUtf8Bytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumInstructions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumStackDepth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumOperandUtf8Bytes);

        MaximumInstructions = maximumInstructions;
        MaximumStackDepth = maximumStackDepth;
        MaximumOperandUtf8Bytes = maximumOperandUtf8Bytes;
    }

    public static RuleComplexityLimits SafeDefaults { get; } = new(
        maximumInstructions: 1_024,
        maximumStackDepth: 64,
        maximumOperandUtf8Bytes: 256);

    public int MaximumInstructions { get; }
    public int MaximumStackDepth { get; }
    public int MaximumOperandUtf8Bytes { get; }
}
