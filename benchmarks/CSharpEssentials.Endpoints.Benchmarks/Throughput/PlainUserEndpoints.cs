using System.Globalization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput;

public static class PlainUserEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id) => Results.Ok(new UserResponse(id, "user" + id.ToString(CultureInfo.InvariantCulture))));
        app.MapPost("/users", (CreateUserRequest user) => Results.Ok(new UserResponse(1, user.Name)));
        app.MapDelete("/users/{id:int}", (int id) => Results.NoContent());
    }
}
