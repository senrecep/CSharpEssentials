namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The body of a payment provider webhook.</summary>
public sealed record ProviderEvent(string Id, string Type, string AccountId, decimal Amount);
