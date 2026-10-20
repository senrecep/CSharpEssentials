using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>The contract kit on PostgreSQL, every call committing on its own (<c>INSERT … ON CONFLICT DO NOTHING</c>).</summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresContractTests(IdempotencyPostgresFixture postgres)
    : IdempotencyRecordStoreContractTests, IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new PerCallEfCoreIdempotencyStore(_services, timeProvider, plainInsert: false, inTransaction: false);

    public void Dispose() => _services.Dispose();
}

/// <summary>The contract kit on PostgreSQL, every call inside a transaction that commits after it.</summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresTransactionContractTests(IdempotencyPostgresFixture postgres)
    : IdempotencyRecordStoreContractTests, IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new PerCallEfCoreIdempotencyStore(_services, timeProvider, plainInsert: false, inTransaction: true);

    public void Dispose() => _services.Dispose();
}

/// <summary>The contract kit on PostgreSQL through the plain <c>INSERT</c> whose unique violation is caught, committing on its own.</summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresPlainInsertContractTests(IdempotencyPostgresFixture postgres)
    : IdempotencyRecordStoreContractTests, IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new PerCallEfCoreIdempotencyStore(_services, timeProvider, plainInsert: true, inTransaction: false);

    public void Dispose() => _services.Dispose();
}

/// <summary>
/// The contract kit on PostgreSQL through the plain <c>INSERT</c> inside a transaction: the unique violation is rolled
/// back to a savepoint, which keeps the transaction usable.
/// </summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresSavepointContractTests(IdempotencyPostgresFixture postgres)
    : IdempotencyRecordStoreContractTests, IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new PerCallEfCoreIdempotencyStore(_services, timeProvider, plainInsert: true, inTransaction: true);

    public void Dispose() => _services.Dispose();
}

/// <summary>The contract kit on PostgreSQL through <see cref="EfCoreIdempotencyRecordStore{TContext}.Detached"/>.</summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresDetachedContractTests(IdempotencyPostgresFixture postgres)
    : IdempotencyRecordStoreContractTests, IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();
    private readonly IdempotencyTestDbContext _unusedContext = postgres.CreateContext();

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(
            _unusedContext, _services.GetRequiredService<IServiceScopeFactory>(), timeProvider).Detached;

    public void Dispose()
    {
        _unusedContext.Dispose();
        _services.Dispose();
    }
}
