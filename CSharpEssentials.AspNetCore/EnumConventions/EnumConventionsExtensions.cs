using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Registers the enum conventions of an ASP.NET Core host (design section 9).
/// </summary>
public static class EnumConventionsExtensions
{
    private const string ReflectionMessage =
        "Enums without generated metadata are bound and serialized with reflection metadata. Mark them [StringEnum] and use AddEnumConventions instead.";

    /// <summary>
    /// Registers <see cref="EnumConventions"/> as a singleton and applies it to the host:
    /// <list type="bullet">
    /// <item>minimal API and MVC JSON options read enums in <see cref="EnumReadMode.Input"/> mode and write
    /// <see cref="EnumConventions.WriteAs"/>;</item>
    /// <item><see cref="EnumBindingExtensions.UseEnumBinding"/> binds route, query, header and form values with the same rules;</item>
    /// <item><see cref="EnumWireFormatAttribute"/> and <see cref="EnumWireFormatExtensions.WithEnumWireFormat{TBuilder}(TBuilder, EnumWireFormat)"/>
    /// override the output format per controller, action, endpoint or group;</item>
    /// <item><see cref="GlobalExceptionHandler"/> maps an <see cref="EnumValueJsonException"/> of a request body to the same 400
    /// problem as a rejected route or query value.</item>
    /// </list>
    /// Only enums selected by <see cref="EnumConventions.CanHandle"/> that have generated metadata (<see cref="StringEnumAttribute"/>)
    /// are affected; every other enum keeps the framework's binding and number output. Calling it again replaces the conventions.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configure">Changes the default conventions, for example <c>c =&gt; c with { WriteAs = EnumWireFormat.Number }</c>.</param>
    /// <returns>A builder for further configuration (<see cref="EnumConventionsBuilder.ConfigureErrors"/>).</returns>
    public static EnumConventionsBuilder AddEnumConventions(
        this IServiceCollection services,
        Func<EnumConventions, EnumConventions>? configure = null)
    {
        EnumConventions conventions = Configure(configure);
        return Register(services, new EnumConventionsRegistration(
            conventions,
            reflectionFallback: null,
            writeAs => new EnumConverterFactory(conventions, EnumReadMode.Input, writeAs)));
    }

    /// <summary>
    /// Like <see cref="AddEnumConventions"/>, and also handles enums without generated metadata that
    /// <see cref="EnumConventions.CanHandle"/> selects (set it, for example <c>c =&gt; c with { CanHandle = t =&gt; true }</c>),
    /// with metadata read by reflection (<see cref="EnumMetadata.GetOrCreateWithReflection"/>), in binding and JSON alike.
    /// Not trimming or AOT safe.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configure">Changes the default conventions.</param>
    /// <returns>A builder for further configuration.</returns>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public static EnumConventionsBuilder AddEnumConventionsWithReflection(
        this IServiceCollection services,
        Func<EnumConventions, EnumConventions>? configure = null)
    {
        EnumConventions conventions = Configure(configure);
        return Register(services, new EnumConventionsRegistration(
            conventions,
            static type => EnumMetadata.GetOrCreateWithReflection(type),
            writeAs => EnumConverterFactory.CreateWithReflectionFallback(conventions, EnumReadMode.Input, writeAs)));
    }

    internal static InvalidOperationException MissingRegistration(string caller) =>
        new($"{caller} requires the enum conventions. Call services.AddEnumConventions() when registering services.");

    private static EnumConventions Configure(Func<EnumConventions, EnumConventions>? configure) =>
        configure is null
            ? EnumConventions.Default
            : configure(EnumConventions.Default) ?? throw new InvalidOperationException("The enum conventions callback returned null.");

    private static EnumConventionsBuilder Register(IServiceCollection services, EnumConventionsRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Replace(ServiceDescriptor.Singleton(registration));
        services.Replace(ServiceDescriptor.Singleton(registration.Conventions));
        services.TryAddSingleton<EnumWireFormatOutput>();
        services.TryAddSingleton<EnumWireFormatResultFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<HttpJsonOptions>, EnumConventionsJsonOptionsSetup>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MvcJsonOptions>, EnumConventionsJsonOptionsSetup>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, EnumWireFormatMvcSetup>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionProblemMapper, EnumValueExceptionProblemMapper>());
        return new EnumConventionsBuilder(services, registration);
    }

    private sealed class EnumWireFormatMvcSetup : IConfigureOptions<MvcOptions>
    {
        void IConfigureOptions<MvcOptions>.Configure(MvcOptions options) => options.Filters.AddService<EnumWireFormatResultFilter>();
    }
}
