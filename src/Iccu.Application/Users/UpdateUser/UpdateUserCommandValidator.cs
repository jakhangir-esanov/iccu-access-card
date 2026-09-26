namespace Iccu.Application.Users.UpdateUser;

using FluentValidation;
using Iccu.Application.Common.Validation;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.Role).IsInEnum();
    }
}
