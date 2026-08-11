using PCDynasty.Contracts.Commands;
using PCDynasty.Contracts.Events;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class ContractEnvelopeTests
{
    [Fact]
    public void CommandAndEventEnvelopesAreImmutableAndCausallyLinked()
    {
        byte[] commandPayload = [1, 2, 3];
        var command = new CommandEnvelope(
            "command:42",
            "command:advance-day",
            new ContractVersion(1, 2),
            issuedAtTick: 10_000,
            correlationId: "correlation:season-5",
            causationId: null,
            commandPayload);
        commandPayload[0] = 99;
        byte[] eventPayload = [4, 5];
        var fact = new EventEnvelope(
            "event:77",
            "event:day-advanced",
            new ContractVersion(1, 1),
            sequence: 77,
            occurredAtTick: 10_001,
            correlationId: command.CorrelationId,
            causationId: command.CommandId,
            eventPayload);
        eventPayload[0] = 99;

        Assert.Equal(1, command.Payload[0]);
        Assert.Equal(4, fact.Payload[0]);
        Assert.Equal(command.CommandId, fact.CausationId);
        Assert.Equal(command.CorrelationId, fact.CorrelationId);
        Assert.All(
            typeof(CommandEnvelope).GetProperties()
                .Concat(typeof(EventEnvelope).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void ContractVersionsExpressMajorWireCompatibility()
    {
        var first = new ContractVersion(1, 0);
        var compatible = new ContractVersion(1, 9);
        var breaking = new ContractVersion(2, 0);

        Assert.True(first.IsWireCompatibleWith(compatible));
        Assert.True(compatible.IsWireCompatibleWith(first));
        Assert.False(first.IsWireCompatibleWith(breaking));
        Assert.Equal("1.9", compatible.ToString());
    }

    [Fact]
    public void RejectsInvalidVersionsSequencesAndCausalIdentifiers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ContractVersion(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EventEnvelope(
            "event:1",
            "event:test",
            new ContractVersion(1, 0),
            sequence: 0,
            occurredAtTick: 1,
            correlationId: "correlation:1",
            causationId: null,
            payload: []));
        Assert.Throws<ArgumentException>(() => new CommandEnvelope(
            "command:1",
            "command:test",
            new ContractVersion(1, 0),
            issuedAtTick: 1,
            correlationId: " ",
            causationId: null,
            payload: []));
    }
}
