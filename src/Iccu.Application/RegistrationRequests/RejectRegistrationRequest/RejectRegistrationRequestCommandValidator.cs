namespace Iccu.Application.RegistrationRequests.RejectRegistrationRequest;

using FluentValidation;

internal sealed class RejectRegistrationRequestCommandValidator : AbstractValidator<RejectRegistrationRequestCommand>
{
    public RejectRegistrationRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
