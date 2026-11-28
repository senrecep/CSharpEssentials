using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Adds the bearer security scheme to <c>components.securitySchemes</c> and a requirement for it to every operation whose
/// endpoint metadata has <see cref="IAuthorizeData"/> and no <see cref="IAllowAnonymous"/> (see <see cref="OpenApiBearerSecurity"/>).
/// </summary>
internal sealed class BearerSecurityTransformer(string schemeName, string bearerFormat, string? description)
    : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        OpenApiBearerSecurity.AddScheme(document, schemeName, bearerFormat, description);
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        OpenApiBearerSecurity.Apply(operation, context.Description.ActionDescriptor.EndpointMetadata, schemeName, context.Document);
        return Task.CompletedTask;
    }
}
