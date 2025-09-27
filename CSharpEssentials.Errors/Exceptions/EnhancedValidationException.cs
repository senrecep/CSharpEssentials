using CSharpEssentials.Errors;

namespace CSharpEssentials.Exceptions;

public class EnhancedValidationException(
    Error[] errors) : Exception($"Validation failed with {errors.Length} errors")
{
    public Error[] Errors => errors;
}
