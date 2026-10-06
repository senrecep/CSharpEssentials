using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Endpoint name (operationId) policy applied to every mapped endpoint that has no explicit name.
/// </summary>
public sealed class OperationNaming
{
    private readonly Func<Type, EndpointBuilder, string?>? _nameFactory;

    private OperationNaming(Func<Type, EndpointBuilder, string?>? nameFactory) => _nameFactory = nameFactory;

    /// <summary>
    /// Gets a policy that leaves endpoint names unchanged.
    /// </summary>
    public static OperationNaming None { get; } = new(null);

    /// <summary>
    /// Gets a policy that names endpoints after their endpoint type (<c>CreateApp</c>).
    /// </summary>
    public static OperationNaming TypeName { get; } = new(static (endpointType, _) => endpointType.Name);

    internal bool IsNone => _nameFactory is null;

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
        return new OperationNaming(nameFactory);
    }

    internal string? GetName(Type endpointType, EndpointBuilder endpoint) => _nameFactory?.Invoke(endpointType, endpoint);
}
