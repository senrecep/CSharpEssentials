// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Turns the error in the <typeparamref name="E1"/> slot into a success value and removes the slot. Every other case passes through.</summary>
    /// <param name="recover">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The success value, or the return value of <paramref name="recover"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recover"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public T Recover(Func<E1, T> recover)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => RawValue,
            2 => TypedResultLogic.NotNull(recover((E1)RawError!), nameof(recover)),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
