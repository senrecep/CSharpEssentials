using System.Runtime.CompilerServices;
using System.Transactions;

using CSharpEssentials.Idempotency;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// <see cref="EfCoreIdempotencyRecordStore{TContext}.Detached"/>: every call resolves a new <typeparamref name="TContext"/>
/// in a scope of its own and runs on it with the ambient transaction suppressed, so it commits on its own connection
/// and neither joins nor waits for the business transaction.
/// </summary>
internal sealed class DetachedEfCoreIdempotencyRecordStore<TContext>(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    bool plainInsert) : IIdempotencyRecordStore
    where TContext : DbContext
{
    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
        => RunAsync(store => store.TryReserveAsync(key, fingerprint, lease, cancellationToken));

    public ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
        => RunAsync(store => store.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken));

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
        => RunAsync(store => store.ReleaseAsync(key, token, cancellationToken));

    private async ValueTask<TResult> RunAsync<TResult>(Func<EfCoreIdempotencyRecordStore<TContext>, ValueTask<TResult>> operation)
    {
        using TransactionScope suppress = new(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        await using ConfiguredAsyncDisposable disposal = scope.ConfigureAwait(false);
        EfCoreIdempotencyRecordStore<TContext> store = new(scope.ServiceProvider.GetRequiredService<TContext>(), scopeFactory, timeProvider, plainInsert);
        return await operation(store).ConfigureAwait(false);
    }
}
