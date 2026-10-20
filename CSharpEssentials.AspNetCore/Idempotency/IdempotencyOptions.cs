using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Settings for <c>UseIdempotency()</c>, configured through <c>AddIdempotency</c> and read through
/// <c>IOptionsMonitor&lt;IdempotencyOptions&gt;</c>, so a configuration reload (for example of <see cref="Mode"/>)
/// applies to the next request without a restart. The store chosen with a <c>Use…Store()</c> call is fixed when
/// <c>AddIdempotency</c> runs.
/// </summary>
public sealed class IdempotencyOptions
{
    /// <summary>
    /// How idempotent endpoints that set no mode of their own (<see cref="IdempotencyEndpointOptions.Mode"/>, or that of
    /// their profile) use their keys. <see cref="IdempotencyMode.Off"/> ignores the header and never touches the store.
    /// <see cref="IdempotencyMode.Observe"/> reserves and stores like <see cref="IdempotencyMode.Enforce"/> but never
    /// answers differently from <see cref="IdempotencyMode.Off"/>: no replay, no <c>Idempotency-Replayed</c>, no
    /// <c>400</c>/<c>409</c>/<c>422</c>; it logs and counts what <see cref="IdempotencyMode.Enforce"/> would have done.
    /// Observe measures duplicates, it does not prevent them; keep it short for operations that move money. A record
    /// stored in Observe is replayed once the mode becomes Enforce; an <see cref="ITransactionalIdempotencyRecordStore"/>
    /// stores it in the transaction open around <c>UseIdempotency</c>, so a response whose transaction rolled back is
    /// never replayed. This switch is separate from
    /// <c>IdempotencyExecutorOptions.Mode</c>, which Mediator and message consumers read. Default:
    /// <see cref="IdempotencyMode.Enforce"/>.
    /// </summary>
    public IdempotencyMode Mode { get; set; } = IdempotencyMode.Enforce;

    /// <summary>
    /// Request header that carries the key, either bare (<c>abc</c>) or as a structured-field string (<c>"abc"</c>);
    /// both name the same record. A key must be 1 to <see cref="MaxKeyLength"/> printable ASCII characters other than
    /// space, <c>"</c> and <c>\</c> (<see cref="IdempotencyKeys.IsValid"/>). Default: <c>Idempotency-Key</c>.
    /// </summary>
    public string HeaderName { get; set; } = "Idempotency-Key";

    /// <summary>Response header added to replayed responses, with the value <c>true</c>. Default: <c>Idempotency-Replayed</c>.</summary>
    public string ReplayedHeaderName { get; set; } = "Idempotency-Replayed";

