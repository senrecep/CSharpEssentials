using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CSharpEssentials.Tests.AspNetCore;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithInvalidOperationException_ShouldSetInternalServerErrorAndReturnTrue()
    {
        // Without AddEnhancedProblemDetails the service is bypassed and the handler writes the body itself.
        var problemDetailsService = new Mock<IProblemDetailsService>();
        ILogger<GlobalExceptionHandler> logger = NullLogger<GlobalExceptionHandler>.Instance;
        var handler = new GlobalExceptionHandler(problemDetailsService.Object, logger);

        var httpContext = new DefaultHttpContext();
        using var body = new MemoryStream();
        httpContext.Response.Body = body;
        var exception = new InvalidOperationException("Test exception");

        bool result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        httpContext.Response.ContentType.Should().StartWith("application/problem+json");
        problemDetailsService.Verify(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Never);
        string json = System.Text.Encoding.UTF8.GetString(body.ToArray());
        json.Should().Contain("\"status\":500").And.NotContain("Test exception");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_ShouldSetInternalServerErrorAndReturnTrue()
    {
        var problemDetailsService = new Mock<IProblemDetailsService>();
        problemDetailsService
            .Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(new ValueTask<bool>(true));

        ILogger<GlobalExceptionHandler> logger = NullLogger<GlobalExceptionHandler>.Instance;
        var handler = new GlobalExceptionHandler(problemDetailsService.Object, logger);

        var httpContext = new DefaultHttpContext();
        var exception = new Exception("Unexpected");

        bool result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
