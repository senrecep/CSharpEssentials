
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore;

public static class DbContextExtensionMethods
{
    public static IQueryable<TEntity> AsNoTracking<TEntity>(
        this IQueryable<TEntity> source, bool isDisabled)
        where TEntity : class =>
        isDisabled ? source : source.AsNoTracking();

    public static void HardDelete<TEntity>(this DbContext context, TEntity? entity)
        where TEntity : class, ISoftDeletableEntityBase
    {
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(entity);

#else

            if (entity is null)

                throw new ArgumentNullException(nameof(entity));

#endif
        entity.MarkAsHardDeleted();
        context.Remove(entity);
    }

    public static void HardDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities)
        where TEntity : class, ISoftDeletableEntityBase
    {
        TEntity[] records = entities as TEntity[] ?? [.. entities];
        records.HardDelete();
        context.Set<TEntity>().RemoveRange(records);
    }

    public static void HardDelete<TEntity>(this DbSet<TEntity> context, TEntity? entity)
    where TEntity : class, ISoftDeletableEntityBase
    {
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(entity);

#else

            if (entity is null)

                throw new ArgumentNullException(nameof(entity));

#endif
        entity.MarkAsHardDeleted();
        context.Remove(entity);
    }

    public static void Delete<TEntity>(this DbSet<TEntity> context, IEnumerable<TEntity> entities)
        where TEntity : class, ISoftDeletableEntityBase
    {
        TEntity[] entityBases = entities as TEntity[] ?? [.. entities];
        entityBases.HardDelete();
        context.RemoveRange(entityBases);
    }


    /// <summary>
    /// Soft-deletes every row matched by <paramref name="source"/> with a single <c>UPDATE</c> statement
    /// (<c>ExecuteUpdate</c>), setting <see cref="ISoftDeletableBase.IsDeleted"/>, <see cref="ISoftDeletable.DeletedAt"/>
    /// and <see cref="ISoftDeletable.DeletedBy"/>. Rows that are already soft-deleted are left untouched.
    /// <para>
    /// The statement bypasses the change tracker and <c>SaveChanges</c> interceptors (audit, domain events),
    /// and already tracked entities are not refreshed. Requires a relational provider.
    /// </para>
    /// </summary>
    /// <returns>The number of rows soft-deleted.</returns>
    public static Task<int> SoftDeleteAsync<TEntity>(
        this IQueryable<TEntity> source,
        DateTimeOffset deletedAt,
        string deletedBy,
        CancellationToken cancellationToken = default)
        where TEntity : class, ISoftDeletable
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(deletedBy);

        DateTimeOffset? deletedAtValue = deletedAt;
        return source
            .Where(entity => !EF.Property<bool>(entity, nameof(ISoftDeletableBase.IsDeleted)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => EF.Property<bool>(entity, nameof(ISoftDeletableBase.IsDeleted)), true)
                .SetProperty(entity => EF.Property<DateTimeOffset?>(entity, nameof(ISoftDeletable.DeletedAt)), deletedAtValue)
                .SetProperty(entity => EF.Property<string?>(entity, nameof(ISoftDeletable.DeletedBy)), deletedBy),
                cancellationToken);
    }

    /// <summary>
    /// Soft-deletes every row matched by <paramref name="source"/> with a single <c>UPDATE</c> statement,
    /// using <paramref name="timeProvider"/> (or <see cref="TimeProvider.System"/>) for the deletion time.
    /// See <see cref="SoftDeleteAsync{TEntity}(IQueryable{TEntity}, DateTimeOffset, string, CancellationToken)"/>.
    /// </summary>
    /// <returns>The number of rows soft-deleted.</returns>
    public static Task<int> SoftDeleteAsync<TEntity>(
        this IQueryable<TEntity> source,
        string deletedBy,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ISoftDeletable =>
        source.SoftDeleteAsync(
            (timeProvider ?? TimeProvider.System).GetUtcNow(), deletedBy, cancellationToken);

    public static async Task MigrateDataAsync<TEntity, TSeedData>(
        this DbContext dbContext,
        IEnumerable<TSeedData> data,
        Func<IQueryable<TEntity>, IEnumerable<TSeedData>, bool> preConditionFunc,
        Func<TSeedData, TEntity> converter,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TSeedData : class
    {
        var dataList = data.ToList();
        DbSet<TEntity> dbSet = dbContext.Set<TEntity>();
        if (preConditionFunc(dbSet, dataList))
            return;

        await dbSet.AddRangeAsync(dataList.Select(converter), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task MigrateDataAsync<TEntity, TSeedData, TKey>(
        this DbContext dbContext,
        IEnumerable<TSeedData> data,
        MigrateDataOptions<TEntity, TSeedData, TKey> options,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TSeedData : class
        where TKey : IEquatable<TKey>
    {
        var dataList = data.ToList();
        DbSet<TEntity> dbSet = dbContext.Set<TEntity>();
        if (options.PreConditionFunc is not null && options.PreConditionFunc(dbSet, dataList))
            return;

        Func<TEntity, TKey> entityKeySelector = options.EntityKeyProperty.Compile();
        Func<TSeedData, TKey> dataKeySelector = options.DataKeyProperty.Compile();
        List<TEntity> entities = await
            options.Query(dbSet)
            .ToListAsync(cancellationToken);

        TEntity[] theyWillBeDeleted = [.. entities
            .Where(entity => dataList
                .All(item => !dataKeySelector(item).Equals(entityKeySelector(entity))))];

        if (options.HardDeleteMode && theyWillBeDeleted.Length != 0)
            theyWillBeDeleted.OfType<ISoftDeletable>().HardDelete();

        TEntity[] theyWillBeUpdated = [.. entities
            .Join(dataList, entityKeySelector, dataKeySelector, (entity, item) => new { entity, item })
            .Where(obj => options.IsUpdatedFunc(obj.entity, obj.item))
            .Select(obj => options.UpdateFunc(obj.entity, obj.item))];

        TEntity[] theyWillBeAdded = [.. dataList
            .Where(item => entities
                .All(entity => !entityKeySelector(entity).Equals(dataKeySelector(item))))
            .Select(options.Converter)];

        if (theyWillBeDeleted.Length != 0)
            dbSet.RemoveRange(theyWillBeDeleted);

        if (theyWillBeUpdated.Length != 0)
            dbSet.UpdateRange(theyWillBeUpdated);

        if (theyWillBeAdded.Length != 0)
            dbSet.AddRange(theyWillBeAdded);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task<Result<T>> FirstOrDefaultAsResultAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        T? entity = await source.FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
            return notFoundError ?? Error.NotFound();
        return entity;
    }

    public static async Task<Result<T>> SingleOrDefaultAsResultAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        T? entity = await source.SingleOrDefaultAsync(cancellationToken);
        if (entity is null)
            return notFoundError ?? Error.NotFound();
        return entity;
    }

    public static async Task<Result<T>> FindAsResultAsync<T>(
        this DbSet<T> source,
        object?[]? keyValues,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        T? entity = await source.FindAsync(keyValues, cancellationToken);
        if (entity is null)
            return notFoundError ?? Error.NotFound();
        return entity;
    }

    public static Task<Result> SaveChangesAsResultAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            async () => { await context.SaveChangesAsync(cancellationToken); },
            ex => Error.Exception(ex, ErrorType.Unknown),
            cancellationToken);
    }

    public static Task<Result<int>> SaveChangesAsResultAsync(
        this DbContext context,
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync<int>(
            () => context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken),
            ex => Error.Exception(ex, ErrorType.Unknown),
            cancellationToken);
    }
}
