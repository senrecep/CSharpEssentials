namespace Examples.Idempotency.EndToEnd.App;

/// <summary>The receipt the message consumer issues for a captured payment.</summary>
public sealed class Receipt
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public string MerchantId { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
