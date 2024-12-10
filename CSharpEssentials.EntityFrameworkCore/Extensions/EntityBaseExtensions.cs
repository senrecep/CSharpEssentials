using System.Linq.Expressions;
using CSharpEssentials.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;

namespace CSharpEssentials.EntityFrameworkCore;

public static class EntityBaseExtensions
{
    public static void EntityBaseGuidIdMap<TEntity>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
    where TEntity : class, IEntityBase<Guid> => builder.EntityBaseMap<TEntity, Guid>(userIdMaxLength);

    public static void SoftDeletableEntityBaseGuidIdMap<TEntity>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
        where TEntity : class, ISoftDeletableEntityBase<Guid> => builder.SoftDeletableEntityBaseMap<TEntity, Guid>(userIdMaxLength);

    public static void EntityBaseMap<TEntity>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
         where TEntity : class, IEntityBase
    {
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(userIdMaxLength).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.UpdatedBy).HasMaxLength(userIdMaxLength).IsRequired(false);
    }

    public static void SoftDeletableEntityBaseMap<TEntity>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
    where TEntity : class, ISoftDeletableEntityBase
    {
        builder.Property(x => x.DeletedAt).IsRequired(false);
        builder.Property(x => x.DeletedBy).HasMaxLength(userIdMaxLength).IsRequired(false);
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Ignore(x => x.IsHardDeleted);
        builder.EntityBaseMap();
    }

    public static void EntityBaseMap<TEntity, TId>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
        where TEntity : class, IEntityBase<TId>
        where TId : IEquatable<TId>
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.EntityBaseMap(userIdMaxLength);
    }

    public static void SoftDeletableEntityBaseMap<TEntity, TId>(this EntityTypeBuilder<TEntity> builder, int userIdMaxLength = 40)
        where TEntity : class, ISoftDeletableEntityBase<TId>
        where TId : IEquatable<TId>
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.SoftDeletableEntityBaseMap(userIdMaxLength);
    }

    public static void OptimisticConcurrencyVersionMap<TEntity>(this EntityTypeBuilder<TEntity> builder, string propertyName = "RowVersion")
        where TEntity : class
    {
        builder
            .Property<byte[]>(propertyName)
            .IsRowVersion();
    }

    public static void AddQueryFilter<T>(this EntityTypeBuilder entityTypeBuilder, Expression<Func<T, bool>> expression)
    {
        ParameterExpression parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
        Expression expressionFilter = ReplacingExpressionVisitor.Replace(
            expression.Parameters.Single(), parameterType, expression.Body);
        LambdaExpression? currentQueryFilter = entityTypeBuilder.Metadata.GetQueryFilter();
        if (currentQueryFilter != null)
        {
            Expression currentExpressionFilter = ReplacingExpressionVisitor.Replace(
                currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
            expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
        }
        LambdaExpression lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
        entityTypeBuilder.HasQueryFilter(lambdaExpression);
    }

    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        Type entityBaseType = typeof(ISoftDeletable);
        IMutableEntityType[] entities = [.. modelBuilder.Model
            .GetEntityTypes()
            .Where(entityType => entityBaseType.IsAssignableFrom(entityType.ClrType))
            .Where(x => x.BaseType is null)];

        foreach (IMutableEntityType? entityType in entities)
            modelBuilder
            .Entity(entityType.ClrType)
            .AddQueryFilter<ISoftDeletable>(e => !e.IsDeleted);
    }
}