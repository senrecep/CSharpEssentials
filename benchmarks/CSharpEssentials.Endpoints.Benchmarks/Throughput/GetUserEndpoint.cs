using System.Globalization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput;

public sealed class GetUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/users/{id:int}", (int id) => Results.Ok(new UserResponse(id, "user" + id.ToString(CultureInfo.InvariantCulture))));
}
