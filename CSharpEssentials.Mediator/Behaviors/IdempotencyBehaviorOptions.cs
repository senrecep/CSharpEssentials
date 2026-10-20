using System.Text.Json;

using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Mediator;

/// <summary>Settings of <see cref="IdempotencyBehavior{TRequest, TResponse}"/>, configured through <c>AddMediatorIdempotencyBehavior</c>.</summary>
public sealed class IdempotencyBehaviorOptions
{
    private readonly Dictionary<Type, IdempotencyExecutionOptions> _requests = [];

    /// <summary>Where the behavior runs relative to the transaction behavior. Default: <see cref="IdempotencyPlacement.OutsideTransaction"/>.</summary>
    public IdempotencyPlacement Placement { get; set; } = IdempotencyPlacement.OutsideTransaction;

    /// <summary>
    /// Finds the owner of a key that the request does not scope itself, from
    /// <see cref="IIdempotencyScopeAccessor.Principal"/>. Default: <see cref="IdempotencyScopes.User"/>. For machine
    /// callers use <see cref="IdempotencyScopes.Client"/>.
    /// </summary>
    public IIdempotencyScopeResolver ScopeResolver { get; set; } = IdempotencyScopes.User;

    /// <summary>
    /// What happens to a request whose scope cannot be resolved. Default: <see cref="Idempotency.UnscopedKeyBehavior.Reject"/>,
    /// which returns <see cref="IdempotencyErrorCodes.ScopeRequired"/>. The HTTP middleware defaults to
    /// <see cref="Idempotency.UnscopedKeyBehavior.Skip"/> instead.
    /// </summary>
    public UnscopedKeyBehavior UnscopedKeyBehavior { get; set; } = UnscopedKeyBehavior.Reject;

    /// <summary>
    /// Whether a failed <c>Result</c> of a request that is not an <see cref="ITransactionalRequest"/> is stored and
    /// replayed. A failure whose first error is <see cref="Errors.ErrorType.Unexpected"/> is kept for
    /// <see cref="IdempotencyExecutorOptions.FailureRetention"/>, any other failure as long as a success. With
    /// <see langword="false"/> every failure releases the key. Default: <see langword="true"/>.
    /// </summary>
    public bool StoreFailures { get; set; } = true;

    /// <summary>
    /// Serializes the request for its default fingerprint and the response for the record store.
    /// Default: <see langword="null"/>, the <see cref="JsonSerializer"/> defaults.
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>
    /// Sets the idempotency settings of <typeparamref name="TRequest"/>, for example a profile, a retention or a mode.
    /// A request instance's <see cref="IIdempotentRequest.IdempotencyRetention"/> and
    /// <see cref="IIdempotentRequest.IdempotencyLease"/> win over these; a setting left <see langword="null"/> comes
    /// from the profile, else from <see cref="IdempotencyExecutorOptions"/>. Calling it again for the same type
    /// configures the same settings.
    /// </summary>
    public IdempotencyBehaviorOptions ForRequest<TRequest>(Action<IdempotencyExecutionOptions> configure)
        where TRequest : IIdempotentRequest
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif
        if (!_requests.TryGetValue(typeof(TRequest), out IdempotencyExecutionOptions? settings))
        {
            settings = new IdempotencyExecutionOptions();
            _requests[typeof(TRequest)] = settings;
        }

        configure(settings);
        return this;
    }

    internal IdempotencyExecutionOptions? Find(Type requestType) =>
        _requests.TryGetValue(requestType, out IdempotencyExecutionOptions? settings) ? settings : null;
}
