namespace Iccu.Application.Common.Validation;

using FluentValidation;
using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using System.Text.RegularExpressions;
using Iccu.Application.Common.Extensions;

internal sealed partial class PersonDetailsValidator : AbstractValidator<PersonDetails>
{
    public const int NameMaxLength = 100;

    private static readonly DateOnly EarliestBirthDate = new(1900, 1, 1);

    public PersonDetailsValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Category).IsInEnum();

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .Matches(NamePattern());

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .Matches(NamePattern());

        RuleFor(x => x.MiddleName!)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(NameMaxLength)
            .Matches(NamePattern())
            .When(details => !string.IsNullOrWhiteSpace(details.MiddleName));

        RuleFor(x => x.BirthDate)
            .Must(birthDate => birthDate >= EarliestBirthDate && birthDate < dateTimeProvider.Today)
            .WithError(ReaderErrors.InvalidBirthDate);

        RuleFor(x => x.Gender)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.Citizenship)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.Phone)
            .Must(phone => PhoneNumber.Normalize(phone, Citizenship.Uzbekistan) is not null)
            .WithError(ReaderErrors.InvalidPhone)
            .When(details => details.Citizenship != Citizenship.Foreign);

        RuleFor(x => x.Phone)
            .Must(phone => PhoneNumber.Normalize(phone, Citizenship.Foreign) is not null)
            .WithError(ReaderErrors.InvalidInternationalPhone)
            .When(details => details.Citizenship == Citizenship.Foreign);
    }

    [GeneratedRegex(@"^\s*\p{L}[\p{L}\p{M}'ʻʼ‘’` -]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex NamePattern();
}
