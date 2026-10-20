using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.Idempotency;

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// Runs every call on a new <see cref="EfCoreIdempotencyRecordStore{TContext}"/> over a new context, optionally inside
/// a transaction that commits after the call, so the contract kit's parallel calls behave like parallel requests.
/// </summary>
internal sealed class PerCallEfCoreIdempotencyStore(IServiceProvider services, TimeProvider timeProvider, bool plainInsert, bool inTransaction)
    : IIdempotencyRecordStore
{
    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
        RunAsync(store => store.TryReserveAsync(key, fingerprint, lease, cancellationToken));

    public ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention,
        CancellationToken cancellationToken = default) =>
        RunAsync(store => store.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken));

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
        RunAsync(store => store.ReleaseAsync(key, token, cancellationToken));

    private async ValueTask<TResult> RunAsync<TResult>(Func<EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>, ValueTask<TResult>> operation)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IdempotencyTestDbContext context = scope.ServiceProvider.GetRequiredService<IdempotencyTestDbContext>();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store =
            new(context, services.GetRequiredService<IServiceScopeFactory>(), timeProvider, plainInsert);
        if (!inTransaction)
            return await operation(store);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        TResult result = await operation(store);
        await transaction.CommitAsync();
        return result;
    }
}
