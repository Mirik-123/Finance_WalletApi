using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Events;

public sealed class UserRoleChangedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public RoleNames OldRole { get; }
    public RoleNames NewRole { get; }

    public UserRoleChangedDomainEvent(Guid userId, RoleNames oldRole, RoleNames newRole)
    {
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}
