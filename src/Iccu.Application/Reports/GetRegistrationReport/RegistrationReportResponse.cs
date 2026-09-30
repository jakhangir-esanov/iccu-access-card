namespace Iccu.Application.Reports.GetRegistrationReport;

using Iccu.Domain.Common.Enums;

public sealed record RegistrationReportResponse(
    DateOnly From,
    DateOnly To,
    ReportGrouping GroupBy,
    int Total,
    IReadOnlyList<PeriodRow> ByPeriod,
    IReadOnlyList<CategoryRow> ByCategory,
    IReadOnlyList<GenderRow> ByGender,
    IReadOnlyList<CitizenshipRow> ByCitizenship,
    IReadOnlyList<UserRow> ByUser);

public sealed record PeriodRow(
    DateOnly Period,
    int Total,
    int Reception,
    int SelfService);

public sealed record CategoryRow(
    ReaderCategory Category,
    int Count);

public sealed record GenderRow(
    Gender? Gender,
    int Count);

public sealed record CitizenshipRow(
    Citizenship? Citizenship,
    int Count);

public sealed record UserRow(
    Guid UserId,
    string FullName,
    int Count);
