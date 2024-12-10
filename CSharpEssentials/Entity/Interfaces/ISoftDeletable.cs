namespace CSharpEssentials.Interfaces;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; }
    string? DeletedBy { get; }
    bool IsDeleted { get; }
    bool IsHardDeleted { get; }

    void MarkAsDeleted(DateTimeOffset deletedAt, string deletedBy);
    void Restore();
    void MarkAsHardDeleted();
}
