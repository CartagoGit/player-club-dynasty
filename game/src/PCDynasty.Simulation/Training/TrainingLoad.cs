namespace PCDynasty.Simulation.Training;

public sealed class TrainingLoad
{
    public TrainingLoad(
        int durationMinutes,
        int intensity,
        int formEmphasis,
        int developmentEmphasis)
    {
        if (durationMinutes is < 1 or > 480)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                durationMinutes,
                "A training session must last between one minute and eight hours.");
        }

        DurationMinutes = durationMinutes;
        Intensity = Score(intensity, nameof(intensity));
        FormEmphasis = Score(formEmphasis, nameof(formEmphasis));
        DevelopmentEmphasis = Score(
            developmentEmphasis,
            nameof(developmentEmphasis));
        WorkloadUnits = (int)Math.Ceiling(durationMinutes * intensity / 100d);
    }

    public int DurationMinutes { get; }

    public int Intensity { get; }

    public int FormEmphasis { get; }

    public int DevelopmentEmphasis { get; }

    public int WorkloadUnits { get; }

    internal static int Score(int value, string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A training score must be between zero and one hundred.");
        }

        return value;
    }
}
