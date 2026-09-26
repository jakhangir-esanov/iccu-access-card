namespace Iccu.Application.RegistrationRequests.SubmitRegistration;

using FluentValidation;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Extensions;
using Iccu.Application.Common.Validation;

internal sealed class SubmitRegistrationCommandValidator : AbstractValidator<SubmitRegistrationCommand>
{
    public SubmitRegistrationCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new PersonDetailsValidator(dateTimeProvider));
        RuleFor(x => x.PhotoFileId).NotEmpty();
        RuleFor(x => x.ConsentGiven).Equal(true).WithError(RegistrationRequestErrors.ConsentRequired);
    }
}
