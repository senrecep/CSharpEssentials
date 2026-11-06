using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each row must discover the same type names, produce compilable output and give
/// the same output cold and incremental, otherwise the benchmark refuses to run.
/// </summary>
public static class GeneratorParity
{
    public static GeneratorDriverRunResult Check(string scenario, int files, GeneratorRow row, Regex names, IReadOnlyCollection<string> expected, bool checkErrors)
    {
        string where = string.Create(CultureInfo.InvariantCulture, $"{scenario}.{row.Name} (Files={files})");
        GeneratorDriver cold = GeneratorRow.CreateDriver(row.Generators, GeneratorRow.MeasuredOptions)
            .RunGeneratorsAndUpdateCompilation(row.OriginalCompilation(), out Compilation output, out _);
        GeneratorDriverRunResult coldResult = cold.GetRunResult();
        NoGeneratorExceptions(where, coldResult);
        SameNames(where, expected, NameSet(coldResult, names));
        if (checkErrors)
        {
            NoErrors(where, output);
        }

        SameOutput(where, coldResult, row.Incremental().GetRunResult());
        return coldResult;
    }

    public static SortedSet<string> NameSet(GeneratorDriverRunResult result, Regex pattern) =>
        NameSet(result, pattern, static match => match.Value);

    public static SortedSet<string> NameSet(GeneratorDriverRunResult result, Regex pattern, Func<Match, string> select)
    {
        SortedSet<string> found = [with(StringComparer.Ordinal)];
        foreach (GeneratedSourceResult source in result.Results.SelectMany(static r => r.GeneratedSources))
        {
            foreach (Match match in pattern.Matches(source.SourceText.ToString()))
            {
                found.Add(select(match));
            }
        }

        return found;
    }

    public static void SameNames(string where, IReadOnlyCollection<string> expected, SortedSet<string> actual)
    {
        if (!actual.SetEquals(expected))
        {
            string missing = string.Join(", ", expected.Except(actual).Take(5));
            string extra = string.Join(", ", actual.Except(expected).Take(5));
            throw new InvalidOperationException(
                $"Parity check failed in {where}: expected {expected.Count} names, found {actual.Count}. Missing: [{missing}] Extra: [{extra}]");
        }
    }

    public static void NoErrors(string where, Compilation output)
    {
        Diagnostic[] errors = [.. output.GetDiagnostics().Where(static d => d.Severity == DiagnosticSeverity.Error)];
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                $"Parity check failed in {where}: {errors.Length} compile errors, first: {errors[0]}");
        }
    }

    public static void NoGeneratorExceptions(string where, GeneratorDriverRunResult result)
    {
        foreach (GeneratorRunResult run in result.Results)
        {
            if (run.Exception is not null)
            {
                throw new InvalidOperationException($"Parity check failed in {where}: {run.Generator.GetGeneratorType().Name} threw {run.Exception}");
            }
        }

        if (result.Diagnostics.FirstOrDefault(static d => d.Severity == DiagnosticSeverity.Error) is { } error)
        {
            throw new InvalidOperationException($"Parity check failed in {where}: generator error {error}");
        }
    }

    public static void SameOutput(string where, GeneratorDriverRunResult cold, GeneratorDriverRunResult incremental)
    {
        string[] left = Flatten(cold);
        string[] right = Flatten(incremental);
        if (left.Length == 0 || !left.SequenceEqual(right, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Parity check failed in {where}: cold run gave {left.Length} sources, incremental run gave {right.Length}, or their text differs.");
        }
    }

    private static string[] Flatten(GeneratorDriverRunResult result) =>
        [.. result.Results
            .SelectMany(static r => r.GeneratedSources)
            .OrderBy(static s => s.HintName, StringComparer.Ordinal)
            .Select(static s => s.HintName + "\n" + s.SourceText)];
}
