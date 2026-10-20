using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

public static class IdempotencyRecordModelBuilderExtensions
{
    /// <summary>
    /// Maps <see cref="IdempotencyRecord"/> for <see cref="EfCoreIdempotencyRecordStore{TContext}"/>: the primary key
    /// <see cref="IdempotencyRecord.KeyHash"/> (fixed-length binary, 32 bytes) is the unique index a reservation inserts
    /// against, and an index on <see cref="IdempotencyRecord.ExpiresAt"/> serves
    /// <see cref="EfCoreIdempotencyRecordStore{TContext}.PurgeExpiredAsync"/>. The namespace, scope and key columns get
    /// the binary collation of <paramref name="providerName"/>; see <see cref="IdempotencyRecordModelOptions.KeyCollation"/>.
    /// Call it from <c>OnModelCreating</c> and add a migration.
    /// </summary>
    /// <param name="modelBuilder">The model of the context the store writes to.</param>
    /// <param name="providerName">
    /// The provider of the context, <c>Database.ProviderName</c>; it picks the collation of the text columns.
    /// </param>
    /// <param name="configure">Sets the table, schema and key collation.</param>
    /// <returns><paramref name="modelBuilder"/>.</returns>
    /// <exception cref="ArgumentException">The configured table name is empty.</exception>
    public static ModelBuilder AddIdempotencyRecords(
        this ModelBuilder modelBuilder,
        string? providerName,
        Action<IdempotencyRecordModelOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        IdempotencyRecordModelOptions options = new();
        configure?.Invoke(options);
        if (string.IsNullOrEmpty(options.TableName))
            throw new ArgumentException($"{nameof(IdempotencyRecordModelOptions.TableName)} must not be empty.", nameof(configure));

        string? collation = options.ResolveKeyCollation(providerName);
        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable(options.TableName, options.Schema);
            entity.HasKey(record => record.KeyHash);
            entity.Property(record => record.KeyHash)
                .HasMaxLength(IdempotencyRecord.KeyHashLength)
                .IsFixedLength()
                .ValueGeneratedNever();
            entity.HasIndex(record => record.ExpiresAt);
            Collate(entity.Property(record => record.Namespace).IsRequired(), collation);
            Collate(entity.Property(record => record.Scope).IsRequired(), collation);
            Collate(entity.Property(record => record.Key).IsRequired(), collation);
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
