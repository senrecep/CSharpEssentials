using System.Globalization;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

internal static class ErrorInputs
{
    public const string Code = "Order.Invalid";

    public static readonly string[] MetadataKeys = ["OrderId", "Field", "Attempt"];
    public static readonly object[] MetadataValues = [42, "Total", 3];

    public static string[] Codes(int count) => Indexed(Code, count);

    public static string[] Descriptions(int count) => Indexed(Inputs.Message + " ", count);

    public static string Copy(string text) => new(text.AsSpan());

    public static ErrorMetadata CseMetadata() =>
        new ErrorMetadata()
            .AddMetadata(MetadataKeys[0], MetadataValues[0])
            .AddMetadata(MetadataKeys[1], MetadataValues[1])
            .AddMetadata(MetadataKeys[2], MetadataValues[2]);

    public static Dictionary<string, object> ErrorOrMetadata() =>
        new()
        {
            { MetadataKeys[0], MetadataValues[0] },
            { MetadataKeys[1], MetadataValues[1] },
            { MetadataKeys[2], MetadataValues[2] },
        };

    private static string[] Indexed(string prefix, int count)
    {
        string[] items = new string[count];
        for (int i = 0; i < count; i++)
            items[i] = prefix + i.ToString(CultureInfo.InvariantCulture);
        return items;
    }
}
