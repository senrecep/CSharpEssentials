using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Validation;

/// <summary>
/// Provides extension methods for validating <see cref="Result{T}"/> instances
/// within functional railway-oriented programming pipelines.
/// </summary>
public static class ValidationResultExtensions
{
    // ==========================================
    // Extensions for: Result<T>
    // ==========================================

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> using the specified validator.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static ValueTask<Result<T>> ValidateWithAsync<T>(
        this Result<T> result,
        IValidator<T> validator,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(validator);
#else
        if (validator is null)
            throw new ArgumentNullException(nameof(validator));
#endif

        if (result.IsFailure)
        {
            return new ValueTask<Result<T>>(result);
        }

        return validator.ValidateAsync(result.Value, ct);
    }


    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inline synchronously using a synchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static Result<T> ValidateWith<T>(
        this Result<T> result,
        Action<T, RuleContext<T>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        if (result.IsFailure)
        {
            return result;
        }

        RuleContext<T> ctx = new();
        configure(result.Value, ctx);
        return ctx.HasErrors
            ? Result<T>.Failure(ctx.Errors)
            : Result<T>.Success(result.Value);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inline using a synchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static ValueTask<Result<T>> ValidateWithAsync<T>(
        this Result<T> result,
        Action<T, RuleContext<T>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        if (result.IsFailure)
        {
            return new ValueTask<Result<T>>(result);
        }

        return Validator.ValidateAsync(result.Value, configure);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inline using an asynchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static ValueTask<Result<T>> ValidateWithAsync<T>(
        this Result<T> result,
        Func<T, RuleContext<T>, CancellationToken, ValueTask> configure,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        if (result.IsFailure)
        {
            return new ValueTask<Result<T>>(result);
        }

        return Validator.ValidateAsync(result.Value, configure, ct);
    }

    // ==========================================
    // Extensions for: Task<Result<T>>
    // ==========================================

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="Task{T}"/> using the specified validator.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this Task<Result<T>> resultTask,
        IValidator<T> validator,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(validator);
#else
        if (resultTask is null)
            throw new ArgumentNullException(nameof(resultTask));
        if (validator is null)
            throw new ArgumentNullException(nameof(validator));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await validator.ValidateAsync(result.Value, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="Task{T}"/> inline using a synchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T, RuleContext<T>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (resultTask is null)
            throw new ArgumentNullException(nameof(resultTask));
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await Validator.ValidateAsync(result.Value, configure).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="Task{T}"/> inline using an asynchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, RuleContext<T>, CancellationToken, ValueTask> configure,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (resultTask is null)
            throw new ArgumentNullException(nameof(resultTask));
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await Validator.ValidateAsync(result.Value, configure, ct).ConfigureAwait(false);
    }

    // ==========================================
    // Extensions for: ValueTask<Result<T>>
    // ==========================================

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="ValueTask{T}"/> using the specified validator.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this ValueTask<Result<T>> resultTask,
        IValidator<T> validator,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(validator);
#else
        if (validator is null)
            throw new ArgumentNullException(nameof(validator));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await validator.ValidateAsync(result.Value, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="ValueTask{T}"/> inline using a synchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Action<T, RuleContext<T>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await Validator.ValidateAsync(result.Value, configure).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the value of a successful <see cref="Result{T}"/> inside a <see cref="ValueTask{T}"/> inline using an asynchronous configuration delegate.
    /// If the result is already a failure, it is returned directly, short-circuiting the pipeline.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateWithAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<T, RuleContext<T>, CancellationToken, ValueTask> configure,
        CancellationToken ct = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await Validator.ValidateAsync(result.Value, configure, ct).ConfigureAwait(false);
    }
}
