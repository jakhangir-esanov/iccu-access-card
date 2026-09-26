namespace Iccu.Api.Extensions;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        var enumNames = Enum.GetNames(context.Type)
            .Select(name => new OpenApiString(name));

        schema.Extensions["x-enumNames"] = new OpenApiArray();
        ((OpenApiArray)schema.Extensions["x-enumNames"]).AddRange(enumNames);
    }
}
