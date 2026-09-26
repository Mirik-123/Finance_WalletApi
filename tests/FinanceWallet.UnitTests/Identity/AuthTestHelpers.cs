using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Services;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

internal static class AuthTestHelpers
{
    public static AuthResultBuilder CreateAuthResultBuilder(
        Mock<IRefreshTokenRepository>? refreshTokens = null,
        Mock<IJwtTokenGenerator>? tokenGenerator = null)
    {
        refreshTokens ??= new Mock<IRefreshTokenRepository>();
        tokenGenerator ??= new Mock<IJwtTokenGenerator>();
        tokenGenerator.Setup(t => t.GenerateToken(It.IsAny<ApplicationUser>()))
            .Returns("access-token");

        var settings = Options.Create(new JwtSettings
        {
            Secret = "test-secret-key-that-is-at-least-32-characters-long",
            Issuer = "FinanceWallet",
            Audience = "FinanceWallet.Clients",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        });

        return new AuthResultBuilder(tokenGenerator.Object, refreshTokens.Object, settings);
    }
}