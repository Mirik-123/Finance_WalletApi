namespace FinanceWallet.Modules.Identity.Contracts.Requests;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName);