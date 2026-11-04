using System.Globalization;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each row must return the same result for the same input, otherwise the
/// benchmark refuses to run.
/// </summary>
internal static class Parity
{
    public static string Of(long value) => value.ToString(CultureInfo.InvariantCulture);

    public static string Of(IReadOnlyList<CloneItem> result, IReadOnlyList<CloneItem> source)
    {
        bool ordered = result.Count == source.Count;
        bool fresh = true;
        for (int i = 0; ordered && i < result.Count; i++)
        {
            ordered = result[i].Id == source[i].Id;
            fresh &= !ReferenceEquals(result[i], source[i]);
        }

        return Describe(result, ordered, fresh);
    }

    public static string Expected(IReadOnlyList<CloneItem> source) => Describe(source, true, true);

    public static void Holds(string scenario, string row, bool condition)
    {
        if (!condition)
            throw new InvalidOperationException($"Parity check failed in {scenario}: {row} returned a value outside the expected range.");
    }

    public static void Same(string scenario, params (string Library, string Outcome)[] outcomes)
    {
        (string firstLibrary, string expected) = outcomes[0];
        foreach ((string library, string outcome) in outcomes)
        {
            if (!string.Equals(outcome, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Parity check failed in {scenario}: {firstLibrary} gave '{expected}' but {library} gave '{outcome}'.");
            }
        }
    }

    private static string Describe(IReadOnlyList<CloneItem> items, bool ordered, bool fresh) =>
        string.Create(CultureInfo.InvariantCulture,
            $"n={items.Count};ids={items.Sum(static x => (long)x.Id)};chars={items.Sum(static x => x.Name.Length)};ordered={ordered};fresh={fresh}");
}
