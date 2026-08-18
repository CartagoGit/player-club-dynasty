using System.Collections.Immutable;
using Godot;
using PCDynasty.Presentation.Components;

namespace PCDynasty.Presentation.Clubs;

public sealed class ClubOverviewView
{
    public ClubOverviewView(
        string clubId,
        string displayName,
        string context,
        IEnumerable<MetricCardView> metrics,
        IEnumerable<string> alerts,
        IEnumerable<string> nextActions)
    {
        ClubId = Required(clubId, nameof(clubId));
        DisplayName = Required(displayName, nameof(displayName));
        Context = Required(context, nameof(context));
        Metrics = Copy(metrics, nameof(metrics));
        Alerts = CopyText(alerts, nameof(alerts));
        NextActions = CopyText(nextActions, nameof(nextActions));
    }

    public string ClubId { get; }
    public string DisplayName { get; }
    public string Context { get; }
    public ImmutableArray<MetricCardView> Metrics { get; }
    public ImmutableArray<string> Alerts { get; }
    public ImmutableArray<string> NextActions { get; }

    private static ImmutableArray<MetricCardView> Copy(
        IEnumerable<MetricCardView> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        return values.Select(value =>
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);
            return value;
        }).ToImmutableArray();
    }

    private static ImmutableArray<string> CopyText(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        return values.Select(value => Required(value, parameterName)).ToImmutableArray();
    }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public partial class ClubOverviewController : Control
{
    private Container Alerts => GetNode<Container>("%Alerts");
    private Label ClubContext => GetNode<Label>("%ClubContext");
    private Label ClubName => GetNode<Label>("%ClubName");
    private ClubMetricGrid Metrics => GetNode<ClubMetricGrid>("%Metrics");
    private Container NextActions => GetNode<Container>("%NextActions");

    public void Bind(ClubOverviewView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        ClubName.Text = view.DisplayName;
        ClubContext.Text = view.Context;
        TooltipText = $"Resumen de {view.DisplayName}";
        Metrics.Bind(view.Metrics);
        BindList(Alerts, view.Alerts, "Sin alertas");
        BindList(NextActions, view.NextActions, "Sin acciones pendientes");
    }

    private static void BindList(
        Node container,
        ImmutableArray<string> items,
        string emptyMessage)
    {
        Clear(container);
        IEnumerable<string> content = items.IsEmpty ? [emptyMessage] : items;
        foreach (string item in content)
        {
            container.AddChild(new Label
            {
                Text = $"• {item}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
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
