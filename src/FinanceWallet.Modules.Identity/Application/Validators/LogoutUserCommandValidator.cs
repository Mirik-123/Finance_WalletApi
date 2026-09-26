using FinanceWallet.Modules.Identity.Application.Commands;
using FluentValidation;

namespace FinanceWallet.Modules.Identity.Application.Validators;

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User id is required.");
    }
}