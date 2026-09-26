using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Shared.Results;
using MediatR;

namespace FinanceWallet.Modules.Identity.Application.Handlers;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutUserCommandHandler(IRefreshTokenRepository refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await _refreshTokens.RevokeAllForUserAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}