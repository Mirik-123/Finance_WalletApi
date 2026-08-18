using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FinanceWallet.Shared.Exceptions;

namespace FinanceWallet.UnitTests.Identity;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_NormalizesAndTrims()
    {
        var email = Email.Create("  User.NAME@Example.COM  ");

        Assert.Equal("user.name@example.com", email.Value);
    }

    [Fact]
    public void Create_ValidEmail_KeepsValue()
    {
        var email = Email.Create("user@example.com");

        Assert.Equal("user@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrWhitespace_Throws(string? value)
    {
        var ex = Assert.Throws<DomainException>(() => Email.Create(value!));
        Assert.Contains("empty", ex.Message);
    }

    [Fact]
    public void Create_TooLong_Throws()
    {
        var longEmail = "a@" + string.Concat(Enumerable.Repeat("d.", 125)) + "coo";

        Assert.Equal(255, longEmail.Length);

        var ex = Assert.Throws<DomainException>(() => Email.Create(longEmail));
        Assert.Contains("too long", ex.Message);
    }

    [Fact]
    public void Create_MaxLength_Accepts()
    {
        var maxEmail = "a@" + string.Concat(Enumerable.Repeat("d.", 125)) + "co";

        var email = Email.Create(maxEmail);

        Assert.Equal(maxEmail, email.Value);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@example")]
    [InlineData("user@@example.com")]
    [InlineData("user name@example.com")]
    [InlineData("user@-example.com")]
    [InlineData("user@example..com")]
    [InlineData("user@example.c")]
    public void Create_InvalidFormat_Throws(string value)
    {
        var ex = Assert.Throws<DomainException>(() => Email.Create(value));
        Assert.Contains("format", ex.Message);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@sub.example.co.uk")]
    [InlineData("a@b.co")]
    [InlineData("user123@example.io")]
    public void Create_ValidFormats_Accepts(string value)
    {
        var email = Email.Create(value);

        Assert.Equal(value, email.Value);
    }

    [Fact]
    public void Equality_TwoEmailsWithSameNormalizedValue_AreEqual()
    {
        var first = Email.Create("User@Example.com");
        var second = Email.Create("user@example.com");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentEmails_AreNotEqual()
    {
        var first = Email.Create("user@example.com");
        var second = Email.Create("other@example.com");

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var email = Email.Create("user@example.com");

        string value = email;

        Assert.Equal("user@example.com", value);
    }
}