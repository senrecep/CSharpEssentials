using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>Enum conventions for Swashbuckle documents.</summary>
public static class SwaggerEnumConventionsExtensions
{
    /// <summary>
    /// Describes the enums handled by the enum conventions (<c>AddEnumConventions</c>, or the defaults: generated metadata and
    /// <c>[StringEnum]</c>) the way they are written: wire names or, in a document whose operations all write numbers, integers;
    /// nullable, flags, collection and default shapes where an enum is used; and operation markers for number and header
    /// selected formats. Plain enums keep the Swashbuckle schema. Calling it twice adds the filters once.
    /// <c>AddSwagger</c> calls it.
    /// </summary>
    /// <param name="options">The Swashbuckle options.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static SwaggerGenOptions AddEnumConventions(this SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.SchemaFilterDescriptors.Exists(static descriptor => descriptor.Type == typeof(EnumSchemaFilter)))
            return options;

        options.SchemaFilter<EnumSchemaFilter>();
        options.OperationFilter<EnumOperationFilter>();
        return options;
    }

    /// <summary>
    /// Registers Swashbuckle (<c>AddSwaggerGen()</c>), adds <see cref="AddEnumConventions(SwaggerGenOptions)"/> after every
    /// other configuration, and wraps the registered <c>ISerializerDataContractResolver</c> so that an undefined enum default
    /// such as <c>[DefaultValue((OrderStatus)42)]</c> is left out instead of failing the document. A resolver registered by type
    /// is not wrapped. Call it after registering your own resolver. Calling it twice, or together with <c>AddSwagger</c>, registers
    /// once. <c>AddSwagger</c> calls it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggerEnumConventions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen();
        EnumDefaultDataContractResolver.Decorate(services);
        services.PostConfigure<SwaggerGenOptions>(static options => options.AddEnumConventions());
        return services;
    }
}
