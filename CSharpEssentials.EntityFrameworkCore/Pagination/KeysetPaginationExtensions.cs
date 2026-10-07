using System.Linq.Expressions;

using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Pagination;

/// <summary>
/// Keyset (cursor) pagination over <see cref="IQueryable{T}"/> by a composite key, with opaque cursors and
/// <see cref="Result{TValue}"/> errors for bad requests.
/// </summary>
public static class KeysetPaginationExtensions
{
    /// <summary>
    /// Keyset (cursor) paginates <paramref name="query"/> by the keys configured in <paramref name="keys"/>, for example
    /// <c>k =&gt; k.Descending(x =&gt; x.CreatedAt).Descending(x =&gt; x.Id)</c>. The keys replace any ordering already
    /// applied to the query. One query with <c>limit + 1</c> rows is sent and no COUNT. An invalid, tampered or mismatched
    /// cursor, or both <c>After</c> and <c>Before</c>, gives a <see cref="ErrorType.Validation"/> failure.
    /// </summary>
    public static Task<Result<KeysetPaginationResponse<T>>> KeysetPaginateAsync<T>(
        this IQueryable<T> query,
        IKeysetPaginationRequest request,
        Func<KeysetOrdering<T>, KeysetOrdering<T>> keys,
        CancellationToken cancellationToken = default) =>
        query.KeysetPaginateAsync(request, keys, KeysetPaginationOptions.Default, cancellationToken);

