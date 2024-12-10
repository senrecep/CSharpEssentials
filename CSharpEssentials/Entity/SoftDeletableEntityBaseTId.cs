using CSharpEssentials.Interfaces;

namespace CSharpEssentials.Entity;
/// <summary>
/// Represents a soft deletable entity base.
/// </summary>
/// <typeparam name="TId"></typeparam>
public abstract class SoftDeletableEntityBase<TId> : SoftDeletableEntityBase, ISoftDeletableEntityBase<TId>
    where TId : IEquatable<TId>
{
    public TId? Id { get; protected set; }
}