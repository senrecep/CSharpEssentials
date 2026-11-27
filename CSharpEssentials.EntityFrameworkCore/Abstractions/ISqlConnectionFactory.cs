using System.Data;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Factory for creating database connections for write operations.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Returns a database connection. Ownership depends on the implementation: a factory such as
    /// <c>DbDataSourceSqlConnectionFactory</c> returns a new connection the caller disposes, while
    /// <c>DbContextSqlSession&lt;TDbContext&gt;</c> returns the context's shared connection, which the caller must not
    /// dispose or close. Code that needs the context's connection and transaction should take <c>ISqlSession</c> instead.
    /// </summary>
    IDbConnection Create();
}
