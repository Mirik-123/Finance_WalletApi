namespace FinanceWallet.Modules.Identity.Contracts.Requests;

public sealed record LoginRequest(
    string Email,
    string Password);