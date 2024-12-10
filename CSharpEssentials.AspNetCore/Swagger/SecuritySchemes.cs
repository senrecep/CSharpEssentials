using Microsoft.OpenApi.Models;

namespace CSharpEssentials.AspNetCore;

public static class SecuritySchemes
{
    public static readonly OpenApiSecurityScheme JwtBearerTokenSecurity = new()
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Bearer Token Authorization",

        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
}
