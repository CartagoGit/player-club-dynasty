using System.Collections.Immutable;
using System.Text;

namespace PCDynasty.Rules.Compilation;

public enum RuleOpcode
{
    PushConstant,
    ReadValue,
    Add,
    Subtract,
    Multiply,
    Divide,
    GreaterThan,
    LessThan,
    Equal,
    Not,
    Select,
    EmitOutcome,
}

public sealed class RuleInstruction
{
    public RuleInstruction(RuleOpcode opcode, string? operand = null)
    {
        if (!Enum.IsDefined(opcode))
        {
            throw new ArgumentOutOfRangeException(nameof(opcode));
        }

        Opcode = opcode;
        Operand = operand;
    }

    public RuleOpcode Opcode { get; }
    public string? Operand { get; }
}

public sealed class RuleIntermediateRepresentation
{
    private RuleIntermediateRepresentation(
        string programId,
        int version,
        ImmutableArray<RuleInstruction> instructions,
        int maximumObservedStackDepth)
    {
        ProgramId = programId;
        Version = version;
        Instructions = instructions;
        MaximumObservedStackDepth = maximumObservedStackDepth;
    }

    public string ProgramId { get; }
    public int Version { get; }
    public ImmutableArray<RuleInstruction> Instructions { get; }
    public int MaximumObservedStackDepth { get; }

    public static RuleIntermediateRepresentation Compile(
        string programId,
        int version,
        IEnumerable<RuleInstruction> instructions,
        RuleComplexityLimits limits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentNullException.ThrowIfNull(instructions);
        ArgumentNullException.ThrowIfNull(limits);

        RuleInstruction[] copied = instructions.Select(instruction =>
        {
            ArgumentNullException.ThrowIfNull(instruction);
            return instruction;
        }).ToArray();
        if (copied.Length == 0 || copied.Length > limits.MaximumInstructions)
        {
            throw new ArgumentException(
                $"A rule must contain between 1 and {limits.MaximumInstructions} instructions.",
                nameof(instructions));
        }

        int stackDepth = 0;
        int maximumObservedStackDepth = 0;
        bool emitted = false;
        foreach (RuleInstruction instruction in copied)
        {
            ValidateOperand(instruction, limits);
            (int consumed, int produced) = StackEffect(instruction.Opcode);
            if (stackDepth < consumed)
            {
                throw new ArgumentException("The rule instruction stack underflows.", nameof(instructions));
            }

            stackDepth = stackDepth - consumed + produced;
            maximumObservedStackDepth = Math.Max(maximumObservedStackDepth, stackDepth);
            if (maximumObservedStackDepth > limits.MaximumStackDepth)
            {
                throw new ArgumentException(
                    $"The rule exceeds stack depth {limits.MaximumStackDepth}.",
                    nameof(instructions));
            }

            emitted |= instruction.Opcode == RuleOpcode.EmitOutcome;
        }

        if (!emitted || stackDepth != 0)
        {
            throw new ArgumentException(
                "A rule must emit an outcome and leave an empty stack.",
                nameof(instructions));
        }

        return new RuleIntermediateRepresentation(
            NormalizeIdentifier(programId, nameof(programId)),
            version,
            copied.ToImmutableArray(),
            maximumObservedStackDepth);
    }

    private static void ValidateOperand(
        RuleInstruction instruction,
        RuleComplexityLimits limits)
    {
        bool requiresOperand = instruction.Opcode is
            RuleOpcode.PushConstant or RuleOpcode.ReadValue or RuleOpcode.EmitOutcome;
        if (requiresOperand)
        {
            string operand = NormalizeIdentifier(instruction.Operand!, nameof(instruction.Operand));
            if (Encoding.UTF8.GetByteCount(operand) > limits.MaximumOperandUtf8Bytes)
            {
                throw new ArgumentException("A rule operand exceeds the configured byte limit.");
            }
        }
        else if (instruction.Operand is not null)
        {
            throw new ArgumentException($"Opcode '{instruction.Opcode}' does not accept an operand.");
        }
    }

    private static (int Consumed, int Produced) StackEffect(RuleOpcode opcode) => opcode switch
    {
        RuleOpcode.PushConstant or RuleOpcode.ReadValue => (0, 1),
        RuleOpcode.Add or RuleOpcode.Subtract or RuleOpcode.Multiply or RuleOpcode.Divide or
            RuleOpcode.GreaterThan or RuleOpcode.LessThan or RuleOpcode.Equal => (2, 1),
        RuleOpcode.Not => (1, 1),
        RuleOpcode.Select => (3, 1),
        RuleOpcode.EmitOutcome => (1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(opcode)),
    };

    private static string NormalizeIdentifier(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
