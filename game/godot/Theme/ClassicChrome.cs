using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// Every bordered surface the shell is built from: plates, caption bars,
/// bevels, strips, table rows and command buttons.
/// </summary>
/// <remarks>
/// The look is the interface grammar documented — and made normative — in
/// <c>docs/wiki/pcfutbol/04-gramatica-visual.md</c>: square plates that get
/// their relief from a bevel (a lit top-left edge, a shadowed bottom-right
/// one) rather than from a radius or a blur, a coloured caption bar that names
/// each plate, one colour per functional area, dense rows and a command bar
/// that never moves.
///
/// Screens ask for a named surface here instead of assembling a StyleBoxFlat
/// each time, which is what let three slightly different panel styles drift
/// apart, and every colour resolves through <see cref="ManagerDesignTokens"/>
/// so a palette switch re-skins the whole shell.
/// </remarks>
public static class ClassicChrome
{
    /// <summary>How a command reads: what it does to the player's world.</summary>
    public enum Intent
    {
        /// <summary>Ordinary navigation.</summary>
        Neutral = 0,

        /// <summary>Moves the career forward. The one command that spends time.</summary>
        Advance = 1,

        /// <summary>Discards something. Always visually separated.</summary>
        Destructive = 2,
    }

    /// <summary>Visual state of an interactive surface.</summary>
    public enum State
    {
        Normal = 0,
        Hovered = 1,
        Pressed = 2,
        Disabled = 3,
        Selected = 4,
    }

    /// <summary>
    /// The universal container: a square plate with a frame and a bevel.
    /// </summary>
    public static StyleBoxFlat Plate(int padding = ManagerDesignTokens.Space12) =>
        Bevelled(ManagerDesignTokens.Surface, ManagerDesignTokens.Border, padding);

    /// <summary>A plate that sits on top of another plate: one step lighter.</summary>
    public static StyleBoxFlat RaisedPlate(int padding = ManagerDesignTokens.Space8) =>
        Bevelled(ManagerDesignTokens.SurfaceRaised, ManagerDesignTokens.Border, padding);

    /// <summary>
    /// The coloured strip that names a plate. The colour identifies the area
    /// the plate belongs to; the text on it is chosen for contrast.
    /// </summary>
    public static StyleBoxFlat Caption(ShellArea area) =>
        Caption(ManagerDesignTokens.AreaColor(area));

    /// <summary>The caption strip in an explicit colour.</summary>
    public static StyleBoxFlat Caption(Color color)
    {
        var box = Flat(color);
        box.BorderColor = ManagerDesignTokens.BevelLight;
        box.BorderWidthTop = 1;
        box.BorderWidthLeft = 1;
        box.BorderWidthRight = 1;
        box.ContentMarginLeft = ManagerDesignTokens.Space12;
        box.ContentMarginRight = ManagerDesignTokens.Space12;
        box.ContentMarginTop = ManagerDesignTokens.Space4;
        box.ContentMarginBottom = ManagerDesignTokens.Space4;
        return box;
    }

    /// <summary>The body of a captioned plate: no top border, the caption owns it.</summary>
    public static StyleBoxFlat CaptionBody(int padding = ManagerDesignTokens.Space12)
    {
        var box = Flat(ManagerDesignTokens.Surface);
        box.BorderColor = ManagerDesignTokens.Border;
        box.BorderWidthLeft = 1;
        box.BorderWidthRight = 1;
        box.BorderWidthBottom = 2;
        box.ContentMarginLeft = padding;
        box.ContentMarginRight = padding;
        box.ContentMarginTop = padding;
        box.ContentMarginBottom = padding;
        return box;
    }

    /// <summary>A recessed well for tables and lists: the bevel runs the other way.</summary>
    public static StyleBoxFlat Inset()
    {
        var box = Flat(ManagerDesignTokens.Canvas);
        box.BorderColor = ManagerDesignTokens.BevelShadow;
        box.BorderWidthLeft = 2;
        box.BorderWidthTop = 2;
        box.BorderWidthRight = 1;
        box.BorderWidthBottom = 1;
        box.ContentMarginLeft = ManagerDesignTokens.Space8;
        box.ContentMarginTop = ManagerDesignTokens.Space4;
        box.ContentMarginRight = ManagerDesignTokens.Space8;
        box.ContentMarginBottom = ManagerDesignTokens.Space4;
        return box;
    }

