using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Shared.Abstractions;
using FinanceWallet.Shared.Results;

namespace FinanceWallet.Modules.Identity.Application.Commands;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null) : ICommand<Result<AuthResult>>;