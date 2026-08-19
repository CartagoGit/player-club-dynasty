using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Plays a full-screen clip for one moment of the game, and gets out of the
/// way the instant the player asks it to.
/// </summary>
/// <remarks>
/// Three rules, all of them lessons rather than preferences:
///
///   1. **A missing clip is not an error.** The pipeline in
///      `game/content/video/video-spec.json` declares the clips we want; a
///      build where none have been approved yet must still boot straight to
///      the menu. So an absent file raises <see cref="Finished"/> on the next
///      frame and nothing is logged as broken.
///   2. **Everything is skippable.** Any key, any click, any pad button ends
///      the clip. A cinematic seen forty times is a wait, not a reward.
///   3. **The player's switch wins.** The moment is checked against
///      <see cref="PresentationPreferences.PlaysVideo"/> before the file is
///      even looked for, so turning video off costs nothing at runtime.
///
/// Godot's <see cref="VideoStreamPlayer"/> reads Ogg Theora and nothing else
/// without an addon, which is why the pipeline transcodes every approved clip
/// to <c>.ogv</c>.
/// </remarks>
public sealed partial class VideoStage : Control
{
    private const string ClipRoot = "res://Video";

    private readonly VideoStreamPlayer _player = new() { Name = "Clip" };
    private readonly Label _hint = new() { Name = "SkipHint" };
    private bool _playing;

    /// <summary>Raised when the clip ends, is skipped, or was never played.</summary>
    [Signal]
    public delegate void FinishedEventHandler();

    public VideoStage()
    {
        Name = "VideoStage";
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsPreset(LayoutPreset.FullRect);

        _player.SetAnchorsPreset(LayoutPreset.FullRect);
        _player.Expand = true;
        _player.MouseFilter = MouseFilterEnum.Ignore;
        _player.Finished += Stop;
        AddChild(_player);

        _hint.Name = "SkipHint";
        _hint.HorizontalAlignment = HorizontalAlignment.Right;
        _hint.VerticalAlignment = VerticalAlignment.Bottom;
        _hint.SetAnchorsPreset(LayoutPreset.FullRect);
        _hint.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        _hint.AddThemeFontSizeOverride("font_size", 12);
        _hint.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_hint);
    }

    /// <summary>The caption telling the player how to skip. Set by the shell, never hard-coded here.</summary>
    public void SetSkipHint(string text) => _hint.Text = text;

    /// <summary>
    /// Plays <paramref name="clip"/> for <paramref name="moment"/> if the
    /// preferences allow it and the file exists. Always ends with
    /// <see cref="Finished"/>, so a caller can await it unconditionally.
    /// </summary>
    /// <param name="clip">Path under <c>res://Video</c>, e.g. <c>Brand/logo_sting</c>.</param>
    public void Play(VideoMoment moment, string clip, PresentationPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        ArgumentException.ThrowIfNullOrWhiteSpace(clip);

        if (!preferences.PlaysVideo(moment))
        {
            CallDeferred(MethodName.RaiseFinished);
            return;
        }

        string path = $"{ClipRoot}/{clip}.ogv";
        if (!ResourceLoader.Exists(path))
        {
            // Expected until the pipeline has approved a clip for this slot.
            CallDeferred(MethodName.RaiseFinished);
            return;
        }

        if (ResourceLoader.Load(path) is not VideoStream stream)
        {
            CallDeferred(MethodName.RaiseFinished);
            return;
        }

        _player.Stream = stream;
        Visible = true;
        _playing = true;
        GrabFocus();
        _player.Play();
    }

    /// <summary>Ends the clip now. Safe to call when nothing is playing.</summary>
    public void Stop()
    {
        if (!_playing)
        {
            return;
        }

        _playing = false;
        _player.Stop();
        Visible = false;
        RaiseFinished();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_playing)
        {
            return;
        }

        if (@event is InputEventKey { Pressed: true }
            or InputEventMouseButton { Pressed: true }
            or InputEventJoypadButton { Pressed: true })
        {
            Stop();
            AcceptEvent();
        }
    }

    private void RaiseFinished() => EmitSignal(SignalName.Finished);
}
