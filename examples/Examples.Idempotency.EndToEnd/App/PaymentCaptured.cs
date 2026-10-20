namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Published after a payment is captured. <see cref="MessageId"/> derives from the payment, so a publish repeated by a
/// replayed command carries the same id and the consumer's inbox drops it.
/// </summary>
public sealed record PaymentCaptured(string MessageId, string MerchantId, Guid PaymentId, decimal Amount);
