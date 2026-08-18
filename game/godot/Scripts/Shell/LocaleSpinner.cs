using Godot;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A minimal loading overlay the language picker shows while a tap
/// is being processed. The spinner is a child of the
/// <see cref="LocaleSection"/> parent so it covers the whole row
/// without an explicit anchor; the section frees the spinner once
/// the load resolves.
/// </summary>
public sealed partial class LocaleSpinner : PanelContainer
{
    public LocaleSpinner()
    {
        Name = "LocaleSpinner";
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        var label = new Label
        {
            Name = "Message",
            Text = "Loading language…",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AddChild(label);
    }
}
