// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Turns the error in the <typeparamref name="E1"/> slot into a success value and removes the slot. Every other case passes through.</summary>
    /// <param name="recover">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result without the <typeparamref name="E1"/> slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recover"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E2, E3> Recover(Func<E1, T> recover)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(recover((E1)RawError!), nameof(recover)), null),
            3 => new(2, default!, RawError),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns the error in the <typeparamref name="E2"/> slot into a success value and removes the slot. Every other case passes through.</summary>
    /// <param name="recover">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result without the <typeparamref name="E2"/> slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recover"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E3> Recover(Func<E2, T> recover)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(recover((E2)RawError!), nameof(recover)), null),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns the error in the <typeparamref name="E3"/> slot into a success value and removes the slot. Every other case passes through.</summary>
    /// <param name="recover">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result without the <typeparamref name="E3"/> slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recover"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2> Recover(Func<E3, T> recover)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => new(3, default!, RawError),
            4 => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(recover((E3)RawError!), nameof(recover)), null),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
