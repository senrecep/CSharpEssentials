using System.Data;
using System.Data.Common;

namespace CSharpEssentials.EntityFrameworkCore.Sql;

/// <summary>
/// <see cref="ISqlReadOnlyConnectionFactory"/> that creates a new connection from a <see cref="DbDataSource"/>
/// (for example the <c>NpgsqlDataSource</c> the application registers). The connection is outside any
/// <c>DbContext</c> transaction; the caller owns and disposes it.
/// </summary>
public sealed class DbDataSourceSqlConnectionFactory(DbDataSource dataSource) : ISqlReadOnlyConnectionFactory
{
    /// <summary>
    /// Creates a new, closed connection.
    /// </summary>
    public IDbConnection Create() => dataSource.CreateConnection();
}
