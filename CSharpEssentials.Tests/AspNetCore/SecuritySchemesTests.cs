using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

public class SecuritySchemesTests
{
    [Fact]
    public void JwtBearerTokenSecurity_ShouldHaveCorrectConfiguration()
    {
        OpenApiSecurityScheme scheme = SecuritySchemes.JwtBearerTokenSecurity;

        scheme.Should().NotBeNull();
        scheme.Scheme.Should().Be("bearer");
        scheme.BearerFormat.Should().Be("JWT");
        scheme.Name.Should().Be("JWT Authentication");
        scheme.In.Should().Be(ParameterLocation.Header);
        scheme.Type.Should().Be(SecuritySchemeType.Http);
        scheme.Description.Should().Be("JWT Bearer Token Authorization");
        SecuritySchemes.JwtBearerSchemeName.Should().Be("Bearer");
    }
}
