using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;

namespace Examples.Idempotency.Redis;

/// <summary>Runs every contract scenario against <see cref="RedisIdempotencyRecordStore"/> on a real Redis.</summary>
public sealed class RedisIdempotencyRecordStoreTests(RedisFixture redis)
    : IdempotencyRecordStoreContractTests, IClassFixture<RedisFixture>
{
    /// <summary>Redis expires keys on its own clock, so the scenarios wait instead of advancing a fake clock.</summary>
    protected override bool UseRealClock => true;

    /// <inheritdoc />
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new RedisIdempotencyRecordStore(redis.Connection);
}
