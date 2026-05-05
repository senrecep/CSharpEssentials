using System.Net.Http.Json;
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
                HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken);
                return await HandleResponseAsync<T>(response, options, cancellationToken);
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
                HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken);
                return await HandleResponseAsync<T>(response, options, cancellationToken);
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
            HttpResponseMessage response = await client.PostAsync(requestUri, content, cancellationToken);
            return HandleResponse(response);
        });
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
                HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, value, options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken);
                return await HandleResponseAsync<T>(response, options, cancellationToken);
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
            HttpResponseMessage response = await client.PutAsync(requestUri, content, cancellationToken);
            return HandleResponse(response);
        });
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
                HttpResponseMessage response = await client.PatchAsync(requestUri, content, cancellationToken);
                return await HandleResponseAsync<T>(response, options, cancellationToken);
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
            HttpResponseMessage response = await client.DeleteAsync(requestUri, cancellationToken);
            return HandleResponse(response);
        });
    }

    public static async Task<Result> SendAsResultAsync(
        this HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            return HandleResponse(response);
        });
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
                HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response, options, cancellationToken);
            },
            HandleException,
            cancellationToken);
    }

    private static Error HandleException(Exception ex)
    {
        if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);

        return Error.Exception(ex, ErrorType.Unexpected);
    }

    private static async Task<Result> ExecuteAsync(Func<Task<Result>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (IOException ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
    }

    private static async Task<Result<T>> HandleResponseAsync<T>(HttpResponseMessage response, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? value = await response.Content.ReadFromJsonAsync<T>(options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken);
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
