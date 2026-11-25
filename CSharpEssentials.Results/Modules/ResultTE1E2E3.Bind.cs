// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, E3> Bind<TOut>(Func<T, Result<TOut, E1, E2, E3>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => bind(RawValue),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