    /// <summary>The band that carries club, date and money above every screen.</summary>
    public static StyleBoxFlat ContextBand()
    {
        var box = Flat(ManagerDesignTokens.Navigation);
        box.BorderColor = ManagerDesignTokens.Accent;
        box.BorderWidthBottom = 2;
        box.ContentMarginLeft = ManagerDesignTokens.Space16;
        box.ContentMarginTop = ManagerDesignTokens.Space8;
        box.ContentMarginRight = ManagerDesignTokens.Space16;
        box.ContentMarginBottom = ManagerDesignTokens.Space8;
        return box;
    }

    /// <summary>Kept for the screens that still call the previous name.</summary>
    public static StyleBoxFlat HeaderBand() => ContextBand();

    /// <summary>The title plate that opens a screen.</summary>
    public static StyleBoxFlat TitlePlate()
    {
        var box = Flat(ManagerDesignTokens.Navigation);
        box.BorderColor = ManagerDesignTokens.Accent;
        box.BorderWidthLeft = 4;
        box.BorderWidthTop = 1;
        box.BorderWidthRight = 1;
        box.BorderWidthBottom = 2;
        box.ContentMarginLeft = ManagerDesignTokens.Space16;
        box.ContentMarginTop = ManagerDesignTokens.Space12;
        box.ContentMarginRight = ManagerDesignTokens.Space16;
        box.ContentMarginBottom = ManagerDesignTokens.Space12;
        return box;
    }

    /// <summary>The strip along the bottom that always shows the last message.</summary>
    public static StyleBoxFlat StatusStrip()
    {
        var box = Flat(ManagerDesignTokens.Canvas);
        box.BorderColor = ManagerDesignTokens.Border;
        box.BorderWidthTop = 1;
        box.ContentMarginLeft = ManagerDesignTokens.Space16;
        box.ContentMarginTop = ManagerDesignTokens.Space4;
        box.ContentMarginRight = ManagerDesignTokens.Space16;
        box.ContentMarginBottom = ManagerDesignTokens.Space4;
        return box;
    }

    /// <summary>The fixed strip that holds the two to four commands of a screen.</summary>
    public static StyleBoxFlat CommandBar()
    {
        var box = Flat(ManagerDesignTokens.Navigation);
        box.BorderColor = ManagerDesignTokens.BevelLight;
        box.BorderWidthTop = 1;
        box.ContentMarginLeft = ManagerDesignTokens.Space16;
        box.ContentMarginTop = ManagerDesignTokens.Space8;
        box.ContentMarginRight = ManagerDesignTokens.Space16;
        box.ContentMarginBottom = ManagerDesignTokens.Space8;
        return box;
    }

    /// <summary>A chunky command: bevelled, in the colour its intent deserves.</summary>
    public static StyleBoxFlat Command(Intent intent, State state)
    {
        Color baseColor = intent switch
        {
            Intent.Advance => ManagerDesignTokens.Mix(ManagerDesignTokens.Positive, ManagerDesignTokens.Navigation, 0.45f),
            Intent.Destructive => ManagerDesignTokens.Mix(ManagerDesignTokens.Negative, ManagerDesignTokens.Navigation, 0.45f),
            _ => ManagerDesignTokens.SurfaceRaised,
        };

        return Interactive(baseColor, ManagerDesignTokens.Border, state, ManagerDesignTokens.Space12);
    }

    /// <summary>A destination in the rail or in the desk grid, tinted by its area.</summary>
    public static StyleBoxFlat AreaEntry(ShellArea area, State state)
    {
        Color tint = ManagerDesignTokens.AreaColor(area);
        Color background = state == State.Selected
            ? ManagerDesignTokens.Mix(ManagerDesignTokens.SurfaceRaised, tint, 0.42f)
            : ManagerDesignTokens.Mix(ManagerDesignTokens.Surface, tint, 0.16f);

        StyleBoxFlat box = Interactive(background, tint, state, ManagerDesignTokens.Space12);
        // The active entry carries the area colour as a thick left edge, the
        // way the classic desk marked the group you were standing in.
        box.BorderWidthLeft = state == State.Selected ? 5 : 2;
        box.BorderColor = tint;
        return box;
    }

    /// <summary>Kept for callers written before areas were colour-coded.</summary>
    public static StyleBoxFlat RailEntry(bool active) =>
        AreaEntry(ShellArea.None, active ? State.Selected : State.Normal);

