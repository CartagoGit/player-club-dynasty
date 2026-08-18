using System.Collections.Immutable;

namespace PCDynasty.Contracts.Content;

public sealed class ContentPackContract
{
    public ContentPackContract(string packId, int version, string contentHash)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        PackId = ContractIdentifier.Normalize(packId, nameof(packId));
        Version = version;
        ContentHash = ContractIdentifier.Normalize(contentHash, nameof(contentHash));
    }

    public string PackId { get; }
    public int Version { get; }
    public string ContentHash { get; }
}

public sealed class ContentImportContract
{
    public ContentImportContract(
        ContractVersion contractVersion,
        string importId,
        string providerId,
        int contentSchemaVersion,
        string formatId,
        string sourceHash,
        IEnumerable<ContentPackContract> packs)
    {
        if (contractVersion.Major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(contractVersion));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contentSchemaVersion);
        ArgumentNullException.ThrowIfNull(packs);
        var packIds = new HashSet<string>(StringComparer.Ordinal);
        ContentPackContract[] copiedPacks = packs.Select(pack =>
        {
            ArgumentNullException.ThrowIfNull(pack);
            if (!packIds.Add(pack.PackId))
            {
                throw new ArgumentException("Pack identifiers must be unique.", nameof(packs));
            }

            return pack;
        }).OrderBy(pack => pack.PackId, StringComparer.Ordinal)
          .ToArray();
        if (copiedPacks.Length == 0)
        {
            throw new ArgumentException("An import must identify at least one pack.", nameof(packs));
        }

        ContractVersion = contractVersion;
        ImportId = ContractIdentifier.Normalize(importId, nameof(importId));
        ProviderId = ContractIdentifier.Normalize(providerId, nameof(providerId));
        ContentSchemaVersion = contentSchemaVersion;
        FormatId = ContractIdentifier.Normalize(formatId, nameof(formatId));
        SourceHash = ContractIdentifier.Normalize(sourceHash, nameof(sourceHash));
        Packs = copiedPacks.ToImmutableArray();
    }

    public ContractVersion ContractVersion { get; }
    public string ImportId { get; }
    public string ProviderId { get; }
    public int ContentSchemaVersion { get; }
    public string FormatId { get; }
    public string SourceHash { get; }
    public ImmutableArray<ContentPackContract> Packs { get; }
}
