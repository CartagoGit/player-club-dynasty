namespace PCDynasty.Presentation.Core;

/// <summary>
/// A stable identifier for a language the shell can render in. The id is the
/// short code the catalog stores; Godot's <c>TranslationServer</c> uses a
/// longer <see cref="LocaleEntry.GodotLocaleCode"/> to look up the resource.
/// </summary>
public readonly record struct LocaleId(string Value)
{
    /// <summary>The Spanish locale shipped with the first slice.</summary>
    public static readonly LocaleId Spanish = new("es");

    /// <summary>The English locale shipped with the first slice.</summary>
    public static readonly LocaleId English = new("en");

    /// <summary>The fallback used when an unknown id is requested.</summary>
    public static LocaleId Default => Spanish;

    public override string ToString() => Value;

    public static implicit operator string(LocaleId id) => id.Value;
}
