
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;
/// <summary>
/// Represents a soft deletable entity base.
/// </summary>
public abstract class SoftDeletableEntityBase : EntityBase, ISoftDeletableEntityBase
{
    public DateTimeOffset? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    public bool IsDeleted { get; private set; }

    public bool IsHardDeleted { get; private set; }

    public void MarkAsDeleted(DateTimeOffset deletedAt, string deletedBy)
    {
        DeletedAt = deletedAt;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }

    public void MarkAsHardDeleted() => IsHardDeleted = true;

    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
        IsDeleted = false;
        IsHardDeleted = false;
    }
}
