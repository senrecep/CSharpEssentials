using System.Reflection;
using FluentAssertions;
using CSharpEssentials.Tests.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointsGeneratorTests
{
    private const string GeneratorTrackingEndpoints = "EndpointTypes";

    private const string LibrarySource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Api;

        public sealed class AppsGroup : IEndpointGroup
        {
            public static string Prefix => "apps";

            public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps");
        }

        [EndpointGroup<AppsGroup>]
        public sealed class AppVersionsGroup : IEndpointGroup
        {
            public static string Prefix => "{appId:int}/versions";
        }

        [EndpointGroup(typeof(AppsGroup))]
        public sealed class CreateApp : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapPost("/", () => TypedResults.Ok());
        }

        [EndpointGroup(typeof(AppsGroup))]
        public sealed partial class DeleteApp : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapDelete("/{appId:int}", (int appId) => TypedResults.NoContent());
        }

        public sealed partial class DeleteApp
        {
        }

        [EndpointGroup<AppVersionsGroup>]
        public sealed class ListVersions : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", (int appId) => TypedResults.Ok(appId));
        }

        public sealed class Health : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/health", () => TypedResults.Ok());
        }

        public static class Nested
        {
            public sealed record class Status : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/status", () => TypedResults.Ok());
            }
        }
        """;

    private const string ModuleSource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Modules.Billing;

        public sealed class Invoices : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/invoices", () => TypedResults.Ok());
        }
        """;

    private const string HostSource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Host;

        public sealed class Root : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => TypedResults.Ok());
        }
        """;

    private const string NamedSource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Named;

        public sealed class Items : IEndpoint
        {
            public const string ArchiveName = "ArchiveItems";

            public static void Map(IEndpointRouteBuilder app)
            {
                app.MapGet("/items", () => TypedResults.Ok());
                app.MapGet("/items/archive", () => TypedResults.Ok()).WithName(ArchiveName);
            }
        }

        public static class LegacyRoutes
        {
            private static readonly string Computed = "Computed";

            public static void MapLegacy(IEndpointRouteBuilder app)
            {
                app.MapGet("/legacy/items", () => "items").WithName("Items");
                app.MapGet("/legacy/attribute", () => "attribute").WithMetadata(new EndpointNameAttribute("Legacy\"Quoted"));
                app.MapGet("/legacy/metadata", () => "metadata").WithMetadata(new EndpointNameMetadata("LegacyMetadata"), new RouteNameMetadata("LegacyRoute"));
                app.MapGet("/legacy/computed", () => "computed").WithName(Computed);
                app.MapGet("/legacy/again", () => "again").WithName("Items");
            }
        }
        """;

    [Fact]
    public Task Generator_Should_Match_Registry_Snapshot()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("SampleApiEndpointRegistry.g.cs");
        return Verify(run.Driver);
    }

    [Fact]
    public Task Generator_Should_Match_Registry_Snapshot_When_Explicit_Names_Are_Reserved()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Named",
            OutputKind.DynamicallyLinkedLibrary,
            NamedSource));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("SampleNamedEndpointRegistry.g.cs");
        return Verify(run.Driver);
    }

    [Fact]
    public void Generator_Should_Not_Reserve_Names_When_Compilation_Has_No_Constant_Explicit_Names()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource));

        ShouldCompile(run);
        EndpointCompilations.GeneratedText(run, "SampleApiEndpointRegistry.g.cs").Should().NotContain("ReserveEndpointNames");
    }

    [Fact]
    public void Generator_Should_Reserve_Host_Names_In_Aggregate_When_Host_Has_No_Endpoints()
    {
        CSharpCompilation api = Compile("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource);

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [api.ToMetadataReference()],
            """
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;

            namespace Sample.Host;

            public static class Routes
            {
                public static void MapRoutes(IEndpointRouteBuilder app) => app.MapGet("/legacy", () => "legacy").WithName("Legacy");
            }
            """));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("SampleHostEndpointAggregate.g.cs");
        EndpointCompilations.GeneratedText(run, "SampleHostEndpointAggregate.g.cs").Should().Contain(
            "global::CSharpEssentials.Endpoints.EndpointMapper.ReserveEndpointNames(app, options, new string[] { \"Legacy\" });");
    }

    [Fact]
    public void Generator_Should_Leave_Name_Reservation_To_Own_Registry_When_Aggregate_Maps_It()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create("Sample.Named", OutputKind.ConsoleApplication, NamedSource));

        ShouldCompile(run);
        EndpointCompilations.GeneratedText(run, "SampleNamedEndpointAggregate.g.cs").Should().NotContain("ReserveEndpointNames");
        EndpointCompilations.GeneratedText(run, "SampleNamedEndpointRegistry.g.cs").Should().Contain(
            "global::CSharpEssentials.Endpoints.EndpointMapper.ReserveEndpointNames(app, options, ReservedEndpointNames);");
    }

    [Fact]
    public Task Generator_Should_Match_Aggregate_Snapshot_When_Host_References_Modules()
    {
        CSharpCompilation module = Compile("Sample.Modules.Billing", OutputKind.DynamicallyLinkedLibrary, ModuleSource);
        CSharpCompilation api = Compile("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource);

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [api.ToMetadataReference(), module.ToMetadataReference()],
            HostSource));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("SampleHostEndpointRegistry.g.cs", "SampleHostEndpointAggregate.g.cs");
        return Verify(run.Driver);
    }

    [Fact]
    public void Generator_Should_Produce_No_Output_When_Assembly_Has_No_Endpoints()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Empty",
            OutputKind.DynamicallyLinkedLibrary,
            "namespace Sample.Empty; public sealed class Service { }"));

        EndpointCompilations.HintNames(run).Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Produce_No_Registry_When_Assembly_Is_Excluded()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Api",
            OutputKind.DynamicallyLinkedLibrary,
            LibrarySource,
            "[assembly: CSharpEssentials.Endpoints.ExcludeFromMapping]"));

        EndpointCompilations.HintNames(run).Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_Skip_Excluded_Types_And_Endpoints_Of_Excluded_Groups()
    {
        const string source = """
            using CSharpEssentials.Endpoints;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;

            namespace Sample.Api;

            [ExcludeFromMapping]
            public sealed class HiddenGroup : IEndpointGroup
            {
                public static string Prefix => "hidden";
            }

            [EndpointGroup<HiddenGroup>]
            public sealed class InHiddenGroup : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => TypedResults.Ok());
            }

            [ExcludeFromMapping]
            public sealed class Hidden : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/hidden", () => TypedResults.Ok());
            }

            public sealed class Visible : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/visible", () => TypedResults.Ok());
            }
            """;

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, source));

        ShouldCompile(run);
        string registry = EndpointCompilations.GeneratedText(run, "SampleApiEndpointRegistry.g.cs");
        registry.Should().Contain("Sample.Api.Visible").And.NotContain("Hidden");
    }

    [Theory]
    [InlineData("MyCompany.Apps.Api", "MyCompanyAppsApi")]
    [InlineData("web-api", "WebApi")]
    [InlineData("my company.web_api", "MyCompanyWeb_api")]
    [InlineData("1st.api", "_1stApi")]
    public void Generator_Should_Sanitize_Assembly_Name_When_Naming_Registry(string assemblyName, string expected)
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(assemblyName, OutputKind.DynamicallyLinkedLibrary, HostSource));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal($"{expected}EndpointRegistry.g.cs");
        EndpointCompilations.GeneratedText(run, $"{expected}EndpointRegistry.g.cs").Should().Contain($"Map{expected}Endpoints(");
    }

    [Fact]
    public void Generator_Should_Use_EndpointRegistryName_When_Present()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.DynamicallyLinkedLibrary,
            HostSource,
            "[assembly: CSharpEssentials.Endpoints.EndpointRegistryName(\"Storefront\")]"));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("StorefrontEndpointRegistry.g.cs");
        EndpointCompilations.GeneratedText(run, "StorefrontEndpointRegistry.g.cs").Should().Contain("MapStorefrontEndpoints(");
    }

    [Theory]
    [InlineData(OutputKind.ConsoleApplication, false, "", true)]
    [InlineData(OutputKind.WindowsApplication, false, "", true)]
    [InlineData(OutputKind.DynamicallyLinkedLibrary, false, "", false)]
    [InlineData(OutputKind.ConsoleApplication, true, "", false)]
    [InlineData(OutputKind.ConsoleApplication, true, "GenerateEndpointAggregate", true)]
    [InlineData(OutputKind.DynamicallyLinkedLibrary, false, "GenerateEndpointAggregate", true)]
    [InlineData(OutputKind.ConsoleApplication, false, "DisableEndpointAggregate", false)]
    [InlineData(OutputKind.DynamicallyLinkedLibrary, false, "DisableEndpointAggregate", false)]
    public void Generator_Should_Apply_Aggregate_Rules(OutputKind outputKind, bool isTestProject, string assemblyAttribute, bool expected)
    {
        string[] sources = assemblyAttribute.Length == 0
            ? [HostSource]
            : [HostSource, $"[assembly: CSharpEssentials.Endpoints.{assemblyAttribute}]"];

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create("Sample.Host", outputKind, sources), isTestProject);

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Contains("SampleHostEndpointAggregate.g.cs").Should().Be(expected);
    }

    [Fact]
    public void Generator_Should_Emit_Empty_Aggregate_When_Executable_Has_No_Endpoints()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            "namespace Sample.Host; public sealed class Service { }"));

        ShouldCompile(run);
        EndpointCompilations.HintNames(run).Should().Equal("SampleHostEndpointAggregate.g.cs");
        EndpointCompilations.GeneratedText(run, "SampleHostEndpointAggregate.g.cs").Should().NotContain("SampleHostEndpointRegistry");
    }

    [Fact]
    public void Generator_Should_Ignore_Referenced_Assemblies_Without_Endpoints_Reference()
    {
        CSharpCompilation unrelated = GeneratorHarness.CreateCompilation(["namespace Sample.Unrelated; public sealed class Service { }"])
            .WithAssemblyName("Sample.Unrelated");

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [unrelated.ToMetadataReference()],
            HostSource));

        ShouldCompile(run);
        string aggregate = EndpointCompilations.GeneratedText(run, "SampleHostEndpointAggregate.g.cs");
        aggregate.Should().Contain("SampleHostEndpointRegistry.MapEndpoints(app, options);").And.NotContain("Unrelated");
    }

    [Fact]
    public void Generator_Should_Skip_Referenced_Registries_When_Their_Names_Collide()
    {
        CSharpCompilation dotted = Compile("Foo.Api", OutputKind.DynamicallyLinkedLibrary, PingSource("FooDotted"));
        CSharpCompilation plain = Compile("FooApi", OutputKind.DynamicallyLinkedLibrary, PingSource("FooPlain"));
        CSharpCompilation api = Compile("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource);

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [dotted.ToMetadataReference(), plain.ToMetadataReference(), api.ToMetadataReference()],
            HostSource));

        ShouldCompile(run);
        string aggregate = EndpointCompilations.GeneratedText(run, "SampleHostEndpointAggregate.g.cs");
        aggregate.Should().Contain("SampleApiEndpointRegistry.MapEndpoints(app, options);").And.NotContain("FooApiEndpointRegistry");
    }

    [Fact]
    public void Generator_Should_Map_Own_Registry_Once_When_Referenced_Registry_Has_Same_Name()
    {
        CSharpCompilation dotted = Compile("Foo.Api", OutputKind.DynamicallyLinkedLibrary, PingSource("FooDotted"));

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "FooApi",
            OutputKind.ConsoleApplication,
            [dotted.ToMetadataReference()],
            HostSource));

        ShouldCompile(run);
        string aggregate = EndpointCompilations.GeneratedText(run, "FooApiEndpointAggregate.g.cs");
        aggregate.Split("FooApiEndpointRegistry.MapEndpoints(").Should().HaveCount(2);
    }

    [Fact]
    public void Generator_Should_Cache_Steps_When_Unrelated_Source_Is_Added()
    {
        CSharpCompilation compilation = EndpointCompilations.Create("Sample.Api", OutputKind.ConsoleApplication, LibrarySource);

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(
            compilation,
            static c => GeneratorHarness.AddSource(c, "namespace Sample.Other; public sealed class Unrelated { }"),
            [.. EndpointCompilations.Generators.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSteps(second, GeneratorTrackingEndpoints, "CollectedEndpoints", "ReservedNames", "Host", "IsTestProject");
        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
        IncrementalCaching.ShouldNotCaptureCompilationObjects(second, GeneratorTrackingEndpoints, "CollectedEndpoints", "ReservedNames", "Host", "IsTestProject");
    }

    [Fact]
    public void Generator_Should_Reserve_Literal_With_Name()
    {
        string aggregate = HostAggregate("""app.MapGet("/a", () => "a").WithName("Literal");""");

        aggregate.Should().Contain("new string[] { \"Literal\" }");
    }

    [Fact]
    public void Generator_Should_Reserve_Const_Field_With_Name()
    {
        string aggregate = HostAggregate("""app.MapGet("/a", () => "a").WithName(ConstName);""");

        aggregate.Should().Contain("new string[] { \"FromConst\" }");
    }

    [Fact]
    public void Generator_Should_Reserve_Named_Argument_With_Name()
    {
        string aggregate = HostAggregate("""app.MapGet("/a", () => "a").WithName(endpointName: "Named");""");

        aggregate.Should().Contain("new string[] { \"Named\" }");
    }

    [Fact]
    public void Generator_Should_Reserve_Static_Call_With_Name()
    {
        string aggregate = HostAggregate("""RoutingEndpointConventionBuilderExtensions.WithName(app.MapGet("/a", () => "a"), "Static");""");

        aggregate.Should().Contain("new string[] { \"Static\" }");
    }

    [Fact]
    public void Generator_Should_Reserve_Endpoint_Name_Metadata()
    {
        string aggregate = HostAggregate("""app.MapGet("/a", () => "a").WithMetadata(new EndpointNameMetadata("FromMetadata"));""");

        aggregate.Should().Contain("new string[] { \"FromMetadata\" }");
    }

    [Fact]
    public void Generator_Should_Reserve_Literal_Passed_To_Unrelated_With_Name()
    {
        string aggregate = HostAggregate("""new Other().WithName("Foreign");""");

        aggregate.Should().Contain("new string[] { \"Foreign\" }");
    }

    [Fact]
    public void Generator_Should_Merge_Literal_And_Semantic_Names_Sorted_And_Distinct()
    {
        string aggregate = HostAggregate("""
            app.MapGet("/a", () => "a").WithName("Zeta");
            app.MapGet("/b", () => "b").WithName(ConstName);
            app.MapGet("/c", () => "c").WithName("Zeta");
            app.MapGet("/d", () => "d").WithName("Alpha");
            """);

        aggregate.Should().Contain("new string[] { \"Alpha\", \"FromConst\", \"Zeta\" }");
    }

    [Fact]
    public void Generator_Should_Cache_Reserved_Names_When_Unrelated_Source_Is_Added()
    {
        CSharpCompilation compilation = EndpointCompilations.Create("Sample.Named", OutputKind.ConsoleApplication, NamedSource);

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(
            compilation,
            static c => GeneratorHarness.AddSource(c, "namespace Sample.Other; public sealed class Unrelated { }"),
            [.. EndpointCompilations.Generators.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSteps(second, GeneratorTrackingEndpoints, "CollectedEndpoints", "ReservedNames");
        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
    }

    [Fact]
    public void Generator_Should_Pass_Only_Types_That_Can_Declare_An_Endpoint_To_The_Semantic_Check()
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText("""
            public interface IFiller { int Compute(int x); }
            public sealed class Filler : IFiller { public int Compute(int x) => x; }
            public sealed class InstanceMapper : IFiller { public int Compute(int x) => x; public void Map() { } }
            public static class Routes { public static void Map(object app) { } }
            public sealed class Ping : IEndpoint { public static void Map(object app) { } }
            public sealed class Explicit : IEndpoint { static void IEndpoint.Map(object app) { } }
            public sealed partial class SplitBase : IEndpoint { }
            public sealed partial class SplitMap { public static void Map(object app) { } }
            public sealed partial class PartialPlain { }
            """);
        MethodInfo isCandidate = EndpointCompilations.Generators.Value.Generators.Single().GetGeneratorType()
            .GetMethod("IsCandidate", BindingFlags.NonPublic | BindingFlags.Static)!;
        isCandidate.Should().NotBeNull();

        string[] candidates = [.. tree.GetRoot().DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .Where(node => (bool)isCandidate.Invoke(null, [node])!)
            .Select(static node => node.Identifier.ValueText)];

        candidates.Should().Equal("Ping", "Explicit", "SplitBase", "SplitMap");
    }

    [Fact]
    public void Generator_Should_Map_Endpoints_Through_Derived_Interfaces_Explicit_Map_And_Split_Partials()
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Api",
            OutputKind.DynamicallyLinkedLibrary,
            """
            using CSharpEssentials.Endpoints;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;
            using Ep = CSharpEssentials.Endpoints.IEndpoint;

            namespace Sample.Api;

            public interface IVersionedEndpoint : IEndpoint;

            public sealed class Versioned : IVersionedEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/versioned", () => TypedResults.Ok());
            }

            public sealed class Explicit : IEndpoint
            {
                static void IEndpoint.Map(IEndpointRouteBuilder app) => app.MapGet("/explicit", () => TypedResults.Ok());
            }

            public sealed class Aliased : Ep
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/aliased", () => TypedResults.Ok());
            }

            public sealed partial class Split : IEndpoint;

            public sealed partial class Split
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/split", () => TypedResults.Ok());
            }
            """));

        ShouldCompile(run);
        string registry = EndpointCompilations.GeneratedText(run, "SampleApiEndpointRegistry.g.cs");
        registry.Should().Contain("global::Sample.Api.Versioned")
            .And.Contain("global::Sample.Api.Explicit")
            .And.Contain("global::Sample.Api.Aliased")
            .And.Contain("global::Sample.Api.Split");
    }

    [Theory]
    [InlineData("public class BaseEndpoint", "public sealed class Derived : BaseEndpoint;", false)]
    [InlineData("public abstract class BaseEndpoint", "public sealed class Derived : BaseEndpoint;", false)]
    [InlineData("public abstract class BaseEndpoint<T>", "public sealed class Derived : BaseEndpoint<int>;", false)]
    [InlineData("public abstract class BaseEndpoint", "public sealed partial class Derived : BaseEndpoint;", true)]
    [InlineData("public abstract class BaseEndpoint", "public sealed class Derived : BaseEndpoint { public static new void Map(IEndpointRouteBuilder app) => BaseEndpoint.Map(app); }", true)]
    public void Generator_Should_Map_Type_That_Inherits_Map_Only_When_Partial(string baseDeclaration, string derivedDeclaration, bool mapped)
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Api",
            OutputKind.DynamicallyLinkedLibrary,
            PingSource("Sample.Api"),
            $$"""
            using CSharpEssentials.Endpoints;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;

            namespace Sample.Api;

            {{baseDeclaration}} : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/base", () => TypedResults.Ok());
            }

            {{derivedDeclaration}}
            """));

        ShouldCompile(run);
        run.OutputDiagnostics.Should().NotContain(static d => d.Id == "CS0108");
        string registry = EndpointCompilations.GeneratedText(run, "SampleApiEndpointRegistry.g.cs");
        registry.Contains("global::Sample.Api.Derived", StringComparison.Ordinal).Should().Be(mapped);
    }

    private static string HostAggregate(string body)
    {
        CSharpCompilation api = Compile("Sample.Api", OutputKind.DynamicallyLinkedLibrary, LibrarySource);

        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(
            "Sample.Host",
            OutputKind.ConsoleApplication,
            [api.ToMetadataReference()],
            $$"""
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;

            namespace Sample.Host;

            public sealed class Other
            {
                public void WithName(string name) { }
            }

            public static class Routes
            {
                private const string ConstName = "FromConst";

                public static void MapRoutes(IEndpointRouteBuilder app)
                {
                    {{body}}
                }
            }
            """));

        ShouldCompile(run);
        return EndpointCompilations.GeneratedText(run, "SampleHostEndpointAggregate.g.cs");
    }

    private static string PingSource(string @namespace) => $$"""
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace {{@namespace}};

        public sealed class Ping : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ping", () => TypedResults.Ok());
        }
        """;

    private static CSharpCompilation Compile(string assemblyName, OutputKind outputKind, string source)
    {
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(assemblyName, outputKind, source));
        ShouldCompile(run);
        return (CSharpCompilation)run.OutputCompilation;
    }

    private static void ShouldCompile(GeneratorRun run)
    {
        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
    }
}
