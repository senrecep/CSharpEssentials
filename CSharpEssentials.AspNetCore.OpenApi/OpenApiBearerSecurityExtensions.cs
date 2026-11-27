using CSharpEssentials.AspNetCore.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>Describes bearer token authentication in the documents of Microsoft.AspNetCore.OpenApi.</summary>
public static class OpenApiBearerSecurityExtensions
{
    /// <summary>
    /// Adds an HTTP <c>bearer</c> security scheme to <c>components.securitySchemes</c> and a requirement for it to every
    /// operation whose endpoint metadata has <see cref="Microsoft.AspNetCore.Authorization.IAuthorizeData"/>
    /// (<c>[Authorize]</c>, <c>.RequireAuthorization()</c>) and no
    /// <see cref="Microsoft.AspNetCore.Authorization.IAllowAnonymous"/> (<c>[AllowAnonymous]</c>, <c>.AllowAnonymous()</c>).
    /// Other operations get no requirement. A scheme already registered under <paramref name="schemeName"/> is replaced.
    /// </summary>
    /// <example><code>services.AddOpenApi(options => options.AddBearerSecurityScheme());</code></example>
    /// <param name="options">The options of one OpenAPI document.</param>
    /// <param name="schemeName">The key of the scheme in <c>components.securitySchemes</c>.</param>
    /// <param name="bearerFormat">The <c>bearerFormat</c> of the scheme.</param>
    /// <param name="description">The description of the scheme, or <see langword="null"/> for none.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static OpenApiOptions AddBearerSecurityScheme(
        this OpenApiOptions options,
        string schemeName = "Bearer",
        string bearerFormat = "JWT",
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerFormat);

        var transformer = new BearerSecurityTransformer(schemeName, bearerFormat, description);
        options.AddDocumentTransformer(transformer);
        options.AddOperationTransformer(transformer);
        return options;
    }
}
