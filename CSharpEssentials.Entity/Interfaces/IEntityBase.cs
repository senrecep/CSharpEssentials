namespace CSharpEssentials.Entity.Interfaces;

public interface IEntityBase : ICreationAudit, IModificationAudit, IDomainEventHolder;
public interface ISoftDeletableEntityBase : IEntityBase, ISoftDeletable;

public interface IEntityBase<TId> : IEntityBase
    where TId : IEquatable<TId>
{
    TId? Id { get; }
}

public interface ISoftDeletableEntityBase<TId> : IEntityBase<TId>, ISoftDeletableEntityBase
    where TId : IEquatable<TId>
{
}