namespace Iccu.Infrastructure.Authentication;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

internal sealed class JwtBearerConfigureOptions(IOptions<JwtOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string HubPathPrefix = "/hubs";
    private const string AccessTokenQueryParameter = "access_token";

    public void Configure(JwtBearerOptions options)
    {
        var jwt = jwtOptions.Value;

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwt.SigningKeyBytes),
            ValidateIssuerSigningKey = true,
            NameClaimType = CustomClaims.Name,
            RoleClaimType = CustomClaims.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents { OnMessageReceived = ReadHubAccessToken };
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    private static Task ReadHubAccessToken(MessageReceivedContext context)
    {
        string? accessToken = context.Request.Query[AccessTokenQueryParameter];

        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(HubPathPrefix))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }
}
