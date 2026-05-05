namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Factory for creating database connections for read-only operations.
/// </summary>
public interface ISqlReadOnlyConnectionFactory : ISqlConnectionFactory
{
}
