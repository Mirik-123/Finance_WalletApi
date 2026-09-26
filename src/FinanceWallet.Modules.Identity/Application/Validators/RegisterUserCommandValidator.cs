using FinanceWallet.Modules.Identity.Application.Commands;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using FluentValidation;

namespace FinanceWallet.Modules.Identity.Application.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
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
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .WithMessage("Password cannot be longer than 100 characters.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name cannot be longer than 100 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name cannot be longer than 100 characters.");
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