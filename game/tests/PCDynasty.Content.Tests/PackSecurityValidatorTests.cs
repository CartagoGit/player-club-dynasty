using PCDynasty.Content.Abstractions.Packs;
using PCDynasty.Content.Validation;
using PCDynasty.Rules.Compilation;
using Xunit;

namespace PCDynasty.Content.Tests;

public sealed class PackSecurityValidatorTests
{
    [Fact]
    public void RejectsForbiddenCapabilitiesPathsCodeAndComplexityDeterministically()
    {
        RuleIntermediateRepresentation program = Program();
        var validator = new PackSecurityValidator(new RuleComplexityLimits(
            maximumInstructions: 3,
            maximumStackDepth: 8,
            maximumOperandUtf8Bytes: 64));
        var first = new PackSecurityManifest(
            "pack:unsafe",
            PackTrustLevel.DeclarativeCommunity,
            PackCapability.Data | PackCapability.NativeCode,
            ["scripts/unsafe.dll", "../escape.json", "/absolute/content.json"],
            [program]);
        var second = new PackSecurityManifest(
            "pack:unsafe",
            PackTrustLevel.DeclarativeCommunity,
            PackCapability.Data | PackCapability.NativeCode,
            ["/absolute/content.json", "../escape.json", "scripts/unsafe.dll"],
            [program]);

        PackSecurityValidationResult firstResult = validator.Validate(first);
        PackSecurityValidationResult secondResult = validator.Validate(second);

        Assert.False(firstResult.IsValid);
        Assert.Contains(firstResult.Diagnostics, item => item.Code == "pack.capability.forbidden");
        Assert.Contains(firstResult.Diagnostics, item => item.Code == "pack.path.forbidden");
        Assert.Contains(firstResult.Diagnostics, item => item.Code == "pack.code.forbidden");
        Assert.Contains(firstResult.Diagnostics, item => item.Code == "pack.rules.complexity");
        Assert.Equal(
            firstResult.Diagnostics.Select(item => (item.Code, item.Message)),
            secondResult.Diagnostics.Select(item => (item.Code, item.Message)));
    }

    [Fact]
    public void AllowsExplicitNativeCodeOnlyForAuditedExtensions()
    {
        var manifest = new PackSecurityManifest(
            "pack:audited",
            PackTrustLevel.AuditedEngineExtension,
            PackCapability.Data |
            PackCapability.NativeCode |
            PackCapability.EngineIntegration,
            ["content/data.json", "extensions/PCDynasty.Custom.dll"],
            [Program()]);

        PackSecurityValidationResult result = new PackSecurityValidator(
            RuleComplexityLimits.SafeDefaults).Validate(manifest);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    private static RuleIntermediateRepresentation Program() =>
        RuleIntermediateRepresentation.Compile(
            "rule:winner",
            1,
            [
                new RuleInstruction(RuleOpcode.ReadValue, "score:a"),
                new RuleInstruction(RuleOpcode.ReadValue, "score:b"),
                new RuleInstruction(RuleOpcode.GreaterThan),
                new RuleInstruction(RuleOpcode.EmitOutcome, "winner:a"),
            ],
            RuleComplexityLimits.SafeDefaults);
}
