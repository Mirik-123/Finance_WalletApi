using FinanceWallet.Shared.Abstractions;

namespace FinanceWallet.Shared.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}