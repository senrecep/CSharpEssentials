using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Builds failed <typeparamref name="TResponse"/> values for pipeline behaviors without reflection.
/// <see cref="Factory"/> is set when <typeparamref name="TResponse"/> is <see cref="Result"/> or a
/// <see cref="Result{T}"/>, and is <see langword="null"/> for any other response type.
/// </summary>
internal static class FailureResponse<TResponse>
{
    internal static readonly IFailureFactory<TResponse>? Factory = default(TResponse) as IFailureFactory<TResponse>;
}
