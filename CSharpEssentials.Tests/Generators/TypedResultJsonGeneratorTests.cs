using System.Reflection;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class TypedResultJsonGeneratorTests
{
    private const string Source = """
        using System.Collections.Generic;
        using System.Text.Json.Serialization;
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;

        namespace Sample;

        public sealed record Dto(int Id);

        [JsonSerializable(typeof(Result<Dto, NotFoundError>))]
        [JsonSerializable(typeof(Result<int, NotFoundError, ConflictError>))]
        [JsonSerializable(typeof(List<Result<int, NotFoundError, ConflictError, Dto>>))]
        [JsonSerializable(typeof(Result<string, NotFoundError>[]))]
        [JsonSerializable(typeof(Dto))]
        internal sealed partial class AppJsonContext;

        [JsonSerializable(typeof(Result<Dto, NotFoundError>))]
        internal sealed partial class OtherJsonContext;

        public static class Outer
        {
            private sealed record Hidden(int Id);

            [JsonSerializable(typeof(Result<Hidden, NotFoundError>))]
            private sealed partial class HiddenJsonContext;
        }
        """;

    private static readonly Assembly[] References = [typeof(Result<int, NotFoundError>).Assembly, typeof(Error).Assembly, typeof(System.Text.Json.JsonSerializer).Assembly];

    private static readonly Lazy<AnalyzerAssembly> ResultsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Results.Generators.dll"));

    [Fact]
    public void Generator_Should_RegisterEveryClosedTypedResult_When_AJsonSerializableAttributeNamesIt()
    {
        GeneratorRun run = Run(GeneratorHarness.CreateCompilation([Source], References));

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        string generated = run.Result.GeneratedTrees.Single().ToString();
        string[] registrations = [.. generated.Split('\n').Select(static line => line.Trim()).Where(static line => line.Contains(".Register<", StringComparison.Ordinal))];
        registrations.Should().Equal(
            "global::CSharpEssentials.ResultPattern.TypedResultJsonConverterFactory.Register<global::Sample.Dto, global::CSharpEssentials.Errors.NotFoundError>();",
            "global::CSharpEssentials.ResultPattern.TypedResultJsonConverterFactory.Register<int, global::CSharpEssentials.Errors.NotFoundError, global::CSharpEssentials.Errors.ConflictError, global::Sample.Dto>();",
            "global::CSharpEssentials.ResultPattern.TypedResultJsonConverterFactory.Register<int, global::CSharpEssentials.Errors.NotFoundError, global::CSharpEssentials.Errors.ConflictError>();",
            "global::CSharpEssentials.ResultPattern.TypedResultJsonConverterFactory.Register<string, global::CSharpEssentials.Errors.NotFoundError>();");
        generated.Should().Contain("[global::System.Runtime.CompilerServices.ModuleInitializer]");
    }

    [Fact]
    public void Generator_Should_EmitNothing_When_NoJsonSerializableAttributeNamesATypedResult()
    {
        GeneratorRun run = Run(GeneratorHarness.CreateCompilation(["""
            using System.Text.Json.Serialization;

            [JsonSerializable(typeof(int))]
            internal sealed partial class PlainJsonContext;
            """], References));

        run.Result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_EmitNothing_Below_CSharp_9()
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation(["""
            using System.Text.Json.Serialization;
            using CSharpEssentials.Errors;
            using CSharpEssentials.ResultPattern;

            [JsonSerializable(typeof(Result<int, NotFoundError>))]
            internal sealed partial class LegacyJsonContext { }
            """], References);
        CSharpParseOptions legacy = new(LanguageVersion.CSharp8);
        compilation = compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.Single(), compilation.SyntaxTrees.Single().WithRootAndOptions(compilation.SyntaxTrees.Single().GetRoot(), legacy));

        GeneratorRun run = Run(compilation);

        run.Result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_Should_GiveEachAssemblyItsOwnClass_When_AnAssemblySeesAnothersInternals()
    {
        GeneratorRun library = Run(GeneratorHarness.CreateCompilation(
            ["[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"Sample.App\")]", Source], References).WithAssemblyName("Sample.Lib"));
        using MemoryStream image = new();
        library.OutputCompilation.Emit(image).Success.Should().BeTrue();

        Compilation app = GeneratorHarness.CreateCompilation(["""
                using System.Text.Json.Serialization;
                using CSharpEssentials.Errors;
                using CSharpEssentials.ResultPattern;

                [JsonSerializable(typeof(Result<long, NotFoundError>))]
                internal sealed partial class AppJsonContext;
                """], References)
            .WithAssemblyName("Sample.App")
            .AddReferences(MetadataReference.CreateFromImage(image.ToArray()));
        GeneratorRun run = Run(app);

        run.OutputDiagnostics.Should().NotContain(static d => d.Id == "CS0436" || d.Severity == DiagnosticSeverity.Error);
        run.Result.GeneratedTrees.Single().ToString().Should().Contain("TypedResultJsonRegistration_SampleApp_");
    }

    private static GeneratorRun Run(Compilation compilation) =>
        GeneratorHarness.Run(compilation, [.. ResultsAssembly.Value.Generators]);
}
