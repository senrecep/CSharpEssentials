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
((IPaginationRequest)request).Normalize(int.MaxValue);
```

A `maxPageSize` below 1 throws `ArgumentOutOfRangeException`.

Compatibility notes:

- `maxPageSize` sits before `CancellationToken`, so `cancellationToken` stays last. Calls that pass the token by name keep compiling. The 5.x async signatures `PaginateAsync(query, request, search, includeTotalCount, cancellationToken)` and `PaginateAsync(query, pageNumber, pageSize, includeTotalCount, cancellationToken)` remain as hidden overloads, so positional calls and assemblies compiled against 5.x still work; they apply the default cap.
- The synchronous `Paginate` overloads gained a trailing parameter. Source code compiles unchanged, but assemblies compiled against 5.x must be recompiled.
- `Normalize()` now calls `Normalize(PaginationDefaults.MaxPageSize)`, and `Normalize(int)` no longer calls `Normalize()`. A custom `IPaginationRequest` that overrides `Normalize()` to add its own rules must also override `Normalize(int maxPageSize)`, because `PaginateAsync` calls the latter.
- `ICursorPaginationRequest<TCursor>.Normalize()` is unchanged and still does not cap `Limit`. Its only consumer is the obsolete single-column cursor `PaginateAsync`; the replacement, `KeysetPaginateAsync`, already clamps `Limit` to `KeysetPaginationOptions.MaxLimit` (default 100). Changing the obsolete path would break callers who are about to move off it anyway.
