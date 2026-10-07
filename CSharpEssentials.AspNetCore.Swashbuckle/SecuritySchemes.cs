using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore;

public static class SecuritySchemes
{
    /// <summary>The name <see cref="JwtBearerTokenSecurity"/> is registered under in <c>components.securitySchemes</c>.</summary>
    public const string JwtBearerSchemeName = "Bearer";

    public static readonly OpenApiSecurityScheme JwtBearerTokenSecurity = new()
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Bearer Token Authorization",
    };
}
