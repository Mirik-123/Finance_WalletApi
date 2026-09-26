using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Results;
using MediatR;

namespace FinanceWallet.Modules.Identity.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResult>>
{
    private readonly IApplicationUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthResultBuilder _authResultBuilder;

    public RegisterUserCommandHandler(
        IApplicationUserRepository users,
        IPasswordHasher passwordHasher,
        AuthResultBuilder authResultBuilder)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _authResultBuilder = authResultBuilder;
    }

    public async Task<Result<AuthResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;

        if (await _users.EmailExistsAsync(email, cancellationToken))
            return Result.Failure<AuthResult>(Error.Conflict(
                "Auth.EmailAlreadyRegistered",
                "An account with this email already exists."));

        var user = ApplicationUser.Create(
            email,
            _passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName);

        await _users.AddAsync(user, cancellationToken);

        var authResult = await _authResultBuilder.BuildAsync(user, cancellationToken: cancellationToken);

        return Result.Success(authResult);
    }
}