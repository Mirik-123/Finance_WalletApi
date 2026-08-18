using System.Reflection;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Shared.Exceptions;

namespace FinanceWallet.UnitTests.Identity;

public class RefreshTokenTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const string TokenHash = "hashed-token";

    [Fact]
    public void Create_SetsAllCoreProperties()
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var token = RefreshToken.Create(UserId, TokenHash, expiresAt);

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(UserId, token.UserId);
        Assert.Equal(TokenHash, token.TokenHash);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenId);
        Assert.Null(token.DeviceName);
        Assert.Null(token.UserAgent);
        Assert.Null(token.IpAddress);
        Assert.True(token.CreatedAt > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void Create_WithMetadata_SetsOptionalProperties()
    {
        var token = RefreshToken.Create(
            UserId, TokenHash, DateTime.UtcNow.AddHours(1),
            deviceName: "iPhone", userAgent: "Mozilla/5.0", ipAddress: "192.168.1.1");

        Assert.Equal("iPhone", token.DeviceName);
        Assert.Equal("Mozilla/5.0", token.UserAgent);
        Assert.Equal("192.168.1.1", token.IpAddress);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Create_WithNonFutureExpiration_Throws(int minutesOffset)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(minutesOffset);

        var ex = Assert.Throws<DomainException>(() => RefreshToken.Create(UserId, TokenHash, expiresAt));
        Assert.Contains("future", ex.Message);
    }

    [Fact]
    public void IsExpired_WhenExpirationInFuture_ReturnsFalse()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));

        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpirationPassed_ReturnsTrue()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.ExpiresAt))!
            .SetValue(token, DateTime.UtcNow.AddMinutes(-1));

        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsActive_ReturnsFalseWhenExpired()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.ExpiresAt))!
            .SetValue(token, DateTime.UtcNow.AddMinutes(-1));

        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_SetsRevokedAtAndDeactivates()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));

        token.Revoke();

        Assert.NotNull(token.RevokedAt);
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_Throws()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));
        token.Revoke();

        var ex = Assert.Throws<DomainException>(() => token.Revoke());
        Assert.Contains("revoked", ex.Message);
    }

    [Fact]
    public void RevokeAndReplace_SetsReplacementAndRevokes()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));
        var replacementId = Guid.NewGuid();

        token.RevokeAndReplace(replacementId);

        Assert.Equal(replacementId, token.ReplacedByTokenId);
        Assert.NotNull(token.RevokedAt);
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RevokeAndReplace_WhenAlreadyRevoked_Throws()
    {
        var token = RefreshToken.Create(UserId, TokenHash, DateTime.UtcNow.AddHours(1));
        token.Revoke();

        Assert.Throws<DomainException>(() => token.RevokeAndReplace(Guid.NewGuid()));
    }
}