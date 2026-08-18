namespace FinanceWallet.Shared.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
