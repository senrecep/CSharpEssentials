using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithError_ShouldSetProperties()
    {
        var error = Error.Failure("Domain.Invalid", "Domain logic failed");

        var exception = new DomainException(error);

        exception.Error.Should().Be(error);
        exception.Message.Should().Be(error.Description);
    }

    [Fact]
    public void Constructor_WithError_ShouldBeAssignableToException()
    {
        var error = Error.Validation("Domain.Validation", "Invalid state");

        var exception = new DomainException(error);

        exception.Should().BeAssignableTo<Exception>();
    }
}
