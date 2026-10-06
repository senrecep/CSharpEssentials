using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests.Generators;

public static class IncrementalCaching
{
    private static readonly IncrementalStepRunReason[] CachedReasons = [IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged];

    public static GeneratorDriverRunResult RunTwice(
        Compilation compilation,
        Func<Compilation, Compilation> secondInput,
        params ISourceGenerator[] generators)
    {
        GeneratorDriver driver = GeneratorHarness.CreateDriver(generators).RunGenerators(compilation);
        return driver.RunGenerators(secondInput(compilation)).GetRunResult();
    }

    public static void ShouldHaveCachedSteps(GeneratorDriverRunResult runResult, params string[] trackingNames)
    {
        foreach (string trackingName in trackingNames)
        {
            StepOutputs(runResult, trackingName)
                .Select(static output => output.Reason)
                .Should().OnlyContain(reason => CachedReasons.Contains(reason), "tracked step '{0}' must be cached on the second run", trackingName);
        }
    }

    public static void ShouldHaveCachedSourceOutputs(GeneratorDriverRunResult runResult)
    {
        IncrementalStepRunReason[] reasons = [.. runResult.Results
            .SelectMany(static result => result.TrackedOutputSteps.Values)
            .SelectMany(static steps => steps)
            .SelectMany(static step => step.Outputs)
            .Select(static output => output.Reason)];

        reasons.Should().NotBeEmpty("the generator must register at least one source output");
        reasons.Should().OnlyContain(reason => CachedReasons.Contains(reason), "every source output must be cached on the second run");
    }

    public static void ShouldNotCaptureCompilationObjects(GeneratorDriverRunResult runResult, params string[] trackingNames)
    {
        foreach (string trackingName in trackingNames)
        {
            List<string> violations = [];
            HashSet<object> visited = new(ReferenceEqualityComparer.Instance);
            foreach ((object value, IncrementalStepRunReason _) in StepOutputs(runResult, trackingName))
            {
                CollectViolations(value, trackingName, visited, violations);
            }

            violations.Should().BeEmpty("tracked step '{0}' must only produce value-equatable models", trackingName);
        }
    }

    private static ImmutableArray<(object Value, IncrementalStepRunReason Reason)> StepOutputs(GeneratorDriverRunResult runResult, string trackingName)
    {
        IncrementalGeneratorRunStep[] steps = [.. runResult.Results
            .SelectMany(result => result.TrackedSteps.TryGetValue(trackingName, out ImmutableArray<IncrementalGeneratorRunStep> named) ? named : [])];

        steps.Should().NotBeEmpty("tracked step '{0}' must exist in the run result", trackingName);
        return [.. steps.SelectMany(static step => step.Outputs)];
    }

    private static void CollectViolations(object? value, string path, HashSet<object> visited, List<string> violations)
    {
        if (value is null)
        {
            return;
        }

        Type type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || value is string || value is decimal)
        {
            return;
        }

        if (value is ISymbol or Compilation or SemanticModel or SyntaxNode or SyntaxTree or Location)
        {
            violations.Add($"{path}: {type.FullName}");
            return;
        }

        if (!type.IsValueType && !visited.Add(value))
        {
            return;
        }

        if (value is IEnumerable items)
        {
            int index = 0;
            foreach (object? item in items)
            {
                CollectViolations(item, $"{path}[{index++}]", visited, violations);
            }

            return;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            CollectViolations(field.GetValue(value), $"{path}.{field.Name}", visited, violations);
        }
    }
}
