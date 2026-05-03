using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CSharpEssentials.Tests.AspNetCore;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithSimpleException_ShouldSetBadRequestAndReturnTrue()
    {
        var problemDetailsService = new Mock<IProblemDetailsService>();
        problemDetailsService
            .Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(new ValueTask<bool>(true));

        ILogger<GlobalExceptionHandler> logger = NullLogger<GlobalExceptionHandler>.Instance;
        var handler = new GlobalExceptionHandler(problemDetailsService.Object, logger);

        var httpContext = new DefaultHttpContext();
        var exception = new InvalidOperationException("Test exception");

        bool result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemDetailsService.Verify(x => x.TryWriteAsync(It.Is<ProblemDetailsContext>(ctx => ctx.HttpContext == httpContext && ctx.Exception == exception)), Times.Once);
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
