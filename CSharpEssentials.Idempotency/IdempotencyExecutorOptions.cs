namespace CSharpEssentials.Idempotency;

/// <summary>Default settings and named profiles of <see cref="IdempotencyExecutor"/>, configured through <c>AddIdempotencyExecutor</c>.</summary>
public sealed class IdempotencyExecutorOptions
{
    /// <summary>
    /// Name of the built-in profile that <see cref="IdempotencyInbox"/> uses: a retention of 7 days, so a message
    /// redelivered by the broker or resent from a dead-letter queue days later is still recognized. Registering a
    /// profile with this name replaces it.
    /// </summary>
    public const string MessagingProfile = "messaging";

    private readonly Dictionary<string, IdempotencyExecutionOptions> _profiles = [with(StringComparer.OrdinalIgnoreCase)];
    private readonly HashSet<string> _registered = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>Creates the defaults with the built-in <see cref="MessagingProfile"/>.</summary>
    public IdempotencyExecutorOptions() =>
        _profiles[MessagingProfile] = new IdempotencyExecutionOptions { Retention = TimeSpan.FromDays(7) };

    /// <summary>The mode of operations that do not set one. Default: <see cref="IdempotencyMode.Enforce"/>.</summary>
    public IdempotencyMode Mode { get; set; } = IdempotencyMode.Enforce;

    /// <summary>How long a stored result is replayed. Default: 24 hours.</summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// How long a reservation blocks the key while its operation runs, and how long the key stays in flight after the
    /// operation threw or its result could not be stored. Keep it longer than the operation's timeout and the caller's
    /// total retry budget. Must be positive and at most <see cref="Retention"/> and <see cref="MaxLease"/>.
    /// Default: 1 minute.
    /// </summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long a result classified as <see cref="IdempotencyResultHandling.StoreAsFailure"/> is replayed. After it
    /// expires a retry runs the operation again. Must be positive and at most <see cref="Retention"/>.
    /// Default: 10 minutes.
    /// </summary>
    public TimeSpan FailureRetention { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Upper bound for every <see cref="Lease"/>, so a key held after a failure cannot block its caller for long. Default: 15 minutes.</summary>
    public TimeSpan MaxLease { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>The registered profiles, including <see cref="MessagingProfile"/>.</summary>
    public IReadOnlyDictionary<string, IdempotencyExecutionOptions> Profiles => _profiles;

    /// <summary>
    /// Registers a named set of settings that an operation selects with <see cref="IdempotencyExecutionOptions.Profile"/>.
    /// A setting the profile leaves <see langword="null"/> comes from these options; a setting the operation gives wins
    /// over the profile. The merged settings are validated by <c>AddIdempotencyExecutor</c>.
    /// </summary>
    /// <exception cref="ArgumentException">The name is empty or already registered, or the profile names another profile.</exception>
    public IdempotencyExecutorOptions AddProfile(string name, Action<IdempotencyExecutionOptions> configure)
    {
        Guard.NotNullOrEmpty(name);
        Guard.NotNull(configure);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The profile name cannot be white space.", nameof(name));
        if (!_registered.Add(name))
            throw new ArgumentException($"The idempotency profile '{name}' is already registered.", nameof(name));

        IdempotencyExecutionOptions profile = new();
        configure(profile);
        if (profile.Profile is not null)
        {
            _registered.Remove(name);
            throw new ArgumentException($"The idempotency profile '{name}' cannot name another profile.", nameof(configure));
        }

        _profiles[name] = profile;
        return this;
    }

    /// <summary>Validates these options and every profile merged with them.</summary>
    /// <exception cref="InvalidOperationException">The options or a profile are invalid.</exception>
    internal void Validate()
    {
        IdempotencyExecutionSettings.Resolve(this, null, "the idempotency executor options");
        foreach (KeyValuePair<string, IdempotencyExecutionOptions> profile in _profiles)
            IdempotencyExecutionSettings.Resolve(this, profile.Value, $"the idempotency profile '{profile.Key}'");
    }
}
