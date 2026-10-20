using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

public static class IdempotencyRecordModelBuilderExtensions
{
    /// <summary>
    /// Maps <see cref="IdempotencyRecord"/> for <see cref="EfCoreIdempotencyRecordStore{TContext}"/>: the primary key
    /// (<see cref="IdempotencyRecord.Namespace"/>, <see cref="IdempotencyRecord.Scope"/>,
    /// <see cref="IdempotencyRecord.Key"/>) is the unique index a reservation inserts against, and an index on
    /// <see cref="IdempotencyRecord.ExpiresAt"/> serves <see cref="EfCoreIdempotencyRecordStore{TContext}.PurgeExpiredAsync"/>.
    /// Call it from <c>OnModelCreating</c> and add a migration.
    /// <para>
    /// The key columns need a binary collation; see <see cref="IdempotencyRecordModelOptions"/>. The string columns have
    /// no maximum length; on a database whose index keys are limited (SQL Server: 900 bytes) configure one after this
    /// call with <c>modelBuilder.Entity&lt;IdempotencyRecord&gt;().Property(...).HasMaxLength(...)</c>.
    /// </para>
    /// </summary>
    /// <param name="modelBuilder">The model of the context the store writes to.</param>
    /// <param name="configure">Sets the table, schema and key collation.</param>
    /// <returns><paramref name="modelBuilder"/>.</returns>
    /// <exception cref="ArgumentException">The configured table name is empty.</exception>
    public static ModelBuilder AddIdempotencyRecords(this ModelBuilder modelBuilder, Action<IdempotencyRecordModelOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        IdempotencyRecordModelOptions options = new();
        configure?.Invoke(options);
        if (string.IsNullOrEmpty(options.TableName))
            throw new ArgumentException($"{nameof(IdempotencyRecordModelOptions.TableName)} must not be empty.", nameof(configure));

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable(options.TableName, options.Schema);
            entity.HasKey(record => new { record.Namespace, record.Scope, record.Key });
            entity.HasIndex(record => record.ExpiresAt);
            Collate(entity.Property(record => record.Namespace).IsRequired(), options.KeyCollation);
            Collate(entity.Property(record => record.Scope).IsRequired(), options.KeyCollation);
            Collate(entity.Property(record => record.Key).IsRequired(), options.KeyCollation);
            entity.Property(record => record.Token).IsRequired();
            entity.Property(record => record.Fingerprint).IsRequired();
            entity.Property(record => record.Payload).IsRequired(false);
            entity.Property(record => record.ExpiresAt).IsRequired();
        });
        return modelBuilder;
    }

    private static void Collate(PropertyBuilder<string> property, string? collation)
    {
        if (collation is not null)
            property.UseCollation(collation);
    }
}
