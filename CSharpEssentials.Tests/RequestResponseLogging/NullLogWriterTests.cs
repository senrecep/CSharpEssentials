using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class NullLogWriterTests
{
    [Fact]
    public void MessageCreator_ShouldBeNull()
    {
        var writer = new NullLogWriter();
        writer.MessageCreator.Should().BeNull();
    }

    [Fact]
    public async Task Write_ShouldReturnCompletedTask()
    {
        var writer = new NullLogWriter();
        var context = new RequestResponseContext(new DefaultHttpContext());

        Task task = writer.Write(context);

        task.IsCompletedSuccessfully.Should().BeTrue();
        await task;
    }
}
