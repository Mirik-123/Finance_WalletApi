using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Domain.Events;
using FinanceWallet.Shared.Exceptions;

namespace FinanceWallet.UnitTests.Identity;

public class ApplicationUserTests
{
    private const string ValidEmail = "test@example.com";
    private const string PasswordHash = "hashed-password";

    [Fact]
    public void Create_SetsCorrectInitialStateAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(ValidEmail, user.Email.Value);
        Assert.Equal(PasswordHash, user.PasswordHash);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(RoleNames.User, user.Role);
        Assert.True(user.CreatedAt > DateTime.UtcNow.AddSeconds(-1));
        Assert.False(user.IsDeleted);
        Assert.False(user.EmailConfirmed);
        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LastLoginAt);

        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserRegisteredDomainEvent>(domainEvent);
        var registeredEvent = (UserRegisteredDomainEvent)domainEvent;
        Assert.Equal(user.Id, registeredEvent.UserId);
        Assert.Equal(ValidEmail, registeredEvent.Email.Value);
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => ApplicationUser.Create("not-an-email", PasswordHash));
        Assert.Contains("format", ex.Message);
    }

    [Fact]
    public void Create_WithEmptyEmail_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => ApplicationUser.Create("", PasswordHash));
        Assert.Contains("empty", ex.Message);
    }

    [Fact]
    public void Create_NormalizesEmailToLower()
    {
        var user = ApplicationUser.Create("Test@Example.COM", PasswordHash);

        Assert.Equal("test@example.com", user.Email.Value);
    }

    [Fact]
    public void Suspend_ChangesStatusAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.Suspend();

        Assert.Equal(UserStatus.Suspended, user.Status);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserSuspendedDomainEvent>(domainEvent);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.Suspend();
        user.ClearEvents();

        user.Suspend();

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void Lock_ChangesStatusAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.Lock();

        Assert.Equal(UserStatus.Locked, user.Status);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserLockedDomainEvent>(domainEvent);
    }

    [Fact]
    public void Lock_WhenAlreadyLocked_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.Lock();
        user.ClearEvents();

        user.Lock();

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void Activate_RestoresActiveAndClearsLockout()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.Lock();
        user.SetLockoutEnd(DateTime.UtcNow.AddHours(1));
        user.ClearEvents();

        user.Activate();

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Null(user.LockoutEnd);
        Assert.Equal(0, user.AccessFailedCount);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserActivatedDomainEvent>(domainEvent);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.Activate();

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void ChangeEmail_ValidatesAndNormalizes()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();
        var newEmail = "  New@Example.COM  ";

        user.ChangeEmail(newEmail);

        Assert.Equal("new@example.com", user.Email.Value);
        Assert.False(user.EmailConfirmed);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserEmailChangedDomainEvent>(domainEvent);
        var emailEvent = (UserEmailChangedDomainEvent)domainEvent;
        Assert.Equal("test@example.com", emailEvent.OldEmail.Value);
        Assert.Equal("new@example.com", emailEvent.NewEmail.Value);
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ConfirmEmail();
        user.ClearEvents();

        user.ChangeEmail(ValidEmail);

        Assert.True(user.EmailConfirmed);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void ChangeEmail_WithInvalidEmail_Throws()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);

        Assert.Throws<DomainException>(() => user.ChangeEmail("invalid"));
    }

    [Fact]
    public void ChangePasswordHash_UpdatesHashAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();
        var newHash = "new-hashed-password";

        user.ChangePasswordHash(newHash);

        Assert.Equal(newHash, user.PasswordHash);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserPasswordChangedDomainEvent>(domainEvent);
    }

    [Fact]
    public void ChangePasswordHash_WhenSameHash_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.ChangePasswordHash(PasswordHash);

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void ConfirmEmail_RaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.ConfirmEmail();

        Assert.True(user.EmailConfirmed);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserEmailConfirmedDomainEvent>(domainEvent);
    }

    [Fact]
    public void ConfirmEmail_WhenAlreadyConfirmed_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ConfirmEmail();
        user.ClearEvents();

        user.ConfirmEmail();

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void RecordLogin_RaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.RecordLogin();

        Assert.NotNull(user.LastLoginAt);
        Assert.Equal(0, user.AccessFailedCount);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserLoggedInDomainEvent>(domainEvent);
    }

    [Fact]
    public void ChangeRole_UpdatesRoleAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.ChangeRole(RoleNames.Admin);

        Assert.Equal(RoleNames.Admin, user.Role);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserRoleChangedDomainEvent>(domainEvent);
        var roleEvent = (UserRoleChangedDomainEvent)domainEvent;
        Assert.Equal(RoleNames.User, roleEvent.OldRole);
        Assert.Equal(RoleNames.Admin, roleEvent.NewRole);
    }

    [Fact]
    public void SoftDelete_SetsDeletedAndRaisesEvent()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.ClearEvents();

        user.SoftDelete();

        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserDeletedDomainEvent>(domainEvent);
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_DoesNothing()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);
        user.SoftDelete();
        user.ClearEvents();

        user.SoftDelete();

        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void UpdateProfile_DoesNotUpdateWhenValuesSame()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash, "John", "Doe");
        user.ClearEvents();

        user.UpdateProfile("John", "Doe");

        Assert.Empty(user.DomainEvents);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void UpdateProfile_UpdatesWhenValuesDiffer()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash, "John", "Doe");
        user.ClearEvents();

        user.UpdateProfile("Jane", "Smith");

        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.NotNull(user.UpdatedAt);
        var domainEvent = Assert.Single(user.DomainEvents);
        Assert.IsType<UserProfileUpdatedDomainEvent>(domainEvent);
    }

    [Fact]
    public void RecordFailedLogin_IncrementsAccessFailedCount()
    {
        var user = ApplicationUser.Create(ValidEmail, PasswordHash);

        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        Assert.Equal(3, user.AccessFailedCount);
    }
}
