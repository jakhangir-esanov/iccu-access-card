namespace Iccu.UnitTests.Infrastructure;

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Iccu.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

public class KohaAuthenticationTests
{
    private static ServiceProvider BuildProvider(string? kohaPassword = "koha-secret")
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "unit-test-signing-key-that-is-long-enough",
                ["Koha:Username"] = "koha",
                ["Koha:Password"] = kohaPassword
            })
            .Build();

        return new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging()
            .AddAuthenticationInternal()
            .BuildServiceProvider();
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(string? authorization)
    {
        await using ServiceProvider provider = BuildProvider();

        var context = new DefaultHttpContext { RequestServices = provider };
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        return await context.AuthenticateAsync(KohaAuthenticationHandler.SchemeName);
    }

    private static string Basic(string credentials) =>
        $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";

    [Fact]
    public async Task Authenticate_WithTheConfiguredCredentials_Succeeds()
    {
        AuthenticateResult result = await AuthenticateAsync(Basic("koha:koha-secret"));

        Assert.True(result.Succeeded);
        Assert.Equal("koha", result.Principal.Identity?.Name);
    }

    [Theory]
    [InlineData("koha:wrong")]
    [InlineData("other:koha-secret")]
    [InlineData("koha")]
    [InlineData("koha:koha-secret ")]
    public async Task Authenticate_WithWrongCredentials_Fails(string credentials)
    {
        AuthenticateResult result = await AuthenticateAsync(Basic(credentials));

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
    }

    [Fact]
    public async Task Authenticate_WithInvalidBase64_Fails()
    {
        AuthenticateResult result = await AuthenticateAsync("Basic not-base64!");

        Assert.NotNull(result.Failure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Bearer some.jwt.token")]
    public async Task Authenticate_WithoutBasicHeader_ReturnsNoResult(string? authorization)
    {
        AuthenticateResult result = await AuthenticateAsync(authorization);

        Assert.True(result.None);
    }

    [Fact]
    public async Task Challenge_Should_AskForBasicCredentials()
    {
        await using ServiceProvider provider = BuildProvider();
        var context = new DefaultHttpContext { RequestServices = provider };

        await context.ChallengeAsync(KohaAuthenticationHandler.SchemeName);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.StartsWith("Basic realm=", context.Response.Headers.WWWAuthenticate.ToString());
    }

    [Fact]
    public void KohaOptions_WithoutAPassword_FailValidation()
    {
        using ServiceProvider provider = BuildProvider(kohaPassword: null);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<KohaOptions>>().Value);
    }
}
