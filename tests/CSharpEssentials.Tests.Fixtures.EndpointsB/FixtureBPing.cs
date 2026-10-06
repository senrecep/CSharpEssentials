using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.EndpointsB;

public sealed class FixtureBPing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/fixture-b/ping", static () => TypedResults.Text("b-pong"));
}
