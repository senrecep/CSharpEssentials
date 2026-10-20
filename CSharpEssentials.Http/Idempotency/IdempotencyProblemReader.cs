using System.Text.Json;

namespace CSharpEssentials.Http;

/// <summary>Reads the start of a 409 problem body to find the <c>Idempotency.InFlight</c> code, then puts it back.</summary>
internal static class IdempotencyProblemReader
{
    internal static async ValueTask<bool> IsInFlightAsync(HttpResponseMessage response, IdempotentRetryOptions options, CancellationToken cancellationToken)
    {
        HttpContent? content = response.Content;
        if (content is null
            || !string.Equals(content.Headers.ContentType?.MediaType, IdempotencyHttpProtocol.ProblemJsonMediaType, StringComparison.OrdinalIgnoreCase)
            || content.Headers.ContentLength > IdempotencyHttpProtocol.MaxProblemBytes)
        {
            return false;
        }

        byte[] buffer = new byte[IdempotencyHttpProtocol.MaxProblemBytes + 1];
        int read = 0;
        bool complete = false;
        Stream? stream = null;
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            timeout.CancelAfter(options.ProblemReadTimeout);
            try
            {
#if NET9_0_OR_GREATER
                stream = await content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
#else
                stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
                while (read < buffer.Length)
                {
                    int count = await stream.ReadAsync(buffer.AsMemory(read), timeout.Token).ConfigureAwait(false);
                    if (count == 0)
                    {
                        complete = true;
                        break;
                    }

                    read += count;
                }
            }
            catch (Exception exception) when (exception is OperationCanceledException or IOException or HttpRequestException or ObjectDisposedException or InvalidOperationException)
            {
                complete = false;
            }
        }

        if (stream is not null)
            Restore(response, content, buffer, read, complete, stream);

        return complete && HasInFlightCode(buffer.AsMemory(0, read));
    }

    private static void Restore(HttpResponseMessage response, HttpContent original, byte[] buffer, int read, bool complete, Stream stream)
    {
        HttpContent restored;
        if (complete)
        {
            restored = new ByteArrayContent(buffer, 0, read);
            CopyHeaders(original, restored);
            response.Content = restored;
            original.Dispose();
            return;
        }

        restored = new StreamContent(new PrefixedStream(buffer, read, stream));
        CopyHeaders(original, restored);
        response.Content = restored;
    }

    private static void CopyHeaders(HttpContent source, HttpContent target)
    {
        foreach (KeyValuePair<string, IEnumerable<string>> header in source.Headers)
            target.Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    private static bool HasInFlightCode(ReadOnlyMemory<byte> body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(IdempotencyHttpProtocol.CodeMember, out JsonElement code)
                && code.ValueKind == JsonValueKind.String
                && code.ValueEquals(IdempotencyHttpProtocol.InFlightCode);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
