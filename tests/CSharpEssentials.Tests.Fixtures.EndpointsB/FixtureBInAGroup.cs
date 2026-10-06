using CSharpEssentials.Endpoints;
using CSharpEssentials.Tests.Fixtures.EndpointsA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.EndpointsB;

[EndpointGroup(typeof(FixtureAGroup))]
public sealed class FixtureBInAGroup : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/from-b", static () => TypedResults.Text("b-in-a"));
}
