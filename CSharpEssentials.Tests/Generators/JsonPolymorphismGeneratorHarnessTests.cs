using System.Text.Json.Serialization;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class JsonPolymorphismGeneratorHarnessTests
{
    private const string Source = """
        using System.Collections.Generic;
        using System.Text.Json.Serialization;
        using CSharpEssentials.Json;

        namespace Core
        {
            [JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
            public interface IEvent { }

            [JsonPolymorphic]
            [JsonDerivedType(typeof(Circle), "circle")]
            [JsonDerivedType(typeof(Square), "square")]
            public abstract class Shape { }

            public sealed class Circle : Shape { }

            [JsonTypeDiscriminator("sq")]
            public sealed class Square : Shape { }

            [JsonPolymorphic]
            public interface IValue { }
        }

        namespace Orders
        {
            [JsonTypeDiscriminator("user.created.v1")]
            public sealed record UserCreated(System.Guid Id) : Core.IEvent;

            [JsonTypeDiscriminator(7)]
            internal sealed record UserDeleted(System.Guid Id) : Core.IEvent;

            [JsonTypeDiscriminator("tri")]
            public sealed class Triangle : Core.Shape { }

            public static class Outer
            {
                [JsonTypeDiscriminator("nested \"quoted\"")]
                public sealed class Nested : Core.IEvent { }

                [JsonTypeDiscriminator("hidden")]
                private sealed class Hidden : Core.IEvent { }
            }

            [JsonTypeDiscriminator("text")]
            public sealed class TextValue : Core.IValue { }

            public class Data
            {
                [JsonPolymorphicProperty("values")]
                public Core.IValue? Value { get; set; }

                [JsonPolymorphicProperty("values")]
                public List<Core.IValue> Items { get; set; } = new();

                [JsonPolymorphicProperty("kind")]
                public Core.IValue?[] Array { get; set; } = System.Array.Empty<Core.IValue?>();

                [JsonPolymorphicProperty("kind")]
                public Dictionary<string, Core.IValue> Map { get; set; } = new();

                [JsonPolymorphicProperty("field")]
                public Core.IValue? Field;

                [JsonPolymorphicProperty("ignored")]
                public string Text { get; set; } = "";
            }
        }
        """;

    private const string ServiceSource = """
        using CSharpEssentials.Json;

        namespace Service;

        public static class Program
        {
            public static void Main() { }
        }
        """;

    private const string PolyfillSource = """
        namespace System
        {
            public class Object { }
            public abstract class ValueType { }
            public abstract class Enum : ValueType { }
            public struct Void { }
            public struct Int32 { }
            public struct Boolean { }
            public sealed class String { }
            public abstract class Attribute { }
        }

        namespace System.Text.Json.Serialization
        {
            public sealed class JsonPolymorphicAttribute : System.Attribute { }
        }

        namespace CSharpEssentials.Json
        {
            public sealed class JsonTypeDiscriminatorAttribute : System.Attribute
            {
                public JsonTypeDiscriminatorAttribute(string value) { }
            }

            public static class JsonPolymorphism { }
        }

        namespace CSharpEssentials.Enums
        {
            public sealed class StringEnumAttribute : System.Attribute { }
        }

        namespace Both
        {
            [CSharpEssentials.Enums.StringEnum] public enum Status { Active }

            [System.Text.Json.Serialization.JsonPolymorphic] public interface IEvent { }

            [CSharpEssentials.Json.JsonTypeDiscriminator("a")] public sealed class A : IEvent { }
        }
        """;

    private static readonly Lazy<AnalyzerAssembly> JsonAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Json.Generators.dll"));

    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    [Fact]
    public Task JsonPolymorphismGenerator_Should_Match_Snapshot()
    {
        GeneratorRun run = GeneratorHarness.Run(CreateCompilation(Source), [.. JsonAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath)).Should().BeEquivalentTo("CSharpEssentialsJsonPolymorphism.g.cs");
        return Verify(run.Driver);
    }

    [Fact]
    public Task JsonPolymorphismGenerator_Should_Chain_To_Referenced_Registries_When_The_Assembly_Has_No_Entries()
    {
        GeneratorRun events = GeneratorHarness.Run(CreateCompilation(Source).WithAssemblyName("Orders.Events"), [.. JsonAssembly.Value.Generators]);
        using MemoryStream image = new();
        events.OutputCompilation.Emit(image).Success.Should().BeTrue();

        Compilation service = CreateCompilation(ServiceSource)
            .WithAssemblyName("Orders.Service")
            .AddReferences(MetadataReference.CreateFromImage(image.ToArray()));
        GeneratorRun run = GeneratorHarness.Run(service, [.. JsonAssembly.Value.Generators]);

        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        return Verify(run.Driver);
    }

    [Fact]
    public void JsonPolymorphismGenerator_Should_Emit_Nothing_Without_Entries_Or_Referenced_Registries()
    {
        GeneratorRun run = GeneratorHarness.Run(CreateCompilation(ServiceSource), [.. JsonAssembly.Value.Generators]);

        run.Result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void JsonPolymorphismGenerator_Should_Emit_Nothing_Below_CSharp_9()
    {
        CSharpCompilation compilation = CreateCompilation(Source.Replace("public sealed record UserCreated(System.Guid Id) : Core.IEvent;", "public sealed class UserCreated : Core.IEvent { }")
            .Replace("internal sealed record UserDeleted(System.Guid Id) : Core.IEvent;", "internal sealed class UserDeleted : Core.IEvent { }"));
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp8);
        compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(compilation.SyntaxTrees.Select(tree => tree.WithRootAndOptions(tree.GetRoot(), parseOptions)));

        GeneratorDriver driver = CSharpGeneratorDriver.Create([.. JsonAssembly.Value.Generators], parseOptions: parseOptions).RunGenerators(compilation);

        driver.GetRunResult().GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void JsonPolymorphismGenerator_Should_Not_Emit_The_Polyfill_When_The_Attribute_Exists()
    {
        GeneratorRun run = GeneratorHarness.Run(CreateCompilation(Source), [.. JsonAssembly.Value.Generators]);

        run.Result.GeneratedTrees.Should().NotContain(static t => t.FilePath.EndsWith("ModuleInitializerAttribute.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void JsonPolymorphismGenerator_Should_Share_The_Polyfill_With_The_Enums_Generator()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Both",
            [CSharpSyntaxTree.ParseText(PolyfillSource, new CSharpParseOptions(LanguageVersion.Latest))],
            [],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. JsonAssembly.Value.Generators, .. EnumsAssembly.Value.Generators]);

        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath))
            .Should().Contain(["JsonPolymorphismModuleInitializerAttribute.g.cs", "ModuleInitializerAttribute.g.cs"]);
        run.OutputDiagnostics.Should().NotContain(static d => d.Location.SourceTree != null &&
            d.Location.SourceTree.FilePath.EndsWith("ModuleInitializerAttribute.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void JsonPolymorphismGenerator_Should_Keep_Every_Step_Cached_After_An_Unrelated_Edit()
    {
        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(
            CreateCompilation(Source),
            static c => GeneratorHarness.AddSource(c, "namespace Other; public class Unrelated { }"),
            [.. JsonAssembly.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSteps(second, "PolymorphicDerivedTypes", "PolymorphicProperties", "PolymorphicEntries", "PolymorphicRegistrySettings");
        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
        IncrementalCaching.ShouldNotCaptureCompilationObjects(second, "PolymorphicDerivedTypes", "PolymorphicProperties", "PolymorphicEntries", "PolymorphicRegistrySettings");
    }

    private static CSharpCompilation CreateCompilation(string source) =>
        GeneratorHarness.CreateCompilation([source], [typeof(JsonTypeDiscriminatorAttribute).Assembly, typeof(JsonPolymorphicAttribute).Assembly]);
}
