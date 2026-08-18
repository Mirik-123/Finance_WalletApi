using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Wallets.Domain.Events;

public sealed class WalletFrozenDomainEvent : DomainEvent
{
    public Guid WalletId { get; }

    public WalletFrozenDomainEvent(Guid walletId)
    {
        WalletId = walletId;
    }
}
