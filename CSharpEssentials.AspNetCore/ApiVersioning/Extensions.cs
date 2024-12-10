
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

public static partial class Extensions
{
    public static IServiceCollection AddAndConfigureApiVersioning(
        this IServiceCollection services,
        Action<ApiVersioningOptions>? configureOptions = null,
        Action<ApiExplorerOptions>? configureExplorer = null)
    {
        services.AddApiVersioning(opt =>
            {

                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"));
                configureOptions?.Invoke(opt);

            })
            .AddApiExplorer(config =>
            {
                config.GroupNameFormat = "'v'VVV";
                config.SubstituteApiVersionInUrl = true;
                configureExplorer?.Invoke(config);
            });


        return services;
    }
}
