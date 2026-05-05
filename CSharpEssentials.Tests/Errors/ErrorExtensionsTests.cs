using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorExtensionsTests
{
    [Fact]
    public void ToIntType_ShouldConvertErrorTypeToInt()
    {
        ErrorType.Failure.ToIntType().Should().Be(0);
        ErrorType.Validation.ToIntType().Should().Be(2);
        ErrorType.NotFound.ToIntType().Should().Be(4);
    }

    [Fact]
    public void ToHttpStatusCode_ShouldMapCorrectly()
    {
        ErrorType.Failure.ToHttpStatusCode().Should().Be(HttpCodes.InternalServerError);
        ErrorType.Unexpected.ToHttpStatusCode().Should().Be(HttpCodes.InternalServerError);
        ErrorType.Validation.ToHttpStatusCode().Should().Be(HttpCodes.BadRequest);
        ErrorType.Conflict.ToHttpStatusCode().Should().Be(HttpCodes.Conflict);
        ErrorType.NotFound.ToHttpStatusCode().Should().Be(HttpCodes.NotFound);
        ErrorType.Unauthorized.ToHttpStatusCode().Should().Be(HttpCodes.Unauthorized);
        ErrorType.Forbidden.ToHttpStatusCode().Should().Be(HttpCodes.Forbidden);
        ErrorType.Unknown.ToHttpStatusCode().Should().Be(HttpCodes.InternalServerError);
    }

    [Fact]
    public void ToErrorType_ShouldMapHttpStatusCodeToErrorType()
    {
        HttpCodes.BadRequest.ToErrorType().Should().Be(ErrorType.Validation);
        HttpCodes.Unauthorized.ToErrorType().Should().Be(ErrorType.Unauthorized);
        HttpCodes.Forbidden.ToErrorType().Should().Be(ErrorType.Forbidden);
        HttpCodes.NotFound.ToErrorType().Should().Be(ErrorType.NotFound);
        HttpCodes.Conflict.ToErrorType().Should().Be(ErrorType.Conflict);
        HttpCodes.InternalServerError.ToErrorType().Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void ToErrorType_WithUnknownStatusCode_ShouldReturnUnexpected()
    {
        418.ToErrorType().Should().Be(ErrorType.Unexpected);
        999.ToErrorType().Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void ToHttpStatusCode_And_ToErrorType_ShouldRoundTrip()
    {
        ErrorType[] errorTypes = new[]
        {
            ErrorType.Validation,
            ErrorType.Conflict,
            ErrorType.NotFound,
            ErrorType.Unauthorized,
            ErrorType.Forbidden
        };

        foreach (ErrorType errorType in errorTypes)
        {
            int statusCode = errorType.ToHttpStatusCode();
            var convertedBack = statusCode.ToErrorType();
            convertedBack.Should().Be(errorType);
        }
    }
}

