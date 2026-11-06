namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The response of <see cref="GetProduct"/>.</summary>
/// <param name="Id">The product id.</param>
/// <param name="Name">The product name.</param>
/// <param name="Price">The price.</param>
public sealed record ProductView(int Id, string Name, decimal Price);
