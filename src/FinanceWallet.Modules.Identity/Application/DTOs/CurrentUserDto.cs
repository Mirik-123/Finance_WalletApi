namespace FinanceWallet.Modules.Identity.Application.DTOs;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string Role);