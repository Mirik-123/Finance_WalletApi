using FinanceWallet.Modules.Wallets.Domain.Enums;
using FinanceWallet.Modules.Wallets.Domain.Events;
using FinanceWallet.Shared.Domain;
using FinanceWallet.Shared.Funds;

namespace FinanceWallet.Modules.Wallets.Domain.Entities;

public class Wallet : AggregateRoot<Guid>
{
    public string UserId { get; private set; } = default!;
    public Money Balance { get; private set; } = default!;
    public Currency Currency { get; private set; }
    public WalletStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Wallet() { }

    public static Wallet Create(string userId, Currency currency)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = Money.Create(0, currency),
            Currency = currency,
            Status = WalletStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        wallet.Raise(new WalletCreatedDomainEvent(wallet.Id, userId));
        return wallet;
    }

    public void Freeze()
    {
        if (Status != WalletStatus.Active)
            throw new InvalidOperationException($"Cannot freeze wallet in {Status} status.");

        Status = WalletStatus.Frozen;
        Raise(new WalletFrozenDomainEvent(Id));
    }

    public void Unfreeze()
    {
        if (Status != WalletStatus.Frozen)
            throw new InvalidOperationException($"Cannot unfreeze wallet in {Status} status.");

        Status = WalletStatus.Active;
        Raise(new WalletUnfrozenDomainEvent(Id));
    }

    public void Close()
    {
        if (Status == WalletStatus.Frozen)
            throw new InvalidOperationException("Cannot close a frozen wallet. Unfreeze it first.");

        if (Status == WalletStatus.Closed)
            throw new InvalidOperationException("Wallet is already closed.");

        Status = WalletStatus.Closed;
        Raise(new WalletClosedDomainEvent(Id));
    }
}
