namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The external payment provider that moves the money.</summary>
public interface IPaymentProvider
{
    /// <summary>Charges <paramref name="amount"/> to the merchant's customer and returns the provider's charge id.</summary>
    ValueTask<string> ChargeAsync(string merchantId, string reference, decimal amount, CancellationToken cancellationToken);
}
