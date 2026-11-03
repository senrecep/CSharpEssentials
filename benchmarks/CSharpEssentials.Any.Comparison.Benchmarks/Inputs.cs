using System.Globalization;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>Inputs shared by every comparison, built once so each library reads the same values.</summary>
internal static class Inputs
{
    public const string IntBranch = "Int";
    public const string TextBranch = "Text";

    public const int IntValue = 42;
    public const string TextValue = "forty-two";
    public const string Warning = "warn";
    public const int Count = 1000;

    public static readonly string[] Texts = [.. Enumerable.Range(0, Count).Select(static i => "t" + i.ToString(CultureInfo.InvariantCulture))];

    public static bool IsInt(int index) => index % 2 == 0;
}
