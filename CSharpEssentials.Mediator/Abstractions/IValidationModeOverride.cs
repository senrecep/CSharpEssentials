namespace CSharpEssentials.Mediator;

/// <summary>
/// Lets a request choose its own <see cref="Mediator.ValidationMode"/> instead of
/// <see cref="ValidationBehaviorOptions.DefaultMode"/>.
/// </summary>
public interface IValidationModeOverride
{
    ValidationMode ValidationMode { get; }
}
