using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Sets the enum output format of an MVC controller or action, in both directions: <see cref="EnumWireFormat.Number"/> keeps a
/// legacy controller on integers while <see cref="EnumConventions.WriteAs"/> is <see cref="EnumWireFormat.String"/>, and
/// <see cref="EnumWireFormat.String"/> opts a new controller out of a global <see cref="EnumWireFormat.Number"/>.
/// Precedence: action &gt; controller &gt; endpoint group (<see cref="EnumWireFormatExtensions"/>) &gt; <see cref="EnumConventions.WriteAs"/>.
/// Reading is not affected. Requires <see cref="EnumConventionsExtensions.AddEnumConventions"/>.
/// </summary>
/// <param name="format">The output format.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class EnumWireFormatAttribute(EnumWireFormat format) : Attribute, IEnumWireFormatMetadata
{
    /// <summary>The output format.</summary>
    public EnumWireFormat Format { get; } = format;

    /// <inheritdoc />
    string? IEnumWireFormatMetadata.VaryHeader => null;

    /// <inheritdoc />
    EnumWireFormat IEnumWireFormatMetadata.SelectFormat(HttpContext httpContext) => Format;
}
