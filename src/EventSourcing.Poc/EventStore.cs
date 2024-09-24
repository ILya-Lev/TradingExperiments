using NEventStore;

namespace EventSourcing.Poc;

public class EventStore : IDisposable
{
    private readonly IStoreEvents _store;

    public EventStore()
    {
        _store = Wireup
            .Init()
            .UsingInMemoryPersistence()
            .InitializeStorageEngine()
            .Build() ?? throw new Exception($"cannot build {nameof(IStoreEvents)}");
    }

    public void Dispose() => _store.Dispose();

    public void AppendToStream(Guid streamId, Command command)
    {
        using var stream = _store.OpenStream(streamId, 0, int.MaxValue);
        stream.Add(new EventMessage() {Body = command});
        stream.CommitChanges(command.EventId);
    }

    public double GetAmount(Guid streamId)
    {
        var result = 0.0;

        using var stream = _store.OpenStream(streamId, 0, int.MaxValue);
        foreach (dynamic item in stream.CommittedEvents)
        {
            //blindly assume it is there (the structure of commands: property name and type)
            if (item.Body is WithdrawalCommand w)
                result -= w.Amount;
            else if (item.Body is DepositCommand d)
                result += d.Amount;
            else
                throw new Exception($"Unsupported payload type! Expected {nameof(WithdrawalCommand)} and {nameof(DepositCommand)} only.");
        }
        return result;
    }
}

public record Command
{
    public Guid EventId { get; } = Guid.NewGuid();
    public double Amount { get; init; }
}

public record DepositCommand : Command { }
public record WithdrawalCommand : Command { }