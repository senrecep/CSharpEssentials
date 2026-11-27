using System.Data;
using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Sql;

/// <summary>
/// <see cref="ISqlSession"/> over the connection and the current transaction of a <typeparamref name="TDbContext"/>,
/// so SQL sent through it joins the transaction of <c>EfCoreTransactionRunner</c> or any transaction begun on the context.
/// <para>
/// <see cref="Create"/> returns the context's own connection, not a new one. Do not dispose or close it
/// (<c>using var connection = factory.Create()</c> breaks the context); the context disposes it.
/// Dapper opens a closed connection for the call and closes it again.
/// </para>
/// </summary>
public sealed class DbContextSqlSession<TDbContext>(TDbContext dbContext) : ISqlSession, ISqlConnectionFactory
    where TDbContext : DbContext
{
    /// <inheritdoc />
    public DbConnection Connection => dbContext.Database.GetDbConnection();

    /// <inheritdoc />
    public DbTransaction? Transaction => dbContext.Database.CurrentTransaction?.GetDbTransaction();

    /// <summary>
    /// Returns <see cref="Connection"/>, the context's own connection. Do not dispose it.
    /// </summary>
    public IDbConnection Create() => Connection;
}
