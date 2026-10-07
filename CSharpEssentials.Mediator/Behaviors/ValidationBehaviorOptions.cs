namespace CSharpEssentials.Mediator;

/// <summary>
/// Options for <see cref="ValidationBehavior{TRequest, TResponse}"/>.
/// </summary>
public sealed class ValidationBehaviorOptions
{
    /// <summary>
    /// The mode for requests that do not implement <see cref="IValidationModeOverride"/>.
    /// Defaults to <see cref="ValidationMode.Enforce"/>.
    /// </summary>
    public ValidationMode DefaultMode { get; set; } = ValidationMode.Enforce;
}
