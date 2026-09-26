namespace Iccu.Infrastructure.Clock;

using Microsoft.Extensions.Options;
using Iccu.Application.Common.Clock;

internal sealed class DateTimeProvider(IOptions<ClockOptions> options) : IDateTimeProvider
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _timeZone));

    public string TimeZoneId => options.Value.TimeZone;

    public DateTime StartOfDayUtc(DateOnly localDate)
    {
        DateTime localMidnight = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, _timeZone);
    }
}
