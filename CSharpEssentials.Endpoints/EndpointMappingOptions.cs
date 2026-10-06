using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Options for one mapping call (<c>Map{Assembly}Endpoints</c>, <c>MapAllEndpoints</c> or <c>MapEndpointsFromAssemblies</c>).
/// </summary>
public sealed class EndpointMappingOptions
{
    private readonly List<Action<IEndpointConventionBuilder, Type>> _configureEach = [];
    private readonly List<Func<Type, bool>> _filters = [];
    private readonly Dictionary<Type, bool> _decisions = [];

    /// <summary>
    /// Gets or sets a value indicating whether mapped and filtered endpoint types are logged at <c>Debug</c>.
    /// </summary>
    public bool LogDiscovered { get; set; }

    /// <summary>
    /// Gets or sets the endpoint name (operationId) policy. Defaults to <see cref="OperationNaming.None"/>.
    /// </summary>
    public OperationNaming OperationNaming { get; set; } = OperationNaming.None;

    /// <summary>
    /// Gets or sets a value indicating whether endpoints without explicit tags are tagged with their innermost group's
    /// type name, without a trailing <c>Group</c> suffix.
    /// </summary>
    public bool AutoTagFromGroup { get; set; }

    internal IReadOnlyList<Action<IEndpointConventionBuilder, Type>> ConfigureEachCallbacks => _configureEach;

    /// <summary>
    /// Adds a callback that runs once per endpoint type, after group conventions and per-type metadata.
    /// Callbacks run in registration order.
    /// </summary>
    /// <param name="configure">Receives the endpoint type's convention builder and the endpoint type.</param>
    /// <returns>The same options instance.</returns>
    public EndpointMappingOptions ConfigureEach(Action<IEndpointConventionBuilder, Type> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _configureEach.Add(configure);
        return this;
    }

    /// <summary>
    /// Adds a predicate that decides whether an endpoint type is mapped. Multiple predicates are AND-combined.
    /// </summary>
    /// <param name="predicate">Returns <see langword="false"/> to skip the endpoint type.</param>
    /// <returns>The same options instance.</returns>
    public EndpointMappingOptions Filter(Func<Type, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _filters.Add(predicate);
        return this;
    }

    internal bool Includes(Type endpointType, out bool evaluated)
    {
        if (_decisions.TryGetValue(endpointType, out bool included))
        {
            evaluated = false;
            return included;
        }

        included = _filters.TrueForAll(filter => filter(endpointType));
        _decisions.Add(endpointType, included);
        evaluated = true;
        return included;
    }
}
