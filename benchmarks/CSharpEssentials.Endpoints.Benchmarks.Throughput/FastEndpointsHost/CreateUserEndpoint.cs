using FastEndpoints;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

// https://fast-endpoints.com/docs/validation
public sealed class CreateUserEndpoint : Endpoint<CreateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
    }

    public override Task HandleAsync(CreateUserRequest req, CancellationToken ct) =>
        Send.OkAsync(new UserResponse(1, req.Name!), ct);
}
