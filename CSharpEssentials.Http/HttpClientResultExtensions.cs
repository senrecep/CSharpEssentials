using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpClientResultExtensions
{
    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static async Task<Result<T>> GetFromJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetCoreAsync(client, requestUri, HttpJson.Reader<T>(options), cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return HandleException(ex);
        }
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static Task<Result<T>> PostAsJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        object value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader<T>(options), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static async Task<Result> PostAsResultAsync(
        this HttpClient client,
        Uri? requestUri,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            using HttpResponseMessage response = await client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static Task<Result<T>> PutAsJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        object value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader<T>(options), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static async Task<Result> PutAsResultAsync(
        this HttpClient client,
        Uri? requestUri,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            using HttpResponseMessage response = await client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static Task<Result<T>> PatchAsJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        object value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpContent content = JsonContent.Create(value, options: options ?? EnhancedJsonSerializerOptions.DefaultOptions);
                using HttpResponseMessage response = await client.PatchAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader<T>(options), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static async Task<Result> DeleteAsResultAsync(
        this HttpClient client,
        Uri? requestUri,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            using HttpResponseMessage response = await client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result> SendAsResultAsync(
        this HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public static Task<Result<T>> SendAsResultAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader<T>(options), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static async Task<Result<T>> GetFromJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetCoreAsync(client, requestUri, HttpJson.Reader(typeInfo), cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return HandleException(ex);
        }
    }

    public static Task<Result<T>> PostAsJsonAsResultAsync<TValue, T>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonTypeInfo<TValue> valueTypeInfo,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, value, valueTypeInfo, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader(typeInfo), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static Task<Result<T>> PutAsJsonAsResultAsync<TValue, T>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonTypeInfo<TValue> valueTypeInfo,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, value, valueTypeInfo, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader(typeInfo), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static Task<Result<T>> PatchAsJsonAsResultAsync<TValue, T>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonTypeInfo<TValue> valueTypeInfo,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpContent content = JsonContent.Create(value, valueTypeInfo);
                using HttpResponseMessage response = await client.PatchAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader(typeInfo), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    public static Task<Result<T>> SendAsResultAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync(response, HttpJson.Reader(typeInfo), cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    private static Error HandleException(Exception ex) => HttpExceptionErrors.ToError(ex);

    private static Task<Result> ExecuteAsync(Func<Task<Result>> action, CancellationToken cancellationToken)
    {
        return Result.TryAsync(action, HandleException, cancellationToken);
    }

    private static async ValueTask<Result<T>> GetCoreAsync<T>(
        HttpClient client,
        Uri? requestUri,
        Func<HttpContent, CancellationToken, Task<T?>> read,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        return await HandleResponseAsync(response, read, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<Result<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        Func<HttpContent, CancellationToken, Task<T?>> read,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? value = await read(response.Content, cancellationToken).ConfigureAwait(false);
            if (value is null)
                return Error.NotFound(description: "Response body was empty or could not be deserialized.");
            return value;
        }

        var error = HttpStatusCodeMapper.ToError(response.StatusCode);
        return error;
    }

    private static Result HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = HttpStatusCodeMapper.ToError(response.StatusCode);
        return error;
    }
}
