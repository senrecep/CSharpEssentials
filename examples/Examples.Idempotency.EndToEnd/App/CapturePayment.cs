using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using Mediator;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Captures a payment. The key is the client's <c>Idempotency-Key</c> header and the scope is the calling merchant,
/// so the command is deduplicated in the database even when the HTTP layer of another instance never saw the key.
/// The fingerprint is the serialized command, so the same key with another amount is rejected.
/// </summary>
public sealed record CapturePayment(string MerchantId, string Key, string Reference, decimal Amount)
    : ICommand<Result<CapturedPayment>>, IIdempotentRequest, ITransactionalRequest
{
    string IIdempotentRequest.IdempotencyKey => Key;

    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Client(MerchantId);
}
