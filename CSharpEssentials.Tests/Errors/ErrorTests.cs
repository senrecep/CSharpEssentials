using System.Text.Json;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorTests
{
    #region Factory Methods

    [Fact]
    public void Failure_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Failure("TEST.FAILURE", "Test failure");

        error.Code.Should().Be("TEST.FAILURE");
        error.Description.Should().Be("Test failure");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Unexpected_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Unexpected("TEST.UNEXPECTED", "Test unexpected");

        error.Code.Should().Be("TEST.UNEXPECTED");
        error.Description.Should().Be("Test unexpected");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void Validation_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Validation("TEST.VALIDATION", "Test validation");

        error.Code.Should().Be("TEST.VALIDATION");
        error.Description.Should().Be("Test validation");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NotFound_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.NotFound("TEST.NOT_FOUND", "Test not found");

        error.Code.Should().Be("TEST.NOT_FOUND");
        error.Description.Should().Be("Test not found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Conflict("TEST.CONFLICT", "Test conflict");

        error.Code.Should().Be("TEST.CONFLICT");
        error.Description.Should().Be("Test conflict");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Unauthorized("TEST.UNAUTHORIZED", "Test unauthorized");

        error.Code.Should().Be("TEST.UNAUTHORIZED");
        error.Description.Should().Be("Test unauthorized");
        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Forbidden("TEST.FORBIDDEN", "Test forbidden");

        error.Code.Should().Be("TEST.FORBIDDEN");
        error.Description.Should().Be("Test forbidden");
        error.Type.Should().Be(ErrorType.Forbidden);
    }

    #endregion

    #region Exception Methods

    [Fact]
    public void Exception_WithException_ShouldCreateErrorWithMetadata()
    {
        InvalidOperationException exception = new("Test exception");
        var error = Error.Exception(exception);

        error.Code.Should().Be(nameof(InvalidOperationException));
        error.Description.Should().Be("Test exception");
        error.Type.Should().Be(ErrorType.Failure);
        error.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void Exception_WithCodeAndException_ShouldUseCode()
    {
        InvalidOperationException exception = new("Test exception");
        var error = Error.Exception("CUSTOM.CODE", exception);

        error.Code.Should().Be("CUSTOM.CODE");
        error.Description.Should().Be("Test exception");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Exception_WithCodeDescriptionAndException_ShouldUseAll()
    {
        InvalidOperationException exception = new("Test exception");
        var error = Error.Exception("CUSTOM.CODE", "Custom description", exception);

        error.Code.Should().Be("CUSTOM.CODE");
        error.Description.Should().Be("Custom description");
        error.Type.Should().Be(ErrorType.Failure);
    }

    #endregion

    #region CreateMany

    [Fact]
    public void CreateMany_WithMultipleErrors_ShouldReturnArray()
    {
        Error[] errors = Error.CreateMany(
            Error.Failure("ERR1", "Error 1"),
            Error.Validation("ERR2", "Error 2")
        );

        errors.Should().HaveCount(2);
        errors[0].Code.Should().Be("ERR1");
        errors[1].Code.Should().Be("ERR2");
    }

    #endregion

    #region Metadata

    [Fact]
    public void Error_WithMetadata_ShouldContainMetadata()
    {
        ErrorMetadata metadata = new() { ["key"] = "value" };
        var error = Error.Failure("TEST", "Test", metadata);

        error.Metadata.Should().NotBeNull();
        error.Metadata["key"].Should().Be("value");
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_SameErrors_ShouldBeEqual()
    {
        var error1 = Error.Failure("CODE", "Description");
        var error2 = Error.Failure("CODE", "Description");

        error1.Should().Be(error2);
    }

    [Fact]
    public void Equality_DifferentErrors_ShouldNotBeEqual()
    {
        var error1 = Error.Failure("CODE1", "Description");
        var error2 = Error.Failure("CODE2", "Description");

        error1.Should().NotBe(error2);
    }

    [Fact]
    public void Equality_WithMetadataInDifferentOrder_ShouldHaveSameHashCode()
    {
        var first = Error.Validation("C", "D", new ErrorMetadata { { "a", 1 }, { "b", 2 } });
        var second = Error.Validation("C", "D", new ErrorMetadata { { "b", 2 }, { "a", 1 } });

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
        new HashSet<Error> { first, second }.Should().ContainSingle();
    }

    [Fact]
    public void Equality_ExceptionWithInnerException_ShouldBeEqualAndHaveSameHashCode()
    {
        Exception exception;
        try
        {
            try
            {
                throw new ArgumentException("Inner");
            }
            catch (ArgumentException inner)
            {
                throw new InvalidOperationException("Outer", inner);
            }
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        var first = Error.Exception(exception);
        var second = Error.Exception(exception);

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    #endregion

    #region Serialization

    [Fact]
    public void JsonSerialization_ShouldWork()
    {
        var error = Error.Validation("TEST", "Test error");
        string json = JsonSerializer.Serialize(error);
        Error deserialized = JsonSerializer.Deserialize<Error>(json);

        deserialized.Code.Should().Be(error.Code);
        deserialized.Description.Should().Be(error.Description);
        deserialized.Type.Should().Be(error.Type);
    }

    [Fact]
    public void JsonSerialization_WithThrownExceptionMetadata_ShouldWriteExceptionDetails()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("Boom");
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        var error = Error.Failure("C", "D", ErrorMetadata.CreateWithException(exception));
        var added = Error.Failure("C", "D", new ErrorMetadata().AddException(exception));

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(error));
        JsonElement written = json.RootElement.GetProperty("Metadata").GetProperty("exception");
        written.GetProperty("exceptionType").GetString().Should().Be("InvalidOperationException");
        written.GetProperty("exceptionMessage").GetString().Should().Be("Boom");
        using var addedJson = JsonDocument.Parse(JsonSerializer.Serialize(added));
        JsonElement addedWritten = addedJson.RootElement.GetProperty("Metadata").GetProperty("exception");
        addedWritten.GetProperty("exceptionType").GetString().Should().Be("InvalidOperationException");
        addedWritten.GetProperty("exceptionMessage").GetString().Should().Be("Boom");
    }

    #endregion
}

