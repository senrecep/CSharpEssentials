using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

public static class OutboxModelBuilderExtensions
{
    /// <summary>
    /// Maps <see cref="OutboxMessage"/> with <see cref="OutboxMessage.Id"/> as the primary key and an index on
    /// (<see cref="OutboxMessage.ProcessedAt"/>, <see cref="OutboxMessage.DeadLetteredAt"/>,
    /// <see cref="OutboxMessage.NextAttemptAt"/>) for the claim query. Call it from <c>OnModelCreating</c> and add a
    /// migration. On PostgreSQL you may replace the index with a partial one
    /// (<c>WHERE "ProcessedAt" IS NULL AND "DeadLetteredAt" IS NULL</c>) in the migration.
    /// </summary>
    /// <param name="modelBuilder">The model of the context the outbox writes to.</param>
    /// <param name="configure">Sets the table and schema.</param>
    /// <returns><paramref name="modelBuilder"/>.</returns>
    /// <exception cref="ArgumentException">The configured table name is empty.</exception>
    public static ModelBuilder AddOutboxMessages(this ModelBuilder modelBuilder, Action<OutboxMessageModelOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        OutboxMessageModelOptions options = new();
        configure?.Invoke(options);
        if (string.IsNullOrEmpty(options.TableName))
            throw new ArgumentException($"{nameof(OutboxMessageModelOptions.TableName)} must not be empty.", nameof(configure));

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable(options.TableName, options.Schema);
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Id).ValueGeneratedNever();
            entity.Property(message => message.Type).IsRequired().HasMaxLength(OutboxMessageModelOptions.TypeMaxLength);
            entity.Property(message => message.Content).IsRequired();
            entity.Property(message => message.Error).IsRequired(false);
            entity.HasIndex(message => new { message.ProcessedAt, message.DeadLetteredAt, message.NextAttemptAt });
        });
        return modelBuilder;
    }
}
