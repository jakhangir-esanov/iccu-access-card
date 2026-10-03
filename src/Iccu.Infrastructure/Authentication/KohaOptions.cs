namespace Iccu.Infrastructure.Authentication;

internal sealed class KohaOptions
{
    internal const string SectionName = "Koha";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
