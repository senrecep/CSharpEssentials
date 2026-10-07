using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Settings for <c>UseIdempotency()</c>, configured through <c>AddIdempotency</c>.
/// </summary>
public sealed class IdempotencyOptions
{
    /// <summary>Request header that carries the key. Default: <c>Idempotency-Key</c>.</summary>
    public string HeaderName { get; set; } = "Idempotency-Key";

    /// <summary>Response header added to replayed responses, with the value <c>true</c>. Default: <c>Idempotency-Replayed</c>.</summary>
    public string ReplayedHeaderName { get; set; } = "Idempotency-Replayed";

    /// <summary>HTTP methods that take part. Default: <c>POST</c> and <c>PATCH</c>.</summary>
    public ISet<string> Methods { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { HttpMethods.Post, HttpMethods.Patch };

    /// <summary>
    /// How long a reservation blocks the key while its request runs. A request still running after this time no
    /// longer blocks a retry. Default: 1 minute.
    /// </summary>
    public TimeSpan InFlightTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>How long a completed response is replayed. Default: 24 hours.</summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// <c>Retry-After</c> sent with the <c>409</c> for a key that is in flight, rounded up to whole seconds.
    /// <see langword="null"/> omits the header. Default: 1 second.
    /// </summary>
    public TimeSpan? RetryAfter { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Longest accepted key; a longer one gets <c>400</c>. Default: 255.</summary>
    public int MaxKeyLength { get; set; } = 255;

    /// <summary>
    /// Largest response body that is stored, in bytes. A larger response is sent unchanged, not stored, and its
    /// reservation is released. Default: 1 MiB.
    /// </summary>
    public long MaxResponseBodySize { get; set; } = 1024 * 1024;

    /// <summary>
    /// Rejects requests to idempotent endpoints that carry no key with <c>400</c>. Default: <see langword="false"/>,
    /// such requests run without idempotency.
    /// </summary>
    public bool RequireKey { get; set; }

    /// <summary>
    /// Decides from the status code whether a response is stored and replayed. Responses that are not stored release
    /// the key, so the client can retry with it. Default: 2xx only.
    /// </summary>
    public Func<int, bool> ShouldStore { get; set; } = static statusCode => statusCode is >= 200 and < 300;

    /// <summary>
    /// Returns the scope that prefixes the key in the store, usually the user (and tenant) id, so clients cannot
    /// replay each other's responses. When it returns <see langword="null"/>, the request runs without idempotency
    /// unless <see cref="AllowUnscopedKeys"/> is set. Default: <see cref="DefaultKeyScope"/>.
    /// </summary>
    public Func<HttpContext, string?> KeyScope { get; set; } = DefaultKeyScope;

    /// <summary>
    /// Uses the bare key when <see cref="KeyScope"/> returns <see langword="null"/>. Any client that sends the same
    /// key then gets the same response. Default: <see langword="false"/>.
    /// </summary>
    public bool AllowUnscopedKeys { get; set; }

    /// <summary>
    /// Response headers stored and replayed. <c>Set-Cookie</c> is never stored.
    /// Default: <c>Content-Type</c>, <c>Content-Language</c>, <c>Location</c>, <c>ETag</c>, <c>Cache-Control</c>.
    /// <c>Content-Encoding</c> and <c>Vary</c> are left out because response compression running outside this
    /// middleware sets them on the compressed stream while the stored body is uncompressed; add them only when the
    /// handler encodes the body itself.
    /// </summary>
    public ISet<string> ReplayedHeaders { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type",
        "Content-Language",
        "Location",
        "ETag",
        "Cache-Control",
    };

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(HeaderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ReplayedHeaderName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(InFlightTimeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(RetentionPeriod, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(RetryAfter ?? TimeSpan.Zero, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MaxKeyLength, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(MaxResponseBodySize);
        // The response is buffered in one array.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(MaxResponseBodySize, Array.MaxLength);
        ArgumentNullException.ThrowIfNull(ShouldStore);
        ArgumentNullException.ThrowIfNull(KeyScope);
    }

    internal Action<IServiceCollection> StoreRegistration { get; private set; } =
        static services => services.TryAddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

    /// <summary>
    /// The authenticated user's <see cref="ClaimTypes.NameIdentifier"/> or <c>sub</c> claim; <see langword="null"/>
    /// for anonymous requests.
    /// </summary>
    public static string? DefaultKeyScope(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ClaimsPrincipal user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            return null;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
    }

    /// <summary>Uses <see cref="InMemoryIdempotencyStore"/> (singleton). Correct for a single instance. This is the default.</summary>
    public IdempotencyOptions UseInMemoryStore()
    {
        StoreRegistration = static services =>
            services.Replace(ServiceDescriptor.Singleton<IIdempotencyStore, InMemoryIdempotencyStore>());
        return this;
    }

    /// <summary>
    /// Uses <see cref="DistributedCacheIdempotencyStore"/> (singleton) on the registered <c>IDistributedCache</c>.
    /// Concurrent requests with the same key are detected on a best-effort basis only.
    /// </summary>
    public IdempotencyOptions UseDistributedCacheStore()
    {
        StoreRegistration = static services =>
            services.Replace(ServiceDescriptor.Singleton<IIdempotencyStore, DistributedCacheIdempotencyStore>());
        return this;
    }

    /// <summary>Uses <typeparamref name="TStore"/> with the given lifetime. Default: scoped.</summary>
    public IdempotencyOptions UseStore<TStore>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TStore : class, IIdempotencyStore
    {
        StoreRegistration = services =>
            services.Replace(ServiceDescriptor.Describe(typeof(IIdempotencyStore), typeof(TStore), lifetime));
        return this;
    }
}
