using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput;

public sealed class CreateUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/users", (CreateUserRequest user) => Results.Ok(new UserResponse(1, user.Name)));
}
