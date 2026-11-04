using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

/// <summary>
/// Converts polymorphic bases marked with <see cref="JsonAcceptClrTypeNamesAttribute"/>.
/// </summary>
[RequiresUnreferencedCode(JsonPolymorphismExtensions.ReflectionMessage)]
[RequiresDynamicCode(JsonPolymorphismExtensions.ReflectionMessage)]
internal sealed class ClrTypeNameConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsDefined(typeof(JsonAcceptClrTypeNamesAttribute), false) &&
        (typeToConvert.IsDefined(typeof(JsonPolymorphicAttribute), false) || typeToConvert.IsDefined(typeof(JsonDerivedTypeAttribute), false));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter?)Activator.CreateInstance(typeof(ClrTypeNameConverter<>).MakeGenericType(typeToConvert));
}
