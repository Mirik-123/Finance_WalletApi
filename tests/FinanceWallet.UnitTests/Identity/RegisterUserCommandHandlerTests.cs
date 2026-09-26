using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Handlers;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Shared.Results;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly PasswordHasherFake _passwordHasher = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _users.Object,
            _passwordHasher,
            AuthTestHelpers.CreateAuthResultBuilder(_refreshTokens));
    }

    [Fact]
    public async Task Handle_ValidRegistration_ReturnsAuthResult()
    {
        var result = await _handler.Handle(new("user@example.com", "Password123!", "John", "Doe"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value!.AccessToken);
        Assert.False(string.IsNullOrEmpty(result.Value.RefreshToken));
        Assert.Equal(3600, result.Value.ExpiresInSeconds);
        Assert.Equal("Bearer", result.Value.TokenType);
    }

    [Fact]
    public async Task Handle_ValidRegistration_CreatesUserWithHashedPasswordAndMetadata()
    {
        ApplicationUser? created = null;
        _users.Setup(u => u.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationUser, CancellationToken>((user, _) => created = user);

        var result = await _handler.Handle(new("user@example.com", "Password123!", "John", "Doe"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal("user@example.com", created!.Email.Value);
        Assert.Equal("John", created.FirstName);
        Assert.Equal("Doe", created.LastName);
        Assert.NotEqual("Password123!", created.PasswordHash);
        Assert.True(_passwordHasher.Verify(created.PasswordHash, "Password123!"));
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        _users.Setup(u => u.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new("user@example.com", "Password123!"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        _users.Verify(u => u.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsDomainException()
    {
        var ex = await Assert.ThrowsAsync<Shared.Exceptions.DomainException>(
            () => _handler.Handle(new("not-an-email", "Password123!"), CancellationToken.None));

        Assert.Contains("format", ex.Message);
    }

    private sealed class PasswordHasherFake : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string hash, string password) => hash == $"hashed:{password}";
    }
}