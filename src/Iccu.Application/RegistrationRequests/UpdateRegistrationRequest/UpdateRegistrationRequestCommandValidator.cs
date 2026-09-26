namespace Iccu.Application.RegistrationRequests.UpdateRegistrationRequest;

using FluentValidation;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Validation;

internal sealed class UpdateRegistrationRequestCommandValidator : AbstractValidator<UpdateRegistrationRequestCommand>
{
    public UpdateRegistrationRequestCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new PersonDetailsValidator(dateTimeProvider));
    }
}
