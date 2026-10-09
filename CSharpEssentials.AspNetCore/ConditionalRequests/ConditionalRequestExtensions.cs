using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Conditional requests (RFC 9110 section 13): <c>ETag</c>/<c>Last-Modified</c> with <c>304 Not Modified</c> on reads, and
/// <c>If-Match</c> optimistic concurrency on writes.
/// </summary>
public static class ConditionalRequestExtensions
{
    /// <summary>
    /// Registers <see cref="ConditionalRequestOptions"/>, the default <see cref="IETagGenerator"/> (unless one is already
    /// registered) and the MVC filters of <see cref="ConditionalGetAttribute"/> and <see cref="IfMatchAttribute"/>.
    /// Calling it again replaces the options and adds nothing else.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configure">Changes the options.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddConditionalRequests(this IServiceCollection services, Action<ConditionalRequestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        ConditionalRequestOptions options = new();
        configure?.Invoke(options);
        services.Replace(ServiceDescriptor.Singleton(options));
        services.TryAddSingleton<IETagGenerator, DefaultETagGenerator>();
        services.TryAddSingleton<ResourceValidatorsResolver>();
        services.TryAddSingleton<ConditionalGetResultFilter>();
        services.TryAddSingleton<IfMatchResourceFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ConditionalRequestsMvcSetup>());
        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="TSource"/> (scoped, unless already registered) as the validator source of
    /// <typeparamref name="T"/>. It is used for values whose runtime type is <typeparamref name="T"/> or derives from it (the
    /// nearest registered base type wins; for the same type the last registration wins) and takes precedence over
    /// <see cref="IVersioned"/> and <see cref="IETagGenerator"/>. Interfaces are not matched: register the class.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="TSource">The source.</typeparam>
    /// <param name="services">The services.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> is an interface.</exception>
    public static IServiceCollection AddETagSource<T, TSource>(this IServiceCollection services)
        where TSource : class, IETagSource<T>
    {
        ArgumentNullException.ThrowIfNull(services);
        if (typeof(T).IsInterface)
            throw new ArgumentException(
                $"{typeof(T)} is an interface. Sources are matched by the runtime type of the value and its base classes, so register the class.");

        services.TryAddScoped<TSource>();
        services.AddSingleton(new ETagSourceRegistration(
            typeof(T),
            static (provider, value) => provider.GetRequiredService<TSource>().GetValidators((T)value)));
        return services;
    }

    /// <summary>
    /// Turns on conditional GET for an endpoint or group: a successful <c>GET</c>/<c>HEAD</c> response with a value (a returned
    /// object, a successful <c>Result&lt;T&gt;</c>, or an <see cref="IValueHttpResult"/> with a 2xx status) gets <c>ETag</c> and
    /// <c>Last-Modified</c> from the value's validators (<see cref="IETagSource{T}"/>, then <see cref="IETagGenerator"/>). A
    /// matching <c>If-None-Match</c> (weak comparison, <c>*</c> matches), or without it an <c>If-Modified-Since</c> not older than
    /// <c>Last-Modified</c>, returns <c>304 Not Modified</c> without a body. Headers already on the response (<c>Cache-Control</c>,
    /// <c>Vary</c>, <c>Content-Location</c>) are kept on the 304, but a replaced result does not run, so set such headers on
    /// <see cref="HttpContext.Response"/> rather than through the result. When the handler already set an <c>ETag</c> header the
    /// response is left alone (not overridden, no 304). <c>Last-Modified</c> is never later than now (<see cref="TimeProvider"/>
    /// from DI, else the system clock). Strings, <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> and failures pass through.
    /// Works with <see cref="ResultEndpointFilter"/> in either order. Requires <see cref="AddConditionalRequests"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint or group builder.</typeparam>
    /// <param name="builder">The endpoint or group builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static TBuilder WithConditionalGet<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ConditionalGetAttribute metadata = new();
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(metadata);
            if (!endpoint.FilterFactories.Contains(ConditionalGetEndpointFilter.Factory))
                endpoint.FilterFactories.Add(ConditionalGetEndpointFilter.Factory);
        });
        return builder;
    }

    /// <summary>
    /// Turns on <c>If-Match</c> handling for an endpoint or group. For <c>POST</c>, <c>PUT</c>, <c>PATCH</c> and <c>DELETE</c> the
    /// header is parsed before the handler into <see cref="Preconditions"/> (<see cref="GetPreconditions"/>): a malformed header
    /// returns 400, a missing one 428 when <paramref name="required"/> is set. The handler compares the current resource with
    /// <see cref="Preconditions.Matches(IVersioned?)"/> or <see cref="Preconditions.Matches(ResourceValidators?)"/> and returns
    /// <see cref="ConditionalRequestErrors.PreconditionFailed"/> (412) on a mismatch; for an atomic check pass
    /// <see cref="Preconditions.TryGetIfMatchVersion"/> as the original concurrency token of the update. Safe methods are not
    /// evaluated, and <c>If-Unmodified-Since</c> is not supported. An endpoint overrides its group.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint or group builder.</typeparam>
    /// <param name="builder">The endpoint or group builder.</param>
    /// <param name="required">When <see langword="true"/>, a request without <c>If-Match</c> returns 428.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static TBuilder WithIfMatch<TBuilder>(this TBuilder builder, bool required = false)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithIfMatch(new IfMatchMetadata(required, LoadCurrent: null));

    /// <summary>
    /// Like <see cref="WithIfMatch{TBuilder}(TBuilder, bool)"/>, and when the request has <c>If-Match</c> it also loads the
    /// current resource with <paramref name="loadCurrent"/> before the handler, computes its validators
    /// (<see cref="IETagSource{T}"/>, then <see cref="IETagGenerator"/>) and returns 412 when it does not exist or does not match
    /// (strong comparison). Not atomic: the resource can change between this check and the handler's write. Prefer passing
    /// <see cref="Preconditions.TryGetIfMatchVersion"/> as the concurrency token of the update. A weak ETag (such as the body hash
    /// of <see cref="ConditionalRequestOptions.UseBodyHashFallback"/>) never matches, so only <c>*</c> passes. Requires
    /// <see cref="AddConditionalRequests"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint or group builder.</typeparam>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The endpoint or group builder.</param>
    /// <param name="loadCurrent">Loads the current resource; <see langword="null"/> when it does not exist.</param>
    /// <param name="required">When <see langword="true"/>, a request without <c>If-Match</c> returns 428.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static TBuilder WithIfMatch<TBuilder, T>(
        this TBuilder builder,
        Func<HttpContext, CancellationToken, ValueTask<T?>> loadCurrent,
        bool required = false)
        where TBuilder : IEndpointConventionBuilder
        where T : class
    {
        ArgumentNullException.ThrowIfNull(loadCurrent);
        return builder.WithIfMatch(new IfMatchMetadata(
            required,
            async (httpContext, cancellationToken) => await loadCurrent(httpContext, cancellationToken).ConfigureAwait(false)));
    }

    /// <summary>
    /// The ETag the default <see cref="IETagGenerator"/> sends for <paramref name="versioned"/>: <c>"{Version}"</c> when the
    /// version is a valid entity tag, otherwise the base64url SHA-256 of it; <see langword="null"/> for an empty version.
    /// </summary>
    /// <param name="versioned">The resource.</param>
    /// <returns>The strong ETag, or <see langword="null"/>.</returns>
    public static EntityTagHeaderValue? ToETag(this IVersioned versioned)
    {
        ArgumentNullException.ThrowIfNull(versioned);
        return EntityTags.FromVersion(versioned.Version);
    }

    /// <summary>
    /// The <c>If-Match</c> precondition of the request, parsed by <see cref="WithIfMatch{TBuilder}(TBuilder, bool)"/> or
    /// <see cref="IfMatchAttribute"/>. Safe methods (<c>GET</c>, <c>HEAD</c>, ...) get an empty precondition.
    /// </summary>
    /// <param name="httpContext">The request.</param>
    /// <returns>The precondition.</returns>
    /// <exception cref="InvalidOperationException">The endpoint has neither <c>WithIfMatch</c> nor <see cref="IfMatchAttribute"/>.</exception>
    public static Preconditions GetPreconditions(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return httpContext.Features.Get<Preconditions>()
            ?? throw new InvalidOperationException(
                "The preconditions are not available. Add WithIfMatch() to the endpoint or [IfMatch] to the action (with services.AddConditionalRequests() for MVC).");
    }

    private static TBuilder WithIfMatch<TBuilder>(this TBuilder builder, IfMatchMetadata metadata)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(metadata);
            if (!endpoint.FilterFactories.Contains(IfMatchEndpointFilter.Factory))
                endpoint.FilterFactories.Add(IfMatchEndpointFilter.Factory);
        });
        return builder;
    }

    private sealed class ConditionalRequestsMvcSetup : IConfigureOptions<MvcOptions>
    {
        void IConfigureOptions<MvcOptions>.Configure(MvcOptions options)
        {
            options.Filters.AddService<ConditionalGetResultFilter>();
            options.Filters.AddService<IfMatchResourceFilter>();
        }
    }
}
