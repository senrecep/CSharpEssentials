using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
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

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _cursorSelectorCache = new(
        Environment.ProcessorCount * 2,
        31
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            ConstantExpression cursorConstant = Expression.Constant(request.Cursor, typeof(TCursor));
            Func<Expression, Expression, BinaryExpression> makeComparison = isAscending
                ? Expression.GreaterThan
                : Expression.LessThan;
            Expression comparison = makeComparison(cursorSelector.Body, cursorConstant);
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
