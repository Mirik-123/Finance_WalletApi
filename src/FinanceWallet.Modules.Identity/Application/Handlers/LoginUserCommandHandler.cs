using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Results;
using MediatR;

namespace FinanceWallet.Modules.Identity.Application.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResult>>
{
    private readonly IApplicationUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthResultBuilder _authResultBuilder;

    public LoginUserCommandHandler(
        IApplicationUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        AuthResultBuilder authResultBuilder)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _authResultBuilder = authResultBuilder;
    }

    public async Task<Result<AuthResult>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(Email.Create(request.Email).Value, cancellationToken);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            return Result.Failure<AuthResult>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));

        if (user.Status == UserStatus.Locked)
            return Result.Failure<AuthResult>(Error.Forbidden(
                "Auth.AccountLocked",
                "Account is locked."));

        if (user.Status == UserStatus.Suspended)
            return Result.Failure<AuthResult>(Error.Forbidden(
                "Auth.AccountSuspended",
                "Account is suspended."));

        user.RecordLogin();

        var activeToken = await _refreshTokens.GetActiveByUserIdAsync(user.Id, cancellationToken);

        var authResult = await _authResultBuilder.BuildAsync(user, activeToken, cancellationToken);

        return Result.Success(authResult);
    }
}