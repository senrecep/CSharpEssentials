using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Adds the bearer security scheme to <c>components.securitySchemes</c> and a requirement for it to every operation whose
/// endpoint metadata has <see cref="IAuthorizeData"/> and no <see cref="IAllowAnonymous"/> (see <see cref="OpenApiBearerSecurity"/>).
/// </summary>
internal sealed class BearerSecurityFilter(string schemeName, string bearerFormat, string? description) : IDocumentFilter, IOperationFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) =>
        OpenApiBearerSecurity.AddScheme(swaggerDoc, schemeName, bearerFormat, description);

    public void Apply(OpenApiOperation operation, OperationFilterContext context) =>
        OpenApiBearerSecurity.Apply(operation, context.ApiDescription.ActionDescriptor.EndpointMetadata, schemeName, context.Document);
}
