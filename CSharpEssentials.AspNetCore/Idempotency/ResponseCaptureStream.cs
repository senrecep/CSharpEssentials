namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Write-only stream that forwards every write to the response and keeps a copy of up to <c>limit</c> bytes.
/// </summary>
internal sealed class ResponseCaptureStream(Stream inner, long limit) : Stream
{
    private MemoryStream? _buffer = new();

    public bool Overflowed => _buffer is null;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public byte[] ToArray() => _buffer?.ToArray() ?? [];

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        inner.Write(buffer);
        Capture(buffer);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        Capture(buffer.Span);
    }

    public override void Flush() => inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buffer?.Dispose();
            _buffer = null;
        }

        base.Dispose(disposing);
    }

    private void Capture(ReadOnlySpan<byte> buffer)
    {
        if (_buffer is null)
            return;
        if (_buffer.Length + buffer.Length > limit)
        {
            _buffer.Dispose();
            _buffer = null;
            return;
        }

        _buffer.Write(buffer);
    }
}
