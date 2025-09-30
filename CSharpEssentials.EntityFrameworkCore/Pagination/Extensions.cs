using System.Linq.Expressions;
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
       CancellationToken cancellationToken = default)
    {
        paginationRequest.Normalize();

        if (search.IsNotNull() && paginationRequest.Search.IsNotEmpty())
            query = query
                .Where(search(paginationRequest.Search));

        int count = await query
            .CountAsync(cancellationToken);

        T[] data = await query
            .Skip(paginationRequest.SkipCount())
            .Take(paginationRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PaginationResponse<T>(data, paginationRequest.PageNumber, data.Length, count);
    }

    public static async Task<CursorPaginationResponse<T, TCursor>> PaginateAsync<T, TCursor>(
        this IQueryable<T> query,
        ICursorPaginationRequest<TCursor> request,
        Expression<Func<T, TCursor>> cursorSelector,
        bool isAscending = true,
        Func<string, Expression<Func<T, bool>>>? search = null,
        Func<IOrderedQueryable<T>, IOrderedQueryable<T>>? order = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        request.Normalize();
        IQueryable<T> q = query;

        if (request.Cursor is not null)
        {
            ParameterExpression parameter = cursorSelector.Parameters[0];
            ConstantExpression cursorConstant = Expression.Constant(request.Cursor, typeof(TCursor));

            Expression comparison = isAscending
                ? Expression.GreaterThan(cursorSelector.Body, cursorConstant)
                : Expression.LessThan(cursorSelector.Body, cursorConstant);

            q = q.Where(Expression.Lambda<Func<T, bool>>(comparison, parameter));
        }

        q = search.IsNotNull() && request.Search.IsNotEmpty() ? q.Where(search(request.Search)) : q;

        IOrderedQueryable<T> cursorOrdered = isAscending ? q.OrderBy(cursorSelector) : q.OrderByDescending(cursorSelector);
        q = order is not null ? order(cursorOrdered) : cursorOrdered;

        List<T> items = await q.Take(request.Limit + 1).ToListAsync(cancellationToken);

        bool hasMore = items.Count > request.Limit;
        if (hasMore.IsTrue())
            items.RemoveAt(items.Count - 1);

        if (hasMore.IsFalse())
            return new CursorPaginationResponse<T, TCursor>(items);

        TCursor cursor = cursorSelector.Compile()(items[^1]);

        return new CursorPaginationResponse<T, TCursor>(items, cursor, hasMore);
    }

}
