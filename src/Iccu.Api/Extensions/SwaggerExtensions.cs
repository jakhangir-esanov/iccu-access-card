namespace Iccu.Api.Extensions;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

internal sealed class SwaggerOptions
{
    internal const string SectionName = "Swagger";

    public bool Enabled { get; set; }
}

internal static class SwaggerExtensions
{
    private const string DocumentName = "v1";
    private const string DocumentTitle = "ICCU API";

    internal static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        SwaggerOptions options = app.Configuration
            .GetSection(SwaggerOptions.SectionName)
            .Get<SwaggerOptions>() ?? new SwaggerOptions();

        if (!options.Enabled)
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", $"{DocumentTitle} {DocumentName}"));

        return app;
    }

    internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo
            {
                Title = DocumentTitle,
                Version = DocumentName,
                Description = "ICCU API built using the clean architecture."
            });

            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
            options.SchemaFilter<EnumSchemaFilter>();
            options.UseAllOfToExtendReferenceSchemas();
            options.SupportNonNullableReferenceTypes();

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Enter your JWT token below.",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

            var basicSecurityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Koha credentials for the koha/ endpoints.",
                Reference = new OpenApiReference
                {
                    Id = "Basic",
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { basicSecurityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
