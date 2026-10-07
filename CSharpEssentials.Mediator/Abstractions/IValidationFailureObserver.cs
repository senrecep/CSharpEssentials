namespace CSharpEssentials.Mediator;

/// <summary>
/// Receives every validation failure seen by <see cref="ValidationBehavior{TRequest, TResponse}"/>,
/// in both <see cref="ValidationMode.Enforce"/> and <see cref="ValidationMode.LogOnly"/>.
/// Observers run in registration order before the behavior returns or continues; an exception thrown
/// by an observer fails the request.
/// </summary>
public interface IValidationFailureObserver
{
    ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken);
}
