using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpContentExtensions
{
    public static Task<Result<string>> ReadAsStringAsResultAsync(this HttpContent content, CancellationToken cancellationToken = default)
    {
        return Result.TryAsync<string>(
            async () =>
            {
#if NETSTANDARD2_1
                return await content.ReadAsStringAsync().ConfigureAwait(false);
#else
                return await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            },
            ex =>
            {
                if (ex is JsonException)
                    return Error.Exception(ex, ErrorType.Validation);

                return HttpExceptionErrors.ToError(ex);
            },
            cancellationToken);
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static Task<Result<T>> ReadFromJsonAsResultAsync<T>(
        this HttpContent content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ReadFromJsonAsResultAsync(content, HttpJson.Reader<T>(options), cancellationToken);

    public static Task<Result<T>> ReadFromJsonAsResultAsync<T>(
        this HttpContent content,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default) =>
        ReadFromJsonAsResultAsync(content, HttpJson.Reader(typeInfo), cancellationToken);

    private static Task<Result<T>> ReadFromJsonAsResultAsync<T>(
        HttpContent content,
        Func<HttpContent, CancellationToken, Task<T?>> read,
        CancellationToken cancellationToken)
    {
        return Result.TryAsync<T>(
            async () =>
            {
                T? value = await read(content, cancellationToken).ConfigureAwait(false);
                if (value is null)
                    return Error.NotFound(description: "Response body was empty or could not be deserialized.");
                return value;
            },
            ex =>
            {
                if (ex is JsonException)
                    return Error.Exception(ex, ErrorType.Validation);

                return HttpExceptionErrors.ToError(ex);
            },
            cancellationToken);
    }
}
