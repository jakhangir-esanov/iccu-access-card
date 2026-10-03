namespace Iccu.Infrastructure.Authentication;

using System.Text;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;

internal sealed class KohaAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<KohaOptions> kohaOptions) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "Basic";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue? header)
            || !string.Equals(header.Scheme, SchemeName, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(header.Parameter))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        byte[] buffer = new byte[header.Parameter.Length];
        if (!Convert.TryFromBase64String(header.Parameter, buffer, out int length))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));
        }

        string credentials = Encoding.UTF8.GetString(buffer, 0, length);
        int separator = credentials.IndexOf(':');
        if (separator < 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));
        }

        string username = credentials[..separator];
        string password = credentials[(separator + 1)..];

        KohaOptions expected = kohaOptions.Value;

        bool usernameMatches = FixedTimeEquals(username, expected.Username);
        bool passwordMatches = FixedTimeEquals(password, expected.Password);

        if (!usernameMatches || !passwordMatches)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, username)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Basic realm=\"iccu\", charset=\"UTF-8\"";

        return base.HandleChallengeAsync(properties);
    }

    private static bool FixedTimeEquals(string actual, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
