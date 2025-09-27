using System.Linq.Expressions;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
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
        ArgumentNullException.ThrowIfNull(entity);
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
        ArgumentNullException.ThrowIfNull(entity);
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

    public static Task MigrateDataAsync<TEntity, TSeedData, TKey>(
        this DbContext dbContext,
        IEnumerable<TSeedData> data,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Func<IQueryable<TEntity>, IEnumerable<TSeedData>, bool> preConditionFunc,
        Expression<Func<TEntity, TKey>> entityKeyProperty,
        Expression<Func<TSeedData, TKey>> dataKeyProperty,
        Func<TEntity, TSeedData, bool> isUpdatedFunc,
        Func<TEntity, TSeedData, TEntity> updateFunc,
        Func<TSeedData, TEntity> converter,
        bool hardDeleteMode = false,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TSeedData : class
        where TKey : IEquatable<TKey>
    {
        var dataList = data.ToList();
        DbSet<TEntity> dbSet = dbContext.Set<TEntity>();
        if (preConditionFunc(dbSet, dataList))
            return Task.CompletedTask;

        return dbContext.MigrateDataAsync(
            dataList,
            query,
            entityKeyProperty,
            dataKeyProperty,
            isUpdatedFunc,
            updateFunc,
            converter,
            hardDeleteMode,
            cancellationToken);

    }

    public static async Task MigrateDataAsync<TEntity, TSeedData, TKey>(
        this DbContext dbContext,
        IEnumerable<TSeedData> data,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<TEntity, TKey>> entityKeyProperty,
        Expression<Func<TSeedData, TKey>> dataKeyProperty,
        Func<TEntity, TSeedData, bool> isUpdatedFunc,
        Func<TEntity, TSeedData, TEntity> updateFunc,
        Func<TSeedData, TEntity> converter,
        bool hardDeleteMode = false,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TSeedData : class
        where TKey : IEquatable<TKey>
    {
        var dataList = data.ToList();
        Func<TEntity, TKey> entityKeySelector = entityKeyProperty.Compile();
        Func<TSeedData, TKey> dataKeySelector = dataKeyProperty.Compile();
        DbSet<TEntity> dbSet = dbContext.Set<TEntity>();
        List<TEntity> entities = await
            query(dbSet)
            .ToListAsync(cancellationToken);

        TEntity[] theyWillBeDeleted = [.. entities
            .Where(entity => dataList
                .All(item => !dataKeySelector(item).Equals(entityKeySelector(entity))))];

        if (hardDeleteMode && theyWillBeDeleted.Length != 0)
            theyWillBeDeleted.OfType<ISoftDeletable>().HardDelete();


        TEntity[] theyWillBeUpdated = [.. entities
            .Join(dataList, entityKeySelector, dataKeySelector, (entity, item) => new { entity, item })
            .Where(obj => isUpdatedFunc(obj.entity, obj.item))
            .Select(obj => updateFunc(obj.entity, obj.item))];

        TEntity[] theyWillBeAdded = [.. dataList
            .Where(item => entities
                .All(entity => !entityKeySelector(entity).Equals(dataKeySelector(item))))
            .Select(converter)];

        if (theyWillBeDeleted.Length != 0)
            dbSet.RemoveRange(theyWillBeDeleted);

        if (theyWillBeUpdated.Length != 0)
            dbSet.UpdateRange(theyWillBeUpdated);

        if (theyWillBeAdded.Length != 0)
            dbSet.AddRange(theyWillBeAdded);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}