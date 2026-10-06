using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Describes a route group that endpoints and nested groups can be placed under with
/// <see cref="EndpointGroupAttribute"/> or <see cref="EndpointGroupAttribute{TGroup}"/>.
/// </summary>
public interface IEndpointGroup
{
    /// <summary>
    /// Gets the route prefix passed to <c>MapGroup</c>. An empty string adds no route segment.
    /// </summary>
    static abstract string Prefix { get; }

    /// <summary>
    /// Applies group-wide conventions such as tags, authorization or endpoint filters.
    /// Called once for every created group builder.
    /// </summary>
    /// <param name="group">The group builder created for this group.</param>
    static virtual void Configure(RouteGroupBuilder group)
    {
    }
}
