namespace PCDynasty.Simulation.Determinism;

public readonly record struct DeterministicNumber
{
    public const long Scale = 1_000_000;

    private DeterministicNumber(long rawValue)
    {
        RawValue = rawValue;
    }

    public long RawValue { get; }

    public static DeterministicNumber FromRaw(long rawValue) => new(rawValue);

    public static DeterministicNumber FromInt32(int value) =>
        new(checked(value * Scale));

    public static DeterministicNumber FromRatio(long numerator, long denominator)
    {
        if (denominator == 0)
        {
            throw new DivideByZeroException();
        }

        Int128 rawValue = (Int128)numerator * Scale / denominator;
        return new DeterministicNumber(checked((long)rawValue));
    }

    public static DeterministicNumber operator +(
        DeterministicNumber left,
        DeterministicNumber right) =>
        new(checked(left.RawValue + right.RawValue));

    public static DeterministicNumber operator -(
        DeterministicNumber left,
        DeterministicNumber right) =>
        new(checked(left.RawValue - right.RawValue));

    public static DeterministicNumber operator *(
        DeterministicNumber left,
        DeterministicNumber right)
    {
        Int128 rawValue = (Int128)left.RawValue * right.RawValue / Scale;
        return new DeterministicNumber(checked((long)rawValue));
    }

    public static DeterministicNumber operator /(
        DeterministicNumber left,
        DeterministicNumber right)
    {
        if (right.RawValue == 0)
        {
            throw new DivideByZeroException();
        }

        Int128 rawValue = (Int128)left.RawValue * Scale / right.RawValue;
        return new DeterministicNumber(checked((long)rawValue));
    }
}
