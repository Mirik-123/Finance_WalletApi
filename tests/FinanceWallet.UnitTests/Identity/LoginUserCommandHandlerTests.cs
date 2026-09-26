using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Handlers;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Shared.Results;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class LoginUserCommandHandlerTests
{
    private const string Password = "Password123!";

    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _users.Object,
            _refreshTokens.Object,
            _passwordHasher,
            AuthTestHelpers.CreateAuthResultBuilder(_refreshTokens));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResultAndRecordsLogin()
    {
        var user = ApplicationUser.Create("user@example.com", _passwordHasher.Hash(Password));
        _users.Setup(u => u.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokens.Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(new("user@example.com", Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value!.AccessToken);
        Assert.NotNull(user.LastLoginAt);
        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorized()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _handler.Handle(new("unknown@example.com", Password), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        var user = ApplicationUser.Create("user@example.com", _passwordHasher.Hash(Password));
        _users.Setup(u => u.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new("user@example.com", "WrongPassword!"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_LockedAccount_ReturnsForbidden()
    {
        var user = ApplicationUser.Create("user@example.com", _passwordHasher.Hash(Password));
        user.Lock();
        _users.Setup(u => u.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new("user@example.com", Password), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
        Assert.Equal("Auth.AccountLocked", result.Error.Code);
    }

    [Fact]
    public async Task Handle_SuspendedAccount_ReturnsForbidden()
    {
        var user = ApplicationUser.Create("user@example.com", _passwordHasher.Hash(Password));
        user.Suspend();
        _users.Setup(u => u.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new("user@example.com", Password), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
        Assert.Equal("Auth.AccountSuspended", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ExistingActiveToken_IsRevokedAndReplaced()
    {
        var user = ApplicationUser.Create("user@example.com", _passwordHasher.Hash(Password));
        var activeToken = RefreshToken.Create(user.Id, "old-hash", DateTime.UtcNow.AddDays(1));
        _users.Setup(u => u.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokens.Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);

        var result = await _handler.Handle(new("user@example.com", Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(activeToken.IsRevoked);
        Assert.NotNull(activeToken.ReplacedByTokenId);
        _refreshTokens.Verify(r => r.Update(activeToken), Times.Once);
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string hash, string password) => hash == $"hashed:{password}";
    }
}