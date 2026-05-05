using System.Net.Http.Json;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public sealed class HttpRequestBuilder
{
    private HttpMethod _method = HttpMethod.Get;
    private Uri? _uri;
    private readonly List<(string Key, string Value)> _headers = [];
    private readonly List<(string Key, string Value)> _queryParameters = [];
    private HttpContent? _content;

    private HttpRequestBuilder() { }

    public static HttpRequestBuilder Get(string uri) => new() { _method = HttpMethod.Get, _uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
    public static HttpRequestBuilder Get(Uri uri) => new() { _method = HttpMethod.Get, _uri = uri };
    public static HttpRequestBuilder Post(string uri) => new() { _method = HttpMethod.Post, _uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
    public static HttpRequestBuilder Post(Uri uri) => new() { _method = HttpMethod.Post, _uri = uri };
    public static HttpRequestBuilder Put(string uri) => new() { _method = HttpMethod.Put, _uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
    public static HttpRequestBuilder Put(Uri uri) => new() { _method = HttpMethod.Put, _uri = uri };
    public static HttpRequestBuilder Patch(string uri) => new() { _method = HttpMethod.Patch, _uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
    public static HttpRequestBuilder Patch(Uri uri) => new() { _method = HttpMethod.Patch, _uri = uri };
    public static HttpRequestBuilder Delete(string uri) => new() { _method = HttpMethod.Delete, _uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
    public static HttpRequestBuilder Delete(Uri uri) => new() { _method = HttpMethod.Delete, _uri = uri };

    public HttpRequestBuilder WithMethod(HttpMethod method)
    {
        _method = method;
        return this;
    }

    public HttpRequestBuilder WithUri(Uri uri)
    {
        _uri = uri;
        return this;
    }

    public HttpRequestBuilder WithUri(string uri)
    {
        _uri = new Uri(uri, UriKind.RelativeOrAbsolute);
        return this;
    }

    public HttpRequestBuilder WithHeader(string name, string value)
    {
        _headers.Add((name, value));
        return this;
    }

    public HttpRequestBuilder WithHeaders(Dictionary<string, string> headers)
    {
        foreach (KeyValuePair<string, string> header in headers)
            _headers.Add((header.Key, header.Value));
        return this;
    }

    public HttpRequestBuilder WithQuery(string name, string value)
    {
        _queryParameters.Add((name, value));
        return this;
    }

    public HttpRequestBuilder WithQuery(Dictionary<string, string?> parameters)
    {
        foreach (KeyValuePair<string, string?> parameter in parameters)
        {
            if (parameter.Value is not null)
                _queryParameters.Add((parameter.Key, parameter.Value));
        }
        return this;
    }

    public HttpRequestBuilder WithContent(HttpContent content)
    {
        _content = content;
        return this;
    }

    public HttpRequestBuilder WithJsonContent(object value, JsonSerializerOptions? options = null)
    {
        _content = JsonContent.Create(value, options: options ?? EnhancedJsonSerializerOptions.DefaultOptions);
        return this;
    }

    public Result<HttpRequestMessage> Build()
    {
        if (_uri is null)
            return Error.Validation("HttpRequestBuilder.UriRequired", "URI must be set before building the request.");

        Result<Uri> uriResult = _queryParameters.Count > 0
            ? _uri.WithQueryString(_queryParameters.ToDictionary(p => p.Key, p => (string?)p.Value))
            : _uri!;

        if (uriResult.IsFailure)
            return uriResult.Errors;

        var request = new HttpRequestMessage(_method, uriResult.Value);

        foreach ((string key, string value) in _headers)
            request.Headers.TryAddWithoutValidation(key, value);

        if (_content is not null)
            request.Content = _content;

        return request;
    }

    public async Task<Result> AsResultAsync(HttpClient client, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        using HttpRequestMessage request = buildResult.Value;
        return await client.SendAsResultAsync(request, cancellationToken);
    }

    public async Task<Result<T>> AsResultAsync<T>(HttpClient client, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        using HttpRequestMessage request = buildResult.Value;
        return await client.SendAsResultAsync<T>(request, options, cancellationToken);
    }
}
