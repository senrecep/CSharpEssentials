using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Endpoint name (operationId) policy applied to every mapped endpoint that has no explicit name.
/// </summary>
public sealed class OperationNaming
{
    private readonly Func<Type, EndpointBuilder, string?>? _nameFactory;

    private OperationNaming(Func<Type, EndpointBuilder, string?>? nameFactory, bool isTypeName)
    {
        _nameFactory = nameFactory;
        IsTypeName = isTypeName;
    }

    /// <summary>
    /// Gets a policy that leaves endpoint names unchanged.
    /// </summary>
    public static OperationNaming None { get; } = new(null, isTypeName: false);

    /// <summary>
    /// Gets a policy that names endpoints after their endpoint type and its containing types (<c>CreateApp</c>, <c>Orders_Create</c>).
    /// A type that maps several routes gets an HTTP method suffix (<c>Items_Get</c>, <c>Items_Post</c>) and a number when methods repeat.
    /// Types whose names collide are qualified with their namespace, and a name already used by an explicit
    /// <c>WithName(...)</c> gets a numeric suffix, so every generated name is unique.
    /// </summary>
    public static OperationNaming TypeName { get; } = new(null, isTypeName: true);

    internal bool IsNone => _nameFactory is null && !IsTypeName;

    internal bool IsTypeName { get; }

    /// <summary>
    /// Creates a policy that names endpoints with <paramref name="nameFactory"/>.
    /// </summary>
    /// <param name="nameFactory">
    /// Receives the endpoint type and the endpoint builder and returns the name, or <see langword="null"/> to leave the endpoint unnamed.
    /// </param>
    /// <returns>The naming policy.</returns>
    public static OperationNaming Custom(Func<Type, EndpointBuilder, string?> nameFactory)
    {
        ArgumentNullException.ThrowIfNull(nameFactory);
        return new OperationNaming(nameFactory, isTypeName: false);
    }

    internal string? GetName(Type endpointType, EndpointBuilder endpoint) => _nameFactory?.Invoke(endpointType, endpoint);
}
