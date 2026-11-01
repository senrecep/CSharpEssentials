using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput;

public sealed record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Range(18, 120)] int Age);
