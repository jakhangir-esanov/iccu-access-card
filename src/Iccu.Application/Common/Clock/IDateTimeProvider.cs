namespace Iccu.Application.Common.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }

    string TimeZoneId { get; }

    DateTime StartOfDayUtc(DateOnly localDate);
}
