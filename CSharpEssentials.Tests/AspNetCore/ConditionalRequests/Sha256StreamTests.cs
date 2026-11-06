using System.Security.Cryptography;
using CSharpEssentials.AspNetCore;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class Sha256StreamTests
{
    private static Stream CreateStream() =>
        (Stream)Activator.CreateInstance(
            typeof(ConditionalRequestOptions).Assembly.GetType("CSharpEssentials.AspNetCore.Sha256Stream", throwOnError: true)!,
            nonPublic: true)!;

    private static byte[] GetHash(Stream stream) =>
        (byte[])stream.GetType().GetMethod("GetHash")!.Invoke(stream, null)!;

    [Fact]
    public void Capabilities_Should_BeWriteOnly()
    {
        using Stream stream = CreateStream();

        stream.CanRead.Should().BeFalse();
        stream.CanSeek.Should().BeFalse();
        stream.CanWrite.Should().BeTrue();
    }

    [Fact]
    public void ReadAndSeekMembers_Should_ThrowNotSupported()
    {
        using Stream stream = CreateStream();

        FluentActions.Invoking(() => stream.Length).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Position).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Position = 0).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Read(new byte[1], 0, 1)).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.Seek(0, SeekOrigin.Begin)).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => stream.SetLength(0)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void WriteArraySegment_Should_HashOnlyTheGivenRange()
    {
        byte[] buffer = [0xFF, 1, 2, 3, 0xFF];
        using Stream stream = CreateStream();

        stream.Write(buffer, 1, 3);
        stream.Flush();

        GetHash(stream).Should().Equal(SHA256.HashData(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public void Write_Should_HashTheConcatenation_When_ArrayAndSpanWritesAreMixed()
    {
        using Stream stream = CreateStream();

        stream.Write([1, 2], 0, 2);
        stream.Write(new ReadOnlySpan<byte>([3, 4]));

        GetHash(stream).Should().Equal(SHA256.HashData(new byte[] { 1, 2, 3, 4 }));
    }

    [Fact]
    public void GetHash_Should_ResetState_When_Called()
    {
        using Stream stream = CreateStream();
        stream.Write([9], 0, 1);
        _ = GetHash(stream);

        stream.Write([1], 0, 1);

        GetHash(stream).Should().Equal(SHA256.HashData(new byte[] { 1 }));
    }
}
