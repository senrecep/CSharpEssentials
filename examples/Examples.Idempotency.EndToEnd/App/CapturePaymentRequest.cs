namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The body of <c>POST /payments</c>.</summary>
public sealed record CapturePaymentRequest(string Reference, decimal Amount);
