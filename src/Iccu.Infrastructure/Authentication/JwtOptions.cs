namespace Iccu.Infrastructure.Authentication;

using System.Text;

internal sealed class JwtOptions
{
    internal const string SectionName = "Jwt";

    internal const int MinSigningKeyBytes = 32;

    public string Issuer { get; init; } = "iccu-api";

    public string Audience { get; init; } = "iccu-admin";

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;

    public int RefreshTokenDays { get; init; } = 7;

    public byte[] SigningKeyBytes => Encoding.UTF8.GetBytes(SigningKey);
}
