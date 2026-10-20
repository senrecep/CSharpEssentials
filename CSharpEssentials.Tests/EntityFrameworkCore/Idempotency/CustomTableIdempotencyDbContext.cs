using CSharpEssentials.EntityFrameworkCore.Idempotency;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// Maps the records to a custom table whose text columns use <see cref="Collation"/>, a SQLite collation that ignores
/// case and trailing spaces, like SQL Server's <c>SQL_Latin1_General_CP1_CI_AS</c> (case-insensitive and PAD SPACE).
/// </summary>
public sealed class CustomTableIdempotencyDbContext(DbContextOptions<CustomTableIdempotencyDbContext> options) : DbContext(options)
{
    public const string TableName = "idem_records";

    public const string Collation = "PAD_SPACE_NOCASE";

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.AddIdempotencyRecords(Database.ProviderName, records =>
        {
            records.TableName = TableName;
            records.KeyCollation = Collation;
        });
}
