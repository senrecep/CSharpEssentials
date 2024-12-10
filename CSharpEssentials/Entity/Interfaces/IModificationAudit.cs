namespace CSharpEssentials.Interfaces;

public interface IModificationAudit
{
    DateTimeOffset? UpdatedAt { get; }
    string? UpdatedBy { get; }

    void SetUpdatedInfo(DateTimeOffset updatedAt, string updatedBy);
}
