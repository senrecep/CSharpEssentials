// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Ensure(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, TError> Ensure<TError>(Func<T, bool> predicate, TError error)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(TypedResultLogic.SuccessTag, RawValue, null) : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> FailIf(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, TError> FailIf<TError>(Func<T, bool> predicate, TError error)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : new(TypedResultLogic.SuccessTag, RawValue, null),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
