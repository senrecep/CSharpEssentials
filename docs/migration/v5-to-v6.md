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
