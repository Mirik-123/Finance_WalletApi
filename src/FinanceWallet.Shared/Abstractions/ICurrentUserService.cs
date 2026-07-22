namespace FinanceWallet.Shared.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }
}
