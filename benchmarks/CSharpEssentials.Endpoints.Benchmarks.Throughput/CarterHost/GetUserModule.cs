using System.Globalization;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.CarterHost;

// https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
public sealed class GetUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/users/{id:int}", (int id) => Results.Ok(new UserResponse(id, "user" + id.ToString(CultureInfo.InvariantCulture))));
}
