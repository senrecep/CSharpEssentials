using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
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
                if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);

                if (ex is JsonException)
                    return Error.Exception(ex, ErrorType.Validation);

                return HttpExceptionErrors.ToError(ex);
            },
            cancellationToken);
    }

    public static Task<Result<T>> ReadFromJsonAsResultAsync<T>(
        this HttpContent content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync<T>(
            async () =>
            {
                T? value = await content.ReadFromJsonAsync<T>(options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
                if (value is null)
                    return Error.NotFound(description: "Response body was empty or could not be deserialized.");
                return value;
            },
            ex =>
            {
                if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);

                if (ex is JsonException)
                    return Error.Exception(ex, ErrorType.Validation);

                return HttpExceptionErrors.ToError(ex);
            },
            cancellationToken);
    }
}
