namespace Iccu.Application.Dashboard.GetDashboard;

using Iccu.Domain.Common.Enums;

public sealed record DashboardResponse(
    DashboardTotals Totals,
    int PendingRequests,
    IReadOnlyList<CategoryCount> ByCategory,
    IReadOnlyList<DailyCount> LastDays);

public sealed record DashboardTotals(
    int Total,
    int Active,
    int Expired,
    int ExpiringSoon,
    int RegisteredToday,
    int RegisteredThisMonth);

public sealed record CategoryCount(
    ReaderCategory Category,
    int Count);

public sealed record DailyCount(
    DateOnly Day,
    int Count);
