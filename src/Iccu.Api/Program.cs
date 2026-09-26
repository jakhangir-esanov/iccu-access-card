using Serilog;
using Iccu.Api.Extensions;
using Iccu.Api.Middleware;
using Iccu.Infrastructure;
using Iccu.Presentation.Common.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseForwardedHeaders();

app.UseSwaggerDocumentation();

app.UseCorsPolicy();

app.UseLogContext();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.UseJobsDashboard();

app.UseRateLimiter();

app.MapHealthChecks("health");

app.MapEndpoints();

await app.RunAsync();
