using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
    private Func<HttpContent>? _jsonContent;
    private bool _followRedirects;
    private int _maxRedirects = 5;
    private string? _idempotencyKey;

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

    /// <summary>
    /// Sends <paramref name="content"/> as the request body. The builder keeps ownership: <see cref="AsResultAsync(HttpClient?, CancellationToken)"/>
    /// does not dispose it, so the same builder can send it again, for example on a retry. Content that can be read only once,
    /// such as a <see cref="StreamContent"/> over a forward-only stream, cannot be sent a second time. Dispose the content when
    /// the builder is no longer used.
    /// </summary>
    /// <param name="content">The request body.</param>
    /// <returns>This builder.</returns>
    public HttpRequestBuilder WithContent(HttpContent content)
    {
        _content = content;
        _jsonContent = null;
        return this;
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public HttpRequestBuilder WithJsonContent(object value, JsonSerializerOptions? options = null)
    {
        _jsonContent = value is null ? null : () => JsonContent.Create(value, options: options ?? ResolveJsonOptions());
        _content = null;
        return this;
    }

    // A distinct name, not a WithJsonContent overload: WithJsonContent(dto, null) would bind to it and throw.
    public HttpRequestBuilder WithJsonTypeInfoContent<T>(T value, JsonTypeInfo<T> typeInfo)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(typeInfo);
#else
        if (typeInfo is null)
            throw new ArgumentNullException(nameof(typeInfo));
#endif
        _jsonContent = value is null ? null : () => JsonContent.Create(value, typeInfo);
        _content = null;
        return this;
    }

    /// <summary>
    /// Sends an <c>Idempotency-Key</c> header. The key is fixed now, not when the request is built: every
    /// <see cref="Build"/> of this builder, and so every retry that builds the request again, sends the same key.
    /// When <paramref name="key"/> is <see langword="null"/> a new key is generated (a UUIDv7 on .NET 9 and later, a
    /// random UUID on netstandard2.1); pass a deterministic key, for example from <c>IdempotencyKeys.For(...)</c>, when
    /// the operation can be started again from scratch. A new logical operation needs a new builder or a new key.
    /// </summary>
    /// <param name="key">The key to send, or <see langword="null"/> to generate one.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is empty or contains a character other than visible ASCII, or contains <c>"</c> or <c>\</c>.
    /// </exception>
    public HttpRequestBuilder WithIdempotencyKey(string? key = null)
    {
        _idempotencyKey = key is null ? IdempotencyHttpProtocol.NewKey() : IdempotencyHttpProtocol.Validate(key, nameof(key));
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
            uriResult = ResolveUri(_uri, conventions);
        }
        catch (EnumValueException exception)
        {
            return Error.Validation("HttpRequestBuilder.InvalidEnumValue", exception.Message);
        }

        if (uriResult.IsFailure)
            return uriResult.Errors;

        var request = new HttpRequestMessage(_method, uriResult.Value);

        if (_jsonContent is not null)
            request.Content = _jsonContent();
        else if (_content is not null)
            request.Content = _content;

        var replacedContentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string value) in _headers)
        {
            if (!request.Headers.TryAddWithoutValidation(key, value) && request.Content is not null)
            {
                if (replacedContentHeaders.Add(key))
                    request.Content.Headers.Remove(key);

                request.Content.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (_idempotencyKey is not null)
            request.SetIdempotencyKey(_idempotencyKey);

        return request;
    }

    private static readonly ConditionalWeakTable<EnumConventions, ConcurrentDictionary<EnumWireFormat, JsonSerializerOptions>> JsonOptionsCache = [];

    private Result<Uri> ResolveUri(Uri uri, EnumConventions conventions)
    {
        string? template = null;
        if (_routeValues.Count > 0)
        {
            Result<string> routed = ApplyRouteValues(uri.OriginalString, conventions);
            if (routed.IsFailure)
                return routed.Errors;
            template = routed.Value;
        }

        if (_queryParameters.Count == 0)
            return template is null ? uri : new Uri(template, UriKind.RelativeOrAbsolute);

        List<KeyValuePair<string, string?>> pairs = [];
        foreach ((string key, object? value) in _queryParameters)
            QueryStringExtensions.AddValues(pairs, key, value, conventions, _enumWriteAs);

        Result<string> query = QueryStringExtensions.BuildQuery(pairs);
        if (template is null || query.IsFailure)
            return uri.AppendQuery(query);

        return QueryStringExtensions.AppendQuery(template, query.Value, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    private Result<string> ApplyRouteValues(string template, EnumConventions conventions)
    {
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

        return template;
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    private JsonSerializerOptions ResolveJsonOptions()
    {
        if (_enumConventions is null)
            return EnhancedJsonSerializerOptions.DefaultOptions;

        EnumConventions conventions = _enumConventions;
        return JsonOptionsCache.GetOrCreateValue(conventions).GetOrAdd(_enumWriteAs ?? conventions.WriteAs, writeAs =>
            EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters.Create(options =>
            {
                options.AddEnumConventions(conventions, EnumReadMode.Data, writeAs);
                options.Converters.Add(new MultiFormatDateTimeConverterFactory());
                options.Converters.Add(new PolymorphicJsonConverterFactory());
            }));
    }

    public async Task<Result> AsResultAsync(HttpClient? client, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        HttpRequestMessage request = buildResult.Value;
        try
        {
            if (_followRedirects)
                return await client.SendWithRedirectsAsResultAsync(request, _maxRedirects, cancellationToken).ConfigureAwait(false);

            return await client.SendAsResultAsync(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Release(request);
        }
    }

    [RequiresUnreferencedCode(HttpJson.SerializationMessage)]
    [RequiresDynamicCode(HttpJson.SerializationMessage)]
    public async Task<Result<T>> AsResultAsync<T>(HttpClient? client, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        HttpRequestMessage request = buildResult.Value;
        try
        {
            if (_followRedirects)
                return await client.SendWithRedirectsAsResultAsync<T>(request, options, _maxRedirects, cancellationToken).ConfigureAwait(false);

            return await client.SendAsResultAsync<T>(request, options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Release(request);
        }
    }

    private void Release(HttpRequestMessage request)
    {
        if (ReferenceEquals(request.Content, _content))
            request.Content = null;

        request.Dispose();
    }

    public async Task<Result<T>> AsResultAsync<T>(HttpClient? client, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        if (client is null)
            return Error.Validation("HttpRequestBuilder.ClientRequired", "HttpClient cannot be null.");

        Result<HttpRequestMessage> buildResult = Build();
        if (buildResult.IsFailure)
            return buildResult.Errors;

        HttpRequestMessage request = buildResult.Value;
        try
        {
            if (_followRedirects)
                return await client.SendWithRedirectsAsResultAsync(request, typeInfo, _maxRedirects, cancellationToken).ConfigureAwait(false);

            return await client.SendAsResultAsync(request, typeInfo, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Release(request);
        }
    }
}
