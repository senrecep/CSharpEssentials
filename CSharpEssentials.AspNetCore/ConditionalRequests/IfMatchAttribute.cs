namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Turns on <c>If-Match</c> handling (optimistic concurrency) for an MVC controller or action. For <c>POST</c>, <c>PUT</c>,
/// <c>PATCH</c> and <c>DELETE</c> the header is parsed before model binding and exposed through
/// <see cref="ConditionalRequestExtensions.GetPreconditions"/>; a malformed header returns 400 and a missing one returns 428 when
/// <see cref="Required"/> is set. The action compares the current resource with <see cref="Preconditions.Matches(ResourceValidators?)"/>
/// and returns <see cref="ConditionalRequestErrors.PreconditionFailed"/> (412) on a mismatch.
/// Precedence: action &gt; controller &gt; endpoint metadata (<see cref="ConditionalRequestExtensions.WithIfMatch{TBuilder}(TBuilder, bool)"/>).
/// Requires <see cref="ConditionalRequestExtensions.AddConditionalRequests"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class IfMatchAttribute : Attribute
{
    /// <summary>When <see langword="true"/>, a request without <c>If-Match</c> returns <c>428 Precondition Required</c>.</summary>
    public bool Required { get; set; }
}
