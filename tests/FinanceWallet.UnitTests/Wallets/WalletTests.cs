using FinanceWallet.Modules.Wallets.Domain.Entities;
using FinanceWallet.Modules.Wallets.Domain.Enums;
using FinanceWallet.Modules.Wallets.Domain.Events;
using FinanceWallet.Shared.Funds;

namespace FinanceWallet.UnitTests.Wallets;

public class WalletTests
{
    private const string UserId = "user-1";

    [Fact]
    public void Create_SetsCorrectInitialState()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);

        Assert.NotEqual(Guid.Empty, wallet.Id);
        Assert.Equal(UserId, wallet.UserId);
        Assert.Equal(WalletStatus.Active, wallet.Status);
        Assert.Equal(0, wallet.Balance.Amount);
        Assert.Equal(Currency.USD, wallet.Currency);
        Assert.True(wallet.CreatedAt > DateTime.UtcNow.AddSeconds(-1));

        var domainEvent = Assert.Single(wallet.DomainEvents);
        Assert.IsType<WalletCreatedDomainEvent>(domainEvent);
        var createdEvent = (WalletCreatedDomainEvent)domainEvent;
        Assert.Equal(wallet.Id, createdEvent.WalletId);
        Assert.Equal(UserId, createdEvent.UserId);
    }

    [Fact]
    public void Freeze_ChangesStatusAndRaisesEvent()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);
        wallet.ClearEvents();

        wallet.Freeze();

        Assert.Equal(WalletStatus.Frozen, wallet.Status);
        var domainEvent = Assert.Single(wallet.DomainEvents);
        Assert.IsType<WalletFrozenDomainEvent>(domainEvent);
        var frozenEvent = (WalletFrozenDomainEvent)domainEvent;
        Assert.Equal(wallet.Id, frozenEvent.WalletId);
    }

    [Fact]
    public void Unfreeze_RestoresActiveStatus()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);
        wallet.Freeze();
        wallet.ClearEvents();

        wallet.Unfreeze();

        Assert.Equal(WalletStatus.Active, wallet.Status);
        var domainEvent = Assert.Single(wallet.DomainEvents);
        Assert.IsType<WalletUnfrozenDomainEvent>(domainEvent);
    }

    [Fact]
    public void Freeze_OnNonActiveWallet_Throws()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);
        wallet.Freeze();

        var ex = Assert.Throws<InvalidOperationException>(() => wallet.Freeze());
        Assert.Contains("Frozen", ex.Message);
    }

    [Fact]
    public void Unfreeze_OnNonFrozenWallet_Throws()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);

        var ex = Assert.Throws<InvalidOperationException>(() => wallet.Unfreeze());
        Assert.Contains("Active", ex.Message);
    }

    [Fact]
    public void Close_OnFrozenWallet_Rejected()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);
        wallet.Freeze();

        var ex = Assert.Throws<InvalidOperationException>(() => wallet.Close());
        Assert.Contains("frozen", ex.Message);
        Assert.Equal(WalletStatus.Frozen, wallet.Status);
    }

    [Fact]
    public void Close_OnAlreadyClosedWallet_Throws()
    {
        var wallet = Wallet.Create(UserId, Currency.USD);
        wallet.Close();

        var ex = Assert.Throws<InvalidOperationException>(() => wallet.Close());
        Assert.Contains("already closed", ex.Message);
    }
}
