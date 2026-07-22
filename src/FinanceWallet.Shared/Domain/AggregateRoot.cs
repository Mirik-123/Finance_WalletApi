using System.Collections.ObjectModel;

namespace FinanceWallet.Shared.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { }

    public IReadOnlyCollection<DomainEvent> DomainEvents =>
        new ReadOnlyCollection<DomainEvent>(_domainEvents);

    protected void Raise(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearEvents() => _domainEvents.Clear();
}
