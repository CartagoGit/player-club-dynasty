namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// A reusable three-line title block: an eyebrow tag, a title and a
/// summary line. Most screen sections open with this shape.
/// </summary>
/// <remarks>
/// The eyebrow is the small uppercase tag above a section title
/// (e.g. "PC DYNASTY · DIRECCIÓN DEPORTIVA"). The title is the screen
/// heading. The summary is a one-liner underneath, often with format
/// placeholders the screen fills with the runtime context (club name,
/// row count, etc.).
/// </remarks>
public sealed record HeaderSection(string Eyebrow, string Title, string Summary);
