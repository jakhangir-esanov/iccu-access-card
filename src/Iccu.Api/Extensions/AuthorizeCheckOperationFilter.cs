namespace Iccu.Api.Extensions;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;

internal sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    private static readonly string[] Scopes = ["gateway"];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        var declaringType = methodInfo.DeclaringType;

        var hasAuthorize = declaringType?.IsDefined(typeof(AuthorizeAttribute), inherit: true) == true
                        || methodInfo.IsDefined(typeof(AuthorizeAttribute), inherit: true);

        if (!hasAuthorize)
        {
            return;
        }

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "oauth2"
            }
        };

        operation.Security =
        [
            new()
            {
                [oAuthScheme] = Scopes
            }
        ];
    }
}
