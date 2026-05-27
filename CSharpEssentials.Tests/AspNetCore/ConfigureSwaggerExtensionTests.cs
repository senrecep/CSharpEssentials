using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CSharpEssentials.Tests.AspNetCore;

public class ConfigureSwaggerExtensionTests
{
    private static OpenApiSecurityScheme CreateBearerScheme() => new()
    {
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http
    };

    [Fact]
    public void AddSwagger_Should_ReturnSameServiceCollection_When_Called()
    {
        var services = new ServiceCollection();
        OpenApiSecurityScheme scheme = CreateBearerScheme();

        IServiceCollection result = services.AddSwagger<DefaultConfigureSwaggerOptions>(scheme);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddSwagger_Should_RegisterSwaggerServices_When_Called()
    {
        var services = new ServiceCollection();
        OpenApiSecurityScheme scheme = CreateBearerScheme();

        services.AddSwagger<DefaultConfigureSwaggerOptions>(scheme);

        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddSwagger_Should_RegisterConfigureSwaggerOptions_When_Called()
    {
        var services = new ServiceCollection();
        OpenApiSecurityScheme scheme = CreateBearerScheme();

        services.AddSwagger<DefaultConfigureSwaggerOptions>(scheme);

        bool hasConfigureOptions = services.Any(sd =>
            sd.ServiceType == typeof(Microsoft.Extensions.Options.IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>));
        hasConfigureOptions.Should().BeTrue();
    }

    [Fact]
    public void UseVersionableSwagger_Should_ConfigureMiddleware_When_NoApiVersionProvider()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(CreateBearerScheme());
        WebApplication app = builder.Build();

        Action act = () => app.UseVersionableSwagger();

        act.Should().NotThrow();
    }

    [Fact]
    public void UseVersionableSwagger_Should_InvokeOptionsCallback_When_CallbackProvided()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(CreateBearerScheme());
        WebApplication app = builder.Build();
        bool callbackInvoked = false;

        app.UseVersionableSwagger(options =>
        {
            callbackInvoked = true;
        });

        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void UseVersionableSwagger_Should_InvokeUiOptionsCallback_When_CallbackProvided()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(CreateBearerScheme());
        WebApplication app = builder.Build();
        bool uiCallbackInvoked = false;

        app.UseVersionableSwagger(uiOptions: uiOptions =>
        {
            uiCallbackInvoked = true;
        });

        uiCallbackInvoked.Should().BeTrue();
    }
}
