
using System.Text.Json;
using CSharpEssentials.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

public static partial class Extensions
{
    public static IServiceCollection ConfigureSystemTextJson(
        this IServiceCollection services,
        JsonSerializerOptions? jsonSerializerOptions = null,
        Action<JsonSerializerOptions>? configureOptions = null)
    {
        jsonSerializerOptions ??= EnhancedJsonSerializerOptions.DefaultOptions;
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ApplyFrom(jsonSerializerOptions);
                configureOptions?.Invoke(options.JsonSerializerOptions);
            });

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ApplyFrom(jsonSerializerOptions);
            configureOptions?.Invoke(options.SerializerOptions);
        });
        return services;
    }
}
