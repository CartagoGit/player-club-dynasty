using System.Globalization;
using Godot;
using PCDynasty.Application.History;
using PCDynasty.Application.Players;
using PCDynasty.Presentation.Components;

namespace PCDynasty.Presentation.Players;

public partial class PlayerProfileController : Control
{
    private Container HistoryItems => GetNode<Container>("%HistoryItems");
    private Container MetricGrid => GetNode<Container>("%MetricGrid");
    private Label PlayerName => GetNode<Label>("%PlayerName");
    private PlayerMetricDetailPanel DetailPanel =>
        GetNode<PlayerMetricDetailPanel>("%MetricDetailPanel");

    public void Bind(
        PlayerSummaryView summary,
        IEnumerable<HistoryEntryView> history)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(history);
        HistoryEntryView[] historySnapshot = history.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            return item;
        }).ToArray();

        PlayerName.Text = summary.DisplayName;
        Clear(MetricGrid);
        AddMetric(summary.Overall, "Valoración");
        AddMetric(summary.Technique, "Técnica");
        AddMetric(summary.Form, "Forma");
        AddMetric(summary.Physical, "Físico");
        AddMetric(summary.Mental, "Mental");

        Clear(HistoryItems);
        foreach (HistoryEntryView item in historySnapshot.OrderByDescending(item => item.Sequence))
        {
            HistoryItems.AddChild(new Label
            {
                Text = $"Día {item.OccurredOnDay} · {item.TypeId}",
                TooltipText = $"Registro {item.Sequence} del historial",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
        }
    }

    private void AddMetric(PlayerIndicatorView indicator, string label)
    {
        var card = new MetricCard();
        card.Bind(new MetricCardView(
            label,
            indicator.Estimate.ToString(CultureInfo.CurrentCulture),
            $"{indicator.MinimumEstimate}–{indicator.MaximumEstimate} · {indicator.Confidence}% confianza",
            $"{label}: {indicator.Estimate}. Confianza {indicator.Confidence} por ciento."));
        card.GuiInput += input => HandleMetricInput(input, indicator);
        MetricGrid.AddChild(card);
    }

    private void HandleMetricInput(
        InputEvent input,
        PlayerIndicatorView indicator)
    {
        bool pointerActivation = input is InputEventMouseButton
        {
            ButtonIndex: MouseButton.Left,
            Pressed: true,
        };
        if (pointerActivation || input.IsActionPressed("ui_accept"))
        {
            DetailPanel.ShowIndicator(indicator);
            GetViewport().SetInputAsHandled();
        }
    }

    private static void Clear(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
