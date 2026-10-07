#if NET9_0_OR_GREATER
using System.Net;

namespace CSharpEssentials.Http;

internal sealed class SsrfGuardedContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly long? _maxLength;
    private readonly CancellationTokenSource? _timeout;
    private readonly Uri? _requestUri;

    public SsrfGuardedContent(HttpContent inner, long? maxLength, CancellationTokenSource? timeout, Uri? requestUri)
    {
        _inner = inner;
        _maxLength = maxLength;
        _timeout = timeout;
        _requestUri = requestUri;

        foreach (KeyValuePair<string, IEnumerable<string>> header in inner.Headers)
            Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        Stream source = await CreateContentReadStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (source.ConfigureAwait(false))
            await source.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        using Stream source = CreateContentReadStream(cancellationToken);
        source.CopyTo(stream);
    }

    protected override Task<Stream> CreateContentReadStreamAsync() => CreateContentReadStreamAsync(CancellationToken.None);

    protected override async Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken) =>
        Wrap(await _inner.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));

    protected override Stream CreateContentReadStream(CancellationToken cancellationToken) =>
        Wrap(_inner.ReadAsStream(cancellationToken));

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _timeout?.Dispose();
        }

        base.Dispose(disposing);
    }

    private SsrfGuardedStream Wrap(Stream stream) =>
        new(stream, _maxLength, _requestUri, _timeout?.Token ?? CancellationToken.None);
}
#endif
