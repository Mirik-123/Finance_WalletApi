using System.Reflection;
using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Handlers;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Results;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _refreshTokens.Object,
            _users.Object,
            AuthTestHelpers.CreateAuthResultBuilder(_refreshTokens));
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokensAndRotatesOldToken()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();
        var user = ApplicationUser.Create("user@example.com", "hashed-password");
        var stored = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddDays(1));
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new(token), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value!.AccessToken);
        Assert.False(string.IsNullOrEmpty(result.Value.RefreshToken));
        Assert.True(stored.IsRevoked);
        Assert.NotNull(stored.ReplacedByTokenId);
        _refreshTokens.Verify(r => r.Update(stored), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsUnauthorized()
    {
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(new("unknown-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorized()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();
        var user = ApplicationUser.Create("user@example.com", "hashed-password");
        var stored = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddDays(1));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.ExpiresAt))!
            .SetValue(stored, DateTime.UtcNow.AddMinutes(-1));
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var result = await _handler.Handle(new(token), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_RevokedToken_ReturnsUnauthorized()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();
        var user = ApplicationUser.Create("user@example.com", "hashed-password");
        var stored = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddDays(1));
        stored.Revoke();
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var result = await _handler.Handle(new(token), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_TokenForMissingUser_ReturnsUnauthorized()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();
        var user = ApplicationUser.Create("user@example.com", "hashed-password");
        var stored = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddDays(1));
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _handler.Handle(new(token), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_TokenForInactiveUser_ReturnsUnauthorized()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();
        var user = ApplicationUser.Create("user@example.com", "hashed-password");
        user.Suspend();
        var stored = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddDays(1));
        _refreshTokens.Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new(token), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }
}