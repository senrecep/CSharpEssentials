using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Sets the enum output format of endpoints and endpoint groups (design section 9.4).
/// </summary>
public static class EnumWireFormatExtensions
{
    /// <summary>
    /// Writes the enums of every response of the endpoints as <paramref name="format"/>, in both directions:
    /// <see cref="EnumWireFormat.Number"/> keeps a legacy group on integers while <see cref="EnumConventions.WriteAs"/> is
    /// <see cref="EnumWireFormat.String"/>, and <see cref="EnumWireFormat.String"/> opts a group out of a global
    /// <see cref="EnumWireFormat.Number"/>. An endpoint overrides its group; on MVC actions an <see cref="EnumWireFormatAttribute"/>
    /// overrides both. Reading is not affected. The host's JSON options are not changed: the other format is written with a
    /// copy built once. Requires <see cref="EnumConventionsExtensions.AddEnumConventions"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint or group builder.</typeparam>
    /// <param name="builder">The endpoint or group builder.</param>
    /// <param name="format">The output format.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static TBuilder WithEnumWireFormat<TBuilder>(this TBuilder builder, EnumWireFormat format)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithEnumWireFormat(new EnumWireFormatAttribute(format));

    /// <summary>
    /// Selects the enum output format per request from a request header, for clients that share routes across versions
    /// (for example <c>ctx =&gt; ctx.Request.Headers["X-Enum-Format"] == "string" ? EnumWireFormat.String : EnumWireFormat.Number</c>).
    /// Every response of the endpoints gets <c>Vary: <paramref name="header"/></c> so caches keep the formats apart. Precedence and
    /// requirements are those of <see cref="WithEnumWireFormat{TBuilder}(TBuilder, EnumWireFormat)"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint or group builder.</typeparam>
    /// <param name="builder">The endpoint or group builder.</param>
    /// <param name="header">The request header <paramref name="selector"/> reads, added to <c>Vary</c>.</param>
    /// <param name="selector">Selects the format of one response.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static TBuilder WithEnumWireFormat<TBuilder>(this TBuilder builder, string header, Func<HttpContext, EnumWireFormat> selector)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentNullException.ThrowIfNull(selector);
        return builder.WithEnumWireFormat(new EnumWireFormatSelector(header, selector));
    }

    private static TBuilder WithEnumWireFormat<TBuilder>(this TBuilder builder, IEnumWireFormatMetadata metadata)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(metadata);
            if (!endpoint.FilterFactories.Contains(EnumWireFormatEndpointFilter.Factory))
                endpoint.FilterFactories.Add(EnumWireFormatEndpointFilter.Factory);
        });
        return builder;
    }
}
