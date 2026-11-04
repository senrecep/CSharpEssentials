using System.Net;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpClientRedirectExtensions
{
    public static async Task<Result> SendWithRedirectsAsResultAsync(
        this HttpClient? client,
        HttpRequestMessage? request,
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
                Result<HttpResponseMessage> sent = await SendFollowingRedirectsAsync(client, request, maxRedirects, cancellationToken).ConfigureAwait(false);
                if (sent.IsFailure)
                    return sent.Errors;

                using HttpResponseMessage response = sent.Value;
                return HandleResponse(response);
            },
            HandleException,
            cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<T>> SendWithRedirectsAsResultAsync<T>(
        this HttpClient? client,
        HttpRequestMessage? request,
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
                Result<HttpResponseMessage> sent = await SendFollowingRedirectsAsync(client, request, maxRedirects, cancellationToken).ConfigureAwait(false);
                if (sent.IsFailure)
                    return sent.Errors;

                using HttpResponseMessage response = sent.Value;
                return await HandleResponseAsync<T>(response, options, cancellationToken).ConfigureAwait(false);
            },
            HandleException,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Result<HttpResponseMessage>> SendFollowingRedirectsAsync(
        HttpClient client,
        HttpRequestMessage request,
        int maxRedirects,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage currentRequest = request;
        int redirects = 0;

        while (true)
        {
            HttpResponseMessage response = await client.SendAsync(
                currentRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            if (!HttpRedirectRules.IsRedirect(response.StatusCode))
                return response;

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

            Uri? previousUri = currentRequest.RequestUri;
            Uri target = redirectUri.IsAbsoluteUri || previousUri is null
                ? redirectUri
                : new Uri(previousUri, redirectUri);

            if (previousUri is not null
                && !HttpRedirectRules.IsSameOrigin(previousUri, target)
                && HttpRedirectRules.HasCredentials(client.DefaultRequestHeaders))
            {
                return Error.Forbidden(
                    "Http.CrossOriginRedirect",
                    $"Redirect to another origin refused because HttpClient.DefaultRequestHeaders carries credentials. Target URI: {target}");
            }

            HttpRequestMessage newRequest = await CloneHttpRequestMessageAsync(currentRequest, response.StatusCode).ConfigureAwait(false);
            newRequest.RequestUri = target;
            if (previousUri is not null)
                HttpRedirectRules.StripCredentialsIfCrossOrigin(newRequest, previousUri, target);

            if (currentRequest != request)
                currentRequest.Dispose();

            currentRequest = newRequest;
            redirects++;
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, HttpStatusCode statusCode)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        clone.Version = request.Version;

        if (HttpRedirectRules.TrySwitchToGet(clone, statusCode) || request.Content is null)
            return clone;

        clone.Content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false));

        foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in request.Content.Headers)
            clone.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);

        return clone;
    }

    private static Error HandleException(Exception ex) => HttpExceptionErrors.ToError(ex);

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
                cancellationToken).ConfigureAwait(false);

            if (value is null)
                return Error.NotFound(description: "Response body was empty or could not be deserialized.");

            return value;
        }

        var error = HttpStatusCodeMapper.ToError(response.StatusCode);
        return error;
    }
}
