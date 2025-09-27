namespace CSharpEssentials.Entity.Interfaces;

public interface ICreationAudit
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }

    void SetCreatedInfo(DateTimeOffset createdAt, string createdBy);
}
