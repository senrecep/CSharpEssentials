using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Adds the bearer security scheme to <c>components.securitySchemes</c> and a requirement for it to every operation whose
/// endpoint metadata has <see cref="IAuthorizeData"/> and no <see cref="IAllowAnonymous"/>.
/// </summary>
internal sealed class BearerSecurityTransformer(string schemeName, string bearerFormat, string? description)
    : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[schemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = bearerFormat,
            Description = description,
        };
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
            return Task.CompletedTask;

        operation.Security ??= [];
        if (operation.Security.Any(requirement => requirement.Keys.Any(scheme => scheme.Reference.Id == schemeName)))
            return Task.CompletedTask;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeName, context.Document)] = [],
        });
        return Task.CompletedTask;
    }
}
