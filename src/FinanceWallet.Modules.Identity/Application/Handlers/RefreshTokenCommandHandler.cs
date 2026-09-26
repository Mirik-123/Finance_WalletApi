using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Shared.Results;
using MediatR;

namespace FinanceWallet.Modules.Identity.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResult>>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IApplicationUserRepository _users;
    private readonly AuthResultBuilder _authResultBuilder;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IApplicationUserRepository users,
        AuthResultBuilder authResultBuilder)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _authResultBuilder = authResultBuilder;
    }

    public async Task<Result<AuthResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenGenerator.HashToken(request.RefreshToken);
        var storedToken = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            return Result.Failure<AuthResult>(Error.Unauthorized(
                "Auth.InvalidRefreshToken",
                "The refresh token is invalid or expired."));

        var user = await _users.GetByIdAsync(storedToken.UserId, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
            return Result.Failure<AuthResult>(Error.Unauthorized(
                "Auth.InvalidRefreshToken",
                "The refresh token is invalid or expired."));

        var authResult = await _authResultBuilder.BuildAsync(user, storedToken, cancellationToken);

        return Result.Success(authResult);
    }
}