using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Endpoints.Generators;

/// <summary>
/// Reports endpoint and group types that the endpoints generator cannot map (CSE1001–CSE1004, CSE1006, CSE1007).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EndpointsAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        EndpointDiagnostics.InaccessibleType,
        EndpointDiagnostics.GroupCycle,
        EndpointDiagnostics.MultipleGroups,
        EndpointDiagnostics.InstanceState,
        EndpointDiagnostics.SkippedType,
        EndpointDiagnostics.InvalidGroupTarget);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } type)
        {
            return;
        }

        bool isEndpoint = EndpointSymbolRules.IsEndpoint(type);
        if (!isEndpoint && !EndpointSymbolRules.IsGroup(type) || EndpointSymbolRules.IsExcluded(type))
        {
            return;
        }

        Location? location = type.Locations.FirstOrDefault();
        string name = type.ToDisplayString();
        if (EndpointSymbolRules.IsAbstractOrOpenGeneric(type))
        {
            context.ReportDiagnostic(Diagnostic.Create(EndpointDiagnostics.SkippedType, location, name));
            return;
        }

        if (!EndpointSymbolRules.IsAccessible(type))
        {
            context.ReportDiagnostic(Diagnostic.Create(EndpointDiagnostics.InaccessibleType, location, name));
        }

        AnalyzeGroupTargets(context, type, location, name);

        if (isEndpoint && HasInstanceState(type))
        {
            context.ReportDiagnostic(Diagnostic.Create(EndpointDiagnostics.InstanceState, location, name));
        }
    }

    private static void AnalyzeGroupTargets(SymbolAnalysisContext context, INamedTypeSymbol type, Location? location, string name)
    {
        IReadOnlyList<GroupTarget> targets = EndpointSymbolRules.GetGroupTargets(type);
        if (targets.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(EndpointDiagnostics.MultipleGroups, location, name));
            return;
        }

        if (targets.Count == 0)
        {
            return;
        }

        GroupTarget target = targets[0];
        if (!EndpointSymbolRules.IsValidGroupTarget(target.Type))
        {
            Location attributeLocation = target.Attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? location ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(
                EndpointDiagnostics.InvalidGroupTarget,
                attributeLocation,
                name,
                target.Type?.ToDisplayString() ?? "null"));
            return;
        }

        if (IsInCycle(type))
        {
            context.ReportDiagnostic(Diagnostic.Create(EndpointDiagnostics.GroupCycle, location, name));
        }
    }

    private static bool IsInCycle(INamedTypeSymbol type)
    {
        HashSet<INamedTypeSymbol> visited = [with(SymbolEqualityComparer.Default), type];
        INamedTypeSymbol current = type;
        while (true)
        {
            IReadOnlyList<GroupTarget> targets = EndpointSymbolRules.GetGroupTargets(current);
            if (targets.Count != 1 || !EndpointSymbolRules.IsValidGroupTarget(targets[0].Type))
            {
                return false;
            }

            var next = (INamedTypeSymbol)targets[0].Type!;
            if (SymbolEqualityComparer.Default.Equals(next, type))
            {
                return true;
            }

            if (!visited.Add(next))
            {
                return false;
            }

            current = next;
        }
    }

    private static bool HasInstanceState(INamedTypeSymbol type)
    {
        foreach (ISymbol member in type.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol { IsStatic: false, IsConst: false }:
                    return true;
                case IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: false, Parameters.Length: > 0 } constructor
                    when !IsCopyConstructor(type, constructor):
                    return true;
                default:
                    break;
            }
        }

        return false;
    }

    private static bool IsCopyConstructor(INamedTypeSymbol type, IMethodSymbol constructor) =>
        type.IsRecord &&
        constructor.Parameters.Length == 1 &&
        SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, type);
}
