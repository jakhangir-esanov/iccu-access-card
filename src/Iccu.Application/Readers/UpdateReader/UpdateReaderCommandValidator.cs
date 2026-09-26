namespace Iccu.Application.Readers.UpdateReader;

using FluentValidation;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Validation;

internal sealed class UpdateReaderCommandValidator : AbstractValidator<UpdateReaderCommand>
{
    public UpdateReaderCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new PersonDetailsValidator(dateTimeProvider));
        RuleFor(x => x.PhotoFileId).NotEmpty();
    }
}
