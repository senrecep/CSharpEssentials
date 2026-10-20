using System.Collections.Concurrent;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Stands in for the payment provider's API: every charge succeeds with a new charge id, and a charge with a key it
/// has already seen returns the charge id of that key.
/// </summary>
public sealed class SandboxPaymentProvider : IPaymentProvider
{
    private readonly ConcurrentDictionary<string, string> _charges = new(StringComparer.Ordinal);

    public ValueTask<string> ChargeAsync(string idempotencyKey, string merchantId, string reference, decimal amount, CancellationToken cancellationToken) =>
        ValueTask.FromResult(_charges.GetOrAdd(idempotencyKey, static _ => "ch_" + Guid.NewGuid().ToString("N")));
}
