using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserActivatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserActivatedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
