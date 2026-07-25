using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserProfileUpdatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string? FirstName { get; }
    public string? LastName { get; }

    public UserProfileUpdatedDomainEvent(Guid userId, string? firstName, string? lastName)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
    }
}
