using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>Writes the <c>Idempotency-Key</c> contract of idempotent endpoints (see <see cref="OpenApiIdempotency"/>).</summary>
internal sealed class IdempotencyOperationFilter(IServiceProvider services) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IdempotencyOptions options = services.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>().CurrentValue;
        OpenApiIdempotency.Apply(
            operation,
            context.ApiDescription.HttpMethod,
            context.ApiDescription.ActionDescriptor.EndpointMetadata,
            options,
            $"the endpoint '{context.ApiDescription.ActionDescriptor.DisplayName}'");
    }
}
