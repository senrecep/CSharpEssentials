using CSharpEssentials.Errors;

namespace CSharpEssentials.Mediator;

/// <summary>
/// A failed validation of a request, as reported to <see cref="IValidationFailureObserver"/>.
/// </summary>
/// <param name="RequestType">The request type that was validated.</param>
/// <param name="Request">The request instance.</param>
/// <param name="Errors">The validation errors, in validator order.</param>
/// <param name="Mode">The mode the request was validated in: <see cref="ValidationMode.Enforce"/> or <see cref="ValidationMode.LogOnly"/>.</param>
public sealed record ValidationFailureContext(Type RequestType, object Request, IReadOnlyList<Error> Errors, ValidationMode Mode);
