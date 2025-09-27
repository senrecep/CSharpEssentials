namespace CSharpEssentials.Entity.Interfaces;

public interface ISoftDeletableBase
{
    bool IsDeleted { get; }
}

public interface ISoftDeletable : ISoftDeletableBase
{
    DateTimeOffset? DeletedAt { get; }
    string? DeletedBy { get; }
    bool IsHardDeleted { get; }

    void MarkAsDeleted(DateTimeOffset deletedAt, string deletedBy);
    void Restore();
    void MarkAsHardDeleted();
}
