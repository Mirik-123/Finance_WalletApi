using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.Validators;

namespace FinanceWallet.UnitTests.Identity;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.Validate(new LoginUserCommand("user@example.com", "Password123!"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyEmail_Fails(string? email)
    {
        var result = _validator.Validate(new LoginUserCommand(email!, "Password123!"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void InvalidEmail_Fails()
    {
        var result = _validator.Validate(new LoginUserCommand("not-an-email", "Password123!"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyPassword_Fails(string? password)
    {
        var result = _validator.Validate(new LoginUserCommand("user@example.com", password!));

        Assert.False(result.IsValid);
    }
}