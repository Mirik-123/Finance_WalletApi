using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Wallets.Domain.Events;

public sealed class WalletClosedDomainEvent : DomainEvent
{
    public Guid WalletId { get; }

    public WalletClosedDomainEvent(Guid walletId)
    {
        WalletId = walletId;
    }
}
