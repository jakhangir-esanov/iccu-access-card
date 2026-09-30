namespace Iccu.Application.Dashboard.GetDashboard;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;

public sealed record GetDashboardQuery : IQuery<DashboardResponse>;

internal sealed class GetDashboardQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    private const int TrendDays = 30;
    private const int ExpiringSoonDays = 30;

    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                COUNT(*)::int AS {nameof(DashboardTotals.Total)},
                COUNT(*) FILTER (WHERE expires_on >= @Today)::int AS {nameof(DashboardTotals.Active)},
                COUNT(*) FILTER (WHERE expires_on < @Today)::int AS {nameof(DashboardTotals.Expired)},
                COUNT(*) FILTER (WHERE expires_on BETWEEN @Today AND @SoonDate)::int AS {nameof(DashboardTotals.ExpiringSoon)},
                COUNT(*) FILTER (WHERE created_at >= @TodayStartUtc)::int AS {nameof(DashboardTotals.RegisteredToday)},
                COUNT(*) FILTER (WHERE created_at >= @MonthStartUtc)::int AS {nameof(DashboardTotals.RegisteredThisMonth)}
            FROM iccu.readers
            WHERE deleted_at IS NULL;

            SELECT COUNT(*)
            FROM iccu.registration_requests
            WHERE status = @PendingStatus AND expires_at > @UtcNow;

            SELECT category AS {nameof(CategoryCount.Category)}, COUNT(*)::int AS {nameof(CategoryCount.Count)}
            FROM iccu.readers
            WHERE deleted_at IS NULL
            GROUP BY category
            ORDER BY category;

            SELECT gender AS {nameof(GenderCount.Gender)}, COUNT(*)::int AS {nameof(GenderCount.Count)}
            FROM iccu.readers
            WHERE deleted_at IS NULL
            GROUP BY gender
            ORDER BY gender NULLS LAST;

            SELECT citizenship AS {nameof(CitizenshipCount.Citizenship)}, COUNT(*)::int AS {nameof(CitizenshipCount.Count)}
            FROM iccu.readers
            WHERE deleted_at IS NULL
            GROUP BY citizenship
            ORDER BY citizenship NULLS LAST;

            SELECT
                (created_at AT TIME ZONE @TimeZone)::date AS {nameof(DailyCount.Day)},
                COUNT(*)::int AS {nameof(DailyCount.Count)}
            FROM iccu.readers
            WHERE deleted_at IS NULL AND created_at >= @TrendStartUtc
            GROUP BY 1
            ORDER BY 1;
            """;

        DateOnly today = dateTimeProvider.Today;
        DateOnly trendStart = today.AddDays(1 - TrendDays);

        var parameters = new
        {
            Today = today,
            SoonDate = today.AddDays(ExpiringSoonDays),
            TodayStartUtc = dateTimeProvider.StartOfDayUtc(today),
            MonthStartUtc = dateTimeProvider.StartOfDayUtc(new DateOnly(today.Year, today.Month, 1)),
            TrendStartUtc = dateTimeProvider.StartOfDayUtc(trendStart),
            PendingStatus = (int)RegistrationRequestStatus.Pending,
            dateTimeProvider.UtcNow,
            TimeZone = dateTimeProvider.TimeZoneId
        };

        await using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        DashboardTotals totals = await grid.ReadSingleAsync<DashboardTotals>();
        int pendingRequests = await grid.ReadSingleAsync<int>();
        List<CategoryCount> byCategory = [.. await grid.ReadAsync<CategoryCount>()];
        List<GenderCount> byGender = [.. await grid.ReadAsync<GenderCount>()];
        List<CitizenshipCount> byCitizenship = [.. await grid.ReadAsync<CitizenshipCount>()];
        Dictionary<DateOnly, int> countsByDay = (await grid.ReadAsync<DailyCount>())
            .ToDictionary(daily => daily.Day, daily => daily.Count);

        List<DailyCount> lastDays = [.. Enumerable.Range(0, TrendDays)
            .Select(offset => trendStart.AddDays(offset))
            .Select(day => new DailyCount(day, countsByDay.GetValueOrDefault(day)))];

        return new DashboardResponse(totals, pendingRequests, byCategory, byGender, byCitizenship, lastDays);
    }
}
