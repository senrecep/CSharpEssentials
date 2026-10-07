#if NET10_0_OR_GREATER
using System.Linq.Expressions;
using System.Reflection;
using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Helpers for EF Core 10 named query filters. The soft-delete filter is registered under <see cref="QueryFilterNames.SoftDelete"/>,
/// so a query can ignore it with <see cref="IgnoreSoftDeleteQueryFilter{TEntity}"/> and keep every other named filter.
/// </summary>
/// <remarks>
/// EF Core does not allow an anonymous filter (<c>HasQueryFilter(filter)</c>, <see cref="EntityBaseExtensions.AddQueryFilter{T}"/>
/// or <see cref="EntityBaseExtensions.ApplySoftDeleteQueryFilter"/>) and named filters on the same entity type. Use these helpers
/// instead of <see cref="EntityBaseExtensions.ApplySoftDeleteQueryFilter"/>, not next to it; they throw early when the entity type
/// already has an anonymous filter. Named keys passed to <c>IgnoreQueryFilters(keys)</c> do not affect anonymous filters, so moving a
/// query to <see cref="IgnoreSoftDeleteQueryFilter{TEntity}"/> also requires moving the model to the named soft-delete filter.
/// </remarks>
public static class NamedQueryFilterExtensions
{
    // A cached array: EF Core 10.0.x compiles the query again on every execution when the keys come from a collection expression.
    private static readonly string[] SoftDeleteFilterKeys = [QueryFilterNames.SoftDelete];

    private static readonly PropertyInfo IsDeletedProperty =
        typeof(ISoftDeletableBase).GetProperty(nameof(ISoftDeletableBase.IsDeleted), BindingFlags.Public | BindingFlags.Instance)!;

    /// <summary>
    /// Registers the named soft-delete filter (<c>!IsDeleted</c>) under <see cref="QueryFilterNames.SoftDelete"/> for
    /// <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The soft-deletable entity type; it must be the root of its hierarchy.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="InvalidOperationException">The entity type already has an anonymous query filter.</exception>
    public static EntityTypeBuilder<TEntity> HasSoftDeleteQueryFilter<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDeletableBase
    {
        ArgumentNullException.ThrowIfNull(builder);

        AddSoftDeleteQueryFilter(builder.Metadata);
        return builder;
    }

    /// <summary>
    /// Registers the named soft-delete filter (<c>!IsDeleted</c>) under <see cref="QueryFilterNames.SoftDelete"/> on every root
    /// entity type in the model that implements <see cref="ISoftDeletableBase"/>. Derived types inherit the filter from their root.
    /// Owned types are skipped, because EF Core does not allow query filters on them. Call it at the end of <c>OnModelCreating</c>,
    /// after the entity types are added to the model.
    /// </summary>
    /// <remarks>
    /// EF Core puts query filters only on root entity types, so a soft-deletable type derived from a root that is not soft-deletable
    /// gets no filter. Make the root implement <see cref="ISoftDeletableBase"/>, or filter such queries yourself.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same model builder, for chaining.</returns>
    /// <exception cref="InvalidOperationException">A soft-deletable root entity type already has an anonymous query filter.</exception>
    public static ModelBuilder ApplyNamedSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        IMutableEntityType[] entityTypes = [.. modelBuilder.Model
            .GetEntityTypes()
            .Where(static entityType =>
                entityType.BaseType is null &&
                !entityType.IsOwned() &&
                typeof(ISoftDeletableBase).IsAssignableFrom(entityType.ClrType))];

        foreach (IMutableEntityType entityType in entityTypes)
            AddSoftDeleteQueryFilter(entityType);

        return modelBuilder;
    }

    /// <summary>
    /// Ignores only the soft-delete filter (<see cref="QueryFilterNames.SoftDelete"/>) for this query; other named filters, such as
    /// <see cref="QueryFilterNames.Tenant"/>, still apply.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <returns>A query that includes soft-deleted rows.</returns>
    public static IQueryable<TEntity> IgnoreSoftDeleteQueryFilter<TEntity>(this IQueryable<TEntity> source)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.IgnoreQueryFilters(SoftDeleteFilterKeys);
    }

    private static void AddSoftDeleteQueryFilter(IMutableEntityType entityType)
    {
        if (entityType.GetDeclaredQueryFilters().Any(static filter => filter.IsAnonymous))
        {
            throw new InvalidOperationException(
                $"The entity type '{entityType.DisplayName()}' already has an anonymous query filter, so the named " +
                $"'{QueryFilterNames.SoftDelete}' filter cannot be added. Register every filter of this entity type by name, for " +
                "example with HasQueryFilter(name, filter), instead of ApplySoftDeleteQueryFilter() or HasQueryFilter(filter).");
        }

        ParameterExpression entity = Expression.Parameter(entityType.ClrType, "entity");
        LambdaExpression filter = Expression.Lambda(Expression.Not(Expression.Property(entity, IsDeletedProperty)), entity);
        entityType.SetQueryFilter(QueryFilterNames.SoftDelete, filter);
    }
}
#endif
