namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

public sealed class CreateUserRequest
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public int Age { get; set; }
}
