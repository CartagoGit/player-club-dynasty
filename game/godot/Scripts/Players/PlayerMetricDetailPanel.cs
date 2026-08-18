using System.Globalization;
using PCDynasty.Application.Players;
using PCDynasty.Presentation.Components;

namespace PCDynasty.Presentation.Players;

public partial class PlayerMetricDetailPanel : DetailDrilldown
{
    public void ShowIndicator(PlayerIndicatorView indicator)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        ShowDetail(new DetailDrilldownView(
            indicator.Key,
            indicator.ExplanationKey,
            [
                new DetailFieldView(
                    "Estimación",
                    indicator.Estimate.ToString(CultureInfo.CurrentCulture)),
                new DetailFieldView(
                    "Intervalo observado",
                    $"{indicator.MinimumEstimate}–{indicator.MaximumEstimate}"),
                new DetailFieldView("Confianza", $"{indicator.Confidence}%"),
                new DetailFieldView("Fuente", indicator.SourceId),
                new DetailFieldView(
                    "Observado",
                    indicator.ObservedOn.ToString("d", CultureInfo.CurrentCulture)),
            ]));
    }
}
