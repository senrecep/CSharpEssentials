using FastEndpoints;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

// https://fast-endpoints.com/docs/model-binding#route-values-without-a-dto
public sealed class DeleteUserEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/users/{id:int}");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        _ = Route<int>("id");
        return Send.NoContentAsync(ct);
    }
}
