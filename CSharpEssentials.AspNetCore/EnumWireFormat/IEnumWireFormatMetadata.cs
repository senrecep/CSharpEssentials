using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Endpoint metadata that selects the enum output format of an endpoint, overriding <see cref="EnumConventions.WriteAs"/>.
/// Added by <see cref="EnumWireFormatAttribute"/> (MVC controller or action) and
/// <see cref="EnumWireFormatExtensions.WithEnumWireFormat{TBuilder}(TBuilder, EnumWireFormat)"/> (endpoint, group).
/// </summary>
public interface IEnumWireFormatMetadata
{
    /// <summary>
    /// The request header the format depends on; it is added to <c>Vary</c> on every response of the endpoint.
    /// <see langword="null"/> for a fixed format.
    /// </summary>
    string? VaryHeader { get; }

    /// <summary>Selects the output format of one response.</summary>
    /// <param name="httpContext">The request.</param>
    /// <returns>The format.</returns>
    EnumWireFormat SelectFormat(HttpContext httpContext);
}
