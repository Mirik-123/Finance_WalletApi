using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.DTOs;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceWallet.Modules.Identity.Application.Services;

public class AuthResultBuilder
{
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public AuthResultBuilder(
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenRepository refreshTokens,
        IOptions<JwtSettings> jwtSettings)
    {
        _tokenGenerator = tokenGenerator;
        _refreshTokens = refreshTokens;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResult> BuildAsync(
        ApplicationUser user,
        RefreshToken? tokenToReplace = null,
        CancellationToken cancellationToken = default)
    {
        var (refreshToken, refreshTokenHash) = RefreshTokenGenerator.Generate();

        var newToken = RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenExpirationInDays));

        if (tokenToReplace is not null)
        {
            tokenToReplace.RevokeAndReplace(newToken.Id);
            _refreshTokens.Update(tokenToReplace);
        }

        await _refreshTokens.AddAsync(newToken, cancellationToken);

        return new AuthResult(
            _tokenGenerator.GenerateToken(user),
            refreshToken,
            _jwtSettings.Value.ExpirationInMinutes * 60);
    }
}