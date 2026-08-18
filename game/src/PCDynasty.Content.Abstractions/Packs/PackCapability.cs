namespace PCDynasty.Content.Abstractions.Packs;

[Flags]
public enum PackCapability
{
    Data = 1 << 0,
    DeclarativeRules = 1 << 1,
    Localization = 1 << 2,
    MediaAssets = 1 << 3,
    NativeCode = 1 << 4,
    EngineIntegration = 1 << 5,
    FileSystemWrite = 1 << 6,
    NetworkAccess = 1 << 7,
}

public sealed class PackCapabilityPolicy
{
    private const PackCapability AllDefined =
        PackCapability.Data |
        PackCapability.DeclarativeRules |
        PackCapability.Localization |
        PackCapability.MediaAssets |
        PackCapability.NativeCode |
        PackCapability.EngineIntegration |
        PackCapability.FileSystemWrite |
        PackCapability.NetworkAccess;

    private static PackCapabilityPolicy Community { get; } = new(
        PackTrustLevel.DeclarativeCommunity,
        PackCapability.Data |
        PackCapability.DeclarativeRules |
        PackCapability.Localization |
        PackCapability.MediaAssets);

    private static PackCapabilityPolicy AuditedExtension { get; } = new(
        PackTrustLevel.AuditedEngineExtension,
        PackCapability.Data |
        PackCapability.DeclarativeRules |
        PackCapability.Localization |
        PackCapability.MediaAssets |
        PackCapability.NativeCode |
        PackCapability.EngineIntegration);

    private PackCapabilityPolicy(
        PackTrustLevel trustLevel,
        PackCapability allowedCapabilities)
    {
        TrustLevel = trustLevel;
        AllowedCapabilities = allowedCapabilities;
    }

    public PackTrustLevel TrustLevel { get; }
    public PackCapability AllowedCapabilities { get; }

    public static PackCapabilityPolicy For(PackTrustLevel trustLevel) => trustLevel switch
    {
        PackTrustLevel.DeclarativeCommunity => Community,
        PackTrustLevel.AuditedEngineExtension => AuditedExtension,
        _ => throw new ArgumentOutOfRangeException(nameof(trustLevel)),
    };

    public bool Allows(PackCapability capability)
    {
        if (capability == 0 || (capability & ~AllDefined) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capability));
        }

        return (AllowedCapabilities & capability) == capability;
    }
}
