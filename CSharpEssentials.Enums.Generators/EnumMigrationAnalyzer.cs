using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Reports a <c>MigrationBuilder.AlterColumn</c> call for a column that <c>ConvertEnumColumn</c> converts in the same
/// <c>Up</c> or <c>Down</c> method of an EF Core migration (CSE0014). The types are matched by metadata name, so the analyzer does
/// nothing in projects without EF Core and CSharpEssentials.EntityFrameworkCore.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumMigrationAnalyzer : DiagnosticAnalyzer
{
    private const string MigrationName = "Microsoft.EntityFrameworkCore.Migrations.Migration";
    private const string MigrationBuilderName = "Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder";
    private const string ExtensionsName = "CSharpEssentials.EntityFrameworkCore.EnumMigrationBuilderExtensions";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(StringEnumDiagnostics.AlterColumnWithEnumConversion);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? migration = start.Compilation.GetTypeByMetadataName(MigrationName);
            INamedTypeSymbol? builder = start.Compilation.GetTypeByMetadataName(MigrationBuilderName);
            INamedTypeSymbol? extensions = start.Compilation.GetTypeByMetadataName(ExtensionsName);
            if (migration is null || builder is null || extensions is null)
                return;

            start.RegisterOperationBlockAction(block => AnalyzeBlock(block, migration, builder, extensions));
        });
    }

    private static void AnalyzeBlock(OperationBlockAnalysisContext context, INamedTypeSymbol migration, INamedTypeSymbol builder, INamedTypeSymbol extensions)
    {
        if (context.OwningSymbol is not IMethodSymbol { IsOverride: true, Name: "Up" or "Down" } method || !DerivesFrom(method.ContainingType, migration))
            return;

        List<(ColumnKey Column, Location Location)> alters = [];
        HashSet<ColumnKey> converted = [];
        foreach (IInvocationOperation invocation in context.OperationBlocks.SelectMany(static block => block.DescendantsAndSelf()).OfType<IInvocationOperation>())
        {
            IMethodSymbol target = invocation.TargetMethod;
            bool alter = target.Name == "AlterColumn" && SymbolEqualityComparer.Default.Equals(target.ContainingType, builder);
            bool convert = target.Name == "ConvertEnumColumn" && SymbolEqualityComparer.Default.Equals(target.ContainingType, extensions);
            if (alter && Key(invocation, "table", "name") is { } altered)
                alters.Add((altered, invocation.Syntax.GetLocation()));
            else if (convert && Key(invocation, "table", "column") is { } conversion)
                converted.Add(conversion);
        }

        foreach ((ColumnKey column, Location location) in alters)
        {
            if (converted.Contains(column))
                context.ReportDiagnostic(Diagnostic.Create(StringEnumDiagnostics.AlterColumnWithEnumConversion, location, $"{column.Table}.{column.Column}", method.Name));
        }
    }

    private static ColumnKey? Key(IInvocationOperation invocation, string tableParameter, string columnParameter)
    {
        string? table = Constant(invocation, tableParameter);
        string? column = Constant(invocation, columnParameter);
        return table is null || column is null ? null : new ColumnKey(Constant(invocation, "schema"), table, column);
    }

    private static string? Constant(IInvocationOperation invocation, string parameter)
    {
        IArgumentOperation? argument = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameter);
        return argument?.Value.ConstantValue is { HasValue: true, Value: string value } ? value : null;
    }

    private static bool DerivesFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        for (INamedTypeSymbol? current = type?.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        }

        return false;
    }

    private readonly record struct ColumnKey(string? Schema, string Table, string Column);
}
