using System.Collections.Generic;
using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Result of a registry scan. <see cref="Locales"/> is the canonical
/// list shown in the language picker; <see cref="Errors"/> are the
/// files the registry saw but could not surface, with a short reason
/// so the user knows why their mod didn't show up.
/// </summary>
public sealed record LocaleRegistryResult(
    ImmutableArray<LocaleMetadata> Locales,
    ImmutableArray<LocaleRegistryError> Errors);

/// <summary>
/// One file the registry skipped during the scan. The id is the file
/// name (without extension) and the reason is a short string the
/// registry panel renders verbatim.
/// </summary>
public sealed record LocaleRegistryError(string Id, string Reason);

/// <summary>
/// Scans <see cref="ILocaleFileSource"/> and produces the list of
/// locales the language picker shows. Built-in locales always win
/// against external mods that ship under the same id; duplicate
/// ids are reported, not silently dropped, so the player can see
/// that a mod was overridden.
/// </summary>
public sealed class LocaleRegistry
{
    private readonly ILocaleFileSource _source;
    private readonly LocaleLoader _loader;

    public LocaleRegistry(ILocaleFileSource source, LocaleLoader loader)
    {
        _source = source;
        _loader = loader;
    }

    /// <summary>
    /// Scans every id the source knows about and returns the metadata
    /// the picker needs. The result is ordered by id so the picker is
    /// stable between sessions.
    /// </summary>
    public LocaleRegistryResult Scan()
    {
        List<LocaleMetadata> builtIn = new();
        List<LocaleMetadata> external = new();
        List<LocaleRegistryError> errors = new();

        foreach (LocaleSourceListing listing in _source.ListIds())
        {
            LocaleMetadata? metadata = _loader.Peek(listing.Id);
            if (metadata is null)
            {
                errors.Add(new LocaleRegistryError(listing.Id, "file is unreadable or missing required metadata"));
                continue;
            }

            if (listing.Source == LocaleSource.BuiltIn)
            {
                builtIn.Add(metadata);
            }
            else
            {
                external.Add(metadata);
            }
        }

        List<LocaleMetadata> resolved = new(builtIn.Count + external.Count);
        resolved.AddRange(builtIn);
        HashSet<string> resolvedIds = new(builtIn.Select(m => m.Id), StringComparer.Ordinal);

        foreach (LocaleMetadata metadata in external)
        {
            if (!resolvedIds.Add(metadata.Id))
            {
                errors.Add(new LocaleRegistryError(metadata.Id, "overridden by the built-in locale with the same id"));
                continue;
            }

            resolved.Add(metadata);
        }

        resolved.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));

        return new LocaleRegistryResult(
            Locales: resolved.ToImmutableArray(),
            Errors: errors.ToImmutableArray());
    }
}
