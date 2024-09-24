using FluentAssertions;

namespace EventSourcing.Poc.Tests;

public class EventStoreDemo
{
    [Fact]
    public void Write_Read_CheckBalance()
    {
        using var store = new EventStore();

        var streamId = Guid.NewGuid();

        store.AppendToStream(streamId, new WithdrawalCommand() { Amount = 25 });//after this balance is negative
        store.GetAmount(streamId).Should().Be(-25);

        store.AppendToStream(streamId, new DepositCommand() { Amount = 100 });
        store.GetAmount(streamId).Should().Be(75);

        store.AppendToStream(streamId, new WithdrawalCommand() { Amount = 15 });
        store.GetAmount(streamId).Should().Be(60);
        //too simple... what about concurrency and event versioning/ordering
    }
}