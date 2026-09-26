namespace Iccu.Application.Reports.GetRegistrationReport;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;

public sealed record GetRegistrationReportQuery(
    DateOnly From,
    DateOnly To,
    ReportGrouping GroupBy) : IQuery<RegistrationReportResponse>;

internal sealed class GetRegistrationReportQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetRegistrationReportQuery, RegistrationReportResponse>
{
    public async Task<Result<RegistrationReportResponse>> Handle(
        GetRegistrationReportQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                date_trunc(@Unit, r.created_at AT TIME ZONE @TimeZone)::date AS {nameof(PeriodRow.Period)},
                COUNT(*)::int AS {nameof(PeriodRow.Total)},
                COUNT(*) FILTER (WHERE r.source = @ReceptionSource)::int AS {nameof(PeriodRow.Reception)},
                COUNT(*) FILTER (WHERE r.source = @SelfServiceSource)::int AS {nameof(PeriodRow.SelfService)}
            FROM iccu.readers r
            WHERE r.deleted_at IS NULL AND r.created_at >= @FromUtc AND r.created_at < @ToUtc
            GROUP BY 1
            ORDER BY 1;

            SELECT r.category AS {nameof(CategoryRow.Category)}, COUNT(*)::int AS {nameof(CategoryRow.Count)}
            FROM iccu.readers r
            WHERE r.deleted_at IS NULL AND r.created_at >= @FromUtc AND r.created_at < @ToUtc
            GROUP BY r.category
            ORDER BY r.category;

            SELECT
                u.id AS {nameof(UserRow.UserId)},
                u.full_name AS {nameof(UserRow.FullName)},
                COUNT(*)::int AS {nameof(UserRow.Count)}
            FROM iccu.readers r
            JOIN iccu.users u ON u.id = r.created_by
            WHERE r.deleted_at IS NULL AND r.created_at >= @FromUtc AND r.created_at < @ToUtc
            GROUP BY u.id, u.full_name
            ORDER BY COUNT(*) DESC, u.full_name;
            """;

        var parameters = new
        {
            Unit = request.GroupBy == ReportGrouping.Month ? "month" : "day",
            TimeZone = dateTimeProvider.TimeZoneId,
            FromUtc = dateTimeProvider.StartOfDayUtc(request.From),
            ToUtc = dateTimeProvider.StartOfDayUtc(request.To.AddDays(1)),
            ReceptionSource = (int)RegistrationSource.Reception,
            SelfServiceSource = (int)RegistrationSource.SelfService
        };

        await using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        List<PeriodRow> byPeriod = [.. await grid.ReadAsync<PeriodRow>()];
        List<CategoryRow> byCategory = [.. await grid.ReadAsync<CategoryRow>()];
        List<UserRow> byUser = [.. await grid.ReadAsync<UserRow>()];

        return new RegistrationReportResponse(
            request.From,
            request.To,
            request.GroupBy,
            byPeriod.Sum(row => row.Total),
            byPeriod,
            byCategory,
            byUser);
    }
}
