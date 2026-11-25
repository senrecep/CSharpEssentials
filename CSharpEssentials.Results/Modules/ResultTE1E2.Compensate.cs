// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2>
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E2> Compensate(Func<E1, Result<T, E2>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => compensate((E1)RawError!),
            3 => new(2, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Compensate(Func<E2, Result<T, E1>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => compensate((E2)RawError!),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
