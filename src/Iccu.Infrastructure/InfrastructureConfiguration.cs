namespace Iccu.Infrastructure;

using Npgsql;
using Dapper;
using Hangfire;
using FluentValidation;
using Iccu.Application;
using Iccu.Domain.Users;
using Hangfire.PostgreSql;
using Iccu.Domain.Readers;
using Iccu.Domain.StoredFiles;
using Iccu.Presentation.Common;
using Iccu.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Iccu.Domain.RefreshTokens;
using Iccu.Infrastructure.Users;
using Iccu.Infrastructure.Clock;
using Iccu.Infrastructure.Readers;
using Microsoft.AspNetCore.Builder;
using Iccu.Infrastructure.Database;
using Iccu.Application.Common.Data;
using Microsoft.EntityFrameworkCore;
using Iccu.Application.Common.Clock;
using System.Threading.RateLimiting;
using Iccu.Infrastructure.StoredFiles;
using Iccu.Domain.RegistrationRequests;
using Iccu.Infrastructure.ObjectStorage;
using Iccu.Infrastructure.Notifications;
using Iccu.Infrastructure.RefreshTokens;
using Iccu.Application.Common.Behaviors;
using Iccu.Infrastructure.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using Iccu.Presentation.Common.Endpoints;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Iccu.Infrastructure.BackgroundJobs;
using Iccu.Infrastructure.Authentication;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Storage;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Iccu.Infrastructure.RegistrationRequests;
using Iccu.Application.Abstractions.Notifications;
using Iccu.Application.Abstractions.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class InfrastructureConfiguration
{
    private const long MaxRequestBodyBytes = 10L * 1024 * 1024;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");

        services.AddAuthenticationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddHealthChecks().AddNpgSql(databaseConnectionString);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddMessaging();
        services.AddModuleInfrastructure(configuration, npgsqlDataSource, databaseConnectionString);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddMessaging(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            configuration.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
    }

    private static void AddModuleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        NpgsqlDataSource npgsqlDataSource,
        string databaseConnectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    npgsqlDataSource,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Iccu))
                .UseSnakeCaseNamingConvention());

        services.Configure<ClockOptions>(configuration.GetSection(ClockOptions.SectionName));

        var storageSection = configuration.GetSection(StorageOptions.SectionName);

        if (string.IsNullOrWhiteSpace(storageSection[nameof(StorageOptions.RootPath)]))
        {
            throw new InvalidOperationException(
                $"{StorageOptions.SectionName}:{nameof(StorageOptions.RootPath)} must point at a directory outside the application, so uploaded files survive a redeploy.");
        }

        services.Configure<StorageOptions>(storageSection);

        services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = MaxRequestBodyBytes);
        services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxRequestBodyBytes);

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IReaderRepository, ReaderRepository>();
        services.AddScoped<IRegistrationRequestRepository, RegistrationRequestRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IPasswordHasher, UserPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IFileStore, LocalDiskFileStore>();

        services.AddSignalR();
        services.AddSingleton<IRegistrationNotifier, SignalRRegistrationNotifier>();

        services.AddHostedService<ExpireRegistrationRequestsJobScheduler>();
        services.AddHostedService<DeleteUnusedFilesJobScheduler>();

        services.AddHangfire(hangfireConfiguration =>
            hangfireConfiguration.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(databaseConnectionString)));

        services.AddHangfireServer();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Login, context => FixedWindow(context, 10, TimeSpan.FromMinutes(1)));
            options.AddPolicy(RateLimitPolicies.PublicRegistration, context => FixedWindow(context, 60, TimeSpan.FromMinutes(10)));
            options.AddPolicy(RateLimitPolicies.PublicUpload, context => FixedWindow(context, 120, TimeSpan.FromMinutes(10)));
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    private static RateLimitPartition<string> FixedWindow(HttpContext context, int permitLimit, TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, Window = window });
}
