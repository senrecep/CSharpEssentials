using CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;
using FluentAssertions;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class ResponseCaptureStreamTests
{
    [Fact]
    public void Write_Should_SendToClientAndCapture_When_WritingSynchronously()
    {
        var client = new MemoryStream();
        var capture = new MemoryStream();
        using var stream = new ResponseCaptureStream(client, capture);

        stream.Write("sync"u8);
        stream.Write("-body"u8.ToArray(), 0, 5);
        stream.WriteByte((byte)'!');

        client.ToArray().Should().Equal("sync-body!"u8.ToArray());
        capture.ToArray().Should().Equal("sync-body!"u8.ToArray());
    }

    [Fact]
    public async Task WriteAsync_Should_NotCapture_When_ClientWriteFails()
    {
        var capture = new MemoryStream();
        await using var stream = new ResponseCaptureStream(new MemoryStream([], writable: false), capture);

        Func<Task> act = () => stream.WriteAsync("data"u8.ToArray(), 0, 4);

        await act.Should().ThrowAsync<NotSupportedException>();
        capture.Length.Should().Be(0);
    }

    [Fact]
    public void Dispose_Should_LeaveInnerStreamsOpen()
    {
        var client = new MemoryStream();
        var capture = new MemoryStream();

        new ResponseCaptureStream(client, capture).Dispose();

        client.CanWrite.Should().BeTrue();
        capture.CanWrite.Should().BeTrue();
    }
}
