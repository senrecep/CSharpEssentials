// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2>
{
    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.</summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="map">Called with the success value.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2> Map<TOut>(Func<T, TOut> map)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(map(RawValue), nameof(map)), null),
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
