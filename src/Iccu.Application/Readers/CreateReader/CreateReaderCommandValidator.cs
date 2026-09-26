namespace Iccu.Application.Readers.CreateReader;

using FluentValidation;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Validation;

internal sealed class CreateReaderCommandValidator : AbstractValidator<CreateReaderCommand>
{
    public CreateReaderCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new PersonDetailsValidator(dateTimeProvider));
        RuleFor(x => x.PhotoFileId).NotEmpty();
    }
}
