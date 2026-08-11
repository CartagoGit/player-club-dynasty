using System.Collections.Immutable;
using Godot;
using PCDynasty.Presentation.Components;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Clubs;

public partial class ClubMetricGrid : GridContainer
{
    public override void _Ready()
    {
        Columns = 3;
        AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space12);
        AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space12);
    }

    public void Bind(IEnumerable<MetricCardView> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ImmutableArray<MetricCardView> snapshot = metrics.Select(metric =>
        {
            ArgumentNullException.ThrowIfNull(metric);
            return metric;
        }).ToImmutableArray();

        Clear();
        foreach (MetricCardView metric in snapshot)
        {
            var card = new MetricCard
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            card.Bind(metric);
            AddChild(card);
        }
    }

    private void Clear()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
}
