# Redis idempotency store example

Reference `IIdempotencyRecordStore` on Redis. The
[External stores](../../CSharpEssentials.Idempotency/Readme.MD#external-stores) section of the Idempotency Readme
describes the algorithm and the store requirements; this project holds the code. It is not a package: copy
`RedisIdempotencyRecordStore.cs` into your application.

| File | Content |
|------|---------|
| `RedisIdempotencyRecordStore.cs` | The store: one Lua script per operation on a hash with `t` (token), `f` (fingerprint) and `p` (payload) |
| `RedisFixture.cs` | A `redis:7-alpine` container started through Testcontainers |
| `RedisIdempotencyRecordStoreTests.cs` | Every scenario of `IdempotencyRecordStoreContractTests` against the container |

## Running

Docker must be running.

```bash
dotnet test examples/Examples.Idempotency.Redis/Examples.Idempotency.Redis.csproj
```

CI runs the same command on every build.
