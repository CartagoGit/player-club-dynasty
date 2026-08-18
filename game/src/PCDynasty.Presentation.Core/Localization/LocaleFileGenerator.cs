namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Serialises a list of <see cref="LocaleFile"/> records to disk.
/// The generator is the only path that writes
/// <c>res://locale/langs/built/*.json</c>; the checked-in JSON files
/// are generated artefacts that cannot drift from the source of
/// truth because <see cref="GenerateAsync"/> validates every record
/// before writing.
/// </summary>
public sealed class LocaleFileGenerator
{
    private readonly string _folder;
    private readonly bool _dryRun;
    private readonly bool _overwrite;

    public LocaleFileGenerator(string folder, bool dryRun = false, bool overwrite = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        _folder = folder;
        _dryRun = dryRun;
        _overwrite = overwrite;
    }

    /// <summary>
    /// Writes every record to <c>{folder}/{id}.json</c>. Throws when
    /// any record's id does not match <see cref="LocaleFile.Lang"/>,
    /// or when the target file already exists and
    /// <see cref="_overwrite"/> is false. Returns the list of files
    /// the generator would have written, even in dry-run mode.
    /// </summary>
    public IReadOnlyList<string> Generate(IEnumerable<(string Id, LocaleFile File)> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<string> written = new();

        foreach ((string id, LocaleFile file) in records)
        {
            if (!string.Equals(file.Lang, id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Record id '{id}' does not match file.Lang '{file.Lang}'.");
            }

            string path = Path.Combine(_folder, $"{id}.json");
            string json = file.ToJson();

            if (_dryRun)
            {
                if (File.Exists(path) && File.ReadAllText(path) != json)
                {
                    throw new InvalidOperationException(
                        $"Dry-run would change '{path}'. Run the generator to refresh the JSON.");
                }

                written.Add(path);
                continue;
            }

            if (File.Exists(path) && !_overwrite)
            {
                throw new InvalidOperationException($"File '{path}' already exists.");
            }

            Directory.CreateDirectory(_folder);
            File.WriteAllText(path, json);
            written.Add(path);
        }

        return written;
    }
}