    /// <summary>The header row of a dense table.</summary>
    public static StyleBoxFlat TableHeader(ShellArea area = ShellArea.None)
    {
        Color tint = ManagerDesignTokens.AreaColor(area);
        var box = Flat(ManagerDesignTokens.Mix(ManagerDesignTokens.Navigation, tint, 0.30f));
        box.BorderColor = ManagerDesignTokens.BevelShadow;
        box.BorderWidthBottom = 2;
        box.ContentMarginLeft = ManagerDesignTokens.Space8;
        box.ContentMarginRight = ManagerDesignTokens.Space8;
        box.ContentMarginTop = ManagerDesignTokens.Space4;
        box.ContentMarginBottom = ManagerDesignTokens.Space4;
        return box;
    }

    /// <summary>A data row: alternating tint, selection wins over parity.</summary>
    public static StyleBoxFlat TableRow(int index, State state)
    {
        Color background = state switch
        {
            State.Selected => ManagerDesignTokens.SurfaceRaised,
            State.Hovered => ManagerDesignTokens.Mix(ManagerDesignTokens.Surface, ManagerDesignTokens.SurfaceRaised, 0.6f),
            _ => index % 2 == 0
                ? ManagerDesignTokens.Surface
                : ManagerDesignTokens.Mix(ManagerDesignTokens.Surface, ManagerDesignTokens.Canvas, 0.35f),
        };

        var box = Flat(background);
        if (state == State.Selected)
        {
            box.BorderColor = ManagerDesignTokens.Accent;
            box.BorderWidthLeft = 3;
            box.BorderWidthTop = 1;
            box.BorderWidthRight = 1;
            box.BorderWidthBottom = 1;
        }
        box.ContentMarginLeft = ManagerDesignTokens.Space8;
        box.ContentMarginRight = ManagerDesignTokens.Space8;
        box.ContentMarginTop = ManagerDesignTokens.Space2;
        box.ContentMarginBottom = ManagerDesignTokens.Space2;
        return box;
    }

    /// <summary>The rotated strip along a screen edge that names the area.</summary>
    public static StyleBoxFlat EdgeRail(ShellArea area)
    {
        Color tint = ManagerDesignTokens.AreaColor(area);
        var box = Flat(ManagerDesignTokens.Mix(ManagerDesignTokens.Navigation, tint, 0.35f));
        box.BorderColor = tint;
        box.BorderWidthRight = 2;
        return box;
    }

    /// <summary>A floating surface: breakdown windows and dialogs.</summary>
    public static StyleBoxFlat Floating()
    {
        var box = Flat(ManagerDesignTokens.Surface);
        box.BorderColor = ManagerDesignTokens.Accent;
        box.BorderWidthLeft = 2;
        box.BorderWidthTop = 2;
        box.BorderWidthRight = 2;
        box.BorderWidthBottom = 2;
        box.ContentMarginLeft = ManagerDesignTokens.Space16;
        box.ContentMarginTop = ManagerDesignTokens.Space12;
        box.ContentMarginRight = ManagerDesignTokens.Space16;
        box.ContentMarginBottom = ManagerDesignTokens.Space12;
        return box;
    }

    /// <summary>Applies the full command styling to a plain <see cref="Button"/>.</summary>
    /// <remarks>
    /// A <see cref="Button"/> is not a container: giving it <see cref="Control"/>
    /// children makes them swallow the click and draw outside the frame. Any
    /// command that needs more than a caption uses
    /// <see cref="SelectableTile"/> instead of a button with children.
    /// </remarks>
    public static Button CommandButton(string name, string text, Intent intent = Intent.Neutral)
    {
        var button = new Button
        {
            Name = name,
            Text = text.ToUpperInvariant(),
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            ClipText = true,
        };
        StyleCommand(button, intent);
        return button;
    }

    /// <summary>Re-applies command styling to an existing button (used after a palette change).</summary>
    public static void StyleCommand(Button button, Intent intent = Intent.Neutral)
    {
        ArgumentNullException.ThrowIfNull(button);
        button.AddThemeStyleboxOverride("normal", Command(intent, State.Normal));
        button.AddThemeStyleboxOverride("hover", Command(intent, State.Hovered));
        button.AddThemeStyleboxOverride("pressed", Command(intent, State.Pressed));
        button.AddThemeStyleboxOverride("disabled", Command(intent, State.Disabled));
        button.AddThemeStyleboxOverride("focus", Focus());
        button.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        button.AddThemeColorOverride("font_hover_color", ManagerDesignTokens.TextPrimary);
        button.AddThemeColorOverride("font_pressed_color", ManagerDesignTokens.TextPrimary);
        button.AddThemeColorOverride("font_disabled_color", ManagerDesignTokens.TextSecondary);
        button.AddThemeFontSizeOverride("font_size", 13);
    }

