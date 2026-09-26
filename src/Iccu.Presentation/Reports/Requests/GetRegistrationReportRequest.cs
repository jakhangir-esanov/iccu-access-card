namespace Iccu.Presentation.Reports.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record GetRegistrationReportRequest
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public ReportGrouping? GroupBy { get; init; }
}
