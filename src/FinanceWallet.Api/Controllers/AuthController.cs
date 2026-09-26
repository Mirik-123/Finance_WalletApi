using System.Security.Claims;
using FinanceWallet.Api.Extensions;
using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Modules.Identity.Contracts.Requests;
using FinanceWallet.Modules.Identity.Contracts.Responses;
using FinanceWallet.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceWallet.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _sender.Send<Result<AuthResult>>(command, cancellationToken);

        return this.ToActionResult(result, MapToAuthResponse);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);

        var result = await _sender.Send<Result<AuthResult>>(command, cancellationToken);

        return this.ToActionResult(result, MapToAuthResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var result = await _sender.Send<Result<AuthResult>>(command, cancellationToken);

        return this.ToActionResult(result, MapToAuthResponse);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new LogoutUserCommand(userId);

        var result = await _sender.Send<Result>(command, cancellationToken);

        return this.ToActionResult(result);
    }

    private static AuthResponse MapToAuthResponse(AuthResult result) =>
        new(result.AccessToken, result.RefreshToken, result.ExpiresInSeconds, result.TokenType);
}