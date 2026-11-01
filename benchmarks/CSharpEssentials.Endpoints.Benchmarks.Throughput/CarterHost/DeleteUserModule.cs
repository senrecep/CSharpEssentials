using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.CarterHost;

// https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
public sealed class DeleteUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapDelete("/users/{id:int}", (int id) => Results.NoContent());
}
