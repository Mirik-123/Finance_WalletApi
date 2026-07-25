using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserDeletedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserDeletedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
