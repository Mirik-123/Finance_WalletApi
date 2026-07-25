using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserRegisteredDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public Email Email { get; }

    public UserRegisteredDomainEvent(Guid userId, Email email)
    {
        UserId = userId;
        Email = email;
    }
}
