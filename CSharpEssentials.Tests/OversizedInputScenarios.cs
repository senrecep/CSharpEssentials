using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests;

internal static class OversizedInputScenarios
{
    public const string ToSnakeCase = "tosnakecase";
    public const string GetRandomItems = "getrandomitems";

    public const int StackSizeBytes = 1024 * 1024;

    public static Action? Find(string name) => name switch
    {
        ToSnakeCase => RunToSnakeCase,
        GetRandomItems => RunGetRandomItems,
        _ => null,
    };

    private static void RunToSnakeCase()
    {
        const int pairs = 3_000_000;
        string input = string.Concat(Enumerable.Repeat("aB", pairs));

        string result = input.ToSnakeCase();

        result.Length.Should().Be(pairs * 3);
        result.AsSpan(0, 7).ToString().Should().Be("a_ba_ba");
    }

    private static void RunGetRandomItems()
    {
        int[] source = new int[6_000_000];
        for (int i = 0; i < source.Length; i++)
            source[i] = i;

        int[] result = source.GetRandomItems(3);

        result.Distinct().Should().HaveCount(3);
    }
}
