using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;

namespace CSharpEssentials.Http;

internal static class HttpJson
{
    internal const string SerializationMessage =
        "JSON serialization with JsonSerializerOptions might need types that cannot be statically analyzed. Use the JsonTypeInfo overload with System.Text.Json source generation in trimmed or native AOT applications.";

    [RequiresUnreferencedCode(SerializationMessage)]
    [RequiresDynamicCode(SerializationMessage)]
    internal static Func<HttpContent, CancellationToken, Task<T?>> Reader<T>(JsonSerializerOptions? options) =>
        (content, cancellationToken) => content.ReadFromJsonAsync<T>(options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken);

    internal static Func<HttpContent, CancellationToken, Task<T?>> Reader<T>(JsonTypeInfo<T> typeInfo) =>
        (content, cancellationToken) => content.ReadFromJsonAsync(typeInfo, cancellationToken);
}
