using System.Data;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Factory for creating database connections for write operations.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    IDbConnection Create();
}
