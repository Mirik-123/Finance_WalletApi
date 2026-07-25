using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserLockedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserLockedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
