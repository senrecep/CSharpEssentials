using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>Describes bearer token authentication in Swashbuckle documents.</summary>
public static class SwaggerBearerSecurityExtensions
{
    /// <summary>
    /// Adds an HTTP <c>bearer</c> security scheme to <c>components.securitySchemes</c> and a requirement for it to every
    /// operation whose endpoint metadata has <see cref="Microsoft.AspNetCore.Authorization.IAuthorizeData"/>
    /// (<c>[Authorize]</c>, <c>.RequireAuthorization()</c>) and no
    /// <see cref="Microsoft.AspNetCore.Authorization.IAllowAnonymous"/> (<c>[AllowAnonymous]</c>, <c>.AllowAnonymous()</c>).
    /// Other operations get no requirement. A scheme already registered under <paramref name="schemeName"/> is replaced.
    /// The document matches the one <c>OpenApiOptions.AddBearerSecurityScheme</c> of CSharpEssentials.AspNetCore.OpenApi
    /// writes. Do not combine it with <c>AddSwagger(securitySchemeName, securityScheme)</c>, whose document level
    /// requirement applies to every operation; use <c>AddSwagger&lt;T&gt;(assembly)</c> instead.
    /// </summary>
    /// <example><code>services.AddSwaggerGen(options => options.AddBearerSecurityScheme());</code></example>
    /// <param name="options">The Swashbuckle options.</param>
    /// <param name="schemeName">The key of the scheme in <c>components.securitySchemes</c>.</param>
    /// <param name="bearerFormat">The <c>bearerFormat</c> of the scheme.</param>
    /// <param name="description">The description of the scheme, or <see langword="null"/> for none.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static SwaggerGenOptions AddBearerSecurityScheme(
        this SwaggerGenOptions options,
        string schemeName = SecuritySchemes.JwtBearerSchemeName,
        string bearerFormat = "JWT",
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerFormat);

        var filter = new BearerSecurityFilter(schemeName, bearerFormat, description);
        options.AddDocumentFilterInstance(filter);
        options.AddOperationFilterInstance(filter);
        return options;
    }
}
