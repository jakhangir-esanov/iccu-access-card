namespace Iccu.Application.Users.CreateUser;

using FluentValidation;
using Iccu.Application.Common.Validation;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.Password).StrongPassword();
    }
}
