using System.Collections.Immutable;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointsAnalyzerTests
{
    private const string Usings = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Api;

        """;

    private const string ValidSource = """
        public sealed class AppsGroup : IEndpointGroup
        {
            public static string Prefix => "apps";
        }

        [EndpointGroup<AppsGroup>]
        public sealed class AppVersionsGroup : IEndpointGroup
        {
            public static string Prefix => "versions";
        }

        [EndpointGroup(typeof(AppVersionsGroup))]
        public sealed class ListVersions : IEndpoint
        {
            public const string Route = "/";

            private static readonly object Gate = new();

            public static void Map(IEndpointRouteBuilder app) => app.MapGet(Route, () => Gate.GetHashCode());
        }

        public sealed record class RecordEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/record", () => "record");
        }

        public readonly struct StructEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/struct", () => "struct");
        }

        public static class Container
        {
            internal sealed class NestedEndpoint : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/nested", () => "nested");
            }
        }

        [ExcludeFromMapping]
        public abstract class ExcludedBase : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/excluded", () => "excluded");
        }
        """;

    private const string Cse1001Source = """
        public static class Container
        {
            private sealed class PrivateEndpoint : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/private", () => "private");
            }
        }

        file sealed class FileEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/file", () => "file");
        }
        """;

    private const string Cse1002Source = """
        [EndpointGroup<SecondGroup>]
        public sealed class FirstGroup : IEndpointGroup
        {
            public static string Prefix => "first";
        }

        [EndpointGroup(typeof(FirstGroup))]
        public sealed class SecondGroup : IEndpointGroup
        {
            public static string Prefix => "second";
        }

        [EndpointGroup<FirstGroup>]
        public sealed class InCycle : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "cycle");
        }
        """;

    private const string Cse1003Source = """
        public sealed class AppsGroup : IEndpointGroup
        {
            public static string Prefix => "apps";
        }

        public sealed class UsersGroup : IEndpointGroup
        {
            public static string Prefix => "users";
        }

        [EndpointGroup<AppsGroup>]
        [EndpointGroup(typeof(UsersGroup))]
        public sealed class Conflicting : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "conflict");
        }
        """;

    private const string Cse1004Source = """
        public sealed class WithField : IEndpoint
        {
            private readonly int _count = 1;

            public int Count => _count;

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/field", () => "field");
        }

        public sealed class WithAutoProperty : IEndpoint
        {
            public string Name { get; set; } = "";

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/property", () => "property");
        }

        public sealed class WithConstructor : IEndpoint
        {
            public WithConstructor(string name) => System.GC.KeepAlive(name);

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/constructor", () => "constructor");
        }

        public sealed class WithPrimaryConstructor(string name) : IEndpoint
        {
            public override string ToString() => name;

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/primary", () => "primary");
        }
        """;

    private const string Cse1006Source = """
        public abstract class AbstractEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/abstract", () => "abstract");
        }

        public sealed class GenericEndpoint<T> : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/generic", () => typeof(T).Name);
        }

        public abstract class AbstractGroup : IEndpointGroup
        {
            public static string Prefix => "abstract";
        }
        """;

    private const string Cse1007Source = """
        public sealed class NotAGroup
        {
        }

        public abstract class AbstractGroup : IEndpointGroup
        {
            public static string Prefix => "abstract";
        }

        [EndpointGroup(typeof(NotAGroup))]
        public sealed class InNonGroup : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "non-group");
        }

        [EndpointGroup(typeof(AbstractGroup))]
        public sealed class InAbstractGroup : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "abstract-group");
        }
        """;

    private const string Cse1008Source = """
        public ref struct RefEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ref", () => "ref");
        }

        public ref struct RefGroup : IEndpointGroup
        {
            public static string Prefix => "ref";
        }

        [EndpointGroup(typeof(RefGroup))]
        public sealed class InRefGroup : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "ref-group");
        }
        """;

    private const string Cse1005Source = """
        public sealed class OrdersGroup : IEndpointGroup
        {
            public static string Prefix => "orders";
        }

        public sealed class FirstUngrouped : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/dup", () => "first");
        }

        public sealed class SecondUngrouped : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("Dup/", () => "second");
        }

        [EndpointGroup<OrdersGroup>]
        public sealed class CreateOrder : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapPost("/", () => "create");
        }

        [EndpointGroup<OrdersGroup>]
        public sealed class CreateOrderAgain : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapMethods("/", new[] { "post", "put" }, () => "again");
        }
        """;

    private const string Cse1005CollectionExpressionSource = """
        public sealed class ReportsGroup : IEndpointGroup
        {
            public static string Prefix => "reports";
        }

        [EndpointGroup<ReportsGroup>]
        public sealed class ReadReport : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapMethods("/x", ["get"], () => "read");
        }

        [EndpointGroup<ReportsGroup>]
        public sealed class ReadReportAgain : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapMethods("/x", ["GET", "HEAD"], () => "again");
        }

        [EndpointGroup<ReportsGroup>]
        public sealed class SpreadReport : IEndpoint
        {
            private static readonly string[] Methods = ["GET"];

            public static void Map(IEndpointRouteBuilder app) => app.MapMethods("/x", [.. Methods], () => "spread");
        }
        """;

    private const string Cse1005NegativeSource = """
        public sealed class UsersGroup : IEndpointGroup
        {
            public static string Prefix => "users";
        }

        public sealed class TeamsGroup : IEndpointGroup
        {
            public static string Prefix => "teams";
        }

        [EndpointGroup<UsersGroup>]
        public sealed class ListUsers : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "users");
        }

        [EndpointGroup<TeamsGroup>]
        public sealed class ListTeams : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "teams");
        }

        [EndpointGroup<TeamsGroup>]
        public sealed class CreateTeam : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapPost("/", () => "create");
        }

        public sealed class DynamicRoute : IEndpoint
        {
            private static string Pattern => "/dynamic";

            public static void Map(IEndpointRouteBuilder app) => app.MapGet(Pattern, () => "dynamic");
        }

        public sealed class DynamicRouteAgain : IEndpoint
        {
            private static string Pattern => "/dynamic";

            public static void Map(IEndpointRouteBuilder app) => app.MapGet(Pattern, () => "dynamic-again");
        }

        public sealed class SubGroupRoute : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGroup("/sub").MapGet("/plain", () => "sub");
        }

        public sealed class PlainRoute : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/plain", () => "plain");
        }
        """;

    [Fact]
    public async Task Analyzer_Should_Report_Nothing_When_Types_Are_Valid()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1001_When_Type_Is_Not_Accessible()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1001Source);

        diagnostics.Select(static d => (d.Id, d.Severity)).Should().Equal(
            ("CSE1001", DiagnosticSeverity.Error),
            ("CSE1001", DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1001_When_Nested_Type_Is_Internal()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1001");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1002_On_Each_Group_In_Cycle()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1002Source);

        diagnostics.Should().OnlyContain(static d => d.Id == "CSE1002" && d.Severity == DiagnosticSeverity.Error);
        diagnostics.Select(static d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Should().BeEquivalentTo(
            "Group chain of 'Sample.Api.FirstGroup' returns to 'Sample.Api.FirstGroup'; endpoints in this chain are not mapped",
            "Group chain of 'Sample.Api.SecondGroup' returns to 'Sample.Api.SecondGroup'; endpoints in this chain are not mapped");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1002_When_Groups_Nest_Without_Cycle()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1002");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1003_When_Both_Group_Attribute_Forms_Are_Present()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1003Source);

        diagnostics.Should().ContainSingle().Which.Should().Match<Diagnostic>(static d => d.Id == "CSE1003" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1003_When_Single_Group_Attribute_Is_Present()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1003");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1004_When_Endpoint_Declares_Instance_State()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1004Source);

        diagnostics.Should().OnlyContain(static d => d.Id == "CSE1004" && d.Severity == DiagnosticSeverity.Warning);
        diagnostics.Select(static d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)).Should().BeEquivalentTo(
            "WithField",
            "WithAutoProperty",
            "WithConstructor",
            "WithPrimaryConstructor");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1004_When_Endpoint_Has_Only_Static_Members_Or_Is_Record()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1004");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1006_When_Type_Is_Abstract_Or_Open_Generic()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1006Source);

        diagnostics.Should().HaveCount(3).And.OnlyContain(static d => d.Id == "CSE1006" && d.Severity == DiagnosticSeverity.Info);
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1006_When_Abstract_Type_Is_Excluded()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1006");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1007_When_Group_Target_Is_Invalid()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1007Source);

        Diagnostic[] invalidTargets = [.. diagnostics.Where(static d => d.Id == "CSE1007")];
        invalidTargets.Should().HaveCount(2).And.OnlyContain(static d => d.Severity == DiagnosticSeverity.Error);
        invalidTargets.Select(static d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)).Should().BeEquivalentTo(
            "EndpointGroup(typeof(NotAGroup))",
            "EndpointGroup(typeof(AbstractGroup))");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1008_When_Endpoint_Or_Group_Is_Ref_Struct()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1008Source);

        Diagnostic[] refStructs = [.. diagnostics.Where(static d => d.Id == "CSE1008")];
        refStructs.Should().HaveCount(2).And.OnlyContain(static d => d.Severity == DiagnosticSeverity.Error);
        refStructs.Select(static d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Should().BeEquivalentTo(
            "Type 'Sample.Api.RefEndpoint' is a ref struct and is not mapped; declare it as a class or a non-ref struct",
            "Type 'Sample.Api.RefGroup' is a ref struct and is not mapped; declare it as a class or a non-ref struct");
        diagnostics.Should().ContainSingle(static d => d.Id == "CSE1007")
            .Which.GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().Contain("Sample.Api.InRefGroup");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1008_When_Endpoint_Is_Struct()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1008");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1007_When_Group_Target_Is_Valid()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1007");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1005_When_Method_And_Route_Repeat_In_Same_Group()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1005Source);

        Diagnostic[] duplicates = [.. diagnostics.Where(static d => d.Id == "CSE1005")];
        duplicates.Should().HaveCount(4).And.OnlyContain(static d => d.Severity == DiagnosticSeverity.Warning);
        duplicates.Select(static d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)).Should().BeEquivalentTo(
            "\"/dup\"",
            "\"Dup/\"",
            "\"/\"",
            "\"/\"");
        duplicates.Select(static d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Should().Contain(
            "Endpoint 'Sample.Api.CreateOrderAgain' maps POST '/', which 'Sample.Api.CreateOrder' also maps in the same group");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1005_When_MapMethods_Uses_Collection_Expression()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1005CollectionExpressionSource);

        diagnostics.Where(static d => d.Id == "CSE1005").Select(static d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Should().BeEquivalentTo(
            "Endpoint 'Sample.Api.ReadReport' maps GET '/x', which 'Sample.Api.ReadReportAgain' also maps in the same group",
            "Endpoint 'Sample.Api.ReadReportAgain' maps GET '/x', which 'Sample.Api.ReadReport' also maps in the same group");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1005_When_Groups_Methods_Or_Patterns_Differ()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1005NegativeSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1005");
    }

    [Fact]
    public async Task Analyzer_Should_Report_Nothing_When_Assembly_Is_Excluded()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Cse1001Source, "[assembly: CSharpEssentials.Endpoints.ExcludeFromMapping]");

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Skip_Invalid_Types_So_Generated_Code_Compiles()
    {
        string[] sources = [.. new[] { Cse1001Source, Cse1002Source, Cse1003Source, Cse1006Source, Cse1007Source, Cse1008Source }
            .Select(static (source, index) => Usings.Replace("namespace Sample.Api;", $"namespace Sample.Api.Case{index};", StringComparison.Ordinal) + source)];
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Api",
            OutputKind.DynamicallyLinkedLibrary,
            [.. sources, Usings + ValidSource]));

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        string registry = EndpointCompilations.GeneratedText(run, "SampleApiEndpointRegistry.g.cs");
        registry.Should().NotContain("Sample.Api.Case");
        registry.Should().Contain("global::Sample.Api.ListVersions").And.Contain("global::Sample.Api.Container.NestedEndpoint");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1009_When_Referenced_Registry_Names_Collide()
    {
        CSharpCompilation compilation = EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [RegistryReference("Foo.Api"), RegistryReference("FooApi")],
            "namespace Sample.Host; public sealed class Marker;");

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EndpointCompilations.Generators.Value.Analyzers);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(static d => d.Id == "CSE1009").Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().Be(
            "Assemblies 'Foo.Api', 'FooApi' all generate the endpoint registry 'Microsoft.AspNetCore.Builder.FooApiEndpointRegistry', so MapAllEndpoints leaves out the referenced ones; give each assembly a distinct name with [assembly: EndpointRegistryName(\"...\")]");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1009_When_Referenced_Registry_Name_Collides_With_Own_Registry()
    {
        CSharpCompilation compilation = EndpointCompilations.Create(
            "FooApi",
            OutputKind.ConsoleApplication,
            [RegistryReference("Foo.Api")],
            "using CSharpEssentials.Endpoints; using Microsoft.AspNetCore.Builder; using Microsoft.AspNetCore.Routing; namespace Sample.Host; public sealed class Ping : IEndpoint { public static void Map(IEndpointRouteBuilder app) => app.MapGet(\"/own\", () => \"own\"); }");

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EndpointCompilations.Generators.Value.Analyzers);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE1009").Which
            .GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().StartWith(
                "Assemblies 'Foo.Api', 'FooApi' all generate the endpoint registry 'Microsoft.AspNetCore.Builder.FooApiEndpointRegistry'");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE1009_When_Custom_Registry_Name_Collides_With_Referenced_Registry()
    {
        CSharpCompilation compilation = EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [RegistryReference("Foo.Api")],
            "using CSharpEssentials.Endpoints; using Microsoft.AspNetCore.Builder; using Microsoft.AspNetCore.Routing; [assembly: EndpointRegistryName(\"FooApi\")] namespace Sample.Host; public sealed class Ping : IEndpoint { public static void Map(IEndpointRouteBuilder app) => app.MapGet(\"/own\", () => \"own\"); }");

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EndpointCompilations.Generators.Value.Analyzers);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE1009").Which
            .GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().StartWith("Assemblies 'Foo.Api', 'Sample.Host' all generate");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1009_When_Host_Without_Endpoints_Shares_Referenced_Registry_Name()
    {
        CSharpCompilation compilation = EndpointCompilations.Create(
            "FooApi",
            OutputKind.ConsoleApplication,
            [RegistryReference("Foo.Api")],
            "namespace Sample.Host; public sealed class Marker;");

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EndpointCompilations.Generators.Value.Analyzers);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1009");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_CSE1009_When_Project_Does_Not_Generate_Aggregate()
    {
        CSharpCompilation compilation = EndpointCompilations.Create(
            "Sample.Library",
            OutputKind.DynamicallyLinkedLibrary,
            [RegistryReference("Foo.Api"), RegistryReference("FooApi")],
            "namespace Sample.Library; public sealed class Marker;");

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EndpointCompilations.Generators.Value.Analyzers);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1009");
    }

    private static CompilationReference RegistryReference(string assemblyName)
    {
        string source = $$"""
            using CSharpEssentials.Endpoints;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;

            namespace {{assemblyName.Replace(".", "_", StringComparison.Ordinal)}}Endpoints;

            public sealed class Ping : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ping", () => "pong");
            }
            """;
        return EndpointCompilations.Run(EndpointCompilations.Create(assemblyName, OutputKind.DynamicallyLinkedLibrary, source)).OutputCompilation.ToMetadataReference();
    }

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, params string[] additionalSources) =>
        AnalyzerHarness.GetAnalyzerDiagnosticsAsync(
            EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, [Usings + source, .. additionalSources]),
            EndpointCompilations.Generators.Value.Analyzers);
}
