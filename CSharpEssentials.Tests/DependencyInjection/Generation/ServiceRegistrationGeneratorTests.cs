using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit.Sdk;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

public class ServiceRegistrationGeneratorTests
{
    private const string Source = """
        using CSharpEssentials.DependencyInjection;
        using Microsoft.Extensions.DependencyInjection;

        namespace Sample;

        public enum Region { Us, Eu }

        public interface IClock;

        [RegisterSingleton]
        public sealed class Clock : IClock;

        public interface IRepository<T>;

        [RegisterScoped(typeof(IRepository<>))]
        public sealed class Repository<T> : IRepository<T>;

        public interface IGreeter
        {
            string Greet();
        }

        public interface IAudit;

        [RegisterScoped(As = ServiceAs.SelfWithInterfaces, Strategy = RegistrationStrategy.TryAdd)]
        public sealed class Greeter : IGreeter, IAudit
        {
            public string Greet() => "hi";
        }

        [RegisterTransient<IGreeter>(Key = Region.Eu)]
        public sealed class EuGreeter : IGreeter
        {
            public string Greet() => "eu";
        }

        [RegisterSingleton(Key = "shared", As = ServiceAs.SelfWithInterfaces)]
        public sealed class SharedGreeter : IGreeter, IAudit
        {
            public string Greet() => "shared";
        }

        [RegisterSingleton(Key = 42, As = ServiceAs.ImplementedInterfaces)]
        public sealed class NumberedClock : IClock, IAudit;

        [RegisterTransient(typeof(IClock), Key = typeof(int))]
        public sealed class TypedClock : IClock;

        [RegisterScoped(As = ServiceAs.Self)]
        public sealed class PlainClock : IClock;

        [Decorates(typeof(IGreeter), Order = 2)]
        public sealed class LoudGreeter(IGreeter inner, IClock clock, [FromKeyedServices("shared")] IAudit shared, int retries = 3) : IGreeter
        {
            public string Greet() => inner.Greet() + clock + shared + retries;
        }

        [Decorates<IGreeter>(Key = Region.Eu, Order = 1)]
        public sealed class EuDecorator(IGreeter inner, [ServiceKey] Region region) : IGreeter
        {
            public string Greet() => inner.Greet() + region;
        }
        """;

    private const string LibrarySource = """
        using CSharpEssentials.DependencyInjection;

        namespace Library;

        public interface IWorker;

        [RegisterScoped]
        public sealed class Worker : IWorker;
        """;

    private const string ProgramSource = """
        public static class Program
        {
            public static void Main()
            {
            }
        }
        """;

    private static readonly string[] AllSteps = ["ServiceTypes", "CollectedServices", "Host", "IsTestProject"];

    [Fact]
    public Task Generator_Should_Match_Snapshot()
    {
        GeneratorRun run = ServiceGeneration.Run(ServiceGeneration.CreateCompilation(Source));

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
        return Verify(run.Driver);
    }

    [Fact]
    public Task Generator_Should_Match_Aggregate_Snapshot_When_Executable_References_Registry()
    {
        MetadataReference library = LibraryReference();
        CSharpCompilation compilation =
            ServiceGeneration.CreateCompilation(Source + ProgramSource, OutputKind.ConsoleApplication, "Sample.App", library);

        GeneratorRun run = ServiceGeneration.Run(compilation);

        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
        return Verify(run.Driver);
    }

    [Fact]
    public void Generator_Should_Cache_All_Steps_When_Unrelated_Source_Is_Added()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(Source);

        GeneratorDriverRunResult second = ServiceGeneration.CreateDriver()
            .RunGenerators(compilation)
            .RunGenerators(GeneratorHarness.AddSource(compilation, "namespace Other; public class Unrelated { }"))
            .GetRunResult();

