
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;

/// <summary>
/// Represents an entity base.
/// </summary>
/// <typeparam name="TId"></typeparam>
public abstract class EntityBase<TId> : EntityBase, IEntityBase<TId>
    where TId : IEquatable<TId>
{
    public TId? Id { get; protected set; }
}

