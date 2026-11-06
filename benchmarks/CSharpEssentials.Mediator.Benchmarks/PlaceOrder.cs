using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// The request of <see cref="IdempotencyBehaviorBenchmarks"/>, one type for every variant. The scope is
/// <c>IdempotencyScope.Global</c>, as the CSharpEssentials.Mediator Readme advises for a caller with no principal.
/// </summary>
/// <param name="IdempotencyKey">The idempotency key.</param>
/// <param name="Sku">The product.</param>
/// <param name="Quantity">0 makes the handler fail; otherwise it returns Quantity + 1.</param>
/// <param name="Notes">Free text that only changes the size of the default fingerprint input.</param>
/// <param name="IdempotencyFingerprint">A supplied fingerprint, or <see langword="null"/> for the SHA-256 of the request as JSON.</param>
public sealed record PlaceOrder(string IdempotencyKey, string Sku, int Quantity, string Notes, string? IdempotencyFingerprint)
    : global::Mediator.ICommand<Result<int>>, global::MediatR.IRequest<Result<int>>, IIdempotentRequest
{
    CSharpEssentials.Idempotency.IdempotencyScope? IIdempotentRequest.IdempotencyScope => CSharpEssentials.Idempotency.IdempotencyScope.Global;
}
