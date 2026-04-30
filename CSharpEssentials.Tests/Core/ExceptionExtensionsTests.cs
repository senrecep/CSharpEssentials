using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class ExceptionExtensionsTests
{
    [Fact]
    public void GetInnerExceptions_WithSimpleException_ShouldReturnSingleException()
    {
        Exception simpleException = new("Simple exception");
        var exceptions = simpleException.GetInnerExceptions().ToList();

        exceptions.Should().HaveCount(1);
        exceptions[0].Should().BeSameAs(simpleException);
    }

    [Fact]
    public void GetInnerExceptions_WithExceptionWithInner_ShouldReturnAllExceptions()
    {
        InvalidOperationException innerException = new("Inner exception");
        Exception exceptionWithInner = new("Outer exception", innerException);
        var exceptions = exceptionWithInner.GetInnerExceptions().ToList();

        exceptions.Should().HaveCount(2);
        exceptions[0].Should().BeSameAs(exceptionWithInner);
        exceptions[1].Should().BeSameAs(innerException);
    }

    [Fact]
    public void GetInnerExceptions_WithExceptionWithMultipleInner_ShouldReturnAllExceptions()
    {
        ArgumentException secondInner = new("Second inner");
        AggregateException firstInner = new("First inner", secondInner);
        Exception exceptionWithMultipleInner = new("Outer exception", firstInner);
        var exceptions = exceptionWithMultipleInner.GetInnerExceptions().ToList();

        exceptions.Should().HaveCountGreaterThan(1);
        exceptions[0].Should().BeSameAs(exceptionWithMultipleInner);
    }

    [Fact]
    public void GetInnerExceptions_ShouldBeEnumerable()
    {
        InvalidOperationException innerException = new("Inner exception");
        Exception exceptionWithInner = new("Outer exception", innerException);
        IEnumerable<Exception> exceptions = exceptionWithInner.GetInnerExceptions();
        var exceptionsList = exceptions.ToList();
        exceptionsList.Should().NotBeNull();
        exceptionsList.Should().NotBeEmpty();
    }

    [Fact]
    public void GetInnerExceptionsMessages_ShouldReturnAllMessages()
    {
        InvalidOperationException innerException = new("Inner exception");
        Exception exceptionWithInner = new("Outer exception", innerException);
        var messages = exceptionWithInner.GetInnerExceptionsMessages().ToList();

        messages.Should().HaveCount(2);
        messages[0].Should().Be("Outer exception");
        messages[1].Should().Be("Inner exception");
    }

    [Fact]
    public void GetInnerExceptionsMessages_WithSimpleException_ShouldReturnSingleMessage()
    {
        Exception simpleException = new("Simple exception");
        var messages = simpleException.GetInnerExceptionsMessages().ToList();

        messages.Should().HaveCount(1);
        messages[0].Should().Be("Simple exception");
    }
}
