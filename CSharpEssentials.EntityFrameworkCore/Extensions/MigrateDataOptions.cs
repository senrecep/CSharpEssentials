using System.Linq.Expressions;

namespace CSharpEssentials.EntityFrameworkCore;

public sealed class MigrateDataOptions<TEntity, TSeedData, TKey>
    where TEntity : class
    where TSeedData : class
    where TKey : IEquatable<TKey>
{
    public required Func<IQueryable<TEntity>, IQueryable<TEntity>> Query { get; init; }
    public required Expression<Func<TEntity, TKey>> EntityKeyProperty { get; init; }
    public required Expression<Func<TSeedData, TKey>> DataKeyProperty { get; init; }
    public required Func<TEntity, TSeedData, bool> IsUpdatedFunc { get; init; }
    public required Func<TEntity, TSeedData, TEntity> UpdateFunc { get; init; }
    public required Func<TSeedData, TEntity> Converter { get; init; }
    public bool HardDeleteMode { get; init; }
    public Func<IQueryable<TEntity>, IEnumerable<TSeedData>, bool>? PreConditionFunc { get; init; }
}
