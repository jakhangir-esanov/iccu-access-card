namespace Iccu.Api.Extensions;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Controllers;

internal sealed class SwaggerOperationIdFilter : IOperationFilter
{
    private readonly Dictionary<string, OperationDescriptor> _operationIds = [];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return;
        }
        if (_operationIds.TryGetValue(descriptor.Id, out var existingDescriptor))
        {
            operation.OperationId = existingDescriptor.ActionName;
            return;
        }

        var controllerName = descriptor.ControllerName;
        var actionName = descriptor.ActionName;

        var duplicateCount = _operationIds.Values.Count(x =>
            !(x.ControllerName != controllerName ||
            !x.ActionName.StartsWith(actionName, StringComparison.Ordinal)));

        var uniqueActionName = duplicateCount > 0
            ? $"{actionName}{duplicateCount + 1}"
            : actionName;

        _operationIds[descriptor.Id] = new OperationDescriptor(controllerName, uniqueActionName);
        operation.OperationId = uniqueActionName;
    }

    private sealed record OperationDescriptor(string ControllerName, string ActionName);
}
