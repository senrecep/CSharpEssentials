
using Asp.Versioning.ApiExplorer;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

public sealed class DefaultConfigureSwaggerOptions(
           IServiceProvider serviceProvider,
           IHostEnvironment environment,
           IConfiguration configuration)
           : ConfigureSwaggerOptions(serviceProvider, environment, configuration);

public abstract class ConfigureSwaggerOptions(
            IServiceProvider serviceProvider,
           IHostEnvironment environment,
           IConfiguration configuration)
           : IConfigureNamedOptions<SwaggerGenOptions>
{
    private static readonly Uri _defaultLicenseUrl = new UriBuilder(Uri.UriSchemeHttps, "opensource.org") { Path = "license/mit" }.Uri;
    public virtual void Configure(SwaggerGenOptions options)
    {
        string title = configuration["Swagger:Title"] ?? "API";
        string description = configuration["Swagger:Description"] ?? "API Description";
        string license = configuration["Swagger:License"] ?? "API License";
        string? licenseUrl = configuration["Swagger:LicenseUrl"];
        var swaggerLicense = new OpenApiLicense
        {
            Name = license,
            Url = licenseUrl is null ? _defaultLicenseUrl : new Uri(licenseUrl)
        };
        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        IApiVersionDescriptionProvider? provider = serviceProvider.GetService<IApiVersionDescriptionProvider>();
        if (provider == null)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = description,
                License = swaggerLicense
            });
        }
        else
        {
            foreach (ApiVersionDescription apiVersionDescription in provider.ApiVersionDescriptions)
                options.SwaggerDoc(apiVersionDescription.GroupName, new OpenApiInfo
                {
                    Title = title,
                    Version = apiVersionDescription.GroupName,
                    Description = CreateDescription(description, apiVersionDescription, environment),
                    License = swaggerLicense
                });
        }


        options.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();

        var timeSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Example = JsonValue.Create("00:00:00")
        };

        options.MapType<TimeSpan>(() => timeSchema);
        options.MapType<TimeOnly>(() => timeSchema);
    }

    public virtual void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static string CreateDescription(string? description, ApiVersionDescription apiVersionDescription,
        IHostEnvironment environment)
    {
        string env = environment.EnvironmentName;
        string version = apiVersionDescription.ApiVersion.ToString();
        description ??= string.Empty;
        description = $"[{env}] {version} {description}";
        return apiVersionDescription.IsDeprecated
            ? $"{description} This API version has been deprecated"
            : description;
    }
}