        IncrementalCaching.ShouldHaveCachedSteps(second, AllSteps);
        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
    }

    [Fact]
    public void Generator_Should_Not_Capture_Compilation_Objects_In_Tracked_Steps()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(Source);

        GeneratorDriverRunResult second = ServiceGeneration.CreateDriver().RunGenerators(compilation).RunGenerators(compilation).GetRunResult();

        IncrementalCaching.ShouldNotCaptureCompilationObjects(second, AllSteps);
    }

    [Fact]
    public void Generator_Should_Rerun_Collected_Step_When_Registration_Changes()
    {
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(Source);
        SyntaxTree tree = compilation.SyntaxTrees.Single();

        GeneratorDriverRunResult second = ServiceGeneration.CreateDriver()
            .RunGenerators(compilation)
            .RunGenerators(compilation.ReplaceSyntaxTree(
                tree,
                tree.WithChangedText(SourceText.From(Source.Replace("[RegisterSingleton]", "[RegisterScoped]", StringComparison.Ordinal)))))
            .GetRunResult();

        Action act = () => IncrementalCaching.ShouldHaveCachedSteps(second, "CollectedServices");

        act.Should().Throw<XunitException>();
    }

    [Fact]
    public void Generator_Should_Skip_Types_With_Errors()
    {
        const string source = """
            using CSharpEssentials.DependencyInjection;

            namespace Sample;

            public interface IFoo;

            [RegisterScoped(typeof(IFoo))]
            public sealed class Broken;

            [RegisterScoped(typeof(IFoo))]
            public sealed class Valid : IFoo;
            """;

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source)));

        sources["GeneratorTestsServiceRegistry.g.cs"].Should().Contain("global::Sample.Valid").And.NotContain("Broken");
    }

    [Fact]
    public void Generator_Should_Register_Record_Classes_And_Decorators()
    {
        const string source = """
            using CSharpEssentials.DependencyInjection;

            namespace Sample;

            public interface IRecordService;

            [RegisterScoped]
            public sealed record RecordService : IRecordService;

            [RegisterSingleton]
            public sealed record class ExplicitRecordService(int Value = 1) : IRecordService;

            [Decorates(typeof(IRecordService))]
            public sealed record RecordDecorator(IRecordService Inner) : IRecordService;
            """;

        GeneratorRun run = ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source));

        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
        string registry = ServiceGeneration.GeneratedSources(run)["GeneratorTestsServiceRegistry.g.cs"];
        registry.Should().Contain("global::Sample.RecordService")
            .And.Contain("global::Sample.ExplicitRecordService")
            .And.Contain("global::Sample.RecordDecorator")
            .And.NotContain("IEquatable");
    }

    [Fact]
    public void Generator_Should_Ignore_Record_Structs()
    {
        const string source = """
            using CSharpEssentials.DependencyInjection;

            namespace Sample;

            public interface IRecordService;

            [RegisterScoped]
            public readonly record struct RecordStructService : IRecordService;
            """;

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source)));

        sources.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Not_Emit_Registry_When_Nothing_Is_Registered()
    {
        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(
            ServiceGeneration.Run(ServiceGeneration.CreateCompilation("namespace Sample; public sealed class Plain;")));

        sources.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Not_Emit_Registry_When_Only_Invalid_Types_Exist()
    {
        const string source = """
            using CSharpEssentials.DependencyInjection;

            namespace Sample;

            public interface IFoo;

            [RegisterScoped(typeof(IFoo))]
            public abstract class Broken;
            """;

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source)));

        sources.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Not_Emit_Registry_When_Assembly_Is_Excluded()
    {
        string source = WithAssemblyAttribute("ExcludeFromRegistration");

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source)));

        sources.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Use_Registry_Name_When_Attribute_Is_Present()
    {
        string source = WithAssemblyAttribute("ServiceRegistryName(\"my.app\")");

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source)));

        sources.Should().ContainKey("MyAppServiceRegistry.g.cs");
        sources["MyAppServiceRegistry.g.cs"].Should().Contain("AddMyAppServices(");
    }

    [Fact]
    public void Generator_Should_Not_Emit_Aggregate_When_Library_Has_No_Opt_In()
    {
        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(LibrarySource)));

        sources.Keys.Should().NotContain(static name => name.EndsWith("ServiceAggregate.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_Should_Emit_Aggregate_When_Project_Is_Executable()
    {
        CSharpCompilation compilation =
            ServiceGeneration.CreateCompilation(LibrarySource + ProgramSource, OutputKind.ConsoleApplication);

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(compilation));

        sources["GeneratorTestsServiceAggregate.g.cs"].Should().Contain("GeneratorTestsServiceRegistry.RegisterServices(services, logger);");
    }

    [Fact]
    public void Generator_Should_Not_Emit_Aggregate_When_Executable_Is_Test_Project()
    {
        CSharpCompilation compilation =
            ServiceGeneration.CreateCompilation(LibrarySource + ProgramSource, OutputKind.ConsoleApplication);

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(compilation, isTestProject: true));

        sources.Should().ContainKey("GeneratorTestsServiceRegistry.g.cs").And.NotContainKey("GeneratorTestsServiceAggregate.g.cs");
    }

    [Fact]
    public void Generator_Should_Emit_Aggregate_When_Opted_In()
    {
        string source = WithAssemblyAttribute("GenerateServiceAggregate");

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(ServiceGeneration.CreateCompilation(source), isTestProject: true));

        sources.Should().ContainKey("GeneratorTestsServiceAggregate.g.cs");
    }

    [Fact]
    public void Generator_Should_Not_Emit_Aggregate_When_Opted_Out()
    {
        string source = WithAssemblyAttribute("DisableServiceAggregate") + ProgramSource;
        CSharpCompilation compilation = ServiceGeneration.CreateCompilation(source, OutputKind.ConsoleApplication);

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(ServiceGeneration.Run(compilation));

        sources.Should().NotContainKey("GeneratorTestsServiceAggregate.g.cs");
    }

    [Fact]
    public void Generator_Should_Aggregate_Only_Referenced_Registries_When_Executable_Has_No_Services()
    {
        CSharpCompilation compilation =
            ServiceGeneration.CreateCompilation(ProgramSource, OutputKind.ConsoleApplication, "Sample.App", LibraryReference());

        GeneratorRun run = ServiceGeneration.Run(compilation);

        IReadOnlyDictionary<string, string> sources = ServiceGeneration.GeneratedSources(run);
        sources.Keys.Should().ContainSingle().Which.Should().Be("SampleAppServiceAggregate.g.cs");
        sources["SampleAppServiceAggregate.g.cs"].Should()
            .Contain("global::Microsoft.Extensions.DependencyInjection.SampleLibraryServiceRegistry.RegisterServices(services, logger);")
            .And.Contain("global::Microsoft.Extensions.DependencyInjection.SampleLibraryServiceRegistry.ApplyDecorators(services, order);");
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Generated_Code_Should_Not_Call_Members_Requiring_Unreferenced_Or_Dynamic_Code()
    {
        CSharpCompilation compilation =
            ServiceGeneration.CreateCompilation(Source + ProgramSource, OutputKind.ConsoleApplication, "Sample.App", LibraryReference());
        GeneratorRun run = ServiceGeneration.Run(compilation);
        SyntaxTree[] generated = [.. run.Result.GeneratedTrees];

        string[] offending = [.. generated.SelectMany(tree =>
        {
            SemanticModel model = run.OutputCompilation.GetSemanticModel(tree);
            return tree.GetRoot().DescendantNodes()
                .Where(static node => node is InvocationExpressionSyntax or ObjectCreationExpressionSyntax)
                .Select(node => model.GetSymbolInfo(node).Symbol)
                .OfType<IMethodSymbol>()
                .Where(static method => method.GetAttributes().Concat(method.ContainingType.GetAttributes()).Any(static attribute =>
                    attribute.AttributeClass?.Name is "RequiresUnreferencedCodeAttribute" or "RequiresDynamicCodeAttribute"))
                .Select(static method => method.ToDisplayString());
        })];

        generated.Should().HaveCount(2);
        offending.Should().BeEmpty();
    }

    private static string WithAssemblyAttribute(string attribute) =>
        LibrarySource.Replace("namespace Library;", $"[assembly: {attribute}]\n\nnamespace Library;", StringComparison.Ordinal);

    private static MetadataReference LibraryReference()
    {
        GeneratorRun library = ServiceGeneration.Run(ServiceGeneration.CreateCompilation(LibrarySource, assemblyName: "Sample.Library"));
        return ServiceGeneration.EmitReference(library.OutputCompilation);
    }
}
