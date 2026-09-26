using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Shared.Abstractions;
using FinanceWallet.Shared.Results;

namespace FinanceWallet.Modules.Identity.Application.Commands;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<Result<AuthResult>>;