namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Computes the validators of a resource type. Register one with
/// <see cref="ConditionalRequestExtensions.AddETagSource{T, TSource}"/>; it wins over <see cref="IVersioned"/> and
/// <see cref="IETagGenerator"/> for values of type <typeparamref name="T"/> (and types derived from it).
/// It is resolved from the request services, so it can be scoped.
/// </summary>
/// <typeparam name="T">The resource type.</typeparam>
public interface IETagSource<in T>
{
    /// <summary>
    /// Returns the validators of <paramref name="value"/>; the source decides between a strong and a weak ETag.
    /// Runs on every request, synchronously: compute from the value, do no I/O.
    /// </summary>
    /// <param name="value">The resource.</param>
    /// <returns>The validators, or <see langword="null"/> when the value has none (the response is sent unchanged).</returns>
    ResourceValidators? GetValidators(T value);
}
