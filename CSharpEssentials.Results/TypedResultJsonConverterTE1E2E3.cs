using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Reads and writes <see cref="Result{T, E1, E2, E3}"/> as <c>{"case":"&lt;name&gt;","value":&lt;case&gt;}</c>. The name is
/// <c>Success</c> for the success value and the name of the slot type for an error (the full name when two slots share a
/// name). <see langword="default"/> is written as <c>null</c> and <c>null</c> reads back as <see langword="default"/>.
/// </summary>
/// <remarks>
/// The result type already carries <see cref="TypedResultJsonConverterFactory"/>, which creates this converter through
/// reflection. Register it yourself when dynamic code is not available (Native AOT): in
/// <c>[JsonSourceGenerationOptions(Converters = [typeof(TypedResultJsonConverter&lt;...&gt;)])]</c> or in
/// <see cref="JsonSerializerOptions.Converters"/>. Each case is (de)serialized as its slot type, so a source-generated context
/// must also list <typeparamref name="T"/> and every error type.
/// </remarks>
/// <typeparam name="T">The success value type.</typeparam>
/// <typeparam name="E1">The first error type.</typeparam>
/// <typeparam name="E2">The second error type.</typeparam>
/// <typeparam name="E3">The third error type.</typeparam>
[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
public sealed class TypedResultJsonConverter<T, E1, E2, E3> : JsonConverter<Result<T, E1, E2, E3>>
    where T : notnull
    where E1 : notnull
    where E2 : notnull
    where E3 : notnull
{
    private static readonly string[] Names = TypedResultJson.CaseNames(typeof(E1), typeof(E2), typeof(E3));

    /// <inheritdoc/>
    public override bool HandleNull => true;

    /// <inheritdoc/>
    public override Result<T, E1, E2, E3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        int index = TypedResultJson.ReadEnvelope(ref reader, Names, typeToConvert, out Utf8JsonReader value);
        return index switch
        {
            0 => new(TypedResultLogic.SuccessTag, TypedResultJson.ReadCase<T>(ref value, options), null),
            1 => new(2, default!, TypedResultJson.ReadCase<E1>(ref value, options)),
            2 => new(3, default!, TypedResultJson.ReadCase<E2>(ref value, options)),
            _ => new(4, default!, TypedResultJson.ReadCase<E3>(ref value, options)),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result<T, E1, E2, E3> value, JsonSerializerOptions options)
    {
        switch (value.Tag)
        {
            case TypedResultLogic.SuccessTag:
                TypedResultJson.WriteCase(writer, Names[0], value.RawValue, options);
                break;
            case 2:
                TypedResultJson.WriteCase(writer, Names[1], (E1)value.RawError!, options);
                break;
            case 3:
                TypedResultJson.WriteCase(writer, Names[2], (E2)value.RawError!, options);
                break;
            case 4:
                TypedResultJson.WriteCase(writer, Names[3], (E3)value.RawError!, options);
                break;
            default:
                writer.WriteNullValue();
                break;
        }
    }
}
