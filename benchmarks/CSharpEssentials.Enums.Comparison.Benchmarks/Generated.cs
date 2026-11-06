namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Where the NetEscapades.EnumGenerators extension classes go. Both generators emit an <c>AsUnderlyingType(this T)</c>
/// extension, and the NetEscapades <c>ToStringFast</c> fallback calls it as an extension, so its class sits in a child
/// namespace where its own overload is found first.
/// </summary>
internal static class Generated
{
    public const string Namespace = "CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated";
}
