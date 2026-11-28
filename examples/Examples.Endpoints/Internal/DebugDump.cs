using CSharpEssentials.Endpoints;

namespace Examples.Endpoints.Internal;

[ExcludeFromMapping]
public sealed class DebugDump : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/debug", () => TypedResults.Text("never mapped"));
}
