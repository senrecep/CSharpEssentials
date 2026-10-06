using CSharpEssentials.Endpoints;

namespace Examples.Endpoints;

public sealed class Health : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/health", () => TypedResults.Text("healthy"));
}
