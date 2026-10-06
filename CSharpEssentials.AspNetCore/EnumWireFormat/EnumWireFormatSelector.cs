using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Endpoint metadata of <see cref="EnumWireFormatExtensions.WithEnumWireFormat{TBuilder}(TBuilder, string, Func{HttpContext, EnumWireFormat})"/>:
/// the format is selected per request, from a request header.
/// </summary>
internal sealed class EnumWireFormatSelector(string varyHeader, Func<HttpContext, EnumWireFormat> selector) : IEnumWireFormatMetadata
{
    public string? VaryHeader { get; } = varyHeader;

    public EnumWireFormat SelectFormat(HttpContext httpContext) => selector(httpContext);
}
