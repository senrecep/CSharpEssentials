namespace CSharpEssentials.Http;

/// <summary>A read-only stream that returns bytes already read from <c>inner</c> before the rest of <c>inner</c>.</summary>
internal sealed class PrefixedStream(byte[] prefix, int prefixLength, Stream inner) : Stream
{
    private int _position;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        if (_position < prefixLength)
            return CopyPrefix(buffer);

        return inner.Read(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position < prefixLength)
            return new ValueTask<int>(CopyPrefix(buffer.Span));

        return inner.ReadAsync(buffer, cancellationToken);
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            inner.Dispose();

        base.Dispose(disposing);
    }

    private int CopyPrefix(Span<byte> buffer)
    {
        int count = Math.Min(buffer.Length, prefixLength - _position);
        prefix.AsSpan(_position, count).CopyTo(buffer);
        _position += count;
        return count;
    }
}
