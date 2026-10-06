using System.Collections.Immutable;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    private const string FromKeyedServicesStub = """
        namespace Microsoft.Extensions.DependencyInjection;

        [System.AttributeUsage(System.AttributeTargets.Parameter)]
        public sealed class FromKeyedServicesAttribute : System.Attribute
        {
            public FromKeyedServicesAttribute() { }

            public FromKeyedServicesAttribute(object? key) => Key = key;

            public object? Key { get; }
        }
        """;

    public static TheoryData<string, DiagnosticSeverity, string> InvalidSources => new()
    {
        { "CSE2001", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo))] public sealed class Foo;" },
        { "CSE2001", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IClock clock);" },
        { "CSE2002", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo))] public sealed class First : IFoo; [RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class Second : IFoo;" },
        { "CSE2002", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo), Key = \"k\")] [RegisterSingleton(typeof(IFoo), Key = \"k\", Strategy = RegistrationStrategy.Throw)] public sealed class Foo : IFoo;" },
        { "CSE2001", DiagnosticSeverity.Error, "[RegisterScoped(typeof(IFoo))] public sealed record FooRecord;" },
        { "CSE2003", DiagnosticSeverity.Info, "[RegisterScoped] public sealed class Worker : IFoo;" },
        { "CSE2003", DiagnosticSeverity.Info, "[RegisterScoped] public sealed record WorkerRecord : IFoo;" },
        { "CSE2004", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed record FooRecordDecorator(IClock Clock) : IFoo;" },
        { "CSE2004", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IClock clock) : IFoo;" },
        { "CSE2005", DiagnosticSeverity.Error, "[Decorates(typeof(IFoo))] public sealed class FooDecorator : IFoo { public FooDecorator(IFoo inner) { } public FooDecorator(IFoo inner, IClock clock) { } }" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterTransient(typeof(IClock))] public sealed class Clock : IClock; [RegisterScoped(typeof(IFoo))] public sealed class Foo : IFoo { public Foo(IClock clock) { } }" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IRepository<>))] public sealed class Repository<T> : IRepository<T>; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IRepository<int> repository) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterTransient(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices(\"utc\")] IClock clock) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo), Key = \"utc\")] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices] IClock clock) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices] IClock clock) : IFoo;" },
        { "CSE2006", DiagnosticSeverity.Info, "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo), Key = \"utc\")] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices(null)] IClock clock) : IFoo;" },
        { "CSE2007", DiagnosticSeverity.Error, "[RegisterScoped] public abstract class Foo : IFoo;" },
        { "CSE2007", DiagnosticSeverity.Error, "[RegisterScoped] public static class Foo;" },
        { "CSE2007", DiagnosticSeverity.Error, "public sealed class Outer { [RegisterScoped] private sealed class Foo : IFoo; }" },
        { "CSE2008", DiagnosticSeverity.Warning, "[Decorates(typeof(IRepository<>))] public sealed class RepositoryDecorator<T>(IRepository<T> inner) : IRepository<T>;" },
    };

    public static TheoryData<string, string> ValidSources => new()
    {
        { "CSE2001", "[RegisterScoped(typeof(IFoo))] public sealed class Foo : IFoo; [Decorates(typeof(IFoo))] public sealed class FooDecorator(IFoo inner) : IFoo;" },
        { "CSE2002", "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class First : IFoo; [RegisterScoped(typeof(IFoo), Key = \"second\")] public sealed class Second : IFoo;" },
        { "CSE2002", "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class First : IFoo; [RegisterScoped(typeof(IFoo))] public sealed class Second : IFoo;" },
        { "CSE2003", "[RegisterScoped] public sealed class Foo : IFoo;" },
        { "CSE2003", "[RegisterScoped] public sealed record PlainRecord;" },
        { "CSE2005", "[RegisterScoped(typeof(IFoo))] public sealed record FooRecord : IFoo; [Decorates(typeof(IFoo))] public sealed record FooRecordDecorator(IFoo Inner) : IFoo;" },
        { "CSE2004", "[Decorates(typeof(IFoo))] public sealed class FooDecorator(IFoo inner, IClock clock) : IFoo;" },
        { "CSE2005", "[Decorates(typeof(IFoo))] public sealed class FooDecorator : IFoo { public FooDecorator(IFoo inner) { } private FooDecorator() { } }" },
        { "CSE2006", "[RegisterSingleton(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterTransient(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo : IFoo { public Foo(IClock clock) { } public Foo() { } }" },
        { "CSE2006", "[RegisterTransient(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo))] public sealed class Foo(IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo), Key = \"local\")] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices] IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterSingleton(typeof(IClock), Key = \"utc\")] [RegisterScoped(typeof(IClock))] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo), Key = \"utc\")] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices] IClock clock) : IFoo;" },
        { "CSE2006", "[RegisterScoped(typeof(IClock), Key = \"utc\")] public sealed class Clock : IClock; [RegisterSingleton(typeof(IFoo), Key = \"utc\")] public sealed class Foo([Microsoft.Extensions.DependencyInjection.FromKeyedServices(null)] IClock clock) : IFoo;" },
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
    public async Task Analyzer_Should_Report_CSE2002_Only_On_Throw_Registration_Generated_After_Duplicate()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            Prelude +
            "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class Zeta : IFoo; " +
            "[RegisterScoped(typeof(IFoo), Strategy = RegistrationStrategy.Throw)] public sealed class Alpha : IFoo;");

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(static d => d.Id == "CSE2002").Subject;
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture)
            .Should().Be("'Sample.Zeta' registers 'Sample.IFoo' with RegistrationStrategy.Throw, but 'Sample.Alpha' registers the same service type and key earlier in this compilation");
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

    [Fact]
    public async Task Analyzer_Should_Report_CSE2009_When_Referenced_Registry_Names_Collide()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(
            "public static class Program { public static void Main() { } }",
            OutputKind.ConsoleApplication,
            "Sample.App",
            ServiceGeneration.CreateLibraryReference("Foo.Api"),
            ServiceGeneration.CreateLibraryReference("FooApi"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(static d => d.Id == "CSE2009").Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().Be(
            "Assemblies 'Foo.Api', 'FooApi' all generate the service registry 'Microsoft.Extensions.DependencyInjection.FooApiServiceRegistry', so AddAllServices leaves out the referenced ones; give each assembly a distinct name with [assembly: ServiceRegistryName(\"...\")]");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE2009_When_Referenced_Registry_Name_Collides_With_Own_Registry()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(
            "namespace Sample; [CSharpEssentials.DependencyInjection.RegisterScoped] public sealed class Worker; public static class Program { public static void Main() { } }",
            OutputKind.ConsoleApplication,
            "FooApi",
            ServiceGeneration.CreateLibraryReference("Foo.Api"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE2009").Which
            .GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().StartWith(
                "Assemblies 'Foo.Api', 'FooApi' all generate the service registry 'Microsoft.Extensions.DependencyInjection.FooApiServiceRegistry'");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE2009_When_Custom_Registry_Name_Collides_With_Referenced_Registry()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(
            "[assembly: CSharpEssentials.DependencyInjection.ServiceRegistryName(\"FooApi\")] namespace Sample; [CSharpEssentials.DependencyInjection.RegisterScoped] public sealed class Worker; public static class Program { public static void Main() { } }",
            OutputKind.ConsoleApplication,
            "Sample.App",
            ServiceGeneration.CreateLibraryReference("Foo.Api"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE2009").Which
            .GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().StartWith("Assemblies 'Foo.Api', 'Sample.App' all generate");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE2009_When_Host_Without_Services_Shares_Referenced_Registry_Name()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(
            "public static class Program { public static void Main() { } }",
            OutputKind.ConsoleApplication,
            "FooApi",
            ServiceGeneration.CreateLibraryReference("Foo.Api"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);

        diagnostics.Should().NotContain(static d => d.Id == "CSE2009");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE2009_When_Project_Does_Not_Generate_Aggregate()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(
            "namespace Sample; public sealed class Plain;",
            OutputKind.DynamicallyLinkedLibrary,
            "Sample.Library",
            ServiceGeneration.CreateLibraryReference("Foo.Api"),
            ServiceGeneration.CreateLibraryReference("FooApi"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);

        diagnostics.Should().NotContain(static d => d.Id == "CSE2009");
    }

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(source);
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(FromKeyedServicesStub, (CSharpParseOptions)compilation.SyntaxTrees[0].Options));

        compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        return AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ServiceGeneration.Analyzers);
    }
}
