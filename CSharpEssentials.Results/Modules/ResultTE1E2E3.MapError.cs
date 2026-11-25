// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, TError, E2, E3> MapError<TError>(Func<E1, TError> map)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(Tag, default!, TypedResultLogic.NotNull(map((E1)RawError!), nameof(map))),
            3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the return value of <paramref name="map"/>. Every other case passes through.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E2"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, TError, E3> MapError<TError>(Func<E2, TError> map)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            3 => new(Tag, default!, TypedResultLogic.NotNull(map((E2)RawError!), nameof(map))),
            2 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the return value of <paramref name="map"/>. Every other case passes through.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E3"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, TError> MapError<TError>(Func<E3, TError> map)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            4 => new(Tag, default!, TypedResultLogic.NotNull(map((E3)RawError!), nameof(map))),
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
