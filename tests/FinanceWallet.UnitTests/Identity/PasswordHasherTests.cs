using BCrypt.Net;
using FinanceWallet.Modules.Identity.Infrastructure.Services;

namespace FinanceWallet.UnitTests.Identity;

public class PasswordHasherTests
{
    private const string Password = "SuperSecret123!";

    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsNonEmptyHashDifferentFromPassword()
    {
        var hash = _hasher.Hash(Password);

        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual(Password, hash);
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        var first = _hasher.Hash(Password);
        var second = _hasher.Hash(Password);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash(Password);

        Assert.True(_hasher.Verify(hash, Password));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash(Password);

        Assert.False(_hasher.Verify(hash, "WrongPassword123!"));
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash(Password);

        Assert.False(_hasher.Verify(hash, string.Empty));
    }

    [Fact]
    public void Verify_MalformedHash_Throws()
    {
        Assert.Throws<SaltParseException>(() => _hasher.Verify("not-a-valid-hash", Password));
    }

    [Fact]
    public void Verify_HashOfAnotherPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("OtherPassword456!");

        Assert.False(_hasher.Verify(hash, Password));
    }
}