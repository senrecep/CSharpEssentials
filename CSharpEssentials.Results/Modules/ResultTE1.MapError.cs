// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, TError> MapError<TError>(Func<E1, TError> map)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(Tag, default!, TypedResultLogic.NotNull(map((E1)RawError!), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
