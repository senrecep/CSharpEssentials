using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpStatusCodeMapperTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, ErrorType.Validation)]
    [InlineData(HttpStatusCode.Unauthorized, ErrorType.Unauthorized)]
    [InlineData(HttpStatusCode.PaymentRequired, ErrorType.Failure)]
    [InlineData(HttpStatusCode.Forbidden, ErrorType.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, ErrorType.NotFound)]
    [InlineData(HttpStatusCode.MethodNotAllowed, ErrorType.Failure)]
    [InlineData(HttpStatusCode.NotAcceptable, ErrorType.Failure)]
    [InlineData(HttpStatusCode.ProxyAuthenticationRequired, ErrorType.Unauthorized)]
    [InlineData(HttpStatusCode.RequestTimeout, ErrorType.Unexpected)]
    [InlineData(HttpStatusCode.Conflict, ErrorType.Conflict)]
    [InlineData(HttpStatusCode.Gone, ErrorType.NotFound)]
    [InlineData(HttpStatusCode.LengthRequired, ErrorType.Validation)]
    [InlineData(HttpStatusCode.PreconditionFailed, ErrorType.Conflict)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, ErrorType.Validation)]
    [InlineData(HttpStatusCode.RequestUriTooLong, ErrorType.Validation)]
    [InlineData(HttpStatusCode.UnsupportedMediaType, ErrorType.Validation)]
    [InlineData(HttpStatusCode.UnprocessableEntity, ErrorType.Validation)]
    [InlineData(HttpStatusCode.TooManyRequests, ErrorType.Conflict)]
    [InlineData(HttpStatusCode.InternalServerError, ErrorType.Unexpected)]
    [InlineData(HttpStatusCode.OK, ErrorType.Failure)]
    public void ToErrorType_Should_Map_Correctly(HttpStatusCode code, ErrorType expected)
    {
        HttpStatusCodeMapper.ToErrorType(code).Should().Be(expected);
    }

    [Fact]
    public void ToError_Should_Create_Structured_Error()
    {
        var error = HttpStatusCodeMapper.ToError(HttpStatusCode.NotFound);

        error.Type.Should().Be(ErrorType.NotFound);
        error.Code.Should().Be("Http.404");
    }

    [Fact]
    public void ToError_WithDescription_Should_Use_Custom_Description()
    {
        var error = HttpStatusCodeMapper.ToError(HttpStatusCode.BadRequest, "Custom bad request");

        error.Description.Should().Be("Custom bad request");
    }

    [Fact]
    public void ToErrorType_UnknownClientError_Should_Map_To_Failure()
    {
        HttpStatusCodeMapper.ToErrorType((HttpStatusCode)418).Should().Be(ErrorType.Failure);
    }
}
