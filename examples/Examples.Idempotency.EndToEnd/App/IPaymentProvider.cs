namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The external payment provider that moves the money.</summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Charges <paramref name="amount"/> to the merchant's customer and returns the provider's charge id. The provider
    /// dedupes on <paramref name="idempotencyKey"/>: a second call with the same key returns the first charge instead
    /// of charging again.
    /// </summary>
    ValueTask<string> ChargeAsync(string idempotencyKey, string merchantId, string reference, decimal amount, CancellationToken cancellationToken);
}
