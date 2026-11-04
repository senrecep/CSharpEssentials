using System.Collections.Immutable;
using System.Globalization;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class JsonPolymorphismAnalyzerTests
{
    private const string Header = """
        using System.Collections.Generic;
        using System.Text.Json;
        using System.Text.Json.Serialization;
        using CSharpEssentials.Json;

        namespace Sample;

        [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
        public interface IEvent { }

        """;

    private static readonly Lazy<AnalyzerAssembly> JsonAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Json.Generators.dll"));

    [Fact]
    public async Task Valid_Hierarchy_Should_Report_Nothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
            [JsonTypeDiscriminator("b")] public sealed class B : IEvent { }
            public abstract class Middle : IEvent { }
            [JsonTypeDiscriminator("c")] public sealed class C : Middle { }
            public sealed class Plain { }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE4001_Should_Report_Two_Types_With_One_Value()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4001", """
            [JsonTypeDiscriminator("same")] public sealed class A : IEvent { }
            [JsonTypeDiscriminator("same")] public sealed class B : IEvent { }
            """);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        Message(diagnostic).Should().Be("'Sample.A' and 'Sample.B' both use the type discriminator 'same' under 'Sample.IEvent'");
    }

    [Fact]
    public async Task CSE4001_Should_Report_A_Value_That_Equals_A_Default_Full_Name()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4001", """
            [JsonTypeDiscriminator("Sample.B")] public sealed class A : IEvent { }
            public sealed class B : IEvent { }
            """, "CSE4005");

        Message(diagnostic).Should().Contain("'Sample.B'");
    }

    [Fact]
    public async Task CSE4002_Should_Report_A_Value_On_A_Listed_Type()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4002", """
            [JsonDerivedType(typeof(Circle), "circle")]
            public abstract class Shape { }
            [JsonTypeDiscriminator("c")] public sealed class Circle : Shape { }
            """);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Theory]
    [InlineData("public abstract class A : IEvent { }")]
    [InlineData("public interface IA : IEvent { }")]
    [InlineData("public sealed class A<T> : IEvent { }")]
    public async Task CSE4003_Should_Report_Types_That_Cannot_Be_Derived_Types(string declaration)
    {
        Diagnostic diagnostic = await SingleAsync("CSE4003", "[JsonTypeDiscriminator(\"a\")] " + declaration);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task CSE4004_Should_Report_A_Member_Named_Like_The_Discriminator()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4004", """
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { public string Kind { get; set; } = ""; }
            """);

        Message(diagnostic).Should().Be("The discriminator property 'kind' of 'Sample.IEvent' equals the JSON name of member 'Kind' on 'Sample.A'");
    }

    [Fact]
    public async Task CSE4004_Should_Read_JsonPropertyName()
    {
        await SingleAsync("CSE4004", """
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { [JsonPropertyName("KIND")] public string Other { get; set; } = ""; }
            """);
    }

    [Fact]
    public async Task CSE4005_Should_Report_Info_For_A_Derived_Type_Without_A_Value()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4005", "public sealed class A : IEvent { }");

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Info);
        Message(diagnostic).Should().Contain("CLR name 'Sample.A'");
    }

    [Fact]
    public async Task CSE4005_Should_Warn_When_The_Aot_Analyzer_Is_Enabled()
    {
        TestAnalyzerConfigOptionsProvider options = new(new Dictionary<string, string> { ["build_property.EnableAotAnalyzer"] = "true" });
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("public sealed class A : IEvent { }", options);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE4005").Which.Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task CSE4006_Should_Report_A_Context_Missing_A_Derived_Type()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4006", """
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
            [JsonTypeDiscriminator("b")] public sealed class B : IEvent { }

            [JsonSerializable(typeof(IEvent))]
            [JsonSerializable(typeof(A))]
            public partial class AppContext : JsonSerializerContext
            {
                public AppContext() : base(null) { }
                protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
                public override System.Text.Json.Serialization.Metadata.JsonTypeInfo? GetTypeInfo(System.Type type) => null;
            }
            """);

        Message(diagnostic).Should().Be("'Sample.AppContext' lists 'Sample.IEvent' but not its derived type 'Sample.B'; AddPolymorphism leaves the type out");
        diagnostic.Properties["DerivedType"].Should().Be("global::Sample.B");
    }

    [Fact]
    public async Task CSE4007_Should_Report_A_Derived_Type_With_Its_Own_Converter()
    {
        await SingleAsync("CSE4007", """
            [JsonTypeDiscriminator("a"), JsonConverter(typeof(AConverter))] public sealed class A : IEvent { }
            public sealed class AConverter : JsonConverter<A>
            {
                public override A Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) => new();
                public override void Write(Utf8JsonWriter writer, A value, JsonSerializerOptions options) { }
            }
            """);
    }

    [Fact]
    public async Task CSE4008_Should_Report_A_Value_Without_A_Polymorphic_Base()
    {
        await SingleAsync("CSE4008", """
            public abstract class Plain { }
            [JsonTypeDiscriminator("a")] public sealed class A : Plain { }
            """);
    }

    [Fact]
    public async Task CSE4009_Should_Report_A_Property_Without_A_Polymorphic_Type()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4009", """
            public sealed class Data { [JsonPolymorphicProperty("k")] public List<string> Items { get; set; } = new(); }
            """);

        Message(diagnostic).Should().Contain("'Items'");
    }

    [Fact]
    public async Task CSE4009_Should_Not_Report_Wrapped_Polymorphic_Types()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
            public sealed class Data
            {
                [JsonPolymorphicProperty("k")] public IEvent? One { get; set; }
                [JsonPolymorphicProperty("k")] public IEvent[] Array { get; set; } = System.Array.Empty<IEvent>();
                [JsonPolymorphicProperty("k")] public IReadOnlyDictionary<string, IEvent> Map { get; set; } = new Dictionary<string, IEvent>();
                [JsonPolymorphicProperty("k")] public IEnumerable<IEvent> Items { get; set; } = new List<IEvent>();
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE4010_Should_Report_Types_Generated_Code_Cannot_Reach()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public sealed class Holder
            {
                [JsonTypeDiscriminator("a")] private sealed class A : IEvent { }
                private sealed class Data { [JsonPolymorphicProperty("k")] public IEvent? Value { get; set; } }
            }
            """);

        diagnostics.Select(static d => d.Id).Should().Equal("CSE4010", "CSE4010");
    }

    [Fact]
    public async Task CSE4012_Should_Report_The_Attribute_On_A_Positional_Record_Parameter()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4012", """
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
            public sealed record Data([property: JsonPolymorphicProperty("k")] IEvent? Value);
            """);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        Message(diagnostic).Should().StartWith("[property: JsonPolymorphicProperty] on parameter 'Value'");
    }

    [Fact]
    public async Task CSE4012_Should_Not_Report_A_Declared_Record_Property()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
            public sealed record Data
            {
                [JsonPolymorphicProperty("k")] public IEvent? Value { get; init; }
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE4011_Should_Report_An_Unlisted_Subtype_Of_A_Closed_Base()
    {
        Diagnostic diagnostic = await SingleAsync("CSE4011", """
            [JsonDerivedType(typeof(Circle), "circle")]
            public abstract class Shape { }
            public sealed class Circle : Shape { }
            public sealed class Square : Shape { }
            """);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Info);
        Message(diagnostic).Should().StartWith("'Sample.Square' derives from 'Sample.Shape'");
    }

    private static async Task<Diagnostic> SingleAsync(string id, string source, params string[] alsoExpected)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);
        diagnostics.Select(static d => d.Id).Should().OnlyContain(d => d == id || alsoExpected.Contains(d));
        return diagnostics.Should().ContainSingle(d => d.Id == id).Subject;
    }

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, TestAnalyzerConfigOptionsProvider? options = null)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(JsonTypeDiscriminatorAttribute).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(Header + source, new CSharpParseOptions(LanguageVersion.Latest), path: "Source.cs"));
        return options is null
            ? AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, JsonAssembly.Value.Analyzers)
            : AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, JsonAssembly.Value.Analyzers, options);
    }

    private static string Message(Diagnostic diagnostic) => diagnostic.GetMessage(CultureInfo.InvariantCulture);
}
