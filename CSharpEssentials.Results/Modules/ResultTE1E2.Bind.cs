// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2>
{
    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2> Bind<TOut>(Func<T, Result<TOut, E1, E2>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => bind(RawValue),
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, TError1> BindWiden<TOut, TError1>(Func<T, Result<TOut, TError1>> bind)
        where TOut : notnull
        where TError1 : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = bind(RawValue);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return Tag switch
        {
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
