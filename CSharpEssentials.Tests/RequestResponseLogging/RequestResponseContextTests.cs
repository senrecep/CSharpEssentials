using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class RequestResponseContextTests
{
    [Fact]
    public void RequestLength_ShouldReturnNull_WhenRequestBodyIsEmpty()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext);

        context.RequestLength.Should().BeNull();
    }

    [Fact]
    public void RequestLength_ShouldReturnLength_WhenRequestBodyHasValue()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext)
        {
            RequestBody = "hello"
        };

        context.RequestLength.Should().Be(5);
    }

    [Fact]
    public void ResponseLength_ShouldReturnNull_WhenResponseBodyIsNull()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext);

        context.ResponseLength.Should().BeNull();
    }

    [Fact]
    public void ResponseLength_ShouldReturnLength_WhenResponseBodyHasValue()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext)
        {
            ResponseBody = "world"
        };

        context.ResponseLength.Should().Be(5);
    }

    [Fact]
    public void ResponseTime_ShouldReturnEmpty_WhenNotSet()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext);

        context.ResponseTime.Should().BeEmpty();
    }

    [Fact]
    public void ResponseTime_ShouldReturnFormattedValue_WhenSet()
    {
        var httpContext = new DefaultHttpContext();
        var context = new RequestResponseContext(httpContext)
        {
            ResponseCreationTime = TimeSpan.FromSeconds(65.5)
        };

        context.ResponseTime.Should().Be("01:05.500");
    }

    [Fact]
    public void Url_ShouldBuildFromHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        httpContext.Request.Path = "/api/test";

        var context = new RequestResponseContext(httpContext);

        context.Url.Should().Be("https://example.com/api/test");
    }
}
