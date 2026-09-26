namespace Iccu.Infrastructure.Clock;

internal sealed class ClockOptions
{
    internal const string SectionName = "Clock";

    public string TimeZone { get; set; } = "Asia/Tashkent";
}
