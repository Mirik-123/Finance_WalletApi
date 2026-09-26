namespace FinanceWallet.Modules.Identity.Application.DTOs;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType = "Bearer");