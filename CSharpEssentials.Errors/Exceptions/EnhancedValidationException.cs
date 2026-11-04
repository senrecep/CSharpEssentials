using CSharpEssentials.Errors;

namespace CSharpEssentials.Exceptions;

public class EnhancedValidationException : Exception
{
    public EnhancedValidationException(Error[] errors)
        : base($"Validation failed with {(errors ?? throw new ArgumentNullException(nameof(errors))).Length} errors") => Errors = errors;

    public Error[] Errors { get; }
}
