using System.Security.Cryptography;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://learn.microsoft.com/dotnet/api/system.security.cryptography.sha256.hashdata
public sealed class MediatorIdempotencyBehavior(HandWrittenIdempotencyStore store) : IPipelineBehavior<PlaceOrder, Result<int>>
{
    private const string RequestType = "CSharpEssentials.Mediator.Benchmarks.PlaceOrder";

    public async ValueTask<Result<int>> Handle(PlaceOrder message, MessageHandlerDelegate<PlaceOrder, Result<int>> next, CancellationToken cancellationToken)
    {
        string key = message.IdempotencyKey;
        if (string.IsNullOrEmpty(key))
        {
            return Result<int>.Failure(Error.Validation("Idempotency.KeyMissing", "The request has no idempotency key."));
        }

        string fingerprint = message.IdempotencyFingerprint ?? Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(message)));
        (string, string) recordKey = (RequestType, key);
        if (store.Records.TryGetValue(recordKey, out IdempotencyRecord? existing))
        {
            if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
            {
                return Result<int>.Failure(Error.Validation("Idempotency.KeyReused", "This idempotency key was already used for a different request."));
            }

            return existing.Payload is null
                ? Result<int>.Failure(Error.Conflict("Idempotency.InFlight", "A request with this idempotency key is still being processed. Retry later."))
                : JsonSerializer.Deserialize<Result<int>>(existing.Payload);
        }

        if (!store.Records.TryAdd(recordKey, new IdempotencyRecord(fingerprint, null)))
        {
            return Result<int>.Failure(Error.Conflict("Idempotency.InFlight", "A request with this idempotency key is still being processed. Retry later."));
        }

        Result<int> result = await next(message, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            store.Records.TryRemove(recordKey, out _);
        }
        else
        {
            store.Records[recordKey] = new IdempotencyRecord(fingerprint, JsonSerializer.SerializeToUtf8Bytes(result));
        }

        return result;
    }
}
