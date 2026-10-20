using CSharpEssentials.EntityFrameworkCore.Idempotency;

using Microsoft.EntityFrameworkCore;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// The business tables and the idempotency records in one database, so the record of a payment command commits or
/// rolls back with the payment itself.
/// </summary>
public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Settlement> Settlements => Set<Settlement>();

    public DbSet<Receipt> Receipts => Set<Receipt>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().Property(payment => payment.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Settlement>().Property(settlement => settlement.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Receipt>().Property(receipt => receipt.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Receipt>().HasIndex(receipt => receipt.PaymentId).IsUnique();
        modelBuilder.AddIdempotencyRecords(Database.ProviderName);
    }
}
