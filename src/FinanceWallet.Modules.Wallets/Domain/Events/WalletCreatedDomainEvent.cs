using FinanceWallet.Shared.Domain;

namespace FinanceWallet.Modules.Wallets.Domain.Events;

public sealed class WalletCreatedDomainEvent : DomainEvent
{
    public Guid WalletId { get; }
    public string UserId { get; }

    public WalletCreatedDomainEvent(Guid walletId, string userId)
    {
        WalletId = walletId;
        UserId = userId;
    }
}
