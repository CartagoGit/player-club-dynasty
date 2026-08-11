using PCDynasty.Core.People;
using PCDynasty.Core.Performance;
using PCDynasty.Knowledge.Observations;

namespace PCDynasty.Application.Players;

public static class PlayerSummaryQuery
{
    public static PlayerSummaryView Execute(
        Person person,
        Observation technique,
        Observation form,
        Observation physical,
        Observation mental,
        CurrentPerformanceWeights weights)
    {
        ArgumentNullException.ThrowIfNull(person);
        Observation[] observations = [technique, form, physical, mental];
        ValidateKnowledgeOwnership(person, observations);

        PlayerIndicatorView techniqueView = CreateObservedIndicator("technique", technique);
        PlayerIndicatorView formView = CreateObservedIndicator("form", form);
        PlayerIndicatorView physicalView = CreateObservedIndicator("physical", physical);
        PlayerIndicatorView mentalView = CreateObservedIndicator("mental", mental);
        PlayerIndicatorView overallView = CreateOverallIndicator(
            technique,
            form,
            physical,
            mental,
            weights);

        return new PlayerSummaryView(
            person.Id.Value,
            $"{person.Profile.GivenName} {person.Profile.FamilyName}",
            techniqueView,
            formView,
            physicalView,
            mentalView,
            overallView);
    }

    private static void ValidateKnowledgeOwnership(Person person, IEnumerable<Observation> observations)
    {
        foreach (Observation observation in observations)
        {
            ArgumentNullException.ThrowIfNull(observation);
            if (observation.SubjectId != person.Id)
            {
                throw new ArgumentException(
                    "Every observation in a player summary must describe the requested person.",
                    nameof(observations));
            }
        }
    }

    private static PlayerIndicatorView CreateObservedIndicator(string key, Observation observation)
    {
        return new PlayerIndicatorView(
            key,
            observation.MinimumEstimate,
            observation.MaximumEstimate,
            Midpoint(observation.MinimumEstimate, observation.MaximumEstimate),
            observation.Confidence,
            observation.SourceId,
            observation.ObservedOn,
            "knowledge.observed-range");
    }

    private static PlayerIndicatorView CreateOverallIndicator(
        Observation technique,
        Observation form,
        Observation physical,
        Observation mental,
        CurrentPerformanceWeights weights)
    {
        int minimum = DeriveScore(
            technique.MinimumEstimate,
            form.MinimumEstimate,
            physical.MinimumEstimate,
            mental.MinimumEstimate,
            weights);
        int maximum = DeriveScore(
            technique.MaximumEstimate,
            form.MaximumEstimate,
            physical.MaximumEstimate,
            mental.MaximumEstimate,
            weights);
        int estimate = DeriveScore(
            Midpoint(technique.MinimumEstimate, technique.MaximumEstimate),
            Midpoint(form.MinimumEstimate, form.MaximumEstimate),
            Midpoint(physical.MinimumEstimate, physical.MaximumEstimate),
            Midpoint(mental.MinimumEstimate, mental.MaximumEstimate),
            weights);

        long totalWeight = (long)weights.Technique + weights.Form + weights.Physical + weights.Mental;
        long weightedConfidence =
            ((long)technique.Confidence * weights.Technique) +
            ((long)form.Confidence * weights.Form) +
            ((long)physical.Confidence * weights.Physical) +
            ((long)mental.Confidence * weights.Mental);
        int confidence = (int)((weightedConfidence + (totalWeight / 2)) / totalWeight);
        DateOnly observedOn = new[]
        {
            technique.ObservedOn,
            form.ObservedOn,
            physical.ObservedOn,
            mental.ObservedOn,
        }.Max();

        return new PlayerIndicatorView(
            "overall",
            minimum,
            maximum,
            estimate,
            confidence,
            "derived:player-summary",
            observedOn,
            "knowledge.weighted-observed-indicators");
    }

    private static int DeriveScore(
        int technique,
        int form,
        int physical,
        int mental,
        CurrentPerformanceWeights weights)
    {
        return CurrentPerformance.Derive(
            new CapabilityProfile(technique, physical, mental),
            form,
            weights).Score;
    }

    private static int Midpoint(int minimum, int maximum)
    {
        return (minimum + maximum + 1) / 2;
    }
}