    /// <inheritdoc cref="KeysetPaginateAsync{T}(IQueryable{T}, IKeysetPaginationRequest, Func{KeysetOrdering{T}, KeysetOrdering{T}}, CancellationToken)"/>
    public static Task<Result<KeysetPaginationResponse<T>>> KeysetPaginateAsync<T>(
        this IQueryable<T> query,
        IKeysetPaginationRequest request,
        Func<KeysetOrdering<T>, KeysetOrdering<T>> keys,
        KeysetPaginationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);
        return query.KeysetPaginateAsync(request, keys(new KeysetOrdering<T>()), options, cancellationToken);
    }

    /// <inheritdoc cref="KeysetPaginateAsync{T}(IQueryable{T}, IKeysetPaginationRequest, Func{KeysetOrdering{T}, KeysetOrdering{T}}, CancellationToken)"/>
    public static Task<Result<KeysetPaginationResponse<T>>> KeysetPaginateAsync<T>(
        this IQueryable<T> query,
        IKeysetPaginationRequest request,
        KeysetOrdering<T> ordering,
        CancellationToken cancellationToken = default) =>
        query.KeysetPaginateAsync(request, ordering, KeysetPaginationOptions.Default, cancellationToken);

    /// <inheritdoc cref="KeysetPaginateAsync{T}(IQueryable{T}, IKeysetPaginationRequest, Func{KeysetOrdering{T}, KeysetOrdering{T}}, CancellationToken)"/>
    public static async Task<Result<KeysetPaginationResponse<T>>> KeysetPaginateAsync<T>(
        this IQueryable<T> query,
        IKeysetPaginationRequest request,
        KeysetOrdering<T> ordering,
        KeysetPaginationOptions options,
        CancellationToken cancellationToken = default)
    {
        Result<IQueryable<T>> page = CreatePageQuery(query, request, ordering, options);
        if (page.IsFailure)
            return page.Errors;

        bool hasAfter = !string.IsNullOrEmpty(request.After);
        bool hasBefore = !string.IsNullOrEmpty(request.Before);
        int limit = Limit(request, options);

        List<T> items = await page.Value.ToListAsync(cancellationToken);
        bool hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(limit);

        if (items.Count == 0)
            return new KeysetPaginationResponse<T>(items, null, null);

        if (hasBefore)
            items.Reverse();

        bool hasNext = hasBefore || hasMore;
        bool hasPrevious = hasBefore ? hasMore : hasAfter;

        return new KeysetPaginationResponse<T>(
            items,
            hasNext ? CreateCursor(ordering, items[^1], KeysetCursorDirection.After, options.Protector) : null,
            hasPrevious ? CreateCursor(ordering, items[0], KeysetCursorDirection.Before, options.Protector) : null);
    }

    /// <summary>Builds the filtered, ordered query that fetches <c>limit + 1</c> rows, without running it.</summary>
    internal static Result<IQueryable<T>> CreatePageQuery<T>(
        IQueryable<T> query,
        IKeysetPaginationRequest request,
        KeysetOrdering<T> ordering,
        KeysetPaginationOptions options)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(options);
        if (ordering.Count == 0)
            throw new ArgumentException("The keyset ordering needs at least one key.", nameof(ordering));

        bool hasAfter = !string.IsNullOrEmpty(request.After);
        bool hasBefore = !string.IsNullOrEmpty(request.Before);
        if (hasAfter && hasBefore)
            return KeysetCursorErrors.AfterAndBefore();

        IQueryable<T> page = query;
        if (hasAfter || hasBefore)
        {
            KeysetCursorDirection direction = hasBefore ? KeysetCursorDirection.Before : KeysetCursorDirection.After;
            string cursor = hasBefore ? request.Before! : request.After!;
            if (cursor.Length > options.MaxCursorLength)
                return KeysetCursorErrors.Invalid(hasBefore ? "before" : "after");

            if (!KeysetCursorCodec.TryDecode(cursor, direction, ordering.Fingerprint, ordering.KeyTypes, options.Protector,
                    out object[] values, out Error error))
                return error;

            page = page.Where(BuildPredicate(ordering, values, reverse: hasBefore, options.PredicateBuilder));
        }

        return Result<IQueryable<T>>.Success(ApplyOrder(page, ordering, reverse: hasBefore).Take(Limit(request, options) + 1));
    }

    private static int Limit(IKeysetPaginationRequest request, KeysetPaginationOptions options) =>
        Math.Clamp(request.Limit, 1, options.MaxLimit);

    private static Expression<Func<T, bool>> BuildPredicate<T>(
        KeysetOrdering<T> ordering,
        object[] values,
        bool reverse,
        IKeysetPredicateBuilder builder)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        var columns = new KeysetColumn[ordering.Count];
        for (int i = 0; i < columns.Length; i++)
        {
            KeysetKey<T> key = ordering.Keys[i];
            columns[i] = new KeysetColumn(
                ParameterReplacer.Replace(key.Selector.Body, key.Selector.Parameters[0], parameter),
                key.CreateValue(values[i]),
                Effective(key.Direction, reverse));
        }

        Expression body = builder.BuildPredicate(columns);
        if (body.Type != typeof(bool))
            throw new InvalidOperationException(
                $"{builder.GetType().Name}.{nameof(IKeysetPredicateBuilder.BuildPredicate)} returned a '{body.Type.Name}' expression; a bool expression is required.");

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static IQueryable<T> ApplyOrder<T>(IQueryable<T> query, KeysetOrdering<T> ordering, bool reverse)
    {
        IQueryable<T> ordered = query;
        for (int i = 0; i < ordering.Count; i++)
        {
            KeysetKey<T> key = ordering.Keys[i];
            ordered = key.ApplyOrder(ordered, first: i == 0, Effective(key.Direction, reverse) == KeysetDirection.Descending);
        }

        return ordered;
    }

    private static KeysetDirection Effective(KeysetDirection direction, bool reverse) =>
        reverse == (direction == KeysetDirection.Ascending) ? KeysetDirection.Descending : KeysetDirection.Ascending;

    private static string CreateCursor<T>(KeysetOrdering<T> ordering, T item, KeysetCursorDirection direction, ICursorProtector protector)
    {
        object[] values = new object[ordering.Count];
        for (int i = 0; i < values.Length; i++)
            values[i] = ordering.Keys[i].GetValue(item);

        return KeysetCursorCodec.Encode(direction, ordering.Fingerprint, ordering.KeyTypes, values, protector);
    }
}
