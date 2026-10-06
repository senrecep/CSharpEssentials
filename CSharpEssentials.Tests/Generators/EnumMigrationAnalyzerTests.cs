using System.Collections.Immutable;
using System.Globalization;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSharpEssentials.Tests.Generators;

public class EnumMigrationAnalyzerTests
{
    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    private const string Usings = """
        using CSharpEssentials.EntityFrameworkCore;
        using CSharpEssentials.Enums;
        using Microsoft.EntityFrameworkCore.Migrations;

        namespace Sample;

        public enum Status { Pending, Shipped }

        """;

    [Theory]
    [InlineData("Up")]
    [InlineData("Down")]
    public async Task CSE0014_Should_Report_AlterColumn_Of_A_Converted_Column(string method)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync($$"""
            public sealed class ConvertStatus : Migration
            {
                protected override void {{method}}(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
                    migrationBuilder.ConvertEnumColumn<Status>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
                    migrationBuilder.AlterColumn<string>(name: "Status", table: "orders", type: "text", nullable: false, oldClrType: typeof(int), oldType: "integer");
                    migrationBuilder.AlterColumn<string>(name: "Note", table: "orders", type: "text", nullable: true);
                }

                protected override void {{(method == "Up" ? "Down" : "Up")}}(MigrationBuilder migrationBuilder)
                {
                }
            }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0014");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage(CultureInfo.InvariantCulture).Should()
            .Be($"AlterColumn changes 'orders.Status', which ConvertEnumColumn converts in the same {method} method; remove the generated AlterColumn");
        SourceText(diagnostic).Should().StartWith("migrationBuilder.AlterColumn<string>(name: \"Status\"");
    }

    [Fact]
    public async Task CSE0014_Should_Not_Report_Different_Columns_Tables_Or_Schemas()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public sealed class ConvertStatus : Migration
            {
                protected override void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.ConvertEnumColumn<Status>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String, schema: "public");
                    migrationBuilder.AlterColumn<string>(name: "Note", table: "orders");
                    migrationBuilder.AlterColumn<string>(name: "Status", table: "archive");
                    migrationBuilder.AlterColumn<string>(name: "Status", table: "orders", schema: "sales");
                }

                protected override void Down(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.AlterColumn<int>(name: "Status", table: "orders");
                }
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", ", schema: \"sales\"")]
    [InlineData(", schema: \"sales\"", "")]
    [InlineData(", schema: null", ", schema: \"sales\"")]
    public async Task CSE0014_Should_Report_When_OneSideHasNoSchema(string convertSchema, string alterSchema)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync($$"""
            public sealed class ConvertStatus : Migration
            {
                protected override void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.ConvertEnumColumn<Status>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String{{convertSchema}});
                    migrationBuilder.AlterColumn<string>(name: "Status", table: "orders"{{alterSchema}});
                }

                protected override void Down(MigrationBuilder migrationBuilder)
                {
                }
            }
            """);

        diagnostics.Should().ContainSingle().Which.Id.Should().Be("CSE0014");
    }

    [Fact]
    public async Task CSE0014_Should_Not_Report_Outside_Migration_Up_And_Down()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public static class Helpers
            {
                public static void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.ConvertEnumColumn<Status>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
                    migrationBuilder.AlterColumn<string>(name: "Status", table: "orders");
                }
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0014_Should_Do_Nothing_Without_Entity_Framework_Core()
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
                namespace Sample;

                public abstract class Migration { protected abstract void Up(MigrationBuilder migrationBuilder); }

                public sealed class MigrationBuilder { public void AlterColumn<T>(string name, string table) { } }

                public static class Extensions { public static void ConvertEnumColumn(this MigrationBuilder builder, string table, string column) { } }

                public sealed class ConvertStatus : Migration
                {
                    protected override void Up(MigrationBuilder migrationBuilder)
                    {
                        migrationBuilder.ConvertEnumColumn("orders", "Status");
                        migrationBuilder.AlterColumn<string>(name: "Status", table: "orders");
                    }
                }
                """, new CSharpParseOptions(LanguageVersion.Latest), path: "Source.cs"));

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EnumsAssembly.Value.Analyzers);

        diagnostics.Where(diagnostic => diagnostic.Id == "CSE0014").Should().BeEmpty();
    }

    private static string SourceText(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.ToString().Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string declarations)
    {
        CSharpCompilation compilation = GeneratorHarness
            .CreateCompilation([], [typeof(StringEnumAttribute).Assembly, typeof(Migration).Assembly, typeof(EnumMigrationBuilderExtensions).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(Usings + declarations, new CSharpParseOptions(LanguageVersion.Latest), path: "Source.cs"));
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EnumsAssembly.Value.Analyzers);
        return [.. diagnostics.Where(diagnostic => diagnostic.Id == "CSE0014")];
    }
}
