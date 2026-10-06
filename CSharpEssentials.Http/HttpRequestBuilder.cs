using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public sealed class HttpRequestBuilder
{
    private HttpMethod _method = HttpMethod.Get;
    private Uri? _uri;
    private readonly List<(string Key, string Value)> _headers = [];
    private readonly List<(string Key, object? Value)> _queryParameters = [];
    private readonly List<(string Name, object? Value)> _routeValues = [];
    private EnumConventions? _enumConventions;
    private EnumWireFormat? _enumWriteAs;
    private HttpContent? _content;
    private bool _followRedirects;
    private int _maxRedirects = 5;

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

    public HttpRequestBuilder WithQuery(string name, object? value)
    {
        _queryParameters.Add((name, value));
        return this;
    }

    public HttpRequestBuilder WithRoute(string name, object? value)
    {
        _routeValues.Add((name, value));
        return this;
    }

    public HttpRequestBuilder WithEnumConventions(EnumConventions conventions, EnumWireFormat? writeAs = null)
    {
        _enumConventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        _enumWriteAs = writeAs;
        return this;
    }

    public HttpRequestBuilder WithContent(HttpContent content)
    {
        _content = content;
        return this;
    }

    public HttpRequestBuilder WithJsonContent(object value, JsonSerializerOptions? options = null)
    {
        _content = JsonContent.Create(value, options: options ?? ResolveJsonOptions());
        return this;
    }

    public HttpRequestBuilder FollowRedirects(int maxRedirects = 5)
    {
        _followRedirects = true;
        _maxRedirects = maxRedirects;
        return this;
    }

    public Result<HttpRequestMessage> Build()
    {
        if (_uri is null)
            return Error.Validation("HttpRequestBuilder.UriRequired", "URI must be set before building the request.");

        EnumConventions conventions = _enumConventions ?? EnumConventions.Default;
        Result<Uri> uriResult;
        try
        {
            uriResult = ApplyRouteValues(_uri, conventions);
            if (uriResult.IsSuccess && _queryParameters.Count > 0)
            {
                List<KeyValuePair<string, string?>> pairs = [];
                foreach ((string key, object? value) in _queryParameters)
                    QueryStringExtensions.AddValues(pairs, key, value, conventions, _enumWriteAs);

                uriResult = uriResult.Value.AppendQuery(QueryStringExtensions.BuildQuery(pairs));
            }
        }
        catch (EnumValueException exception)
        {
            return Error.Validation("HttpRequestBuilder.InvalidEnumValue", exception.Message);
        }

        if (uriResult.IsFailure)
            return uriResult.Errors;

        var request = new HttpRequestMessage(_method, uriResult.Value);

        foreach ((string key, string value) in _headers)
            request.Headers.TryAddWithoutValidation(key, value);

        if (_content is not null)
            request.Content = _content;

        return request;
    }

    private static readonly ConcurrentDictionary<(EnumConventions Conventions, EnumWireFormat? WriteAs), JsonSerializerOptions> JsonOptionsCache = new();

    private Result<Uri> ApplyRouteValues(Uri uri, EnumConventions conventions)
    {
        if (_routeValues.Count == 0)
            return uri;

        string template = uri.OriginalString;
        foreach ((string name, object? value) in _routeValues)
        {
            string placeholder = "{" + name + "}";
            if (!template.Contains(placeholder, StringComparison.Ordinal))
                return Error.Validation("HttpRequestBuilder.RouteParameterNotFound", $"The URI has no '{placeholder}' placeholder.");
            if (value is null)
                return Error.Validation("HttpRequestBuilder.RouteValueRequired", $"The route value '{name}' cannot be null.");

            string text = EnumValueFormatter.TryFormat(value, conventions, out string? formatted, _enumWriteAs)
                ? formatted
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            template = template.Replace(placeholder, Uri.EscapeDataString(text), StringComparison.Ordinal);
        }

        return new Uri(template, UriKind.RelativeOrAbsolute);
    }

    private JsonSerializerOptions ResolveJsonOptions() =>
        _enumConventions is null
            ? EnhancedJsonSerializerOptions.DefaultOptions
            : JsonOptionsCache.GetOrAdd((_enumConventions, _enumWriteAs), static key =>
                EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters.Create(options =>
                {
                    options.AddEnumConventions(key.Conventions, EnumReadMode.Data, key.WriteAs);
                    options.Converters.Add(new MultiFormatDateTimeConverterFactory());
                    options.Converters.Add(new PolymorphicJsonConverterFactory());
                }));

    public async Task<Result> AsResultAsync(HttpClient? client, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        using HttpRequestMessage request = buildResult.Value;
        if (_followRedirects)
            return await client.SendWithRedirectsAsResultAsync(request, _maxRedirects, cancellationToken);

        return await client.SendAsResultAsync(request, cancellationToken);
    }

    public async Task<Result<T>> AsResultAsync<T>(HttpClient? client, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        using HttpRequestMessage request = buildResult.Value;
        if (_followRedirects)
            return await client.SendWithRedirectsAsResultAsync<T>(request, options, _maxRedirects, cancellationToken);

        return await client.SendAsResultAsync<T>(request, options, cancellationToken);
    }
}
