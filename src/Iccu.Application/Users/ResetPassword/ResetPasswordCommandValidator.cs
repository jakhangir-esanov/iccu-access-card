namespace Iccu.Application.Users.ResetPassword;

using FluentValidation;
using Iccu.Application.Common.Validation;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword();
    }
}
