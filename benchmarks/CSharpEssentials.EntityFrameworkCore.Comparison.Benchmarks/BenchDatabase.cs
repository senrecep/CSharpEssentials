using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// One in-memory SQLite connection, opened once and kept open for a class's lifetime. Every row that writes runs in a
/// transaction that is rolled back, so the tables do not grow between invocations.
/// </summary>
public sealed class BenchDatabase : IDisposable
{
    private BenchDatabase(SqliteConnection connection) => Connection = connection;

    public SqliteConnection Connection { get; }

    public static BenchDatabase Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var database = new BenchDatabase(connection);
        using PlainContext context = database.Plain();
        context.Database.EnsureCreated();
        return database;
    }

    public DbContextOptions<TContext> Options<TContext>(Action<DbContextOptionsBuilder<TContext>>? configure = null)
        where TContext : DbContext
    {
        DbContextOptionsBuilder<TContext> builder = new DbContextOptionsBuilder<TContext>().UseSqlite(Connection);
        configure?.Invoke(builder);
        return builder.Options;
    }

    public PlainContext Plain() => new(Options<PlainContext>());

    public static async Task<int> SaveAndRollBackAsync(DbContext context)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        int written = await context.SaveChangesAsync();
        await transaction.RollbackAsync();
        context.ChangeTracker.Clear();
        return written;
    }

    public void Dispose() => Connection.Dispose();
}
