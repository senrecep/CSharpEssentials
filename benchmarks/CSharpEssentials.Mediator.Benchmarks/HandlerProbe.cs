namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Counts the handler calls of the behavior suites, so the self-check can see when a handler ran.</summary>
public static class HandlerProbe
{
    /// <summary>Incremented by every handler of <see cref="CreateCustomer"/>, <see cref="PlaceOrder"/>, <see cref="ReserveStock"/>, <see cref="GetProduct"/>, <see cref="LogPing"/> and <see cref="LogPayloadPing"/>.</summary>
    public static int HandlerCalls { get; set; }
}
