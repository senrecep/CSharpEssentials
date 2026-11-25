using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Registered first in the Minimal API and MVC JSON options by <c>AddEnhancedProblemDetails</c>. A typed result
/// (<see cref="Result{T, E1}"/> and its siblings) that reaches the response writer was returned without <c>ToHttpResult()</c>,
/// and its error would go out as a <c>200</c> body. Writing it throws <see cref="InvalidOperationException"/> before any byte
/// is written. Reading goes through the type's own converter. Other serializer options, such as those of caching and
/// idempotency, keep the type's own converter.
/// </summary>
internal sealed class TypedResultHttpJsonGuard : JsonConverterFactory
{
    private static readonly TypedResultJsonConverterFactory Envelope = new();

    public override bool CanConvert(Type typeToConvert) => Envelope.CanConvert(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
            throw NotConverted(typeToConvert);

        return CreateClosed(typeToConvert);
    }

    internal static InvalidOperationException NotConverted(Type typeToConvert) =>
        new($"The typed result {typeToConvert} was returned from an HTTP endpoint without ToHttpResult(), so its error would be " +
            "sent as a 200 response. Convert it with ToHttpResult(), ToOk(), ToCreated() or ToNoContent(), or add ResultEndpointFilter to the endpoint.");

    [RequiresDynamicCode("Closes the guard converter over the typed result type.")]
    private static JsonConverter CreateClosed(Type typeToConvert) =>
        (JsonConverter)Activator.CreateInstance(typeof(Guard<>).MakeGenericType(typeToConvert))!;

    private sealed class Guard<TResult> : JsonConverter<TResult>
    {
        private JsonConverter<TResult>? _reader;

        public override TResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            _reader ??= (JsonConverter<TResult>)Envelope.CreateConverter(typeof(TResult), options);
            return _reader.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, TResult value, JsonSerializerOptions options) =>
            throw NotConverted(typeof(TResult));
    }
}
