using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class PrefixedStreamTests
{
    private static readonly byte[] Prefix = [1, 2, 3, 99];

    [Fact]
    public void Read_Should_Return_Prefix_Then_Inner_Bytes()
    {
        using var stream = new PrefixedStream(Prefix, 3, new MemoryStream([4, 5]));
        byte[] buffer = new byte[2];

        stream.Read(buffer, 0, 2).Should().Be(2);
        buffer.Should().Equal(1, 2);
        stream.Read(buffer, 0, 2).Should().Be(1);
        buffer[0].Should().Be(3);
        stream.Read(buffer, 0, 2).Should().Be(2);
        buffer.Should().Equal(4, 5);
        stream.Read(buffer, 0, 2).Should().Be(0);
    }

    [Fact]
    public async Task ReadAsync_Should_Return_Prefix_Then_Inner_Bytes()
    {
        await using var stream = new PrefixedStream(Prefix, 3, new MemoryStream([4, 5]));
        using var output = new MemoryStream();

        await stream.CopyToAsync(output);

        output.ToArray().Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task ReadAsync_Memory_Should_Return_Prefix_Then_Inner_Bytes()
    {
        await using var stream = new PrefixedStream(Prefix, 1, new MemoryStream([7]));
        byte[] buffer = new byte[4];

        (await stream.ReadAsync(buffer.AsMemory())).Should().Be(1);
        buffer[0].Should().Be(1);
        (await stream.ReadAsync(buffer.AsMemory())).Should().Be(1);
        buffer[0].Should().Be(7);
    }

    [Fact]
    public void Stream_Should_Be_ReadOnly_And_Not_Seekable()
    {
        using var stream = new PrefixedStream(Prefix, 0, new MemoryStream());

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
        var stream = new PrefixedStream(Prefix, 1, inner);

        stream.Dispose();

        inner.CanRead.Should().BeFalse();
    }
}
