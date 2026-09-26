using System.Security.Cryptography;
using System.Text;

namespace FinanceWallet.Modules.Identity.Application.Services;

public static class RefreshTokenGenerator
{
    private const int TokenByteLength = 64;

    public static (string Token, string Hash) Generate()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenByteLength)).ToLowerInvariant();
        return (token, HashToken(token));
    }

    public static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}