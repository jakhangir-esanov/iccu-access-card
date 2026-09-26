namespace Iccu.Api.Extensions;

internal sealed class CorsOptions
{
    internal const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}

internal static class CorsExtensions
{
    private const string DefaultCorsPolicy = "DefaultPolicy";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        CorsOptions options = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        return services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy(DefaultCorsPolicy, builder =>
            {
                builder
                    .WithOrigins(options.AllowedOrigins)
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        return app.UseCors(DefaultCorsPolicy);
    }
}
