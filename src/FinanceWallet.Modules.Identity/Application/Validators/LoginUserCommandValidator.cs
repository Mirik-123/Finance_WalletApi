using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FluentValidation;

namespace FinanceWallet.Modules.Identity.Application.Validators;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(254)
            .WithMessage("Email cannot be longer than 254 characters.")
            .Must(IsValidEmail)
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }

    private static bool IsValidEmail(string? email)
    {
        try
        {
            Email.Create(email!);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}