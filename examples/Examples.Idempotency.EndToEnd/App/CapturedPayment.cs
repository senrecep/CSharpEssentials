namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The response of <see cref="CapturePayment"/>, stored with its idempotency record and replayed to retries.</summary>
public sealed record CapturedPayment(Guid PaymentId, string ProviderChargeId, decimal Amount);
