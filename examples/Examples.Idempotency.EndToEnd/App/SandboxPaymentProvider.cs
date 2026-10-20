namespace Examples.Idempotency.EndToEnd.App;

/// <summary>Stands in for the payment provider's API: every charge succeeds with a new charge id.</summary>
public sealed class SandboxPaymentProvider : IPaymentProvider
{
    public ValueTask<string> ChargeAsync(string merchantId, string reference, decimal amount, CancellationToken cancellationToken) =>
        ValueTask.FromResult("ch_" + Guid.NewGuid().ToString("N"));
}
