namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.CarterHost;

public sealed record CreateUserRequest(string? Name, string? Email, int Age);
