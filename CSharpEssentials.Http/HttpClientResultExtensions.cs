using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpClientResultExtensions
{
    public static Task<Result<T>> GetFromJsonAsResultAsync<T>(
        this HttpClient client,
        Uri? requestUri,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

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
                HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
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
            HttpResponseMessage response = await client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

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
                HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
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
            HttpResponseMessage response = await client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

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
                HttpResponseMessage response = await client.PatchAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
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
            HttpResponseMessage response = await client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
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
            HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return HandleResponse(response);
        }, cancellationToken).ConfigureAwait(false);
    }

    public static Task<Result<T>> SendAsResultAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () =>
            {
                HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken);
    }

    private static Error HandleException(Exception ex) => HttpExceptionErrors.ToError(ex);

    private static Task<Result> ExecuteAsync(Func<Task<Result>> action, CancellationToken cancellationToken)
    {
        return Result.TryAsync(action, HandleException, cancellationToken);
    }

    private static async Task<Result<T>> HandleResponseAsync<T>(HttpResponseMessage response, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? value = await response.Content.ReadFromJsonAsync<T>(options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken).ConfigureAwait(false);
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
