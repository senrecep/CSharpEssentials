using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using Mediator;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// A settlement event from the payment provider's webhook. The provider's event id is the key and the provider
/// account is the scope; the fingerprint is constant because the event id is authoritative: a resend that serializes
/// the event differently is still the same event.
/// </summary>
public sealed record RecordSettlement(string EventId, string AccountId, decimal Amount)
    : ICommand<Result<Guid>>, IIdempotentRequest, ITransactionalRequest
{
    public const string Consumer = "payment-provider-webhook";

    public const string AccountScopeKind = "account";

    string IIdempotentRequest.IdempotencyKey => IdempotencyKeys.FromMessage(EventId, Consumer);

    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Custom(AccountScopeKind, AccountId);

    string? IIdempotentRequest.IdempotencyFingerprint => Consumer;
}
