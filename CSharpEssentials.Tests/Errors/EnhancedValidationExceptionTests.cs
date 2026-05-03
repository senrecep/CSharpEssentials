using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class EnhancedValidationExceptionTests
{
    [Fact]
    public void Constructor_WithErrors_ShouldSetProperties()
    {
        Error[] errors =
        [
            Error.Validation("Name.Empty", "Name is required"),
            Error.Validation("Email.Invalid", "Email is invalid")
        ];

        var exception = new EnhancedValidationException(errors);

        exception.Errors.Should().BeEquivalentTo(errors);
        exception.Message.Should().Be("Validation failed with 2 errors");
    }

    [Fact]
    public void Constructor_WithSingleError_ShouldSetProperties()
    {
        Error[] errors = [Error.Validation("Age.Range", "Age must be between 18 and 120")];

        var exception = new EnhancedValidationException(errors);

        exception.Errors.Should().ContainSingle();
        exception.Errors[0].Code.Should().Be("Age.Range");
    }

    [Fact]
    public void Constructor_ShouldBeAssignableToException()
    {
        Error[] errors = [Error.Validation("Test", "Test error")];

        var exception = new EnhancedValidationException(errors);

        exception.Should().BeAssignableTo<Exception>();
    }
}
