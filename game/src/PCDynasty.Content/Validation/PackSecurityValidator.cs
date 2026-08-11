using System.Collections.Immutable;
using System.Text;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Abstractions.Packs;
using PCDynasty.Rules.Compilation;

namespace PCDynasty.Content.Validation;

public sealed class PackSecurityManifest
{
    public PackSecurityManifest(
        string packId,
        PackTrustLevel trustLevel,
        PackCapability requestedCapabilities,
        IEnumerable<string> paths,
        IEnumerable<RuleIntermediateRepresentation> rulePrograms)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(rulePrograms);
        _ = PackCapabilityPolicy.For(trustLevel).Allows(requestedCapabilities);

        PackId = Normalize(packId, nameof(packId));
        TrustLevel = trustLevel;
        RequestedCapabilities = requestedCapabilities;
        Paths = paths.Select(path => Normalize(path, nameof(paths)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        RulePrograms = rulePrograms.Select(program =>
        {
            ArgumentNullException.ThrowIfNull(program);
            return program;
        }).OrderBy(program => program.ProgramId, StringComparer.Ordinal)
          .ThenBy(program => program.Version)
          .ToImmutableArray();
    }

    public string PackId { get; }
    public PackTrustLevel TrustLevel { get; }
    public PackCapability RequestedCapabilities { get; }
    public ImmutableArray<string> Paths { get; }
    public ImmutableArray<RuleIntermediateRepresentation> RulePrograms { get; }

    private static string Normalize(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public sealed class PackSecurityValidationResult
{
    internal PackSecurityValidationResult(ImmutableArray<ContentDiagnostic> diagnostics)
    {
        Diagnostics = diagnostics;
    }

    public ImmutableArray<ContentDiagnostic> Diagnostics { get; }
    public bool IsValid => Diagnostics.All(item => item.Severity != ContentDiagnosticSeverity.Error);
}

public sealed class PackSecurityValidator
{
    private static readonly HashSet<string> BinaryCodeExtensions = new(
        [".dll", ".so", ".dylib"],
        StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ScriptCodeExtensions = new(
        [".cs", ".gd", ".js", ".ts", ".py", ".lua"],
        StringComparer.OrdinalIgnoreCase);

    private readonly RuleComplexityLimits limits;

    public PackSecurityValidator(RuleComplexityLimits limits)
    {
        ArgumentNullException.ThrowIfNull(limits);
        this.limits = limits;
    }

    public PackSecurityValidationResult Validate(PackSecurityManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var diagnostics = ImmutableArray.CreateBuilder<ContentDiagnostic>();
        PackCapabilityPolicy policy = PackCapabilityPolicy.For(manifest.TrustLevel);
        if (!policy.Allows(manifest.RequestedCapabilities))
        {
            Add(
                diagnostics,
                manifest,
                "pack.capability.forbidden",
                $"Trust level '{manifest.TrustLevel}' does not permit all requested capabilities.");
        }

        foreach (string path in manifest.Paths)
        {
            if (IsForbiddenPath(path))
            {
                Add(diagnostics, manifest, "pack.path.forbidden", $"Pack path '{path}' escapes its root.");
            }

            string extension = Path.GetExtension(path);
            bool binaryCode = BinaryCodeExtensions.Contains(extension);
            bool scriptCode = ScriptCodeExtensions.Contains(extension);
            bool binaryAllowed =
                manifest.TrustLevel == PackTrustLevel.AuditedEngineExtension &&
                policy.Allows(PackCapability.NativeCode);
            if (scriptCode || (binaryCode && !binaryAllowed))
            {
                Add(diagnostics, manifest, "pack.code.forbidden", $"Pack code path '{path}' is not permitted.");
            }
        }

        foreach (RuleIntermediateRepresentation program in manifest.RulePrograms)
        {
            bool excessive =
                program.Instructions.Length > limits.MaximumInstructions ||
                program.MaximumObservedStackDepth > limits.MaximumStackDepth ||
                program.Instructions.Any(instruction => instruction.Operand is not null &&
                    Encoding.UTF8.GetByteCount(instruction.Operand) > limits.MaximumOperandUtf8Bytes);
            if (excessive)
            {
                Add(
                    diagnostics,
                    manifest,
                    "pack.rules.complexity",
                    $"Rule '{program.ProgramId}' exceeds pack security complexity limits.");
            }
        }

        return new PackSecurityValidationResult(diagnostics
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToImmutableArray());
    }

    private static bool IsForbiddenPath(string path)
    {
        string portable = path.Replace('\\', '/');
        return Path.IsPathRooted(path) ||
               (portable.Length > 0 && portable[0] == '/') ||
               (portable.Length > 1 && portable[1] == ':') ||
               portable.Split('/', StringSplitOptions.RemoveEmptyEntries)
                   .Any(segment => segment == "..");
    }

    private static void Add(
        ImmutableArray<ContentDiagnostic>.Builder diagnostics,
        PackSecurityManifest manifest,
        string code,
        string message) =>
        diagnostics.Add(new ContentDiagnostic(
            ContentDiagnosticSeverity.Error,
            code,
            message,
            manifest.PackId));
}
