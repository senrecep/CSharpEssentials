namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>Inputs shared by every comparison, built once so each library reads the same values.</summary>
internal static class Inputs
{
    public const string Short = "Short";
    public const string Medium = "Medium";
    public const string Long = "Long";

    public const string NumberJson = "2";
    public const string UnknownJson = "\"gone\"";
    public const string IsoJson = "\"2024-03-14T15:30:45.123Z\"";
    public const string LateListJson = "\"14-Mar-2024\"";
    public const int SourceCount = 1000;

    public static readonly OrderStatus[] Values =
        [.. Enumerable.Range(0, 16).Select(static i => (OrderStatus)(i % 6))];

    public static readonly string ValuesJson =
        "[" + string.Join(",", Values.Select(static v => "\"" + WireName(v) + "\"")) + "]";

    public static readonly int[] Source = [.. Enumerable.Range(0, SourceCount)];

    public static readonly Guid Id = new("c9a646d3-9c61-4cb7-bfcd-ee2522c8f633");

    public static string PascalCase(string length) => length switch
    {
        Short => "OrderLineIds",
        Medium => "CustomerShippingAddressLineValidationResultCacheEntryKeyValueIds",
        _ => string.Concat(Enumerable.Repeat("CustomerShippingAddressLineValidationResultCacheEntryKey", 5)),
    };

    private static string WireName(OrderStatus value) => value switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.InProgress => "in_progress",
        OrderStatus.OnHold => "on_hold",
        OrderStatus.Shipped => "shipped",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Cancelled => "cancelled",
        OrderStatus.Unknown => "unknown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
}
