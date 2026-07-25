using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserEmailChangedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public Email OldEmail { get; }
    public Email NewEmail { get; }

    public UserEmailChangedDomainEvent(Guid userId, Email oldEmail, Email newEmail)
    {
        UserId = userId;
        OldEmail = oldEmail;
        NewEmail = newEmail;
    }
}
