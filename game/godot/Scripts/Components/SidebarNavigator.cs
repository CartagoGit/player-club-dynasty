using System.Collections.Immutable;
using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Components;

public sealed class NavigationItemView
{
    public NavigationItemView(
        string id,
        string label,
        string accessibleDescription,
        bool enabled = true)
    {
        Id = Required(id, nameof(id));
        Label = Required(label, nameof(label));
        AccessibleDescription = Required(
            accessibleDescription,
            nameof(accessibleDescription));
        Enabled = enabled;
    }

    public string Id { get; }
    public string Label { get; }
    public string AccessibleDescription { get; }
    public bool Enabled { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public partial class SidebarNavigator : VBoxContainer
{
    [Signal]
    public delegate void DestinationRequestedEventHandler(string destinationId);

    public void Bind(
        IEnumerable<NavigationItemView> items,
        string? selectedDestinationId)
    {
        ArgumentNullException.ThrowIfNull(items);
        ImmutableArray<NavigationItemView> snapshot = items.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            return item;
        }).ToImmutableArray();
        if (snapshot.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() !=
            snapshot.Length)
        {
            throw new ArgumentException("Navigation destination identifiers must be unique.", nameof(items));
        }

        ClearItems();
        foreach (NavigationItemView item in snapshot)
        {
            AddChild(CreateButton(item, selectedDestinationId));
        }
    }

    private Button CreateButton(
        NavigationItemView item,
        string? selectedDestinationId)
    {
        bool selected = string.Equals(
            item.Id,
            selectedDestinationId,
            StringComparison.Ordinal);
        var button = new Button
        {
            Text = item.Label,
            Alignment = HorizontalAlignment.Left,
            Disabled = !item.Enabled,
            FocusMode = FocusModeEnum.All,
            TooltipText = item.AccessibleDescription,
            ToggleMode = true,
            ButtonPressed = selected,
            CustomMinimumSize = new Vector2(
                ManagerDesignTokens.SidebarWidth,
                ManagerDesignTokens.MinimumPointerTarget),
        };
        string destinationId = item.Id;
        button.Pressed += () => EmitSignal(
            SignalName.DestinationRequested,
            destinationId);
        return button;
    }

    private void ClearItems()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
}
