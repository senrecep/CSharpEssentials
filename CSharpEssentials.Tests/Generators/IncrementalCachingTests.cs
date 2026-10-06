using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit.Sdk;

namespace CSharpEssentials.Tests.Generators;

public class IncrementalCachingTests
{
    private const string Source = """
        namespace Sample;

        [System.AttributeUsage(System.AttributeTargets.Class)]
        public sealed class TrackedAttribute : System.Attribute { }

        [Tracked]
        public partial class Customer { }
        """;

    [Fact]
    public void ShouldHaveCachedSteps_Should_Pass_When_Unrelated_Source_Is_Added()
    {
        GeneratorDriverRunResult second = RunTwice(static c => GeneratorHarness.AddSource(c, "namespace Other; public class Unrelated { }"));

        Action act = () =>
        {
            IncrementalCaching.ShouldHaveCachedSteps(second, TrackedNamesGenerator.NamesStep);
            IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveCachedSteps_Should_Throw_When_Tracked_Value_Changes()
    {
        GeneratorDriverRunResult second = RunTwice(static c =>
        {
            SyntaxTree tree = c.SyntaxTrees.Single();
            return c.ReplaceSyntaxTree(tree, tree.WithChangedText(SourceText.From(Source.Replace("Customer", "Order", StringComparison.Ordinal))));
        });

        Action act = () => IncrementalCaching.ShouldHaveCachedSteps(second, TrackedNamesGenerator.NamesStep);

        act.Should().Throw<XunitException>().WithMessage("*Names*");
    }

    [Fact]
    public void ShouldHaveCachedSteps_Should_Throw_When_Tracking_Name_Is_Unknown()
    {
        GeneratorDriverRunResult second = RunTwice(static c => c);

        Action act = () => IncrementalCaching.ShouldHaveCachedSteps(second, "Missing");

        act.Should().Throw<XunitException>().WithMessage("*Missing*");
    }

    [Fact]
    public void ShouldNotCaptureCompilationObjects_Should_Pass_When_Step_Outputs_Are_Values()
    {
        GeneratorDriverRunResult second = RunTwice(static c => c);

        Action act = () => IncrementalCaching.ShouldNotCaptureCompilationObjects(second, TrackedNamesGenerator.NamesStep);

        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldNotCaptureCompilationObjects_Should_Throw_When_Step_Outputs_Symbols()
    {
        GeneratorDriverRunResult second = RunTwice(static c => c);

        Action act = () => IncrementalCaching.ShouldNotCaptureCompilationObjects(second, TrackedNamesGenerator.SymbolsStep);

        act.Should().Throw<XunitException>().WithMessage("*Symbols*NamedTypeSymbol*");
    }

    private static GeneratorDriverRunResult RunTwice(Func<Compilation, Compilation> secondInput) =>
        IncrementalCaching.RunTwice(
            GeneratorHarness.CreateCompilation([Source]),
            secondInput,
            new TrackedNamesGenerator().AsSourceGenerator());
}
