using System.Security.Cryptography;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each row must describe the same API, otherwise the benchmark refuses to run.
/// </summary>
internal static class Parity
{
    public static string Hash(byte[] body) => Convert.ToHexString(SHA256.HashData(body));

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

    public static void True(string scenario, bool condition, string detail)
    {
        if (!condition)
            throw new InvalidOperationException($"Parity check failed in {scenario}: {detail}.");
    }
}
