using System.Globalization;

using FastEndpoints;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

// https://fast-endpoints.com/docs/get-started and https://fast-endpoints.com/docs/model-binding
public sealed class GetUserEndpoint : Endpoint<GetUserRequest, UserResponse>
{
    public override void Configure()
    {
        Get("/users/{id:int}");
        AllowAnonymous();
    }

    public override Task HandleAsync(GetUserRequest req, CancellationToken ct) =>
        Send.OkAsync(new UserResponse(req.Id, "user" + req.Id.ToString(CultureInfo.InvariantCulture)), ct);
}
