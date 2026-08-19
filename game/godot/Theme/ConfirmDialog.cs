using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The modal question: a dimmed board, one floating plate with a caption
/// bar, a sentence, and a row of commands.
/// </summary>
/// <remarks>
/// The classic desk never navigated away from a decision without asking.
/// Leaving a career, discarding a lineup, accepting an offer that empties
/// the budget — each one stopped the screen and put the choice in a plate
/// you had to answer.
///
/// Three rules make it a dialog rather than a panel that happens to float:
/// the backdrop eats every click, so nothing behind it can be reached by
/// accident; Escape resolves to the cancel action, so the safe answer is
/// always one key away; and the cancel button takes focus on open, so
/// hitting Enter without reading cannot destroy anything.
///
/// It is deliberately not a Godot <c>AcceptDialog</c>: that draws its own
/// window chrome from the platform theme, which is the one surface we
/// cannot dress in the game's grammar.
/// </remarks>
public sealed partial class ConfirmDialog : Control, IPaletteAware
{
    private readonly ColorRect _backdrop = new() { Name = "Backdrop" };
    private readonly PanelContainer _plate = new() { Name = "Plate" };
    private readonly PanelContainer _captionBar = new() { Name = "Caption" };
    private readonly Label _title = new() { Name = "Title" };
    private readonly Label _message = new() { Name = "Message" };
    private readonly HBoxContainer _commands = new() { Name = "Commands" };
    private readonly ShellArea _area;
    private Button? _cancel;

    public ConfirmDialog(string title, string message, ShellArea area = ShellArea.None)
    {
        Name = "ConfirmDialog";
        _area = area;
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        // The backdrop is what makes this modal: it covers the whole board
        // and stops the pointer, so the screen underneath cannot be clicked
        // while the question is open.
        _backdrop.SetAnchorsPreset(LayoutPreset.FullRect);
        _backdrop.MouseFilter = MouseFilterEnum.Stop;
        AddChild(_backdrop);

        var centre = new CenterContainer { Name = "Centre" };
        centre.SetAnchorsPreset(LayoutPreset.FullRect);
        centre.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(centre);

        _plate.CustomMinimumSize = new Vector2(520, 0);
        centre.AddChild(_plate);

        var stack = new VBoxContainer { Name = "Stack" };
        stack.AddThemeConstantOverride("separation", 0);
        _plate.AddChild(stack);

        _title.Text = title.ToUpperInvariant();
        _title.AddThemeFontSizeOverride("font_size", 13);
        _title.VerticalAlignment = VerticalAlignment.Center;
        _captionBar.CustomMinimumSize = new Vector2(0, ManagerDesignTokens.CaptionHeight);
        _captionBar.AddChild(_title);
        stack.AddChild(_captionBar);

        var body = new MarginContainer { Name = "Body" };
        body.AddThemeConstantOverride("margin_left", ManagerDesignTokens.Space16);
        body.AddThemeConstantOverride("margin_right", ManagerDesignTokens.Space16);
        body.AddThemeConstantOverride("margin_top", ManagerDesignTokens.Space16);
        body.AddThemeConstantOverride("margin_bottom", ManagerDesignTokens.Space12);
        stack.AddChild(body);

        var bodyStack = new VBoxContainer { Name = "BodyStack" };
        bodyStack.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);
        body.AddChild(bodyStack);

        _message.Text = message;
        _message.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _message.AddThemeFontSizeOverride("font_size", 13);
        bodyStack.AddChild(_message);

        _commands.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _commands.Alignment = BoxContainer.AlignmentMode.End;
        bodyStack.AddChild(_commands);

        Repaint();
    }

    /// <summary>
    /// Add one answer. The first command added with <paramref name="isCancel"/>
    /// becomes the safe answer: it takes focus and Escape resolves to it.
    /// </summary>
    public Button AddCommand(
        string name,
        string text,
        Action chosen,
        ClassicChrome.Intent intent = ClassicChrome.Intent.Neutral,
        bool isCancel = false)
    {
        ArgumentNullException.ThrowIfNull(chosen);
        Button button = ClassicChrome.CommandButton(name, text, intent);
        // CommandButton clips its caption so a command bar keeps its shape.
        // An answer must never be half-read ("LEAVE WITHOUT SA"), so here the
        // button grows to its text instead and the plate grows with it.
        button.ClipText = false;
        button.AutowrapMode = TextServer.AutowrapMode.Off;
        button.CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget);
        button.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space8);
        button.Pressed += () =>
        {
            Dismiss();
            chosen();
        };
        _commands.AddChild(button);
        if (isCancel && _cancel is null)
        {
            _cancel = button;
        }

        return button;
    }

    public override void _Ready()
    {
        // Focus lands on the safe answer, never on the destructive one.
        _cancel?.GrabFocus();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Escape })
        {
            AcceptEvent();
            _cancel?.EmitSignal(BaseButton.SignalName.Pressed);
        }
    }

    /// <summary>Remove the dialog. Safe to call twice.</summary>
    public void Dismiss()
    {
        if (IsInsideTree() && !IsQueuedForDeletion())
        {
            QueueFree();
        }
    }

    public void OnPaletteChanged() => Repaint();

    private void Repaint()
    {
        _backdrop.Color = new Color(0f, 0f, 0f, 0.55f);
        _plate.AddThemeStyleboxOverride("panel", ClassicChrome.Floating());
        _captionBar.AddThemeStyleboxOverride("panel", ClassicChrome.Caption(_area));
        _title.AddThemeColorOverride(
            "font_color",
            ManagerDesignTokens.OnArea(ManagerDesignTokens.AreaColor(_area)));
        _message.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        foreach (Node child in _commands.GetChildren())
        {
            if (child is Button button)
            {
                ClassicChrome.StyleCommand(button);
            }
        }
    }
}
