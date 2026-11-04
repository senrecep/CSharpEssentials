using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class SsrfGuardedStreamTests
{
    private static readonly Uri RequestUri = new("https://example.com/data");

    [Fact]
    public void Read_Should_Return_Inner_Bytes_Within_Limit()
    {
        using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3]), 3, RequestUri, CancellationToken.None);
        byte[] buffer = new byte[8];

        stream.Read(buffer, 0, 8).Should().Be(3);
        buffer[..3].Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Read_Should_Throw_ResponseTooLarge_When_Limit_Exceeded()
    {
        using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3]), 2, RequestUri, CancellationToken.None);

        SsrfBlockedException exception = FluentActions.Invoking(() => stream.Read(new byte[8], 0, 8))
            .Should().Throw<SsrfBlockedException>().Which;

        exception.Reason.Should().Be(SsrfBlockReason.ResponseTooLarge);
        exception.RequestUri.Should().Be(RequestUri);
    }

    [Fact]
    public void Read_Should_Throw_Timeout_When_Timeout_Token_Cancelled()
    {
        using var timeout = new CancellationTokenSource();
        timeout.Cancel();
        using var stream = new SsrfGuardedStream(new MemoryStream([1]), null, RequestUri, timeout.Token);

        SsrfBlockedException exception = FluentActions.Invoking(() => stream.Read(new byte[1], 0, 1))
            .Should().Throw<SsrfBlockedException>().Which;

        exception.Reason.Should().Be(SsrfBlockReason.Timeout);
        exception.InnerException.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public async Task ReadAsync_Should_Count_Toward_Limit()
    {
        await using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3]), 2, RequestUri, CancellationToken.None);

        (await stream.ReadAsync(new byte[2].AsMemory())).Should().Be(2);

        SsrfBlockedException exception = (await FluentActions.Awaiting(() => stream.ReadAsync(new byte[2].AsMemory()).AsTask())
            .Should().ThrowAsync<SsrfBlockedException>()).Which;
        exception.Reason.Should().Be(SsrfBlockReason.ResponseTooLarge);
    }

    [Fact]
    public async Task ReadAsync_With_Caller_Token_And_Timeout_Token_Should_Read_All_Bytes()
    {
        using var timeout = new CancellationTokenSource();
        using var caller = new CancellationTokenSource();
        await using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3, 4, 5]), null, RequestUri, timeout.Token);
        using var output = new MemoryStream();

        await stream.CopyToAsync(output, 2, caller.Token);

        output.ToArray().Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task ReadAsync_Should_Throw_Timeout_When_Timeout_Token_Cancelled_With_Caller_Token()
    {
        using var timeout = new CancellationTokenSource();
        using var caller = new CancellationTokenSource();
        await using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3]), null, RequestUri, timeout.Token);

        (await stream.ReadAsync(new byte[1].AsMemory(), caller.Token)).Should().Be(1);
        await timeout.CancelAsync();

        SsrfBlockedException exception = (await FluentActions.Awaiting(() => stream.ReadAsync(new byte[1].AsMemory(), caller.Token).AsTask())
            .Should().ThrowAsync<SsrfBlockedException>()).Which;
        exception.Reason.Should().Be(SsrfBlockReason.Timeout);
    }

    [Fact]
    public async Task ReadAsync_Should_Throw_OperationCanceled_When_Caller_Token_Cancelled()
    {
        using var timeout = new CancellationTokenSource();
        using var caller = new CancellationTokenSource();
        await using var stream = new SsrfGuardedStream(new MemoryStream([1, 2, 3]), null, RequestUri, timeout.Token);

        (await stream.ReadAsync(new byte[1].AsMemory(), caller.Token)).Should().Be(1);
        await caller.CancelAsync();

        await FluentActions.Awaiting(() => stream.ReadAsync(new byte[1].AsMemory(), caller.Token).AsTask())
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Stream_Should_Be_ReadOnly_And_Not_Seekable()
    {
        using var stream = new SsrfGuardedStream(new MemoryStream(), null, RequestUri, CancellationToken.None);

        stream.CanRead.Should().BeTrue();
        stream.CanSeek.Should().BeFalse();
        stream.CanWrite.Should().BeFalse();
        FluentActions.Invoking(() => stream.Length).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Position).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Position = 0).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Seek(0, SeekOrigin.Begin)).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.SetLength(0)).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Write([1], 0, 1)).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(stream.Flush).Should().NotThrow();
    }

    [Fact]
    public void Dispose_Should_Dispose_Inner_Stream()
    {
        var inner = new MemoryStream([1]);
        var stream = new SsrfGuardedStream(inner, null, RequestUri, CancellationToken.None);

        stream.Dispose();

        inner.CanRead.Should().BeFalse();
    }
}
