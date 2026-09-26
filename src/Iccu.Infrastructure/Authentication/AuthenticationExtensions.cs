namespace Iccu.Infrastructure.Authentication;

using Iccu.Domain.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Iccu.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

internal static class AuthenticationExtensions
{
    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(
                options => options.SigningKeyBytes.Length >= JwtOptions.MinSigningKeyBytes,
                $"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)} must be at least {JwtOptions.MinSigningKeyBytes} bytes.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.User, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Admin), nameof(UserRole.Receptionist)))
            .AddPolicy(Policies.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Admin)));

        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        return services;
    }
}
