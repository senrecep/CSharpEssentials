using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Endpoints.Generators;

internal static class EndpointRouteReader
{
    private const string RouteBuilderExtensions = "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions";

    private const string RouteBuilderInterface = "Microsoft.AspNetCore.Routing.IEndpointRouteBuilder";

    public static IReadOnlyList<EndpointRoute> Read(IInvocationOperation invocation, ISymbol containingSymbol)
    {
        if (containingSymbol is not IMethodSymbol { IsStatic: true, Name: "Map", Parameters.Length: 1 } map ||
            map.ContainingType is not { } endpoint ||
            !string.Equals(map.Parameters[0].Type.ToDisplayString(), RouteBuilderInterface, StringComparison.Ordinal) ||
            !EndpointSymbolRules.IsEndpoint(endpoint) ||
            !TryGetGroupKey(endpoint, out string groupKey))
        {
            return [];
        }

        IMethodSymbol target = invocation.TargetMethod;
        if (!string.Equals(target.ContainingType?.ToDisplayString(), RouteBuilderExtensions, StringComparison.Ordinal) ||
            !IsMapParameter(invocation, map.Parameters[0]) ||
            GetConstantString(invocation, "pattern") is not { } pattern)
        {
            return [];
        }

        IReadOnlyList<string> methods = GetMethods(invocation, target.Name);
        Location location = GetArgument(invocation, "pattern")!.Syntax.GetLocation();
        string endpointName = endpoint.ToDisplayString();
        return [.. methods.Select(method => new EndpointRoute(groupKey, method, pattern, endpointName, location))];
    }

    private static bool TryGetGroupKey(INamedTypeSymbol endpoint, out string groupKey)
    {
        groupKey = string.Empty;
        if (EndpointSymbolRules.IsExcluded(endpoint) ||
            EndpointSymbolRules.IsAbstractOrOpenGeneric(endpoint) ||
            endpoint.IsRefLikeType ||
            !EndpointSymbolRules.IsAccessible(endpoint))
        {
            return false;
        }

        GroupChain chain = EndpointSymbolRules.ResolveGroupChain(endpoint);
        if (chain.Status != GroupChainStatus.Valid)
        {
            return false;
        }

        if (chain.Groups.Count > 0)
        {
            groupKey = EndpointSymbolRules.GetFullyQualifiedName(chain.Groups[chain.Groups.Count - 1]);
        }

        return true;
    }

    private static bool IsMapParameter(IInvocationOperation invocation, IParameterSymbol parameter) =>
        invocation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal == 0)?.Value is IParameterReferenceOperation reference &&
        SymbolEqualityComparer.Default.Equals(reference.Parameter, parameter);

    private static IReadOnlyList<string> GetMethods(IInvocationOperation invocation, string name) =>
        name switch
        {
            "MapGet" => ["GET"],
            "MapPost" => ["POST"],
            "MapPut" => ["PUT"],
            "MapDelete" => ["DELETE"],
            "MapPatch" => ["PATCH"],
            "MapMethods" => GetConstantMethods(GetArgument(invocation, "httpMethods")?.Value),
            _ => [],
        };

    private static IReadOnlyList<string> GetConstantMethods(IOperation? value)
    {
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        if (value is { Syntax: CollectionExpressionSyntax collection, SemanticModel: { } semanticModel })
        {
            return GetConstantMethods(collection, semanticModel);
        }

        if (value is not IArrayCreationOperation { Initializer: { } initializer })
        {
            return [];
        }

        List<string> methods = [];
        foreach (IOperation element in initializer.ElementValues)
        {
            if (element.ConstantValue is not { HasValue: true, Value: string method })
            {
                return [];
            }

            methods.Add(method.ToUpperInvariant());
        }

        return [.. methods.Distinct(StringComparer.Ordinal)];
    }

    private static IReadOnlyList<string> GetConstantMethods(CollectionExpressionSyntax collection, SemanticModel semanticModel)
    {
        List<string> methods = [];
        foreach (CollectionElementSyntax element in collection.Elements)
        {
            if (element is not ExpressionElementSyntax { Expression: { } expression } ||
                semanticModel.GetConstantValue(expression) is not { HasValue: true, Value: string method })
            {
                return [];
            }

            methods.Add(method.ToUpperInvariant());
        }

        return [.. methods.Distinct(StringComparer.Ordinal)];
    }

    private static string? GetConstantString(IInvocationOperation invocation, string parameterName) =>
        GetArgument(invocation, parameterName)?.Value is { ConstantValue: { HasValue: true, Value: string value } } ? value : null;

    private static IArgumentOperation? GetArgument(IInvocationOperation invocation, string parameterName) =>
        invocation.Arguments.FirstOrDefault(argument => string.Equals(argument.Parameter?.Name, parameterName, StringComparison.Ordinal));
}
