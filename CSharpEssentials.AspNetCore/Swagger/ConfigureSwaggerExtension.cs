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
     Assembly assembly)
        where TConfigureSwaggerOptions : ConfigureSwaggerOptions
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            string xmlFile = $"{assembly.GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, [] }
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
        .UseSwaggerUI(options =>
        {
            if (apiVersionDescriptionProvider == null)
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{serviceName} V1");
            }
            else
            {
                foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{serviceName} {description.GroupName.ToUpperInvariant()}");
            }


            options.DisplayOperationId();
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.ShowCommonExtensions();
            options.EnableValidator();

            uiOptions?.Invoke(options);
        });
    }
}