    /// <summary>HTTP methods that take part. Default: <c>POST</c> and <c>PATCH</c>.</summary>
    public ISet<string> Methods { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { HttpMethods.Post, HttpMethods.Patch };

    /// <summary>
    /// The lease: how long a reservation blocks the key while its request runs. A request still running after this
    /// time no longer blocks a retry. It is also how long the key stays in flight after an outcome that is neither
    /// stored nor released (an exception or abort after the handler started, a status in
    /// <see cref="NeverStoreStatusCodes"/>, a response that cannot be stored). Keep it longer than the request timeout
    /// and the client's total retry budget. Must be positive and at most <see cref="RetentionPeriod"/> and
    /// <see cref="MaxInFlightTimeout"/>. Default: 1 minute.
    /// </summary>
    public TimeSpan InFlightTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Upper bound for <see cref="InFlightTimeout"/>, so a key held after a failure cannot lock its client out for
    /// long. Default: 15 minutes.
    /// </summary>
    public TimeSpan MaxInFlightTimeout { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>How long a stored response other than 5xx is replayed. Default: 24 hours.</summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// How long a stored 5xx response is replayed. After it expires a retry with the same key runs the handler again,
    /// which repeats any side effect the failed run already caused; set it to <see cref="RetentionPeriod"/> for the
    /// strictest behaviour. Must be positive and at most <see cref="RetentionPeriod"/>. Default: 10 minutes.
    /// </summary>
    public TimeSpan ServerErrorRetention { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// <c>Retry-After</c> sent with every <c>409</c> <c>Idempotency.InFlight</c>, rounded up to whole seconds (at least
    /// 1). Must be positive. Default: 1 second.
    /// </summary>
    public TimeSpan RetryAfter { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Longest accepted key, counted after an <c>sf-string</c> value (<c>"abc"</c>) is unquoted; a longer one gets
    /// <c>400</c> <c>Idempotency.KeyInvalid</c>. Default: 255.
    /// </summary>
    public int MaxKeyLength { get; set; } = IdempotencyKeys.MaxKeyLength;

    /// <summary>
    /// Largest response body that is stored, in bytes. A larger response is sent unchanged and not stored: a 4xx
    /// releases the key, any other status keeps it in flight until <see cref="InFlightTimeout"/>, because the handler
    /// ran and a retry must not run it again at once. Default: 1 MiB.
    /// </summary>
    public long MaxResponseBodySize { get; set; } = 1024 * 1024;

    /// <summary>
    /// Rejects requests to idempotent endpoints that carry no key with <c>400</c> <c>Idempotency.KeyMissing</c>.
    /// Default: <see langword="false"/>, such requests run without idempotency.
    /// </summary>
    public bool RequireKey { get; set; }

    /// <summary>
    /// Decides whether a response is stored and replayed; when it returns <see langword="false"/> the key is released,
    /// so the client can retry with it. It is asked only after the handler started and only for statuses outside
    /// <see cref="NeverStoreStatusCodes"/>: a request whose handler did not start is always released, and a status in
    /// <see cref="NeverStoreStatusCodes"/> keeps the key in flight. On an endpoint without the execution filter
    /// (<see cref="IdempotencyFilterInstalledMetadata"/>) it is asked for 2xx only, with
    /// <see cref="IdempotencyOutcome.HandlerStarted"/> <see langword="false"/>; other statuses release the key. A stored response that cannot be captured (encoded
    /// by the handler, <c>text/event-stream</c>, larger than <see cref="MaxResponseBodySize"/>) is not stored either;
    /// it keeps the key in flight unless its status is 4xx. Default: <see cref="IdempotencyStorePolicies.Default"/>.
    /// </summary>
    public Func<IdempotencyOutcome, bool> ShouldStore { get; set; } = IdempotencyStorePolicies.Default;

    /// <summary>
    /// Statuses that are never stored because they mean "retry later". Returned after the handler started (for
    /// example a request timeout that hit the handler), they keep the key in flight until
    /// <see cref="InFlightTimeout"/>; returned before it, they release the key. Must not be empty; every entry must be
    /// 100 to 599. Default: 503 and 504.
    /// </summary>
    public ISet<int> NeverStoreStatusCodes { get; } = new HashSet<int>
    {
        StatusCodes.Status503ServiceUnavailable,
        StatusCodes.Status504GatewayTimeout,
    };

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> on requests to an idempotent endpoint that lacks the filter
    /// which sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/> (<see cref="IdempotencyFilterInstalledMetadata"/>),
    /// instead of logging a warning once per endpoint. Without the filter only 2xx responses of that endpoint are
    /// stored; every other outcome releases its key. Default: <see langword="false"/>.
    /// </summary>
    public bool RequireExecutionFilter { get; set; }

    /// <summary>
    /// Namespace of the idempotency records written by this middleware, so HTTP keys never meet the keys of message
    /// consumers or jobs that share the store. Default: <c>http</c>.
    /// </summary>
    public string Namespace { get; set; } = "http";

    /// <summary>
    /// Resolves the scope that owns the key from <see cref="HttpContext.User"/>, usually the user (and tenant), so
    /// clients cannot replay each other's responses. <see cref="IdempotencyScope.Global"/> shares the key between all
    /// callers. When it returns <see langword="null"/> (unresolved, for example an anonymous request),
    /// <see cref="UnscopedKeyBehavior"/> applies. Machine principals (client credentials) need
    /// <see cref="IdempotencyScopes.Client"/>. Default: <see cref="IdempotencyScopes.User"/>.
    /// </summary>
    public IIdempotencyScopeResolver ScopeResolver { get; set; } = IdempotencyScopes.User;

    /// <summary>
    /// What happens when a request carries a key but <see cref="ScopeResolver"/> cannot resolve its scope:
    /// <see cref="UnscopedKeyBehavior.Skip"/> runs it without idempotency and logs a warning
    /// (<c>Idempotency.UnscopedKeySkipped</c>), <see cref="UnscopedKeyBehavior.Reject"/> answers <c>400</c>
    /// <c>Idempotency.ScopeRequired</c>, <see cref="UnscopedKeyBehavior.Global"/> uses a key shared by every caller.
    /// Default: <see cref="UnscopedKeyBehavior.Skip"/>.
    /// </summary>
    public UnscopedKeyBehavior UnscopedKeyBehavior { get; set; } = UnscopedKeyBehavior.Skip;

    /// <summary>
    /// Response headers stored and replayed. <c>Set-Cookie</c> is never stored.
    /// Default: <c>Content-Type</c>, <c>Content-Language</c>, <c>Location</c>, <c>ETag</c>, <c>Cache-Control</c>,
    /// <c>Retry-After</c>. <c>Content-Encoding</c> and <c>Vary</c> are left out because response compression running
    /// outside this middleware sets them on the compressed stream while the stored body is uncompressed. A response
    /// whose body the handler encodes itself is not stored at all.
    /// </summary>
    public ISet<string> ReplayedHeaders { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type",
        "Content-Language",
        "Location",
        "ETag",
        "Cache-Control",
        "Retry-After",
    };

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(HeaderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ReplayedHeaderName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(InFlightTimeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(RetentionPeriod, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MaxInFlightTimeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(InFlightTimeout, RetentionPeriod);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(InFlightTimeout, MaxInFlightTimeout);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ServerErrorRetention, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ServerErrorRetention, RetentionPeriod);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(RetryAfter, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MaxKeyLength, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(MaxResponseBodySize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(MaxResponseBodySize, Array.MaxLength);
        ArgumentNullException.ThrowIfNull(ShouldStore);
        ArgumentOutOfRangeException.ThrowIfZero(NeverStoreStatusCodes.Count);
        foreach (int neverStoreStatusCode in NeverStoreStatusCodes)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(neverStoreStatusCode, 100);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(neverStoreStatusCode, 599);
        }

        uint mode = (uint)Mode;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mode, (uint)IdempotencyMode.Enforce);
        uint unscopedKeyBehavior = (uint)UnscopedKeyBehavior;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(unscopedKeyBehavior, (uint)UnscopedKeyBehavior.Global);
        ArgumentException.ThrowIfNullOrEmpty(Namespace);
        ArgumentNullException.ThrowIfNull(ScopeResolver);
        foreach (KeyValuePair<string, IdempotencyEndpointOptions> profile in Profiles)
            IdempotencyEndpointSettings.Resolve(this, profile.Value, $"the idempotency profile '{profile.Key}'");
    }

    internal Dictionary<string, IdempotencyEndpointOptions> Profiles { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// Registers a named set of settings that endpoints select with <c>.WithIdempotency("name")</c>,
    /// <c>[Idempotent(Profile = "name")]</c> or <see cref="IdempotencyEndpointOptions.Profile"/>. A setting the
    /// profile leaves <see langword="null"/> comes from these options; a setting the endpoint gives wins over the
    /// profile. Names are compared case-insensitively. The profile merged with these options is validated in
    /// <c>AddIdempotency</c> and throws <see cref="InvalidOperationException"/> when invalid, for example when its
    /// <see cref="IdempotencyEndpointOptions.ServerErrorRetention"/> exceeds its retention.
    /// </summary>
    /// <exception cref="ArgumentException">The name is empty, already registered, or the profile names another profile.</exception>
    public IdempotencyOptions AddProfile(string name, Action<IdempotencyEndpointOptions> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);
        if (Profiles.ContainsKey(name))
            throw new ArgumentException($"The idempotency profile '{name}' is already registered.", nameof(name));

        IdempotencyEndpointOptions profile = new();
        configure(profile);
        if (profile.Profile is not null)
            throw new ArgumentException($"The idempotency profile '{name}' cannot select another profile ('{profile.Profile}').", nameof(configure));

        Profiles.Add(name, profile);
        return this;
    }

    internal Action<IServiceCollection> StoreRegistration { get; private set; } =
        static services => services.TryAddSingleton<IIdempotencyRecordStore, InMemoryIdempotencyRecordStore>();

    /// <summary>
    /// Uses <see cref="InMemoryIdempotencyRecordStore"/> (singleton). Correct for a single instance. This is the default.
    /// </summary>
    public IdempotencyOptions UseInMemoryStore()
    {
        StoreRegistration = static services =>
            services.Replace(ServiceDescriptor.Singleton<IIdempotencyRecordStore, InMemoryIdempotencyRecordStore>());
        return this;
    }

    /// <summary>
    /// Uses <see cref="DistributedCacheIdempotencyRecordStore"/> (singleton) on the registered <c>IDistributedCache</c>.
    /// Meant for a single instance or development: concurrent requests with the same key are detected on a best-effort
    /// basis only.
    /// </summary>
    public IdempotencyOptions UseDistributedCacheStore()
    {
        StoreRegistration = static services =>
            services.Replace(ServiceDescriptor.Singleton<IIdempotencyRecordStore, DistributedCacheIdempotencyRecordStore>());
        return this;
    }

    /// <summary>Uses <typeparamref name="TStore"/> with the given lifetime. Default: scoped.</summary>
    public IdempotencyOptions UseStore<TStore>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TStore : class, IIdempotencyRecordStore
    {
        StoreRegistration = services =>
            services.Replace(ServiceDescriptor.Describe(typeof(IIdempotencyRecordStore), typeof(TStore), lifetime));
        return this;
    }
}
