using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpStatusCodeMapperCoverageTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, ErrorType.Validation)]
    [InlineData(HttpStatusCode.ExpectationFailed, ErrorType.Validation)]
    [InlineData(HttpStatusCode.ServiceUnavailable, ErrorType.Unexpected)]
    [InlineData(HttpStatusCode.Continue, ErrorType.Failure)]
    [InlineData(HttpStatusCode.Found, ErrorType.Failure)]
    public void ToErrorType_Should_Map_Remaining_Codes(HttpStatusCode code, ErrorType expected)
    {
        HttpStatusCodeMapper.ToErrorType(code).Should().Be(expected);
    }

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
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, ErrorType.Validation)]
    [InlineData(HttpStatusCode.ExpectationFailed, ErrorType.Validation)]
    [InlineData(HttpStatusCode.UnprocessableEntity, ErrorType.Validation)]
    [InlineData(HttpStatusCode.TooManyRequests, ErrorType.Conflict)]
    [InlineData(HttpStatusCode.InternalServerError, ErrorType.Unexpected)]
    [InlineData(HttpStatusCode.BadGateway, ErrorType.Unexpected)]
    [InlineData((HttpStatusCode)418, ErrorType.Failure)]
    [InlineData(HttpStatusCode.OK, ErrorType.Failure)]
    public void ToError_Should_Map_Type_And_Code(HttpStatusCode code, ErrorType expected)
    {
        Error error = HttpStatusCodeMapper.ToError(code);

        error.Type.Should().Be(expected);
        error.Code.Should().Be($"Http.{(int)code}");
    }

    [Fact]
    public void ToError_Should_Agree_With_ToErrorType_For_Every_Status_Code()
    {
        for (int code = 100; code < 600; code++)
        {
            var status = (HttpStatusCode)code;

            HttpStatusCodeMapper.ToError(status).Type.Should().Be(HttpStatusCodeMapper.ToErrorType(status), $"status {code}");
        }
    }

    [Fact]
    public void ToError_Without_Description_Should_Use_Default_Message()
    {
        Error error = HttpStatusCodeMapper.ToError(HttpStatusCode.NotFound);

        error.Description.Should().Be("HTTP request failed with status code 404 (NotFound).");
    }
}
