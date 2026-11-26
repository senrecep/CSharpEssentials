using System.Runtime.CompilerServices;

using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Tests whether a pipeline response is a failed result. Before net10 the JIT boxes a struct response that is tested against
/// <see cref="IResultBase"/> in shared generic code, so the CSE result structs are read through a cached
/// <see cref="IFailureProbe{TSelf}"/> instead; any other response falls back to the interface test. Callers on net10 and later
/// keep the plain pattern, which the JIT already folds there and which this extra call layer would slow down.
/// </summary>
internal static class ResponseFailure
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFailure<TResponse>(TResponse response)
    {
        if (typeof(TResponse).IsValueType && Probe<TResponse>.Instance is { } probe)
            return probe.IsFailureOf(in response);
        return response is IResultBase { IsFailure: true };
    }

    private static class Probe<TResponse>
    {
        internal static readonly IFailureProbe<TResponse>? Instance = default(TResponse) as IFailureProbe<TResponse>;
    }
}
