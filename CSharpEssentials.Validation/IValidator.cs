using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Validation;

/// <summary>
/// Validates an instance of <typeparamref name="T"/> and returns a <see cref="Result{T}"/>.
/// Returns a completed <see cref="ValueTask"/> when all rules are synchronous — zero heap allocation.
/// </summary>
public interface IValidator<T>
{
    /// <summary>
    /// Gets the execution order. Validators with a lower value run before those with a higher value.
    /// Validators sharing the same order value run concurrently. Defaults to <c>0</c>.
    /// </summary>
    int Order => 0;

    /// <summary>Validates <paramref name="instance"/>. Synchronous-only validators complete synchronously.</summary>
    ValueTask<Result<T>> ValidateAsync(T instance, CancellationToken ct = default);
}
