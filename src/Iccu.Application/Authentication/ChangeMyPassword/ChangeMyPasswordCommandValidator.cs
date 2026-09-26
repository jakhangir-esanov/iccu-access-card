namespace Iccu.Application.Authentication.ChangeMyPassword;

using FluentValidation;
using Iccu.Application.Common.Validation;

internal sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword();
    }
}