    /// <summary>The unmistakable keyboard focus rule.</summary>
    public static StyleBoxFlat Focus()
    {
        var box = Flat(new Color(0f, 0f, 0f, 0f));
        box.BorderColor = ManagerDesignTokens.Focus;
        box.BorderWidthLeft = 2;
        box.BorderWidthTop = 2;
        box.BorderWidthRight = 2;
        box.BorderWidthBottom = 2;
        return box;
    }

    /// <summary>The club crest placeholder: initials on a disc.</summary>
    public static Control CrestPlaceholder(string clubName, int size = 34)
    {
        var crest = new PanelContainer
        {
            Name = "Crest",
            CustomMinimumSize = new Vector2(size, size),
        };
        crest.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.SurfaceRaised,
            BorderColor = ManagerDesignTokens.Accent,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = size / 2,
            CornerRadiusTopRight = size / 2,
            CornerRadiusBottomRight = size / 2,
            CornerRadiusBottomLeft = size / 2,
        });

        var initials = new Label
        {
            Name = "Initials",
            Text = Initials(clubName),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        initials.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        initials.AddThemeFontSizeOverride("font_size", 13);
        crest.AddChild(initials);
        return crest;
    }

    private static StyleBoxFlat Interactive(Color background, Color frame, State state, int padding)
    {
        Color fill = state switch
        {
            State.Hovered => ManagerDesignTokens.Mix(background, ManagerDesignTokens.TextPrimary, 0.12f),
            State.Pressed => ManagerDesignTokens.Darken(background, 0.82f),
            State.Disabled => ManagerDesignTokens.Mix(background, ManagerDesignTokens.Canvas, 0.55f),
            State.Selected => ManagerDesignTokens.Mix(background, ManagerDesignTokens.TextPrimary, 0.06f),
            _ => background,
        };

        var box = Flat(fill);
        box.BorderColor = state == State.Disabled
            ? ManagerDesignTokens.Mix(frame, ManagerDesignTokens.Canvas, 0.6f)
            : frame;
        box.BorderWidthLeft = 1;
        box.BorderWidthTop = 1;
        box.BorderWidthRight = 1;
        box.BorderWidthBottom = 1;
        box.ContentMarginLeft = padding;
        box.ContentMarginRight = padding;
        box.ContentMarginTop = ManagerDesignTokens.Space8;
        box.ContentMarginBottom = ManagerDesignTokens.Space8;
        // Pressed inverts the bevel, which is what makes a flat rectangle read
        // as a physical key going down.
        Bevel(box, inverted: state == State.Pressed, muted: state == State.Disabled);
        return box;
    }

    private static StyleBoxFlat Bevelled(Color background, Color frame, int padding)
    {
        var box = Flat(background);
        box.BorderColor = frame;
        box.BorderWidthLeft = 1;
        box.BorderWidthTop = 1;
        box.BorderWidthRight = 1;
        box.BorderWidthBottom = 1;
        box.ContentMarginLeft = padding;
        box.ContentMarginTop = padding;
        box.ContentMarginRight = padding;
        box.ContentMarginBottom = padding;
        Bevel(box, inverted: false, muted: false);
        return box;
    }

    /// <summary>
    /// Godot's StyleBoxFlat carries one border colour, so the bevel is drawn
    /// with the shadow offsets: a lit top-left and a shadowed bottom-right are
    /// approximated by expanding the top-left margin and shrinking the other.
    /// </summary>
    private static void Bevel(StyleBoxFlat box, bool inverted, bool muted)
    {
        if (muted)
        {
            box.ShadowSize = 0;
            return;
        }

        box.ShadowColor = inverted ? ManagerDesignTokens.BevelLight : ManagerDesignTokens.BevelShadow;
        box.ShadowSize = 2;
        box.ShadowOffset = inverted ? new Vector2(-1, -1) : new Vector2(1, 1);
    }

    private static StyleBoxFlat Flat(Color background) =>
        new()
        {
            BgColor = background,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
        };

    private static string Initials(string clubName)
    {
        string[] words = clubName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length switch
        {
            0 => "··",
            1 => words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant(),
            _ => $"{words[0][0]}{words[^1][0]}".ToUpperInvariant(),
        };
    }
}
