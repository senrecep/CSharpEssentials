using System.Data.Common;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>A context whose conventions and model come from the test; the model is cached per <see cref="StoredOrderModel.Key"/>.</summary>
public sealed class StoredOrderContext(DbContextOptions<StoredOrderContext> options, StoredOrderModel model) : DbContext(options)
{
    public StoredOrderModel Setup { get; } = model;

    public DbSet<StoredOrder> Orders => Set<StoredOrder>();

    public static StoredOrderContext Sqlite(DbConnection connection, StoredOrderModel model) =>
        new(Build().UseSqlite(connection).Options, model);

    public static StoredOrderContext Postgres(string connectionString, StoredOrderModel model) =>
        new(Build().UseNpgsql(connectionString).Options, model);

    public static StoredOrderContext InMemory(StoredOrderModel model) =>
        new(Build().UseInMemoryDatabase(model.Key).Options, model);

    /// <summary>The 4.x shaped model: one table, a manual conversion, a JSON column; no enum convention.</summary>
    public static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredOrder>(order =>
        {
            order.ToTable("orders");
            order.Property(o => o.ManualStatus).HasConversion<string>();
            order.OwnsOne(o => o.Details, details => details.ToJson("details"));
        });
    }

    /// <summary>The 5.0 model of a new project: default conventions plus per-property settings.</summary>
    public static StoredOrderModel Default(string key, Action<ModelBuilder>? configure = null, EnumConventions? conventions = null) =>
        new(
            key,
            builder => builder.ConfigureEnumConventions(conventions ?? EnumConventions.Default),
            modelBuilder =>
            {
                ConfigureOrders(modelBuilder);
                modelBuilder.Entity<StoredOrder>().Property(o => o.SharedPermissions).HasEnumStorage(EnumStorage.String);
                configure?.Invoke(modelBuilder);
            });

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        Setup.Conventions?.Invoke(configurationBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder) => Setup.Model(modelBuilder);

    private static DbContextOptionsBuilder<StoredOrderContext> Build() =>
        new DbContextOptionsBuilder<StoredOrderContext>()
            .ReplaceService<IModelCacheKeyFactory, StoredOrderModelCacheKeyFactory>();
}
