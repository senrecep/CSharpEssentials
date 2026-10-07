#if NET9_0_OR_GREATER
namespace CSharpEssentials.Http;

internal sealed class SsrfGuardedStream(Stream inner, long? maxLength, Uri? requestUri, CancellationToken timeoutToken) : Stream
{
    // Like any Stream, this assumes a single concurrent reader; the counter and linked token source are not synchronized.
    private long _bytesRead;
    private CancellationTokenSource? _linked;
    private CancellationToken _linkedCallerToken;

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
        if (timeoutToken.IsCancellationRequested)
            throw SsrfGuardHandler.CreateTimeoutException(requestUri, new OperationCanceledException(timeoutToken));

        return Count(inner.Read(buffer));
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!timeoutToken.CanBeCanceled)
            return Count(await inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false));

        try
        {
            return Count(await inner.ReadAsync(buffer, GetReadToken(cancellationToken)).ConfigureAwait(false));
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw SsrfGuardHandler.CreateTimeoutException(requestUri, ex);
        }
    }

    // Copy loops pass the same caller token to every read, so the linked source is reused until the token changes.
    private CancellationToken GetReadToken(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
            return timeoutToken;

        if (_linked is null || _linkedCallerToken != cancellationToken)
        {
            _linked?.Dispose();
            _linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);
            _linkedCallerToken = cancellationToken;
        }

        return _linked.Token;
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
        {
            _linked?.Dispose();
            inner.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        _linked?.Dispose();
        await inner.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }

    private int Count(int read)
    {
        _bytesRead += read;
        if (maxLength is { } max && _bytesRead > max)
        {
            throw new SsrfBlockedException(
                SsrfBlockReason.ResponseTooLarge,
                $"The response content exceeded the limit of {max} bytes.",
                requestUri);
        }

        return read;
    }
}
#endif
