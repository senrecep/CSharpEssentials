using System.Collections.Immutable;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

public class DependencyInjectionAnalyzerTests
{
    private const string Prelude = """
        using System;
        using CSharpEssentials.DependencyInjection;

        namespace Sample;

        public interface IFoo;

        public interface IClock;

        public interface IRepository<T>;

        """;

    public static TheoryData<string, DiagnosticSeverity, string> InvalidSources => new()
    {
        { "CSE2001", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo))] public sealed class Foo;" },
        { "CSE2001", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IClock clock);" },
        { "CSE2002", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class First : IFoo; [RegisterScoped(typeof(IFoo))] public sealed class Second : IFoo;" },
        { "CSE2003", DiagnosticSeverity.Info, "[RegisterScoped] public sealed class Worker : IFoo;" },
        { "CSE2004", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IClock clock) : IFoo;" },
        { "CSE2005", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator : IFoo { public FooDecorator(IFoo inner) { } public FooDecorator(IFoo inner, IClock clock) { } }" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterTransient(typeof(IClock))] public sealed class Clock : IClock; [RegisterScoped(typeof(IFoo))] public sealed class Foo : IFoo { public Foo(IClock clock) { } }" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IRepository<>))] public sealed class Repository<T> : IRepository<T>; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IRepository<int> repository) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterTransient(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices(\"utc\")] IClock clock) : IFoo;" },
        { "CSE2007", DiagnosticSeverity.Error, "[RegisterScoped] public abstract class Foo : IFoo;" },
        { "CSE2007", DiagnosticSeverity.Error, "[RegisterScoped] public static class Foo;" },
        { "CSE2007", DiagnosticSeverity.Error, "public sealed class Outer { [RegisterScoped] private sealed class Foo : IFoo; }" },
        { "CSE2008", DiagnosticSeverity.Warning, "[Decorates(typeof(IRepository<>))] public sealed class RepositoryDecorator<T>(IRepository<T> inner) : IRepository<T>;" },
    };

    public static TheoryData<string, string> ValidSources => new()
    {
        { "CSE2001", "[RegisterScoped(typeof(IFoo))] public sealed class Foo : IFoo; [Decorates(typeof(IFoo))] public sealed class FooDecorator(IFoo inner) : IFoo;" },
        { "CSE2002", "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class First : IFoo; [RegisterScoped(typeof(IFoo), Key = \"second\")] public sealed class Second : IFoo;" },
        { "CSE2003", "[RegisterScoped] public sealed class Foo : IFoo;" },
        { "CSE2004", "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IFoo inner, IClock clock) : IFoo;" },
        { "CSE2005", "[Decorates(typeof(IFoo))] public sealed class FooDecorator : IFoo { public FooDecorator(IFoo inner) { } private FooDecorator() { } }" },
        { "CSE2006", "[RegisterSingleton(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterTransient(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo : IFoo { public Foo(IClock clock) { } public Foo() { } }" },
        { "CSE2006", "[RegisterTransient(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2007", "[RegisterScoped] public sealed class Foo : IFoo; public sealed class Outer { [RegisterScoped] internal sealed class Nested : IClock; }" },
        { "CSE2008", "[Decorates(typeof(IRepository<int>))] public sealed class RepositoryDecorator(IRepository<int> inner) : IRepository<int>;" },
    };

    [Theory]
    [MemberData(nameof(InvalidSources))]
    public async Task Analyzer_Should_Report_Diagnostic_When_Declaration_Is_Invalid(string id, DiagnosticSeverity severity, string declaration)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Prelude + declaration);

        diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == id).Which.Severity.Should().Be(severity);
    }

    [Theory]
    [MemberData(nameof(ValidSources))]
    public async Task Analyzer_Should_Not_Report_Diagnostic_When_Declaration_Is_Valid(string id, string declaration)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Prelude + declaration);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == id);
    }

    [Fact]
    public async Task Analyzer_Should_Report_At_Attribute_Location()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Prelude + "[RegisterScoped(typeof(IFoo))] public sealed class Foo;");

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Location.SourceTree!.ToString().Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)
            .Should().Be("RegisterScoped(typeof(IFoo))");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE2006_With_Lifetimes_At_Consumer_Attribute()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            Prelude + "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;");

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(static d => d.Id == "CSE2006").Subject;
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture)
            .Should().Be("Singleton 'Sample.Foo' depends on 'Sample.IClock' through parameter 'clock', but 'Sample.Clock' registers it as Scoped");
        diagnostic.Location.SourceTree!.ToString().Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)
            .Should().Be("RegisterSingleton(typeof(IFoo))");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_When_Assembly_Is_Excluded()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            Prelude.Replace("namespace Sample;", "[assembly: ExcludeFromRegistration]\n\nnamespace Sample;", StringComparison.Ordinal) +
            "[RegisterScoped(typeof(IFoo))] public sealed class Foo;");

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_For_Excluded_Type()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Prelude + "[RegisterScoped(typeof(IFoo))] [ExcludeFromRegistration] public sealed class Foo;");

        diagnostics.Should().BeEmpty();
    }

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source) =>
        AnalyzerHarness.GetAnalyzerDiagnosticsAsync(ServiceGeneration.CreateCompilation(source), ServiceGeneration.Analyzers);
}
