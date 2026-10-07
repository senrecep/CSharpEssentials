# Migrating from 5.x to 6.0

## Pagination (`CSharpEssentials.EntityFrameworkCore`)

### Single-column cursor `PaginateAsync` is obsolete

`PaginateAsync<T, TCursor>(IQueryable<T>, ICursorPaginationRequest<TCursor>, Expression<Func<T, TCursor>>, ...)` is marked `[Obsolete]`. It still works, but every call now raises warning CS0618, which fails the build under `TreatWarningsAsErrors`. It will be removed in a later major version.

It filters by one column only, so a page that ends inside a group of equal values (such as a timestamp) skips the rest of that group, and it does not cap `Limit`. Move to `KeysetPaginateAsync` and add a unique tie-breaker to the key:

```csharp
// 5.x
CursorPaginationResponse<Log, DateTime> response = await db.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTime> { Limit = 20, Cursor = lastCreatedAt },
    x => x.CreatedAt,
    isAscending: false);
// response.Items, response.Next, response.HasMore

// 6.0
Result<KeysetPaginationResponse<Log>> page = await db.Logs.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    k => k.Descending(x => x.CreatedAt).Descending(x => x.Id),
    ct);
// page.Value.Items, page.Value.NextCursor, page.Value.HasNext, PreviousCursor/HasPrevious for backward paging
```

| 5.x | 6.0 |
|---|---|
| `CursorPaginationRequest<TCursor>.Cursor` (raw value) | `KeysetPaginationRequest.After` / `Before` (opaque string from `NextCursor` / `PreviousCursor`) |
| `Limit` (no upper bound) | `Limit`, clamped to `KeysetPaginationOptions.MaxLimit` (default 100) |
| `isAscending` and `thenBy` | `KeysetOrdering<T>.Ascending(...)` / `.Descending(...)` per key column |
| `search` delegate | apply `.Where(...)` to the query before `KeysetPaginateAsync` |
| `CursorPaginationResponse<T, TCursor>.Next` / `HasMore` | `KeysetPaginationResponse<T>.NextCursor` / `HasNext` |

`CursorPaginationRequest<TCursor>`, `ICursorPaginationRequest<TCursor>` and `CursorPaginationResponse<T, TCursor>` are not marked `[Obsolete]`. They are plain DTOs that may appear in public API contracts, client models and serialized payloads, so obsoleting them would break builds that never call the obsolete method. They stay for DTO compatibility and are slated for removal, together with the obsolete overload, in a future major version.

Cursors issued by `PaginateAsync` are raw values and are not accepted by `KeysetPaginateAsync`; clients restart from the first page once. If you cannot migrate yet, keep the call inside a member or type marked `[Obsolete]`, which does not report CS0618 for its body.

### Offset page size is capped at 100

In 5.x, `IPaginationRequest.Normalize()` only raised `PageNumber` and `PageSize` to 1, so a client could ask for `pageSize=100000` and get it. In 6.0 it also lowers `PageSize` to `PaginationDefaults.MaxPageSize` (100). The offset `PaginateAsync` and `Paginate` overloads (request and `pageNumber`/`pageSize` variants) take a new optional `maxPageSize` parameter, default `PaginationDefaults.MaxPageSize`, and pass it to `Normalize(int maxPageSize)`.

```csharp
// 6.0 default: at most 100 rows
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, cancellationToken: ct);

// A larger cap for one endpoint
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, maxPageSize: 500, cancellationToken: ct);

// 5.x behavior: no cap
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, maxPageSize: int.MaxValue, cancellationToken: ct);
```

`PaginateAsync` and `Paginate` normalize the request themselves, so pass `maxPageSize` to them; calling `Normalize` before them does not change their cap. Call `Normalize(int.MaxValue)` only when you normalize a request yourself without `PaginateAsync` or `Paginate`:

```csharp
// Only when you page the query yourself, without PaginateAsync/Paginate
IPaginationRequest manual = request;
manual.Normalize(int.MaxValue);
List<User> rows = await db.Users.OrderBy(u => u.Id).Skip(manual.SkipCount()).Take(manual.PageSize).ToListAsync(ct);
```

A `maxPageSize` below 1 throws `ArgumentOutOfRangeException`.

Compatibility notes:

- `maxPageSize` sits before `CancellationToken`, so `cancellationToken` stays last. Calls that pass the token by name keep compiling. The 5.x async signatures `PaginateAsync(query, request, search, includeTotalCount, cancellationToken)` and `PaginateAsync(query, pageNumber, pageSize, includeTotalCount, cancellationToken)` remain as hidden overloads, so positional calls and assemblies compiled against 5.x still work; they apply the default cap.
- The synchronous `Paginate` overloads gained a trailing parameter. Source code compiles unchanged, but assemblies compiled against 5.x must be recompiled.
- `Normalize()` now calls `Normalize(PaginationDefaults.MaxPageSize)`, and `Normalize(int)` no longer calls `Normalize()`. A custom `IPaginationRequest` that overrides `Normalize()` to add its own rules must also override `Normalize(int maxPageSize)`, because `PaginateAsync` and `Paginate` call the latter. The 5.x direction (`Normalize(int)` calling `Normalize()`) cannot be kept: `Normalize()` now applies the default cap of 100, so it would lower every larger `maxPageSize`, including `int.MaxValue`. `ICursorPaginationRequest<TCursor>` keeps the 5.x direction because its `Normalize()` does not cap.
- `ICursorPaginationRequest<TCursor>.Normalize()` is unchanged and still does not cap `Limit`. Its only consumer is the obsolete single-column cursor `PaginateAsync`; the replacement, `KeysetPaginateAsync`, already clamps `Limit` to `KeysetPaginationOptions.MaxLimit` (default 100). Changing the obsolete path would break callers who are about to move off it anyway.
