namespace FinanceWallet.Modules.Identity.Contracts.Responses;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType);