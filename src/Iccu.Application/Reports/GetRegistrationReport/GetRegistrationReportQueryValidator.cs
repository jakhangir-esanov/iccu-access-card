namespace Iccu.Application.Reports.GetRegistrationReport;

using FluentValidation;

internal sealed class GetRegistrationReportQueryValidator : AbstractValidator<GetRegistrationReportQuery>
{
    private const int MaxRangeDays = 3 * 366;

    public GetRegistrationReportQueryValidator()
    {
        RuleFor(x => x.GroupBy).IsInEnum();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x)
            .Must(x => x.To.DayNumber - x.From.DayNumber <= MaxRangeDays)
            .WithName(nameof(GetRegistrationReportQuery.To))
            .WithMessage("The report period must not exceed three years.");
    }
}
