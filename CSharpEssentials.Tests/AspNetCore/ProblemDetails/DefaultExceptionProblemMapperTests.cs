using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public class DefaultExceptionProblemMapperTests
{
    [Fact]
    public void TryMap_Should_Return499_When_OperationCanceledAndRequestWasAborted()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var httpContext = new DefaultHttpContext { RequestAborted = cts.Token };

        bool mapped = DefaultExceptionProblemMapper.Instance.TryMap(httpContext, new OperationCanceledException(cts.Token), out ExceptionProblem? problem);

        mapped.Should().BeTrue();
        problem!.StatusCode.Should().Be(499);
        problem.Title.Should().Be("Client Closed Request");
    }

    [Fact]
    public void TryMap_Should_Return500_When_TaskCanceledAndRequestWasNotAborted()
    {
        var httpContext = new DefaultHttpContext();

        bool mapped = DefaultExceptionProblemMapper.Instance.TryMap(httpContext, new TaskCanceledException("timeout"), out ExceptionProblem? problem);

        mapped.Should().BeTrue();
        problem!.StatusCode.Should().Be(500);
        problem.Detail.Should().BeNull();
    }

    [Fact]
    public void TryMap_Should_Return500WithoutExposingMessage_When_OperationCanceledAndRequestWasNotAborted()
    {
        var httpContext = new DefaultHttpContext();

        DefaultExceptionProblemMapper.Instance.TryMap(httpContext, new OperationCanceledException("secret"), out ExceptionProblem? problem);

        problem!.StatusCode.Should().Be(500);
        problem.Title.Should().Be("An unexpected error occurred.");
        problem.Detail.Should().BeNull();
    }

    [Theory]
    [InlineData(400, "The request is invalid.")]
    [InlineData(408, "The request timed out.")]
    [InlineData(413, "The request body is too large.")]
    [InlineData(415, "The request content type is not supported.")]
    [InlineData(431, "The request headers are too large.")]
    public void TryMap_Should_UseFixedDetail_When_BadHttpRequestIsThrown(int statusCode, string detail)
    {
        var httpContext = new DefaultHttpContext();

        DefaultExceptionProblemMapper.Instance.TryMap(
            httpContext, new BadHttpRequestException("framework secret", statusCode), out ExceptionProblem? problem);

        problem.Should().Be(new ExceptionProblem(statusCode, Detail: detail));
    }
}
