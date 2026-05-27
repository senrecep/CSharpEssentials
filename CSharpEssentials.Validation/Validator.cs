using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Validation;

/// <summary>
/// Base class for model validators. Override <see cref="Configure"/> to define validation rules.
/// Both synchronous and asynchronous rules are supported — the method returns <see cref="ValueTask"/>
/// so validators with only synchronous rules incur zero heap allocation.
/// </summary>
/// <typeparam name="T">The model type to validate.</typeparam>
public abstract class Validator<T> : IValidator<T>
{
    /// <inheritdoc/>
    public ValueTask<Result<T>> ValidateAsync(T instance, CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(instance);
#else
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));
#endif
        RuleContext<T> ctx = new();
        ValueTask configTask = Configure(instance, ctx, ct);
        if (configTask.IsCompletedSuccessfully)
            return new ValueTask<Result<T>>(Validator.BuildResult(instance, ctx));
        return FinishAsync(instance, ctx, configTask);
    }

    private static async ValueTask<Result<T>> FinishAsync(T instance, RuleContext<T> ctx, ValueTask configTask)
    {
        await configTask.ConfigureAwait(false);
        return Validator.BuildResult(instance, ctx);
    }

    /// <inheritdoc cref="IValidator{T}.Order"/>
    public virtual int Order => 0;

    /// <summary>
    /// Define validation rules here. The <paramref name="model"/> instance is available directly —
    /// no expression trees, no lambdas over <c>x =></c>. Return <c>default</c>
    /// when all rules are synchronous.
    /// </summary>
    /// <example>
    /// <code>
    /// // Sync-only validator
    /// protected override ValueTask Configure(CreateUserRequest model, RuleContext&lt;CreateUserRequest&gt; rules, CancellationToken ct = default)
    /// {
    ///     rules.For(() => model.Name).NotEmpty().MaxLength(100);
    ///     rules.For(() => model.Age).GreaterThan(0);
    ///     return ValueTask.CompletedTask;
    /// }
    ///
    /// // Async validator (e.g. database uniqueness check)
    /// protected override async ValueTask Configure(CreateUserRequest model, RuleContext&lt;CreateUserRequest&gt; rules, CancellationToken ct = default)
    /// {
    ///     rules.For(() => model.Name).NotEmpty();
    ///     await rules.For(() => model.Email)
    ///                .MustAsync(async (e, c) => await _db.IsUniqueAsync(e, c),
    ///                           "Email.NotUnique", "Email is already taken.", c);
    /// }
    /// </code>
    /// </example>
    protected abstract ValueTask Configure(T model, RuleContext<T> rules, CancellationToken ct = default);

    /// <summary>
    /// Merges the rules of <paramref name="other"/> into the current validation context.
    /// Call this from <see cref="Configure"/> to compose validators without inheritance.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override async ValueTask Configure(Order model, RuleContext&lt;Order&gt; rules, CancellationToken ct = default)
    /// {
    ///     await Include(new BaseOrderValidator(), model, rules, ct);
    ///     rules.For(() => model.PaymentReference).NotEmpty();
    /// }
    /// </code>
    /// </example>
    protected ValueTask Include(Validator<T> other, T model, RuleContext<T> rules, CancellationToken ct = default)
        => other.Configure(model, rules, ct);
}

/// <summary>
/// Provides inline (class-free) validation via static factory methods.
/// </summary>
/// <example>
/// <code>
/// // Sync lambda
/// var result = await Validator.ValidateAsync(request, (model, rules) =>
/// {
///     rules.For(() => model.Name).NotEmpty();
///     rules.For(() => model.Age).GreaterThan(0);
/// });
///
/// // Async lambda
/// var result = await Validator.ValidateAsync(request, async (model, rules, ct) =>
/// {
///     rules.For(() => model.Name).NotEmpty();
///     await rules.For(() => model.Email)
///                .MustAsync(async (e, c) => await _db.IsUniqueAsync(e, c), "Email.NotUnique", "...", c);
/// });
/// </code>
/// </example>
public static class Validator
{
    internal static Result<T> BuildResult<T>(T instance, RuleContext<T> ctx) =>
        ctx.HasErrors
            ? Result<T>.Failure(ctx.Errors)
            : Result<T>.Success(instance);

    /// <summary>
    /// Validates <paramref name="instance"/> inline with a synchronous configuration delegate.
    /// Returns a synchronously-completed <see cref="ValueTask"/> — no heap allocation.
    /// </summary>
    public static ValueTask<Result<T>> ValidateAsync<T>(T instance, Action<T, RuleContext<T>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif
        RuleContext<T> ctx = new();
        configure(instance, ctx);
        return new ValueTask<Result<T>>(BuildResult(instance, ctx));
    }

    /// <summary>
    /// Validates <paramref name="instance"/> inline with an asynchronous configuration delegate.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateAsync<T>(
        T instance,
        Func<T, RuleContext<T>, CancellationToken, ValueTask> configure,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif
        RuleContext<T> ctx = new();
        await configure(instance, ctx, ct).ConfigureAwait(false);
        return BuildResult(instance, ctx);
    }
}
