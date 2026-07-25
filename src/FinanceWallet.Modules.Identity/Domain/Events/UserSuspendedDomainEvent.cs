using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserSuspendedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserSuspendedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
