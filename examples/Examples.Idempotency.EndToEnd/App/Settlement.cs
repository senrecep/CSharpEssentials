namespace Examples.Idempotency.EndToEnd.App;

/// <summary>A settlement the payment provider reported through its webhook.</summary>
public sealed class Settlement
{
    public Guid Id { get; set; }

    public string ProviderEventId { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
