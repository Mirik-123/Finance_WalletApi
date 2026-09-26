using FinanceWallet.Modules.Identity.Application.Commands;
using FluentValidation;

namespace FinanceWallet.Modules.Identity.Application.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}