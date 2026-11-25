// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E2 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E3 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
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
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E2 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E3 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
