using PCDynasty.Content.Abstractions.Packs;
using Xunit;

namespace PCDynasty.Content.Tests;

public sealed class PackCapabilityTests
{
    [Fact]
    public void DeclarativeCommunityPacksCannotExecuteCodeOrUseAmbientAuthority()
    {
        PackCapabilityPolicy policy = PackCapabilityPolicy.For(
            PackTrustLevel.DeclarativeCommunity);

        Assert.True(policy.Allows(PackCapability.Data));
        Assert.True(policy.Allows(PackCapability.DeclarativeRules));
        Assert.True(policy.Allows(PackCapability.Localization));
        Assert.False(policy.Allows(PackCapability.NativeCode));
        Assert.False(policy.Allows(PackCapability.EngineIntegration));
        Assert.False(policy.Allows(PackCapability.FileSystemWrite));
        Assert.False(policy.Allows(PackCapability.NetworkAccess));
    }

    [Fact]
    public void AuditedExtensionsGainOnlyExplicitEngineCapabilities()
    {
        PackCapabilityPolicy policy = PackCapabilityPolicy.For(
            PackTrustLevel.AuditedEngineExtension);

        Assert.True(policy.Allows(PackCapability.Data));
        Assert.True(policy.Allows(PackCapability.NativeCode));
        Assert.True(policy.Allows(PackCapability.EngineIntegration));
        Assert.False(policy.Allows(PackCapability.FileSystemWrite));
        Assert.False(policy.Allows(PackCapability.NetworkAccess));
    }

    [Fact]
    public void CapabilitySetsAreImmutableAndRejectUndefinedFlags()
    {
        PackCapabilityPolicy community = PackCapabilityPolicy.For(
            PackTrustLevel.DeclarativeCommunity);
        PackCapabilityPolicy repeated = PackCapabilityPolicy.For(
            PackTrustLevel.DeclarativeCommunity);

        Assert.Same(community, repeated);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            community.Allows((PackCapability)(1 << 20)));
    }
}
