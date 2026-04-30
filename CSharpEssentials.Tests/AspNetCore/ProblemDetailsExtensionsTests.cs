using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore;

public class ProblemDetailsExtensionsTests
{
    [Fact]
    public void ToProblemDetails_WithSingleError_ShouldCreateProblemDetails()
    {
        Error error = TestData.Errors.Validation;

        EnhancedProblemDetails problemDetails = error.ToProblemDetails();

        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be(400);
        problemDetails.Errors.Should().Contain(error);
    }

    [Fact]
    public void ToProblemDetails_WithMultipleErrors_ShouldCreateProblemDetails()
    {
        Error[] errors = [TestData.Errors.Validation, TestData.Errors.NotFound];

        EnhancedProblemDetails problemDetails = errors.ToProblemDetails();

        problemDetails.Should().NotBeNull();
        problemDetails.Errors.Should().HaveCount(2);
        problemDetails.ErrorCodes.Should().HaveCount(2);
    }

    [Fact]
    public void ToProblemDetails_WithCustomStatusCode_ShouldUseCustomStatusCode()
    {
        Error error = TestData.Errors.NotFound;

        EnhancedProblemDetails problemDetails = error.ToProblemDetails(statusCode: 404);

        problemDetails.Status.Should().Be(404);
    }

    [Fact]
    public void ToProblemDetails_WithMetadata_ShouldIncludeMetadata()
    {
        Error error = TestData.Errors.Validation;
        ErrorMetadata metadata = TestData.Errors.SimpleMetadata;

        EnhancedProblemDetails problemDetails = error.ToProblemDetails(extensions: metadata);

        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public void ToProblemDetails_WithResult_ShouldThrow_WhenResultIsSuccess()
    {
        var result = Result.Success();

        Action action = () => result.ToProblemDetails();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToProblemDetails_WithResult_ShouldCreateProblemDetails_WhenResultIsFailure()
    {
        var result = Result.Failure(TestData.Errors.Validation);

        EnhancedProblemDetails problemDetails = result.ToProblemDetails();

        problemDetails.Should().NotBeNull();
        problemDetails.Errors.Should().Contain(TestData.Errors.Validation);
    }

    [Fact]
    public void ToActionResult_WithError_ShouldReturnObjectResult()
    {
        Error error = TestData.Errors.Validation;

        IActionResult result = error.ToActionResult();

        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ToActionResult_WithHttpContext_ShouldAddHttpContextDetails()
    {
        Error error = TestData.Errors.Validation;
        DefaultHttpContext httpContext = new()
        {
            Request =
            {
                Method = "GET",
                Path = "/test"
            },
            TraceIdentifier = "trace-id"
        };

        IActionResult result = error.ToActionResult(httpContext);

        result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public void ToProblemResult_WithError_ShouldReturnProblemResult()
    {
        Error error = TestData.Errors.Validation;

        IResult result = error.ToProblemResult();

        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblemResult_WithResult_ShouldThrow_WhenResultIsSuccess()
    {
        var result = Result.Success();

        Action action = () => result.ToProblemResult();

        action.Should().Throw<InvalidOperationException>();
    }
}

