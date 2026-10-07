using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Pagination;

public static class Extensions
{
    public static async Task<PaginationResponse<T>> PaginateAsync<T>(
       this IQueryable<T> query,
       IPaginationRequest paginationRequest,
       Func<string, Expression<Func<T, bool>>>? search = null,
        bool includeTotalCount = true,
       CancellationToken cancellationToken = default)
    {
        paginationRequest.Normalize();

        if (search.IsNotNull() && paginationRequest.Search.IsNotEmpty())
            query = query
                .Where(search(paginationRequest.Search));

        int count = includeTotalCount ? await query
            .CountAsync(cancellationToken)
            : -1;

        IReadOnlyList<T> data = await query
            .Skip(paginationRequest.SkipCount())
            .Take(paginationRequest.PageSize)
            .ToListAsync(cancellationToken);


        return new PaginationResponse<T>(data, paginationRequest.PageNumber, paginationRequest.PageSize, count);
    }

    /// <summary>
    /// Offset-paginates <paramref name="query"/> using a page number and page size.
    /// Values below 1 are normalized to 1. Apply an ordering before paging for stable results.
    /// </summary>
    public static Task<PaginationResponse<T>> PaginateAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        bool includeTotalCount = true,
        CancellationToken cancellationToken = default) =>
        query.PaginateAsync(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
            search: null,
            includeTotalCount,
            cancellationToken);

    /// <summary>
    /// Synchronous counterpart of
    /// <see cref="PaginateAsync{T}(IQueryable{T}, IPaginationRequest, Func{string, Expression{Func{T, bool}}}?, bool, CancellationToken)"/>.
    /// Also works on non-EF queryables (e.g. <c>list.AsQueryable()</c>).
    /// </summary>
    public static PaginationResponse<T> Paginate<T>(
        this IQueryable<T> query,
        IPaginationRequest paginationRequest,
        Func<string, Expression<Func<T, bool>>>? search = null,
        bool includeTotalCount = true)
    {
        paginationRequest.Normalize();

        if (search.IsNotNull() && paginationRequest.Search.IsNotEmpty())
            query = query
                .Where(search(paginationRequest.Search));

        int count = includeTotalCount ? query.Count() : -1;

        IReadOnlyList<T> data = [.. query
            .Skip(paginationRequest.SkipCount())
            .Take(paginationRequest.PageSize)];

        return new PaginationResponse<T>(data, paginationRequest.PageNumber, paginationRequest.PageSize, count);
    }

    /// <summary>
    /// Synchronous counterpart of <see cref="PaginateAsync{T}(IQueryable{T}, int, int, bool, CancellationToken)"/>.
    /// </summary>
    public static PaginationResponse<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        bool includeTotalCount = true) =>
        query.Paginate(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
            search: null,
            includeTotalCount);

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _cursorSelectorCache = new(
        Environment.ProcessorCount * 2,
        31
    );

    private static TCursor GetCursor<T, TCursor>(
        this T data,
        Expression<Func<T, TCursor>> cursorSelector)
    {
        return Unsafe.As<Func<T, TCursor>>(
            _cursorSelectorCache.GetOrAdd(
                cursorSelector,
                static key => key.Compile(preferInterpretation: false)
            )
        )(data);
    }

    /// <summary>
    /// Single-column cursor pagination: returns the rows whose <paramref name="cursorSelector"/> value is after
    /// <see cref="ICursorPaginationRequest{TCursor}.Cursor"/>. The cursor value is sent as a SQL parameter.
    /// <para>
    /// The cursor column must be unique. <paramref name="thenBy"/> only changes ORDER BY, not the filter, so with a
    /// non-unique column (such as a timestamp) a page that ends inside a group of equal values skips the rest of that
    /// group. <see cref="ICursorPaginationRequest{TCursor}.Normalize"/> only raises <c>Limit</c> to 1 and does not cap it;
    /// validate the upper bound yourself. Prefer
    /// <see cref="KeysetPaginationExtensions.KeysetPaginateAsync{T}(IQueryable{T}, IKeysetPaginationRequest, Func{Keyset.KeysetOrdering{T}, Keyset.KeysetOrdering{T}}, CancellationToken)"/>,
    /// which supports composite keys, opaque cursors, backward paging and a maximum limit.
    /// </para>
    /// </summary>
    public static async Task<CursorPaginationResponse<T, TCursor>> PaginateAsync<T, TCursor>(
        this IQueryable<T> query,
        ICursorPaginationRequest<TCursor> request,
        Expression<Func<T, TCursor>> cursorSelector,
        bool isAscending = true,
        Func<string, Expression<Func<T, bool>>>? search = null,
        Func<IOrderedQueryable<T>, IOrderedQueryable<T>>? thenBy = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        request.Normalize();
        IQueryable<T> q = query;

        if (request.Cursor.IsNotNull() &&
            !EqualityComparer<TCursor>.Default.Equals(request.Cursor, default))
        {
            ParameterExpression parameter = cursorSelector.Parameters[0];
            Expression cursorParameter = Expression.Property(
                Expression.Constant(new KeysetParameter<TCursor>(request.Cursor)),
                nameof(KeysetParameter<>.Value));
            MethodInfo compareMethod = typeof(TCursor).GetMethod(nameof(IComparable<>.CompareTo), [typeof(TCursor)])!;
            Expression compareCall = Expression.Call(cursorSelector.Body, compareMethod, cursorParameter);
            Expression comparison = isAscending
                ? Expression.GreaterThan(compareCall, Expression.Constant(0))
                : Expression.LessThan(compareCall, Expression.Constant(0));
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            q = q.Where(lambda);
        }

        q = search.IsNotNull() && request.Search.IsNotEmpty() ? q.Where(search(request.Search)) : q;

        IOrderedQueryable<T> cursorOrdered = isAscending ? q.OrderBy(cursorSelector) : q.OrderByDescending(cursorSelector);
        q = thenBy is not null ? thenBy(cursorOrdered) : cursorOrdered;

        List<T> items = await q.Take(request.Limit + 1).ToListAsync(cancellationToken);

        bool hasMore = items.Count > request.Limit;
        if (hasMore.IsTrue())
            items.RemoveAt(items.Count - 1);

        if (!hasMore || items.Count == 0)
        {
            return new CursorPaginationResponse<T, TCursor>(items, default, hasMore);
        }

        TCursor? nextCursor = items[^1].GetCursor(cursorSelector);

        return new CursorPaginationResponse<T, TCursor>(items, nextCursor, hasMore);
    }

}
