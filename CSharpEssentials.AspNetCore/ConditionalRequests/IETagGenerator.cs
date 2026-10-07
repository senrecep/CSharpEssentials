namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Computes the validators of a value no <see cref="IETagSource{T}"/> is registered for. The default implementation, registered by
/// <see cref="ConditionalRequestExtensions.AddConditionalRequests"/> unless one is already registered, uses <see cref="IVersioned"/>
/// (strong ETag) and, when <see cref="ConditionalRequestOptions.UseBodyHashFallback"/> is set, a weak hash of the JSON body.
/// </summary>
public interface IETagGenerator
{
    /// <summary>Returns the validators of <paramref name="value"/>.</summary>
    /// <param name="value">The resource.</param>
    /// <returns>The validators, or <see langword="null"/> when the value has none (the response is sent unchanged).</returns>
    ResourceValidators? GetValidators(object value);
}
