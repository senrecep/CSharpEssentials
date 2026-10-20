namespace Examples.Idempotency.EndToEnd.App;

/// <summary>A payment captured through the payment provider for a merchant.</summary>
public sealed class Payment
{
    public Guid Id { get; set; }

    public string MerchantId { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? ProviderChargeId { get; set; }
}
