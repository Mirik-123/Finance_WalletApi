using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Shared.Abstractions;
using FinanceWallet.Shared.Results;

namespace FinanceWallet.Modules.Identity.Application.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<Result<AuthResult>>;