using FinanceWallet.Shared.Abstractions;
using FinanceWallet.Shared.Results;

namespace FinanceWallet.Modules.Identity.Application.Commands;

public sealed record LogoutUserCommand(Guid UserId) : ICommand<Result>;