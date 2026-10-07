using System.Reflection;
using Asp.Versioning.ApiExplorer;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CSharpEssentials.AspNetCore;

public static class ConfigureSwaggerExtension
{
    public static IServiceCollection AddSwagger<TConfigureSwaggerOptions>(
     this IServiceCollection services,
     string securitySchemeName,
     OpenApiSecurityScheme securityScheme,
     Assembly? assembly = null)
        where TConfigureSwaggerOptions : ConfigureSwaggerOptions
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(securitySchemeName);
        ArgumentNullException.ThrowIfNull(securityScheme);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(securitySchemeName, securityScheme);
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference(securitySchemeName, document), [] }
            });
            var factory = new SwashbuckleSchemaIdFactory();
            options.CustomSchemaIds(factory.GetSchemaId);
        });
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>>(provider => new XmlCommentsConfigureOptions(provider, assembly));
        services.ConfigureOptions<TConfigureSwaggerOptions>();
        // After every Configure (XML comments included), so the enum descriptions append to the XML summaries.
        services.PostConfigure<SwaggerGenOptions>(static options => options.AddEnumConventions());
        return services;
    }

    public static IApplicationBuilder UseVersionableSwagger(
        this IApplicationBuilder app,
        Action<SwaggerOptions>? options = null,
        Action<SwaggerUIOptions>? uiOptions = null)
    {
        IConfiguration configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        IApiVersionDescriptionProvider? apiVersionDescriptionProvider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
        string serviceName = configuration["Swagger:Title"] ?? "API";
        return app.UseSwagger(s =>
        {
            s.RouteTemplate = "swagger/{documentName}/swagger.json";
            options?.Invoke(s);
        })
        .UseSwaggerUI(swaggerUiOptions =>
        {
            if (apiVersionDescriptionProvider == null)
            {
                swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", $"{serviceName} V1");
            }
            else
            {
                foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    swaggerUiOptions.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{serviceName} {description.GroupName.ToUpperInvariant()}");
            }


            swaggerUiOptions.DisplayOperationId();
            swaggerUiOptions.DisplayRequestDuration();
            swaggerUiOptions.EnableDeepLinking();
            swaggerUiOptions.EnableFilter();
            swaggerUiOptions.ShowExtensions();
            swaggerUiOptions.ShowCommonExtensions();
            swaggerUiOptions.EnableValidator();

            uiOptions?.Invoke(swaggerUiOptions);
        });
    }
}
