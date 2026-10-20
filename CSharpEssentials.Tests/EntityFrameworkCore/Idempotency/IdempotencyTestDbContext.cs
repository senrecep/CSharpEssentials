using CSharpEssentials.EntityFrameworkCore.Idempotency;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

public sealed class IdempotencyPayment
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>A business table next to the idempotency records, mapped with the provider's binary key collation.</summary>
public sealed class IdempotencyTestDbContext(DbContextOptions<IdempotencyTestDbContext> options) : DbContext(options)
{
    public DbSet<IdempotencyPayment> Payments => Set<IdempotencyPayment>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.AddIdempotencyRecords(Database.ProviderName);
}
