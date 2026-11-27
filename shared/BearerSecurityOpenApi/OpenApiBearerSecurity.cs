using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Describes bearer token authentication in the Microsoft.OpenApi 2.x model, for both the Swashbuckle and the
/// Microsoft.AspNetCore.OpenApi packages: an HTTP <c>bearer</c> scheme in <c>components.securitySchemes</c> and a
/// requirement for it on every operation whose endpoint metadata has <see cref="IAuthorizeData"/> and no
/// <see cref="IAllowAnonymous"/>.
/// </summary>
internal static class OpenApiBearerSecurity
{
    public static void AddScheme(OpenApiDocument document, string schemeName, string bearerFormat, string? description)
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
    }

    public static void Apply(OpenApiOperation operation, IList<object> endpointMetadata, string schemeName, OpenApiDocument? document)
    {
        if (!endpointMetadata.OfType<IAuthorizeData>().Any() || endpointMetadata.OfType<IAllowAnonymous>().Any())
            return;

        operation.Security ??= [];
        if (operation.Security.Any(requirement => requirement.Keys.Any(scheme => scheme.Reference.Id == schemeName)))
            return;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeName, document)] = [],
        });
    }
}
