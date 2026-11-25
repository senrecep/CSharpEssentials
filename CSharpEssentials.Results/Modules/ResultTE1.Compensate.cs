// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Compensate(Func<E1, Result<T, E1>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => this,
            2 => compensate((E1)RawError!),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
