using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorTests
{
    private const string ErrorCode = "ErrorCode";
    private const string ErrorDescription = "ErrorDescription";
    private static readonly ErrorMetadata Metadata = new()
    {
        { "key1", "value1" },
        { "key2", 10 },
        { "key3", DateTime.UtcNow }
    };

    [Fact]
    public void CreateError_WhenFailureError_ShouldHaveErrorTypeFailure()
    {
        // Act
        var error = Error.Failure(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Failure);
    }

    [Fact]
    public void CreateError_WhenUnexpectedError_ShouldHaveErrorTypeFailure()
    {
        // Act
        var error = Error.Unexpected(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Unexpected);
    }

    [Fact]
    public void CreateError_WhenValidationError_ShouldHaveErrorTypeValidation()
    {
        // Act
        var error = Error.Validation(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Validation);
    }

    [Fact]
    public void CreateError_WhenConflictError_ShouldHaveErrorTypeConflict()
    {
        // Act
        var error = Error.Conflict(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Conflict);
    }

    [Fact]
    public void CreateError_WhenNotFoundError_ShouldHaveErrorTypeNotFound()
    {
        // Act
        var error = Error.NotFound(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.NotFound);
    }

    [Fact]
    public void CreateError_WhenNotAuthorizedError_ShouldHaveErrorTypeUnauthorized()
    {
        // Act
        var error = Error.Unauthorized(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Unauthorized);
    }

    [Fact]
    public void CreateError_WhenForbiddenError_ShouldHaveErrorTypeForbidden()
    {
        // Act
        var error = Error.Forbidden(ErrorCode, ErrorDescription, Metadata);

        // Assert
        ValidateError(error, expectedErrorType: ErrorType.Forbidden);
    }



    private static void ValidateError(Error error, ErrorType expectedErrorType)
    {
        error.Code.Should().Be(ErrorCode);
        error.Description.Should().Be(ErrorDescription);
        error.Type.Should().Be(expectedErrorType);
        error.NumericType.Should().Be((int)expectedErrorType);
        error.Metadata.Should().BeEquivalentTo(Metadata);
    }
}
