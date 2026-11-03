using System.Globalization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// 500 hand-written <c>GET /items{i}/{id:int}</c> endpoints built in a loop, with the same handler shape as the generated
/// startup libraries: one int route parameter and <c>Results.Ok</c> of the shared <c>ItemResponse</c> record. The routes,
/// handlers and names are created once, so every mapping call reuses the same instances.
/// </summary>
public static class LoopEndpoints
{
    /// <summary>The endpoint count.</summary>
    public const int Count = 500;

    private static readonly string[] Routes = [.. Enumerable.Range(0, Count).Select(static i => "/items" + i.ToString(CultureInfo.InvariantCulture) + "/{id:int}")];

    private static readonly Delegate[] Handlers = [.. Enumerable.Range(0, Count).Select(static i => CreateHandler("item" + i.ToString(CultureInfo.InvariantCulture)))];

    /// <summary>Gets the endpoint names: <c>Item{i}Endpoint</c>, which is what <c>OperationNaming.TypeName</c> gives the <c>Cse500</c> types.</summary>
    public static IReadOnlyList<string> Names { get; } = [.. Enumerable.Range(0, Count).Select(static i => "Item" + i.ToString(CultureInfo.InvariantCulture) + "Endpoint")];

    /// <summary>Maps every endpoint straight onto <paramref name="app"/>.</summary>
    /// <param name="app">The route builder.</param>
    public static void MapDirect(IEndpointRouteBuilder app)
    {
        for (int i = 0; i < Count; i++)
        {
            app.MapGet(Routes[i], Handlers[i]);
        }
    }

    /// <summary>Maps every endpoint straight onto <paramref name="app"/> and names it with <c>WithName</c>.</summary>
    /// <param name="app">The route builder.</param>
    public static void MapDirectWithName(IEndpointRouteBuilder app)
    {
        for (int i = 0; i < Count; i++)
        {
            app.MapGet(Routes[i], Handlers[i]).WithName(Names[i]);
        }
    }

    /// <summary>Maps every endpoint inside its own <c>MapGroup(string.Empty)</c>, the wrapper CSE adds per endpoint type.</summary>
    /// <param name="app">The route builder.</param>
    public static void MapEmptyGroup(IEndpointRouteBuilder app)
    {
        for (int i = 0; i < Count; i++)
        {
            app.MapGroup(string.Empty).MapGet(Routes[i], Handlers[i]);
        }
    }

    private static Func<int, IResult> CreateHandler(string name) =>
        id => Results.Ok(new Startup.Plain500.ItemResponse(id, name));
}
