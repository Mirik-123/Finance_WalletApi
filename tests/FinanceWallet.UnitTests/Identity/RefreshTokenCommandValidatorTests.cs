using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Application.Validators;

namespace FinanceWallet.UnitTests.Identity;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.Validate(new RefreshTokenCommand("some-refresh-token"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyToken_Fails(string? token)
    {
        var result = _validator.Validate(new RefreshTokenCommand(token!));

        Assert.False(result.IsValid);
    }
}