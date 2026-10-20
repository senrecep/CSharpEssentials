using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>Writes the <c>Idempotency-Key</c> contract of idempotent endpoints (see <see cref="OpenApiIdempotency"/>).</summary>
internal sealed class IdempotencyOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        IdempotencyOptions options = context.ApplicationServices.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>().CurrentValue;
        OpenApiIdempotency.Apply(
            operation,
            context.Description.HttpMethod,
            context.Description.ActionDescriptor.EndpointMetadata,
            options,
            $"the endpoint '{context.Description.ActionDescriptor.DisplayName}'");
        return Task.CompletedTask;
    }
}
