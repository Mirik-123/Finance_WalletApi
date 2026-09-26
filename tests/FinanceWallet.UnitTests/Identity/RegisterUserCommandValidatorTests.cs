using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.Validators;

namespace FinanceWallet.UnitTests.Identity;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Password123!", "John", "Doe"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void InvalidEmail_Fails(string? email)
    {
        var result = _validator.Validate(new RegisterUserCommand(email!, "Password123!"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void EmailTooLong_Fails()
    {
        var longEmail = "a@" + string.Concat(Enumerable.Repeat("d.", 125)) + "coo";

        var result = _validator.Validate(new RegisterUserCommand(longEmail, "Password123!"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")]
    public void InvalidPassword_Fails(string? password)
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", password!));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FirstNameTooLong_Fails()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Password123!", new string('a', 101)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LastNameTooLong_Fails()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Password123!", null, new string('a', 101)));

        Assert.False(result.IsValid);
    }
}