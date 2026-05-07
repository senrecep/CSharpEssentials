using System.Reflection;
using Asp.Versioning.ApiExplorer;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CSharpEssentials.AspNetCore;

public static class ConfigureSwaggerExtension
{
    public static IServiceCollection AddSwagger<TConfigureSwaggerOptions>(
     this IServiceCollection services,
     OpenApiSecurityScheme securityScheme,
     Assembly? assembly = null)
        where TConfigureSwaggerOptions : ConfigureSwaggerOptions
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            assembly ??= Assembly.GetCallingAssembly();
            string xmlFile = $"{assembly.GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
#if NET8_0_OR_GREATER
                { securityScheme, [] }
#else
                { securityScheme, Array.Empty<string>() }
#endif
            });
            var factory = new SwashbuckleSchemaIdFactory();
            options.CustomSchemaIds(factory.GetSchemaId);

            options.SchemaFilter<EnumSchemaFilter>();
        });
        services.ConfigureOptions<TConfigureSwaggerOptions>();
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
