using System.Globalization;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each column must return the same result for the same input, otherwise the
/// benchmark refuses to run.
/// </summary>
internal static class Parity
{
    public static string Of(int value) => value.ToString(CultureInfo.InvariantCulture);

    public static string Of(string? value) => value ?? "<null>";

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
}
