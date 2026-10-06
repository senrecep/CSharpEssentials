using System.Collections.Immutable;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;

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
    public async Task Analyzer_Should_Not_Report_CSE1007_When_Group_Target_Is_Valid()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(ValidSource);

        diagnostics.Should().NotContain(static d => d.Id == "CSE1007");
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
        string[] sources = [.. new[] { Cse1001Source, Cse1002Source, Cse1003Source, Cse1006Source, Cse1007Source }
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

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, params string[] additionalSources) =>
        AnalyzerHarness.GetAnalyzerDiagnosticsAsync(
            EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, [Usings + source, .. additionalSources]),
            EndpointCompilations.Generators.Value.Analyzers);
}
