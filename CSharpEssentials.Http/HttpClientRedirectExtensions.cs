using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpClientRedirectExtensions
{
    public static async Task<Result> SendWithRedirectsAsResultAsync(
        this HttpClient client,
        HttpRequestMessage request,
        int maxRedirects = 5,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("Http.ClientRequired", "HttpClient cannot be null.");

        if (request is null)
            return Error.Validation("Http.RequestRequired", "HttpRequestMessage cannot be null.");

        return await Result.TryAsync(
            async () =>
            {
                HttpRequestMessage currentRequest = request;
                int redirects = 0;

                while (true)
                {
                    HttpResponseMessage response = await client.SendAsync(
                        currentRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    if (!IsRedirectStatusCode(response.StatusCode))
                        return HandleResponse(response);

                    if (redirects >= maxRedirects)
                    {
                        response.Dispose();
                        return Error.Unexpected(
                            "Http.RedirectLimitExceeded",
                            $"Redirect limit exceeded after {maxRedirects} attempts. Last URI: {currentRequest.RequestUri}");
                    }

                    Uri? redirectUri = response.Headers.Location;
                    if (redirectUri is null)
                    {
                        response.Dispose();
                        return Error.Unexpected(
                            "Http.MissingRedirectLocation",
                            "Redirect response missing Location header.");
                    }

                    response.Dispose();

                    HttpRequestMessage newRequest = await CloneHttpRequestMessageAsync(currentRequest);

                    if (currentRequest != request)
                        currentRequest.Dispose();

                    newRequest.RequestUri = redirectUri.IsAbsoluteUri
                        ? redirectUri
                        : new Uri(currentRequest.RequestUri!, redirectUri);

                    currentRequest = newRequest;
                    redirects++;
                }
            },
            HandleException,
            cancellationToken);
    }

    public static async Task<Result<T>> SendWithRedirectsAsResultAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        JsonSerializerOptions? options = null,
        int maxRedirects = 5,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("Http.ClientRequired", "HttpClient cannot be null.");

        if (request is null)
            return Error.Validation("Http.RequestRequired", "HttpRequestMessage cannot be null.");

        return await Result.TryAsync(
            async () =>
            {
                HttpRequestMessage currentRequest = request;
                int redirects = 0;

                while (true)
                {
                    HttpResponseMessage response = await client.SendAsync(
                        currentRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    if (!IsRedirectStatusCode(response.StatusCode))
                        return await HandleResponseAsync<T>(response, options, cancellationToken);

                    if (redirects >= maxRedirects)
                    {
                        response.Dispose();
                        return Error.Unexpected(
                            "Http.RedirectLimitExceeded",
                            $"Redirect limit exceeded after {maxRedirects} attempts. Last URI: {currentRequest.RequestUri}");
                    }

                    Uri? redirectUri = response.Headers.Location;
                    if (redirectUri is null)
                    {
                        response.Dispose();
                        return Error.Unexpected(
                            "Http.MissingRedirectLocation",
                            "Redirect response missing Location header.");
                    }

                    response.Dispose();

                    HttpRequestMessage newRequest = await CloneHttpRequestMessageAsync(currentRequest);

                    if (currentRequest != request)
                        currentRequest.Dispose();

                    newRequest.RequestUri = redirectUri.IsAbsoluteUri
                        ? redirectUri
                        : new Uri(currentRequest.RequestUri!, redirectUri);

                    currentRequest = newRequest;
                    redirects++;
                }
            },
            HandleException,
            cancellationToken);
    }

    private static bool IsRedirectStatusCode(HttpStatusCode statusCode)
    {
        int code = (int)statusCode;
        return code is >= 300 and < 400;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        clone.Version = request.Version;

        if (request.Content is null)
            return clone;

        clone.Content = new StreamContent(await request.Content.ReadAsStreamAsync().ConfigureAwait(false));

        foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in request.Content.Headers)
            clone.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);

        return clone;
    }

    private static Error HandleException(Exception ex)
    {
        if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);

        return Error.Exception(ex, ErrorType.Unexpected);
    }

    private static Result HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = HttpStatusCodeMapper.ToError(response.StatusCode);
        return error;
    }

    private static async Task<Result<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? value = await response.Content.ReadFromJsonAsync<T>(
                options ?? EnhancedJsonSerializerOptions.DefaultOptions,
                cancellationToken);

            if (value is null)
                return Error.NotFound(description: "Response body was empty or could not be deserialized.");

            return value;
        }

        var error = HttpStatusCodeMapper.ToError(response.StatusCode);
        return error;
    }
}
