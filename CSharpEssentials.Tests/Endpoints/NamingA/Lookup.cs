using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Endpoints.NamingA;

[ExcludeFromMapping]
public sealed class Lookup : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/naming/a/lookup", () => "a");
}
