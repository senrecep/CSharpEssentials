namespace CSharpEssentials.Entity.Interfaces;

public interface IDeletableEntityBase : IEntityBase, ISoftDeletable { }
public interface IDeletableEntityBase<TId> : IEntityBase<TId>, IDeletableEntityBase
    where TId : IEquatable<TId>
{

}
