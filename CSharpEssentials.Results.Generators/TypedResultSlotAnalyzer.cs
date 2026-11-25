using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// Reports typed result types whose slots are ambiguous: a repeated error type (CSE5002), an error slot that converts to another
/// (CSE5003), a success type that converts to or from an error slot (CSE5004), and a slot-adding combinator whose added slot the
/// result already covers (CSE5006).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypedResultSlotAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> SlotAddingMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal, "Ensure", "FailIf", "BindWiden", "EnsureAsync", "FailIfAsync", "BindWidenAsync", "TryCatch", "TryCatchAsync", "SelectMany");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        TypedResultDiagnostics.DuplicateSlot,
        TypedResultDiagnostics.OverlappingSlots,
        TypedResultDiagnostics.SuccessFitsErrorSlot,
        TypedResultDiagnostics.CombinatorRepeatsSlot);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            var symbols = TypedResultSymbols.Create(start.Compilation);
            if (symbols is null)
                return;

            start.RegisterSyntaxNodeAction(nodeContext => AnalyzeTypeName(nodeContext, symbols), SyntaxKind.GenericName);
            start.RegisterOperationAction(operationContext => AnalyzeInvocation(operationContext, symbols), OperationKind.Invocation);
        });
    }

    private static void AnalyzeTypeName(SyntaxNodeAnalysisContext context, TypedResultSymbols symbols)
    {
        var name = (GenericNameSyntax)context.Node;
        INamedTypeSymbol? result = symbols.AsTypedResult(context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol as ITypeSymbol);
        if (result is null)
            return;

        Location location = name.GetLocation();
        string display = result.ToDisplayString();
        foreach (SlotPair pair in symbols.RepeatedOrOverlapping(result))
        {
            context.ReportDiagnostic(pair.Relation switch
            {
                SlotRelation.Repeats => Diagnostic.Create(TypedResultDiagnostics.DuplicateSlot, location, display, pair.Earlier.ToDisplayString()),
                SlotRelation.ConvertsTo => Diagnostic.Create(TypedResultDiagnostics.OverlappingSlots, location, display, pair.Later.ToDisplayString(), pair.Earlier.ToDisplayString()),
                _ => Diagnostic.Create(TypedResultDiagnostics.OverlappingSlots, location, display, pair.Earlier.ToDisplayString(), pair.Later.ToDisplayString()),
            });
        }

        ITypeSymbol success = result.TypeArguments[0];
        foreach (ITypeSymbol slot in TypedResultSymbols.ErrorSlots(result))
        {
            bool overlaps = success is not ITypeParameterSymbol && TypedResultSymbols.AreSame(success, slot) ||
                symbols.Fits(success, slot) || symbols.Fits(slot, success);
            if (overlaps)
                context.ReportDiagnostic(Diagnostic.Create(TypedResultDiagnostics.SuccessFitsErrorSlot, location, display, success.ToDisplayString(), slot.ToDisplayString()));
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, TypedResultSymbols symbols)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (!SlotAddingMethods.Contains(method.Name) || !SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, symbols.Assembly))
            return;

        INamedTypeSymbol? result = symbols.AsTypedResult(Unwrap(method.ReturnType));
        INamedTypeSymbol? receiver = symbols.AsTypedResult(Unwrap(Receiver(invocation)));
        if (result is null || receiver is null)
            return;

        int kept = receiver.TypeArguments.Length - 1;
        string hint = Hint(method.Name);
        foreach (SlotPair pair in symbols.RepeatedOrOverlapping(result).Where(pair => pair.LaterIndex >= kept))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                TypedResultDiagnostics.CombinatorRepeatsSlot,
                invocation.Syntax.GetLocation(),
                method.Name,
                result.ToDisplayString(),
                pair.Later.ToDisplayString(),
                pair.Relation switch
                {
                    SlotRelation.Repeats => "repeats",
                    SlotRelation.ConvertsTo => "converts to",
                    _ => "accepts",
                },
                pair.Earlier.ToDisplayString(),
                hint));
        }
    }

    private static ITypeSymbol? Receiver(IInvocationOperation invocation) =>
        invocation.Instance?.Type ?? (invocation.Arguments.IsEmpty ? null : invocation.Arguments[0].Value.Type);

    private static string Hint(string method) => method switch
    {
        "BindWiden" or "BindWidenAsync" or "SelectMany" => "call Bind<TOut> and let the inner result widen",
        "TryCatch" or "TryCatchAsync" => "catch an exception type that is not already a slot",
        _ => "cast the error to the slot type to keep the error set",
    };

    private static ITypeSymbol? Unwrap(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "Task" or "ValueTask", TypeArguments.Length: 1 } task &&
        task.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
            ? task.TypeArguments[0]
            : type;
}
