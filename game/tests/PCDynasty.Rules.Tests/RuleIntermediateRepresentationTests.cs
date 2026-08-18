using PCDynasty.Rules.Compilation;
using Xunit;

namespace PCDynasty.Rules.Tests;

public sealed class RuleIntermediateRepresentationTests
{
    [Fact]
    public void CompilesADeclarativeRuleIntoValidatedImmutableInstructions()
    {
        RuleInstruction[] source =
        [
            new(RuleOpcode.ReadValue, "score:a"),
            new(RuleOpcode.ReadValue, "score:b"),
            new(RuleOpcode.GreaterThan),
            new(RuleOpcode.EmitOutcome, "winner:a"),
        ];

        RuleIntermediateRepresentation program = RuleIntermediateRepresentation.Compile(
            "rule:match-winner",
            version: 2,
            source,
            RuleComplexityLimits.SafeDefaults);
        source[0] = new RuleInstruction(RuleOpcode.PushConstant, "999");

        Assert.Equal("rule:match-winner", program.ProgramId);
        Assert.Equal(2, program.Version);
        Assert.Equal(RuleOpcode.ReadValue, program.Instructions[0].Opcode);
        Assert.Equal(2, program.MaximumObservedStackDepth);
        Assert.DoesNotContain(program.Instructions, instruction =>
            instruction.Opcode.ToString().Contains("Call", StringComparison.OrdinalIgnoreCase) ||
            instruction.Opcode.ToString().Contains("Jump", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RejectsExcessiveInstructionAndOperandComplexity()
    {
        var tight = new RuleComplexityLimits(
            maximumInstructions: 2,
            maximumStackDepth: 2,
            maximumOperandUtf8Bytes: 4);

        Assert.Throws<ArgumentException>(() => RuleIntermediateRepresentation.Compile(
            "rule:too-long",
            1,
            [
                new RuleInstruction(RuleOpcode.PushConstant, "1"),
                new RuleInstruction(RuleOpcode.PushConstant, "2"),
                new RuleInstruction(RuleOpcode.Add),
            ],
            tight));
        Assert.Throws<ArgumentException>(() => RuleIntermediateRepresentation.Compile(
            "rule:operand",
            1,
            [
                new RuleInstruction(RuleOpcode.PushConstant, "12345"),
                new RuleInstruction(RuleOpcode.EmitOutcome, "ok"),
            ],
            tight));
    }

    [Fact]
    public void RejectsInvalidStackProgramsAndUndefinedOpcodes()
    {
        Assert.Throws<ArgumentException>(() => RuleIntermediateRepresentation.Compile(
            "rule:underflow",
            1,
            [
                new RuleInstruction(RuleOpcode.Add),
                new RuleInstruction(RuleOpcode.EmitOutcome, "invalid"),
            ],
            RuleComplexityLimits.SafeDefaults));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RuleInstruction((RuleOpcode)999));
    }
}
