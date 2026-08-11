using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Bridges the engine-neutral content layer (f00010, f00014) to the Godot
/// presentation. Loads the starter-world pack on demand and exposes the
/// normalized entities as immutable projections the UI can consume.
/// </summary>
/// <remarks>
/// ADR 0001: presentation consumes read models, never domain entities.
/// This service is the only place where the presentation layer talks to
/// the content-import pipeline.
/// </remarks>
public sealed class WorldService
{
    private readonly IContentReader _reader;
    private readonly string _contentRoot;
    private readonly object _gate = new();

    private NormalizedContentModel? _clubs;
    private NormalizedContentModel? _people;
    private NormalizedContentModel? _competitions;
    private NormalizedContentModel? _countries;
    private NormalizedContentModel? _rules;

    private WorldService(IContentReader reader, string contentRoot)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        if (string.IsNullOrWhiteSpace(contentRoot))
        {
            throw new ArgumentException("Content root cannot be empty.", nameof(contentRoot));
        }

        _contentRoot = contentRoot;
    }

    /// <summary>
    /// Singleton accessor. Initialized once via <see cref="Initialize"/> from the
    /// Godot bootstrap. Throws if accessed before initialization (test isolation).
    /// </summary>
    public static WorldService? Instance { get; private set; }

    /// <summary>
    /// Initialize the singleton. Idempotent: subsequent calls are no-ops if the
    /// path matches the existing instance, otherwise the second call replaces
    /// the cached models.
    /// </summary>
    public static WorldService Initialize(IContentReader reader, string contentRoot)
    {
        lock (typeof(WorldService))
        {
            if (Instance is not null && Instance._contentRoot == contentRoot)
            {
                return Instance;
            }

            Instance = new WorldService(reader, contentRoot);
            return Instance;
        }
    }

    /// <summary>
    /// Force a reload of the cached models. Used by tests and by the editor's
    /// "reload content" menu (future slice).
    /// </summary>
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        var clubs = await ReadFileAsync("clubs.json", cancellationToken).ConfigureAwait(false);
        var people = await ReadFileAsync("people.json", cancellationToken).ConfigureAwait(false);
        var competitions = await ReadFileAsync("competitions.json", cancellationToken).ConfigureAwait(false);
        var countries = await ReadFileAsync("countries.json", cancellationToken).ConfigureAwait(false);
        var rules = await ReadFileAsync("rules.json", cancellationToken).ConfigureAwait(false);

        EnsureSuccess(clubs, "clubs.json");
        EnsureSuccess(people, "people.json");
        EnsureSuccess(competitions, "competitions.json");
        EnsureSuccess(countries, "countries.json");
        EnsureSuccess(rules, "rules.json");

        lock (_gate)
        {
            _clubs = clubs.Model;
            _people = people.Model;
            _competitions = competitions.Model;
            _countries = countries.Model;
            _rules = rules.Model;
        }
    }

    /// <summary>
    /// Snapshot of the clubs in the world, projected as immutable rows for the UI.
    /// </summary>
    public IReadOnlyList<ClubRow> ListClubs()
    {
        var clubs = RequireModel(ref _clubs, "clubs.json");
        var people = RequireModel(ref _people, "people.json");
        var countries = RequireModel(ref _countries, "countries.json");

        var countryNames = BuildCountryShortNames(countries);
        var playersPerClub = CountPlayers(people);

        return clubs.Entities
            .Where(entity => entity.KindId == "club")
            .Select(entity => ProjectClub(entity, countryNames, playersPerClub))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    /// <summary>
    /// Snapshot of the people who play for the given club.
    /// </summary>
    public IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);

        var people = RequireModel(ref _people, "people.json");

        return people.Entities
            .Where(entity => entity.KindId == "person"
                && BelongsToClub(entity, clubExternalId))
            .Select(ProjectPlayer)
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    /// <summary>
    /// Snapshot of every competition the given club participates in,
    /// including the rival clubs that share the competition.
    /// </summary>
    public IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);

        var competitions = RequireModel(ref _competitions, "competitions.json");
        var clubs = RequireModel(ref _clubs, "clubs.json");

        var clubNames = BuildClubShortNames(clubs);

        return competitions.Entities
            .Where(entity => entity.KindId == "competition"
                && CompetitionIncludesClub(entity, clubExternalId))
            .Select(entity => ProjectCompetition(entity, clubExternalId, clubNames))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    /// <summary>
    /// Look up a club by its external identifier. Returns null if missing.
    /// </summary>
    public ClubRow? FindClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);

        return ListClubs().FirstOrDefault(club =>
            string.Equals(club.ExternalId, clubExternalId, StringComparison.Ordinal));
    }

    private async Task<ContentReadResult> ReadFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string path = Path.Combine(_contentRoot, "data", fileName);
        var request = new ContentReadRequest(
            sourcePath: path,
            providerId: "starter-world",
            expectedSchemaVersion: 1);
        return await _reader.ReadAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static NormalizedContentModel RequireModel(ref NormalizedContentModel? slot, string fileName)
    {
        if (slot is not null)
        {
            return slot;
        }

        throw new InvalidOperationException(
            $"WorldService has not loaded '{fileName}'. Call ReloadAsync() first.");
    }

    private static void EnsureSuccess(ContentReadResult result, string fileName)
    {
        if (!result.IsSuccess)
        {
            var errors = string.Join("; ", result.Diagnostics
                .Where(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error)
                .Select(diagnostic => $"[{diagnostic.Code}] {diagnostic.Message}"));
            throw new InvalidOperationException(
                $"Failed to load {fileName}: {errors}");
        }
    }

    private static bool BelongsToClub(NormalizedContentEntity person, string clubExternalId)
    {
        foreach (var reference in person.References)
        {
            if (string.Equals(reference.RelationId, "club", StringComparison.Ordinal)
                && string.Equals(reference.TargetExternalId, clubExternalId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, string> BuildCountryShortNames(NormalizedContentModel countries)
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entity in countries.Entities)
        {
            if (entity.KindId != "country")
            {
                continue;
            }

            if (entity.Values.TryGetValue("short-name", out var shortName)
                && !string.IsNullOrWhiteSpace(shortName))
            {
                names[entity.ExternalId] = shortName;
            }
            else if (entity.Values.TryGetValue("display-name", out var displayName))
            {
                names[entity.ExternalId] = displayName;
            }
        }

        return names;
    }

    private static Dictionary<string, int> CountPlayers(NormalizedContentModel people)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entity in people.Entities)
        {
            if (entity.KindId != "person")
            {
                continue;
            }

            var occupation = entity.Values.TryGetValue("occupation", out var occ)
                ? occ
                : string.Empty;
            if (!string.Equals(occupation, "player", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var reference in entity.References)
            {
                if (string.Equals(reference.RelationId, "club", StringComparison.Ordinal))
                {
                    counts.TryGetValue(reference.TargetExternalId, out var current);
                    counts[reference.TargetExternalId] = current + 1;
                }
            }
        }

        return counts;
    }

    private static bool CompetitionIncludesClub(NormalizedContentEntity competition, string clubExternalId)
    {
        foreach (var reference in competition.References)
        {
            if (string.Equals(reference.RelationId, "participant", StringComparison.Ordinal)
                && string.Equals(reference.TargetExternalId, clubExternalId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, string> BuildClubShortNames(NormalizedContentModel clubs)
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entity in clubs.Entities)
        {
            if (entity.KindId != "club")
            {
                continue;
            }

            string name = ReadValue(entity, "short-name", string.Empty);
            if (name.Length == 0)
            {
                name = ReadValue(entity, "display-name", entity.ExternalId);
            }

            names[entity.ExternalId] = name;
        }

        return names;
    }

    private static CompetitionRow ProjectCompetition(
        NormalizedContentEntity entity,
        string clubExternalId,
        Dictionary<string, string> clubNames)
    {
        string displayName = ReadValue(entity, "display-name", entity.ExternalId);
        string format = ReadValue(entity, "format", "—");
        string frequency = ReadValue(entity, "edition-frequency", "—");

        var rivals = new List<string>();
        foreach (var reference in entity.References)
        {
            if (!string.Equals(reference.RelationId, "participant", StringComparison.Ordinal))
            {
                continue;
            }

            if (string.Equals(reference.TargetExternalId, clubExternalId, StringComparison.Ordinal))
            {
                continue;
            }

            if (clubNames.TryGetValue(reference.TargetExternalId, out var rivalName))
            {
                rivals.Add(rivalName);
            }
        }

        rivals.Sort(StringComparer.Ordinal);

        return new CompetitionRow(
            ExternalId: entity.ExternalId,
            DisplayName: displayName,
            Format: format,
            Frequency: frequency,
            Rivals: rivals.ToImmutableArray());
    }

    private static ClubRow ProjectClub(
        NormalizedContentEntity entity,
        Dictionary<string, string> countryNames,
        Dictionary<string, int> playersPerClub)
    {
        string displayName = ReadValue(entity, "display-name", entity.ExternalId);
        string shortName = ReadValue(entity, "short-name", displayName);
        string city = ReadValue(entity, "city", "—");
        string foundedYear = ReadValue(entity, "founded-year", "—");
        int prestige = ParseInt(ReadValue(entity, "prestige", "0"));
        int fanbase = ParseInt(ReadValue(entity, "fanbase", "0"));

        string countryShortName = "—";
        foreach (var reference in entity.References)
        {
            if (string.Equals(reference.RelationId, "country", StringComparison.Ordinal)
                && countryNames.TryGetValue(reference.TargetExternalId, out var name))
            {
                countryShortName = name;
                break;
            }
        }

        playersPerClub.TryGetValue(entity.ExternalId, out var squadSize);

        return new ClubRow(
            ExternalId: entity.ExternalId,
            DisplayName: displayName,
            ShortName: shortName,
            City: city,
            CountryShortName: countryShortName,
            FoundedYear: foundedYear,
            Prestige: prestige,
            Fanbase: fanbase,
            SquadSize: squadSize);
    }

    private static PlayerRow ProjectPlayer(NormalizedContentEntity entity)
    {
        var values = entity.Values;
        string displayName = ReadValue(entity, "display-name", entity.ExternalId);
        int technique = ParseInt(ReadValue(entity, "technique", "0"));
        int form = ParseInt(ReadValue(entity, "form", "0"));
        int physical = ParseInt(ReadValue(entity, "physical", "0"));
        int mental = ParseInt(ReadValue(entity, "mental", "0"));
        int birthYear = ParseInt(ReadValue(entity, "birth-year", "0"));
        int overall = (technique + form + physical + mental) / 4;

        return new PlayerRow(
            ExternalId: entity.ExternalId,
            DisplayName: displayName,
            BirthYear: birthYear,
            Technique: technique,
            Form: form,
            Physical: physical,
            Mental: mental,
            Overall: overall);
    }

    private static string ReadValue(NormalizedContentEntity entity, string key, string fallback)
    {
        return entity.Values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}

/// <summary>
/// Immutable projection of a club for the UI layer. Decoupled from any
/// domain entity so the presentation never depends on the engine-neutral
/// assemblies' mutable types.
/// </summary>
public sealed record ClubRow(
    string ExternalId,
    string DisplayName,
    string ShortName,
    string City,
    string CountryShortName,
    string FoundedYear,
    int Prestige,
    int Fanbase,
    int SquadSize);

/// <summary>
/// Immutable projection of a player for the UI layer.
/// </summary>
public sealed record PlayerRow(
    string ExternalId,
    string DisplayName,
    int BirthYear,
    int Technique,
    int Form,
    int Physical,
    int Mental,
    int Overall);

/// <summary>
/// Immutable projection of a competition the player club participates in.
/// </summary>
public sealed record CompetitionRow(
    string ExternalId,
    string DisplayName,
    string Format,
    string Frequency,
    IReadOnlyList<string> Rivals);
