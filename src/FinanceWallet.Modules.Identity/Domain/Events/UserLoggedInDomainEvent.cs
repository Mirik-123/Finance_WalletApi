using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserLoggedInDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public DateTime LoggedInAt { get; }

    public UserLoggedInDomainEvent(Guid userId, DateTime loggedInAt)
    {
        UserId = userId;
        LoggedInAt = loggedInAt;
    }
}
