using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Reads and writes <see cref="Result{T, E1}"/> as <c>{"case":"&lt;name&gt;","value":&lt;case&gt;}</c>. The name is
/// <c>Success</c> for the success value and the name of the slot type for an error (the full name when two slots share a
/// name). <see langword="default"/> is written as <c>null</c> and <c>null</c> reads back as <see langword="default"/>.
/// </summary>
/// <remarks>
/// The result type already carries <see cref="TypedResultJsonConverterFactory"/>, which creates this converter through
/// reflection unless the bundled source generator registered it for a type named in a <c>[JsonSerializable]</c> attribute.
/// Where dynamic code is not available (Native AOT) and the generator does not see the type, register it yourself with
/// <see cref="TypedResultJsonConverterFactory.Register{T, E1}"/>, in
/// <c>[JsonSourceGenerationOptions(Converters = [typeof(TypedResultJsonConverter&lt;...&gt;)])]</c> or in
/// <see cref="JsonSerializerOptions.Converters"/>. Each case is (de)serialized as its slot type, so a source-generated context
/// must also list <typeparamref name="T"/> and every error type.
/// </remarks>
/// <typeparam name="T">The success value type.</typeparam>
/// <typeparam name="E1">The first error type.</typeparam>
[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
public sealed class TypedResultJsonConverter<T, E1> : JsonConverter<Result<T, E1>>
    where T : notnull
    where E1 : notnull
{
    private static readonly string[] Names = TypedResultJson.CaseNames(typeof(E1));

    /// <inheritdoc/>
    public override bool HandleNull => true;

    /// <inheritdoc/>
    public override Result<T, E1> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        int index = TypedResultJson.ReadEnvelope(ref reader, Names, typeToConvert, out Utf8JsonReader value);
        return index switch
        {
            0 => new(TypedResultLogic.SuccessTag, TypedResultJson.ReadCase<T>(ref value, options), null),

            _ => new(2, default!, TypedResultJson.ReadCase<E1>(ref value, options)),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result<T, E1> value, JsonSerializerOptions options)
    {
        switch (value.Tag)
        {
            case TypedResultLogic.SuccessTag:
                TypedResultJson.WriteCase(writer, Names[0], value.RawValue, options);
                break;
            case 2:
                TypedResultJson.WriteCase(writer, Names[1], (E1)value.RawError!, options);
                break;
            default:
                writer.WriteNullValue();
                break;
        }
    }
}
