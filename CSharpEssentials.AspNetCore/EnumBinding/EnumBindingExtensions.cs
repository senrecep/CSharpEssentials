using System.Text.Json;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Binds enum route, query, header and form values with the enum conventions.
/// </summary>
public static class EnumBindingExtensions
{
    /// <summary>
    /// The 4.x registration of enum binding, forwarded to <see cref="EnumConventionsExtensions.AddEnumConventions"/>:
    /// <see cref="EnumBindingOptions.CanBind"/> becomes <see cref="EnumConventions.CanHandle"/>,
    /// <see cref="EnumBindingOptions.AllowIntegerValues"/> becomes <see cref="EnumConventions.AcceptNumbers"/> and
    /// <see cref="EnumBindingOptions.ErrorFactory"/> becomes <see cref="EnumConventionsBuilder.ConfigureErrors"/>. Only enums
    /// with generated metadata (<see cref="StringEnumAttribute"/>) are bound. It also applies the conventions to the JSON options.
    /// </summary>
    /// <exception cref="NotSupportedException"><see cref="EnumBindingOptions.NamingPolicy"/> is not snake_case.</exception>
    [Obsolete("Use services.AddEnumConventions(). Enum naming now comes from [StringEnum(Naming = ...)] on the enum.")]
    public static IServiceCollection AddEnumBinding(this IServiceCollection services, Action<EnumBindingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        EnumBindingOptions options = new();
        configure?.Invoke(options);
        if (options.NamingPolicy is not null && !ReferenceEquals(options.NamingPolicy, JsonNamingPolicy.SnakeCaseLower))
        {
            throw new NotSupportedException(
                "Runtime enum naming policies are not supported. Set the naming at build time with the CSharpEssentialsEnumNaming " +
                "MSBuild property, [StringEnum(Naming = ...)] or [JsonStringEnumMemberName] (enum conventions design, section 4.1).");
        }

        Predicate<Type> canBind = options.CanBind;
        EnumConventionsBuilder builder = services.AddEnumConventions(c => c with
        {
            AcceptNumbers = options.AllowIntegerValues,
            CanHandle = type => canBind(type),
        });
        if (options.ErrorFactory is { } errorFactory)
            builder.ConfigureErrors((error, key) => errorFactory(key, error.EnumType, error.AllowedValues));
        return services;
    }

    /// <summary>
    /// Normalizes the enum route, query, header and form values of the selected endpoint before model binding, for Minimal API
    /// (including <c>[AsParameters]</c>) and MVC actions, with the rules of the JSON converter: a wire name, alias, C# member
    /// name or (with <see cref="EnumConventions.AcceptNumbers"/>) the number of a defined member, after trimming surrounding whitespace, and never the
    /// fallback member. Arrays bind repeated keys and comma separated values (<c>?s=a&amp;s=b</c>, <c>?s=a,b</c>); flags enums
    /// accept a comma separated value. A rejected value returns a 400 problem response with one error per key (see
    /// <see cref="EnumConventionsBuilder.ConfigureErrors"/>). Enums the conventions do not handle keep the framework's binding.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="EnumConventionsExtensions.AddEnumConventions"/>. Must run after routing selected the endpoint (after
    /// <c>UseRouting</c> when it is called explicitly).
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="EnumConventionsExtensions.AddEnumConventions"/> was not called.</exception>
    public static IApplicationBuilder UseEnumBinding(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (app.ApplicationServices.GetService<EnumConventionsRegistration>() is null)
            throw EnumConventionsExtensions.MissingRegistration(nameof(UseEnumBinding));
        return app.UseMiddleware<EnumBindingMiddleware>();
    }
}
