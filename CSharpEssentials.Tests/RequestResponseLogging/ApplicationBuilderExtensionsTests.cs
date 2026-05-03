using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void AddRequestResponseLogging_ShouldReturnBuilder()
    {
        var mockBuilder = new Mock<IApplicationBuilder>();
        mockBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                   .Returns(mockBuilder.Object);

        IApplicationBuilder result = mockBuilder.Object.AddRequestResponseLogging(_ => { });

        result.Should().Be(mockBuilder.Object);
    }

    [Fact]
    public void AddRequestResponseLogging_WithLoggerFactory_ShouldConfigureOptions()
    {
        var mockBuilder = new Mock<IApplicationBuilder>();
        mockBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                   .Returns(mockBuilder.Object);

        ILoggerFactory loggerFactory = Mock.Of<ILoggerFactory>();

        IApplicationBuilder result = mockBuilder.Object.AddRequestResponseLogging(options =>
        {
            options.UseLogger(loggerFactory, _ => { });
        });

        result.Should().Be(mockBuilder.Object);
    }
}
