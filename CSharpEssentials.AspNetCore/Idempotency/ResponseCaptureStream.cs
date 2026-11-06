using System.Buffers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Write-only stream that forwards every write to the response and keeps a copy of up to <c>limit</c> bytes in a pooled
/// buffer; <see cref="Captured"/> is valid until the stream is disposed.
/// </summary>
internal sealed class ResponseCaptureStream(Stream inner, long limit, IHeaderDictionary headers) : Stream
{
    private byte[] _buffer = [];
    private int _length;
    private bool _used;
    private bool _encodedAtFirstUse;

    public bool Overflowed { get; private set; }

    /// <summary>
    /// Whether the handler encoded the body itself. <c>Content-Encoding</c> is read before the first write or flush
    /// reaches the inner stream: response compression outside the middleware sets it only while that call passes
    /// through it, and then the captured bytes are still the uncompressed body.
    /// </summary>
    public bool EncodedByHandler => _used ? _encodedAtFirstUse : IsEncoded(headers.ContentEncoding);

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public ReadOnlyMemory<byte> Captured => _buffer.AsMemory(0, _length);

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        BeforeFirstUse();
        inner.Write(buffer);
        Capture(buffer);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        BeforeFirstUse();
        await inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        Capture(buffer.Span);
    }

    public override void Flush()
    {
        BeforeFirstUse();
        inner.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        BeforeFirstUse();
        return inner.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Overflowed = true;
            ReturnBuffer();
        }

        base.Dispose(disposing);
    }

    private void BeforeFirstUse()
    {
        if (_used)
            return;
        _used = true;
        _encodedAtFirstUse = IsEncoded(headers.ContentEncoding);
    }

    private static bool IsEncoded(StringValues contentEncoding)
    {
        foreach (string? value in contentEncoding)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;
            foreach (string coding in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.Equals(coding, "identity", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private void Capture(ReadOnlySpan<byte> buffer)
    {
        if (Overflowed || buffer.IsEmpty)
            return;
        long required = (long)_length + buffer.Length;
        if (required > limit)
        {
            Overflowed = true;
            ReturnBuffer();
            return;
        }

        if (required > _buffer.Length)
        {
            byte[] previous = _buffer;
            _buffer = ArrayPool<byte>.Shared.Rent((int)Math.Min(Math.Max(required, Math.Max(256L, previous.Length * 2L)), Math.Min(limit, Array.MaxLength)));
            previous.AsSpan(0, _length).CopyTo(_buffer);
            if (previous.Length > 0)
                ArrayPool<byte>.Shared.Return(previous);
        }

        buffer.CopyTo(_buffer.AsSpan(_length));
        _length += buffer.Length;
    }

    private void ReturnBuffer()
    {
        byte[] buffer = _buffer;
        _buffer = [];
        _length = 0;
        if (buffer.Length > 0)
            ArrayPool<byte>.Shared.Return(buffer);
    }
}
