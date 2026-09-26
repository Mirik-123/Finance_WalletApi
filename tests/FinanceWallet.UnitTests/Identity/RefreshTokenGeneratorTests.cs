using FinanceWallet.Modules.Identity.Application.Services;

namespace FinanceWallet.UnitTests.Identity;

public class RefreshTokenGeneratorTests
{
    [Fact]
    public void Generate_ReturnsRandomToken()
    {
        var (first, _) = RefreshTokenGenerator.Generate();
        var (second, _) = RefreshTokenGenerator.Generate();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Generate_ReturnsTokenAndMatchingHash()
    {
        var (token, hash) = RefreshTokenGenerator.Generate();

        Assert.Equal(128, token.Length);
        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, RefreshTokenGenerator.HashToken(token));
    }

    [Fact]
    public void HashToken_IsDeterministic()
    {
        const string token = "some-refresh-token-value";

        Assert.Equal(RefreshTokenGenerator.HashToken(token), RefreshTokenGenerator.HashToken(token));
    }

    [Fact]
    public void HashToken_DifferentTokens_ProduceDifferentHashes()
    {
        var first = RefreshTokenGenerator.HashToken("token-one");
        var second = RefreshTokenGenerator.HashToken("token-two");

        Assert.NotEqual(first, second);
    }
}