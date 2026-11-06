namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// The one <see cref="HttpClient"/> every library wraps, and the check that no library changed it. Flurl sets the
/// timeout of a client it wraps to infinite, so the client starts with an infinite timeout. With it, the CSE, BCL and
/// Refit rows allocate no CancellationTokenSource. Flurl inherits Timeout = InfiniteTimeSpan and links a CTS per call,
/// which also makes HttpClient link one. RestSharp always creates a 100 s CTS and a linked CTS. Both are library defaults.
/// </summary>
internal static class SharedClient
{
    public static HttpClient Create(StubHandler stub) =>
        new(stub, disposeHandler: false) { BaseAddress = Inputs.BaseAddress, Timeout = Timeout.InfiniteTimeSpan };

    public static void AssertUntouched(string scenario, HttpClient client)
    {
        if (client.DefaultRequestHeaders.Any() || client.Timeout != Timeout.InfiniteTimeSpan || client.BaseAddress != Inputs.BaseAddress)
        {
            throw new InvalidOperationException(
                $"Parity check failed in {scenario}: a library changed the shared HttpClient (headers '{client.DefaultRequestHeaders}', timeout {client.Timeout}, base {client.BaseAddress}).");
        }
    }
}
