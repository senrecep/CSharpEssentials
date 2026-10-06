using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.EndpointsA;

[EndpointGroup<FixtureAGroup>]
public sealed class FixtureAPing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ping", static () => TypedResults.Text("a-pong"));
}
