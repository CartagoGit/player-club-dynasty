namespace PCDynasty.Application.Players;

public sealed class PlayerSummaryView
{
    public PlayerSummaryView(
        int personId,
        string displayName,
        PlayerIndicatorView technique,
        PlayerIndicatorView form,
        PlayerIndicatorView physical,
        PlayerIndicatorView mental,
        PlayerIndicatorView overall)
    {
        PersonId = personId;
        DisplayName = displayName;
        Technique = technique;
        Form = form;
        Physical = physical;
        Mental = mental;
        Overall = overall;
    }

    public int PersonId { get; }

    public string DisplayName { get; }

    public PlayerIndicatorView Technique { get; }

    public PlayerIndicatorView Form { get; }

    public PlayerIndicatorView Physical { get; }

    public PlayerIndicatorView Mental { get; }

    public PlayerIndicatorView Overall { get; }
}

public sealed class PlayerIndicatorView
{
    public PlayerIndicatorView(
        string key,
        int minimumEstimate,
        int maximumEstimate,
        int estimate,
        int confidence,
        string sourceId,
        DateOnly observedOn,
        string explanationKey)
    {
        Key = key;
        MinimumEstimate = minimumEstimate;
        MaximumEstimate = maximumEstimate;
        Estimate = estimate;
        Confidence = confidence;
        SourceId = sourceId;
        ObservedOn = observedOn;
        ExplanationKey = explanationKey;
    }

    public string Key { get; }

    public int MinimumEstimate { get; }

    public int MaximumEstimate { get; }

    public int Estimate { get; }

    public int Confidence { get; }

    public string SourceId { get; }

    public DateOnly ObservedOn { get; }

    public string ExplanationKey { get; }
}
