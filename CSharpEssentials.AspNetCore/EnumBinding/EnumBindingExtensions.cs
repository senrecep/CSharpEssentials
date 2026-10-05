using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Registers enum query and route binding that accepts the same spellings as the JSON converter.
/// </summary>
public static class EnumBindingExtensions
{
    /// <summary>
    /// Configures <see cref="EnumBindingOptions"/>. Optional: <see cref="UseEnumBinding"/> works with the defaults.
    /// </summary>
    public static IServiceCollection AddEnumBinding(this IServiceCollection services, Action<EnumBindingOptions>? configure = null)
    {
        OptionsBuilder<EnumBindingOptions> builder = services.AddOptions<EnumBindingOptions>();
        if (configure is not null)
            builder.Configure(configure);
        return services;
    }

    /// <summary>
    /// Normalizes enum query and route values of the selected endpoint before model binding, for Minimal API
    /// (including <c>[AsParameters]</c>) and MVC actions. A value is accepted as the naming policy name
    /// (snake_case by default, <c>[JsonStringEnumMemberName]</c> honored), the C# member name (case-insensitive)
    /// or the number of a defined member. Flags enums accept a comma separated list.
    /// An invalid value returns a 400 problem response with one validation error per key.
    /// </summary>
    /// <remarks>
    /// Must run after routing selected the endpoint (after <c>UseRouting</c> when it is called explicitly).
    /// </remarks>
    public static IApplicationBuilder UseEnumBinding(this IApplicationBuilder app) =>
        app.UseMiddleware<EnumBindingMiddleware>();
}
