namespace Iccu.UnitTests.Fakes;

using Iccu.Application.Common.Clock;

internal sealed class FakeClock(DateTime? utcNow = null) : IDateTimeProvider
{
    public static readonly DateTime DefaultUtcNow = new(2026, 9, 26, 7, 0, 0, DateTimeKind.Utc);

    private static readonly TimeSpan TashkentOffset = TimeSpan.FromHours(5);

    public DateTime UtcNow { get; set; } = utcNow ?? DefaultUtcNow;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow.Add(TashkentOffset));

    public string TimeZoneId => "Asia/Tashkent";

    public DateTime StartOfDayUtc(DateOnly localDate) =>
        DateTime.SpecifyKind(localDate.ToDateTime(TimeOnly.MinValue) - TashkentOffset, DateTimeKind.Utc);
}
