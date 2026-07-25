using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserEmailConfirmedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserEmailConfirmedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
