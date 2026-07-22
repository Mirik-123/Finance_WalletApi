using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Wallets.Domain.Events;

public sealed class WalletUnfrozenDomainEvent : DomainEvent
{
    public Guid WalletId { get; }

    public WalletUnfrozenDomainEvent(Guid walletId)
    {
        WalletId = walletId;
    }
}
