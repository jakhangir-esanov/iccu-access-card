namespace Iccu.UnitTests.Infrastructure;

using Iccu.Infrastructure.Clock;
using Microsoft.Extensions.Options;

public class DateTimeProviderTests
{
    private readonly DateTimeProvider _provider = new(Options.Create(new ClockOptions { TimeZone = "Asia/Tashkent" }));

    [Fact]
    public void StartOfDayUtc_Should_ReturnTashkentMidnightInUtc()
    {
        DateTime start = _provider.StartOfDayUtc(new DateOnly(2026, 9, 26));

        Assert.Equal(new DateTime(2026, 9, 25, 19, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(DateTimeKind.Utc, start.Kind);
    }

    [Fact]
    public void TimeZoneId_Should_ExposeTheConfiguredZoneForSqlGrouping()
    {
        Assert.Equal("Asia/Tashkent", _provider.TimeZoneId);
    }
}
