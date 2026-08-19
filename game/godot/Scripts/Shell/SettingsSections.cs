using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Builds the controls for the preference tabs and writes every change
/// through to the store.
/// </summary>
/// <remarks>
/// Split out of <see cref="MainMenuController"/>, which had grown into a
/// menu router *and* a preferences form. The controller now decides where the
/// player is; this class decides what a preference looks like and what happens
/// when it moves. The settings board still knows neither.
///
/// Every handler follows the same shape — load, transform, save — so a
/// preference can never be written from a stale copy of the record, which is
/// the failure mode of "keep the current preferences in a field".
/// </remarks>
internal sealed class SettingsSections
{
    private readonly PresentationPreferencesStore _store;
    private readonly OptionsSection _copy;

    internal SettingsSections(PresentationPreferencesStore store, OptionsSection copy)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _copy = copy ?? throw new ArgumentNullException(nameof(copy));
    }

    /// <summary>The graphics tab: one row per preference.</summary>
    internal VBoxContainer BuildGraphics(PresentationPreferences preferences)
    {
        VBoxContainer section = Section("DisplaySection", _copy.Hint);
        section.AddChild(Toggle("Fullscreen", _copy.Fullscreen, preferences.Fullscreen, pressed =>
        {
            Store(store => store.Load().WithFullscreen(pressed));
            ApplyFullscreen(pressed);
        }));
        section.AddChild(Toggle("ReducedMotion", _copy.ReducedMotion, preferences.ReducedMotion, pressed =>
            Store(store => store.Load().WithReducedMotion(pressed))));
        section.AddChild(Toggle("DenseTables", _copy.DenseTables, preferences.DenseTables, pressed =>
            Store(store => store.Load().WithDenseTables(pressed))));
        return section;
    }

    /// <summary>
    /// The audio tab: a switch and a level per bus. The level stays usable
    /// while the bus is off, because turning a bus back on and finding it at
    /// the level you left it is what a player expects.
    /// </summary>
    internal VBoxContainer BuildAudio(PresentationPreferences preferences)
    {
        OptionsAudioSection copy = _copy.Audio;
        VBoxContainer section = Section("AudioSection", copy.Hint);
        section.AddChild(Toggle("MusicEnabled", copy.Music, preferences.MusicEnabled, pressed =>
            Store(store => store.Load().WithMusicEnabled(pressed))));
        section.AddChild(Level("MusicVolume", copy.MusicVolume, preferences.MusicVolume, level =>
            Store(store => store.Load().WithMusicVolume(level))));
        section.AddChild(new HSeparator());
        section.AddChild(Toggle("EffectsEnabled", copy.Effects, preferences.EffectsEnabled, pressed =>
            Store(store => store.Load().WithEffectsEnabled(pressed))));
        section.AddChild(Level("EffectsVolume", copy.EffectsVolume, preferences.EffectsVolume, level =>
            Store(store => store.Load().WithEffectsVolume(level))));
        return section;
    }

    /// <summary>
    /// The video tab: the master switch first, then one row per moment. The
    /// per-moment rows grey out when the master is off, so the screen never
    /// offers a choice that would not be honoured.
    /// </summary>
    internal VBoxContainer BuildVideo(PresentationPreferences preferences)
    {
        OptionsVideoSection copy = _copy.Video;
        VBoxContainer section = Section("VideoSection", copy.Hint);

        CheckButton intro = Toggle("IntroVideo", copy.Intro, preferences.IntroVideoEnabled, pressed =>
            Store(store => store.Load().WithIntroVideoEnabled(pressed)));
        CheckButton match = Toggle("MatchVideo", copy.MatchIntro, preferences.MatchVideoEnabled, pressed =>
            Store(store => store.Load().WithMatchVideoEnabled(pressed)));
        CheckButton celebrations = Toggle("CelebrationVideo", copy.Celebrations, preferences.CelebrationVideoEnabled, pressed =>
            Store(store => store.Load().WithCelebrationVideoEnabled(pressed)));

        void ApplyMaster(bool enabled)
        {
            intro.Disabled = !enabled;
            match.Disabled = !enabled;
            celebrations.Disabled = !enabled;
        }

        CheckButton master = Toggle("VideoEnabled", copy.Enabled, preferences.VideoEnabled, pressed =>
        {
            Store(store => store.Load().WithVideoEnabled(pressed));
            ApplyMaster(pressed);
        });

        section.AddChild(master);
        section.AddChild(new HSeparator());
        section.AddChild(intro);
        section.AddChild(match);
        section.AddChild(celebrations);
        ApplyMaster(preferences.VideoEnabled);
        return section;
    }

    private static VBoxContainer Section(string name, string hint)
    {
        var section = new VBoxContainer { Name = name };
        section.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        if (!string.IsNullOrWhiteSpace(hint))
        {
            var label = new Label { Name = "Hint", Text = hint, AutowrapMode = TextServer.AutowrapMode.WordSmart };
            label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            label.AddThemeFontSizeOverride("font_size", 12);
            section.AddChild(label);
        }
        return section;
    }

    private static CheckButton Toggle(string name, string text, bool pressed, Action<bool> onToggled)
    {
        var toggle = new CheckButton
        {
            Name = name,
            Text = text,
            ButtonPressed = pressed,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            Alignment = HorizontalAlignment.Left,
        };
        toggle.Toggled += value => onToggled(value);
        return toggle;
    }

    /// <summary>A named level row: caption, slider and the value in figures.</summary>
    private static HBoxContainer Level(string name, string text, int value, Action<int> onChanged)
    {
        var row = new HBoxContainer { Name = name };
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var caption = new Label
        {
            Name = "Caption",
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(180, ManagerDesignTokens.MinimumPointerTarget),
        };
        caption.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        row.AddChild(caption);

        var slider = new HSlider
        {
            Name = "Slider",
            MinValue = PresentationPreferences.MinimumVolume,
            MaxValue = PresentationPreferences.MaximumVolume,
            Step = 5,
            Value = value,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        row.AddChild(slider);

        var figure = new Label
        {
            Name = "Value",
            Text = value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(46, 0),
        };
        figure.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        row.AddChild(figure);

        slider.ValueChanged += amount =>
        {
            int level = (int)amount;
            figure.Text = level.ToString(System.Globalization.CultureInfo.InvariantCulture);
            onChanged(level);
        };
        return row;
    }

    /// <summary>Reads, transforms and persists the preferences in one place.</summary>
    private void Store(Func<PresentationPreferencesStore, PresentationPreferences> change) =>
        _store.Save(change(_store));

    private static void ApplyFullscreen(bool enabled)
    {
        if (DisplayServer.GetName() == "headless")
        {
            return;
        }

        DisplayServer.WindowSetMode(enabled
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
    }
}
