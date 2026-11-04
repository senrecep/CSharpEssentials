using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Self-check only. Runs each row with step tracking on, applies the edit, and prints every tracked step with its counts
/// per <see cref="IncrementalStepRunReason"/>, plus the generated file and character counts. Only CSE rows are asserted:
/// every source output must be Cached or Unchanged after the unrelated edit.
/// </summary>
public static class IncrementalReport
{
    private static readonly GeneratorDriverOptions TrackedOptions = new(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);

    public static void Print(string scenario, int files, GeneratorRow row)
    {
        GeneratorDriver warm = GeneratorRow.CreateDriver(row.Generators, TrackedOptions).RunGenerators(row.OriginalCompilation());
        GeneratorDriverRunResult result = warm.RunGenerators(row.EditedCompilation()).GetRunResult();
        GeneratedSourceResult[] sources = [.. result.Results.SelectMany(static r => r.GeneratedSources)];
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{scenario}.{row.Name} Files={files}: {sources.Length} generated files, {sources.Sum(static s => s.SourceText.Length)} chars"));

        foreach (GeneratorRunResult run in result.Results)
        {
            string generator = run.Generator.GetGeneratorType().Name;
            Write(generator, "step", run.TrackedSteps);
            Write(generator, "output", run.TrackedOutputSteps);
            if (row.IsCse)
            {
                AssertCached(string.Create(CultureInfo.InvariantCulture, $"{scenario}.{row.Name} (Files={files}) {generator}"), run.TrackedOutputSteps);
            }
        }
    }

    private static void Write(string generator, string kind, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> steps)
    {
        foreach ((string key, ImmutableArray<IncrementalGeneratorRunStep> runs) in steps.OrderBy(static s => s.Key, StringComparer.Ordinal))
        {
            string counts = string.Join(" ", runs
                .SelectMany(static r => r.Outputs)
                .GroupBy(static o => o.Reason)
                .OrderBy(static g => g.Key)
                .Select(static g => string.Create(CultureInfo.InvariantCulture, $"{g.Key}={g.Count()}")));
            Console.WriteLine($"  {generator} {kind} {key}: {counts}");
        }
    }

    private static void AssertCached(string where, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> outputs)
    {
        foreach ((string key, ImmutableArray<IncrementalGeneratorRunStep> runs) in outputs)
        {
            foreach (IncrementalStepRunReason reason in runs.SelectMany(static r => r.Outputs).Select(static o => o.Reason))
            {
                if (reason is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged))
                {
                    throw new InvalidOperationException($"Incremental check failed in {where}: output '{key}' was {reason} after an unrelated edit.");
                }
            }
        }
    }
}
