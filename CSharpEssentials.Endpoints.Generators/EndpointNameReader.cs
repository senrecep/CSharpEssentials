using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Endpoints.Generators;

internal static class EndpointNameReader
{
    private const string WithName = "WithName";

    private const string WithMetadata = "WithMetadata";

    private const string ConventionExtensions = "Microsoft.AspNetCore.Builder.RoutingEndpointConventionBuilderExtensions";

    private const string EndpointNameInterface = "Microsoft.AspNetCore.Routing.IEndpointNameMetadata";

    private const string RouteNameInterface = "Microsoft.AspNetCore.Routing.IRouteNameMetadata";

    private static readonly string[] NameTypes =
    [
        "Microsoft.AspNetCore.Routing.EndpointNameAttribute",
        "Microsoft.AspNetCore.Routing.EndpointNameMetadata",
        "Microsoft.AspNetCore.Routing.RouteNameMetadata",
    ];

    public static bool IsCandidate(SyntaxNode node) =>
        node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: WithName or WithMetadata } };

    public static IReadOnlyList<EndpointNameUse> Read(IInvocationOperation invocation)
    {
        IMethodSymbol target = invocation.TargetMethod;
        if (target.Name is not (WithName or WithMetadata)
            || !string.Equals(target.ContainingType?.ToDisplayString(), ConventionExtensions, StringComparison.Ordinal))
        {
            return [];
        }

        return target.Name == WithName ? ReadWithName(invocation) : ReadWithMetadata(invocation);
    }

    private static IReadOnlyList<EndpointNameUse> ReadWithName(IInvocationOperation invocation)
    {
        IArgumentOperation? argument = invocation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal == 1);
        if (argument is null || !TryGetName(argument.Value, out string? name))
        {
            return [];
        }

        return [new EndpointNameUse(name, isEndpointName: true, isRouteName: true, argument.Value.Syntax.GetLocation())];
    }

    private static List<EndpointNameUse> ReadWithMetadata(IInvocationOperation invocation)
    {
        IArgumentOperation? argument = invocation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal == 1);
        if (argument?.Value is not IArrayCreationOperation { Initializer: { } initializer })
        {
            return [];
        }

        List<EndpointNameUse> uses = [];
        foreach (IOperation element in initializer.ElementValues)
        {
            IOperation value = Unwrap(element);
            if (value.Type is not { } type)
            {
                continue;
            }

            bool isEndpointName = Implements(type, EndpointNameInterface);
            bool isRouteName = Implements(type, RouteNameInterface);
            if (!isEndpointName && !isRouteName)
            {
                continue;
            }

            string? name = null;
            if (value is IObjectCreationOperation creation &&
                NameTypes.Contains(type.ToDisplayString(), StringComparer.Ordinal) &&
                creation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal == 0) is { } nameArgument &&
                !TryGetName(nameArgument.Value, out name))
            {
                continue;
            }

            uses.Add(new EndpointNameUse(name, isEndpointName, isRouteName, value.Syntax.GetLocation()));
        }

        return uses;
    }

    private static bool TryGetName(IOperation value, out string? name)
    {
        name = null;
        Optional<object?> constant = Unwrap(value).ConstantValue;
        if (!constant.HasValue)
        {
            return true;
        }

        name = constant.Value as string;
        return name is not null;
    }

    private static IOperation Unwrap(IOperation value)
    {
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        return value;
    }

    private static bool Implements(ITypeSymbol type, string interfaceName) =>
        string.Equals(type.ToDisplayString(), interfaceName, StringComparison.Ordinal) ||
        type.AllInterfaces.Any(candidate => string.Equals(candidate.ToDisplayString(), interfaceName, StringComparison.Ordinal));
}
