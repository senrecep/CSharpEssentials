using System.Collections.Immutable;
using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Enums.Runtime;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class StringEnumGeneratorHarnessTests
{
    private const string Source = """
        using CSharpEssentials.Enums;

        namespace Sample;

        [StringEnum]
        public enum OrderStatus
        {
            Pending,
            InProgress,
            Shipped
        }
        """;

    private const string RichSource = """
        using System;
        using System.ComponentModel;
        using System.Runtime.Serialization;
        using System.Text.Json.Serialization;
        using CSharpEssentials.Enums;

        namespace Sample;

        [StringEnum(Naming = EnumNaming.KebabCaseLower, Storage = EnumStorage.Integer)]
        public enum Contract
        {
            /// <summary>
            /// Waiting for
            /// approval.
            /// </summary>
            PendingApproval,

            [Description("Shipped to the customer")]
            [EnumAlias("Sent", "Dispatched")]
            [JsonStringEnumMemberName("shipped")]
            [EnumMember(Value = "ignored")]
            HTTPShipped,

            [EnumMember(Value = "on \"hold\"")]
            OnHold,

            [Obsolete("Use OnHold")]
            Paused,

            [EnumFallback]
            Unknown = 99,
        }

        [StringEnum, Flags]
        internal enum Permissions : byte
        {
            None = 0,
            Read = 1,
            Write = 2,
            ReadWrite = Read | Write,
        }

        [StringEnum]
        public enum Signed : long
        {
            Negative = -5,
            [Obsolete("Use Negative")]
            Min = long.MinValue,
            Alias = -5,
        }

        [StringEnum]
        public enum Huge : ulong
        {
            Zero = 0,
            [Obsolete("Use Zero")]
            Max = ulong.MaxValue,
        }

        public class Order
        {
            [StringEnum]
            public enum State { Open, Closed }

            internal class Line
            {
                [StringEnum]
                public enum Kind { Product, Shipping }
            }

            [StringEnum]
            private enum Hidden { A }
        }

        public class Generic<T>
        {
            [StringEnum]
            public enum Skipped { A }
        }
        """;

    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    [Fact]
    public Task StringEnumGenerator_Should_Match_Snapshot()
    {
        GeneratorRun run = GeneratorHarness.Run(CreateCompilation(), [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        return Verify(run.Driver);
    }

    [Fact]
    public Task StringEnumGenerator_Should_Match_Snapshot_For_Attributes_Flags_Nested_And_Extreme_Values()
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly]);
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
            RichSource,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse),
            path: "Rich.cs"));

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath)).Should().BeEquivalentTo(
            "Sample.ContractExtensions.g.cs",
            "Sample.PermissionsExtensions.g.cs",
            "Sample.SignedExtensions.g.cs",
            "Sample.HugeExtensions.g.cs",
            "Sample.Order+StateExtensions.g.cs",
            "Sample.Order+Line+KindExtensions.g.cs",
            "__CSharpEssentialsEnumRegistry.g.cs");
        return Verify(run.Driver);
    }

    [Theory]
    [InlineData(LanguageVersion.CSharp7_3, NullableContextOptions.Disable)]
    [InlineData(LanguageVersion.CSharp8, NullableContextOptions.Enable)]
    public void StringEnumGenerator_Should_Emit_Code_Compatible_With_Older_Language_Versions(
        LanguageVersion languageVersion,
        NullableContextOptions nullableOptions)
    {
        const string legacySource = """
            using CSharpEssentials.Enums;

            namespace Sample
            {
                [StringEnum]
                public enum OrderStatus
                {
                    Pending,
                    InProgress,
                    Shipped
                }
            }
            """;
        CSharpParseOptions parseOptions = new(languageVersion);
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly]);
        compilation = compilation
            .WithOptions(compilation.Options.WithNullableContextOptions(nullableOptions))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(legacySource, parseOptions, path: "Legacy.cs"));

        GeneratorDriver driver = CSharpGeneratorDriver.Create([.. EnumsAssembly.Value.Generators], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> generatorDiagnostics);

        GeneratorDriverRunResult result = driver.GetRunResult();
        result.GeneratedTrees.Should().ContainSingle();
        result.GeneratedTrees[0].ToString().Should().NotContain("ToWireName").And.NotContain("Obsolete").And.NotContain("ModuleInitializer");
        generatorDiagnostics.Should().BeEmpty();
        output.GetDiagnostics().Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void StringEnumGenerator_Should_Return_Cached_Outputs_When_Compilation_Is_Identical()
    {
        CSharpCompilation compilation = CreateCompilation();

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(compilation, static c => c, [.. EnumsAssembly.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
    }

    [Fact]
    public void StringEnumGenerator_Should_Cache_Models_When_An_Unrelated_File_Changes()
    {
        CSharpCompilation compilation = CreateCompilation();

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(
            compilation,
            static c => GeneratorHarness.AddSource(c, "namespace Sample; public class Unrelated { }"),
            [.. EnumsAssembly.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSteps(second, "EnumModels", "EnumGeneratorSettings", "EnumRegistry", "EnumModuleInitializer");
        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
        IncrementalCaching.ShouldNotCaptureCompilationObjects(second, "EnumModels", "EnumGeneratorSettings", "EnumRegistry", "EnumModuleInitializer");
    }

    [Fact]
    public void StringEnumGenerator_Should_Generate_SameNamedEnums_In_DifferentNamespaces()
    {
        const string first = """
            using CSharpEssentials.Enums;

            namespace Orders;

            [StringEnum]
            public enum Status { Open, Closed }
            """;
        const string second = """
            using CSharpEssentials.Enums;

            namespace Users;

            [StringEnum]
            public enum Status { Active, Suspended }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([first, second], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath))
            .Should().BeEquivalentTo("Orders.StatusExtensions.g.cs", "Users.StatusExtensions.g.cs", "__CSharpEssentialsEnumRegistry.g.cs");
    }

    [Fact]
    public void StringEnumGenerator_Should_Use_Distinct_Hint_Names_When_Extensions_Class_Names_Collide()
    {
        const string source = """
            using CSharpEssentials.Enums;

            namespace Sample;

            public class Order
            {
                [StringEnum]
                public enum State { A }
            }

            [StringEnum]
            public enum Order_State { B }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath)).Should().BeEquivalentTo(
            "Sample.Order+StateExtensions.g.cs",
            "Sample.Order_StateExtensions.g.cs",
            "__CSharpEssentialsEnumRegistry.g.cs");
    }

    [Theory]
    [InlineData(null, "SnakeCaseLower", "in_progress")]
    [InlineData("KebabCaseUpper", "SnakeCaseLower", "IN-PROGRESS")]
    [InlineData("camelcase", "SnakeCaseLower", "inProgress")]
    [InlineData("bogus", "SnakeCaseLower", "in_progress")]
    [InlineData("KebabCaseLower", "PascalCase", "InProgress")]
    public void StringEnumGenerator_Should_Apply_Naming_Priority(string? projectNaming, string enumNaming, string expected)
    {
        string source = $$"""
            using CSharpEssentials.Enums;

            namespace Sample;

            [StringEnum{{(projectNaming is not null && enumNaming == "SnakeCaseLower" ? string.Empty : $"(Naming = EnumNaming.{enumNaming})")}}]
            public enum OrderStatus { InProgress }
            """;
        Dictionary<string, string> options = projectNaming is null ? [] : new() { ["build_property.CSharpEssentialsEnumNaming"] = projectNaming };

        Dictionary<string, string> wireNames = GenerateWireNames(source, options, "Sample.OrderStatusExtensions");

        wireNames["InProgress"].Should().Be(expected);
    }

    [Theory]
    [InlineData(EnumNaming.SnakeCaseLower)]
    [InlineData(EnumNaming.SnakeCaseUpper)]
    [InlineData(EnumNaming.KebabCaseLower)]
    [InlineData(EnumNaming.KebabCaseUpper)]
    [InlineData(EnumNaming.CamelCase)]
    public void Generated_Wire_Names_Should_Match_JsonNamingPolicy(EnumNaming naming)
    {
        string[] corpus = [.. ((IEnumerable<object[]>)EnumNameConverterTests.Corpus).Select(static row => (string)row[0])];
        string source = $$"""
            using CSharpEssentials.Enums;

            namespace Corpus;

            [StringEnum]
            public enum Names { {{string.Join(", ", corpus)}} }
            """;
        JsonNamingPolicy policy = naming switch
        {
            EnumNaming.SnakeCaseUpper => JsonNamingPolicy.SnakeCaseUpper,
            EnumNaming.KebabCaseLower => JsonNamingPolicy.KebabCaseLower,
            EnumNaming.KebabCaseUpper => JsonNamingPolicy.KebabCaseUpper,
            EnumNaming.CamelCase => JsonNamingPolicy.CamelCase,
            _ => JsonNamingPolicy.SnakeCaseLower,
        };

        Dictionary<string, string> wireNames = GenerateWireNames(
            source,
            new Dictionary<string, string> { ["build_property.CSharpEssentialsEnumNaming"] = naming.ToString() },
            "Corpus.NamesExtensions");

        wireNames.Should().HaveCount(corpus.Length);
        foreach (string name in corpus)
        {
            wireNames[name].Should().Be(policy.ConvertName(name), "member '{0}' must be named like System.Text.Json names it", name);
        }
    }

    private static Dictionary<string, string> GenerateWireNames(string source, Dictionary<string, string> options, string extensionsClass)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, new TestAnalyzerConfigOptionsProvider(options), [.. EnumsAssembly.Value.Generators]);

        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        INamedTypeSymbol extensions = run.OutputCompilation.GetTypeByMetadataName(extensionsClass)!;
        return extensions.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(static f => f.IsConst && f.Name.EndsWith("WireName", StringComparison.Ordinal))
            .ToDictionary(static f => f.Name[..^"WireName".Length], static f => (string)f.ConstantValue!);
    }

    private static CSharpCompilation CreateCompilation() =>
        GeneratorHarness.CreateCompilation([Source], [typeof(StringEnumAttribute).Assembly]);
}
