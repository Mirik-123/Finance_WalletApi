using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Domain.Events;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Identity.Domain.Entities;

public class ApplicationUser : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserStatus Status { get; private set; }
    public RoleNames Role { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool EmailConfirmed { get; private set; }

    private ApplicationUser() { }

    public static ApplicationUser Create(string email, string passwordHash, string? firstName = null, string? lastName = null)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            Role = RoleNames.User,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow
        };

        user.Raise(new UserRegisteredDomainEvent(user.Id, user.Email));

        return user;
    }

    public void UpdateProfile(string? firstName, string? lastName)
    {
        if (FirstName != firstName || LastName != lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAt = DateTime.UtcNow;

            Raise(new UserProfileUpdatedDomainEvent(Id, firstName, lastName));
        }
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (PasswordHash == newPasswordHash)
            return;

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserPasswordChangedDomainEvent(Id, UpdatedAt.Value));
    }

    public void ChangeEmail(string newEmail)
    {
        var oldEmail = Email;
        var newEmailValue = Email.Create(newEmail);

        if (oldEmail == newEmailValue)
            return;

        Email = newEmailValue;
        EmailConfirmed = false;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserEmailChangedDomainEvent(Id, oldEmail, newEmailValue));
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            return;

        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserEmailConfirmedDomainEvent(Id));
    }

    public void Suspend()
    {
        if (Status == UserStatus.Suspended)
            return;

        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserSuspendedDomainEvent(Id));
    }

    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserActivatedDomainEvent(Id));
    }

    public void Lock()
    {
        if (Status == UserStatus.Locked)
            return;

        Status = UserStatus.Locked;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserLockedDomainEvent(Id));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserLoggedInDomainEvent(Id, LastLoginAt.Value));
    }

    public void RecordFailedLogin()
    {
        AccessFailedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLockoutEnd(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(RoleNames newRole)
    {
        if (Role == newRole)
            return;

        var oldRole = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserRoleChangedDomainEvent(Id, oldRole, newRole));
    }

    public void SoftDelete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Raise(new UserDeletedDomainEvent(Id));
    }
}
