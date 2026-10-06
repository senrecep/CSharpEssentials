using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>Enum columns against a real SQLite database: stored values, check constraints and tolerant reads.</summary>
public sealed class EnumStorageSqliteTests
{
    [Fact]
    public async Task SaveChanges_Should_StoreCanonicalValues()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(SaveChanges_Should_StoreCanonicalValues)));

        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        (await ScalarAsync(context, "\"Status\"")).Should().Be("pending_approval");
        (await ScalarAsync(context, "\"PreviousStatus\"")).Should().Be("shipped");
        (await ScalarAsync(context, "\"Priority\"")).Should().Be("2");
        (await ScalarAsync(context, "\"Permissions\"")).Should().Be("3");
        (await ScalarAsync(context, "\"SharedPermissions\"")).Should().Be("[\"read\",\"delete\"]");
        (await ScalarAsync(context, "\"Color\"")).Should().Be("1");
        (await ScalarAsync(context, "\"ManualStatus\"")).Should().Be("PendingApproval");
        (await ScalarAsync(context, "\"History\"")).Should().Be("[\"pending\",\"shipped\"]");
        (await ScalarAsync(context, "\"Priorities\"")).Should().Be("[0,2]");
        (await ScalarAsync(context, "\"details\"")).Should().Be("{\"Color\":1,\"Priority\":1,\"Status\":\"shipped\"}");
    }

    [Fact]
    public async Task Query_Should_RoundTripEveryEnumShape()
    {
        StoredOrderModel model = StoredOrderContext.Default(nameof(Query_Should_RoundTripEveryEnumShape));
        await using SqliteConnection connection = await OpenAsync();
        await using (StoredOrderContext writer = await CreateAsync(connection, model))
        {
            writer.Orders.Add(SampleOrder());
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);
        StoredOrder order = await reader.Orders.SingleAsync();

        order.Should().BeEquivalentTo(SampleOrder());
    }

    [Fact]
    public async Task Query_Should_FilterByWireName()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(Query_Should_FilterByWireName)));
        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        int count = await context.Orders.CountAsync(order => order.Status == StoredOrderStatus.PendingApproval && order.Priority == StoredPriority.High);

        count.Should().Be(1);
    }

    [Fact]
    public async Task SaveChanges_Should_StoreNull_When_NullableEnumIsNull()
    {
        StoredOrderModel model = StoredOrderContext.Default(nameof(SaveChanges_Should_StoreNull_When_NullableEnumIsNull));
        await using SqliteConnection connection = await OpenAsync();
        await using (StoredOrderContext writer = await CreateAsync(connection, model))
        {
            StoredOrder order = SampleOrder();
            order.PreviousStatus = null;
            writer.Orders.Add(order);
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);

        (await ScalarAsync(reader, "\"PreviousStatus\"")).Should().BeNull();
        (await reader.Orders.SingleAsync()).PreviousStatus.Should().BeNull();
    }

    [Fact]
    public async Task CheckConstraint_Should_RejectUnknownWireName()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(CheckConstraint_Should_RejectUnknownWireName)));
        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        Func<Task> update = () => context.Database.ExecuteSqlRawAsync("UPDATE orders SET \"Status\" = 'cancelled'");

        (await update.Should().ThrowAsync<SqliteException>()).Which.Message.Should().Contain("ck_orders_Status_enum");
    }

    [Fact]
    public async Task CheckConstraint_Should_RejectUnknownInteger()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(CheckConstraint_Should_RejectUnknownInteger)));
        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        Func<Task> priority = () => context.Database.ExecuteSqlRawAsync("UPDATE orders SET \"Priority\" = 7");
        Func<Task> permissions = () => context.Database.ExecuteSqlRawAsync("UPDATE orders SET \"Permissions\" = 8");

        (await priority.Should().ThrowAsync<SqliteException>()).Which.Message.Should().Contain("ck_orders_Priority_enum");
        (await permissions.Should().ThrowAsync<SqliteException>()).Which.Message.Should().Contain("ck_orders_Permissions_enum");
    }

    [Fact]
    public async Task Query_Should_ReadFallbackMember_When_StoredValueIsUnknown()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(Query_Should_ReadFallbackMember_When_StoredValueIsUnknown),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(order => order.Shipment).HasEnumCheckConstraint(false));
        await using SqliteConnection connection = await OpenAsync();
        await using (StoredOrderContext writer = await CreateAsync(connection, model))
        {
            writer.Orders.Add(SampleOrder());
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);
        await ExecuteAsync(reader, "UPDATE orders SET \"Shipment\" = 'lost_at_sea'");

        (await reader.Orders.SingleAsync()).Shipment.Should().Be(StoredShipment.Unknown);
    }

    [Fact]
    public async Task Query_Should_ReadAliasAndCase_When_StoredValueIsNotCanonical()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(Query_Should_ReadAliasAndCase_When_StoredValueIsNotCanonical),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(order => order.Status).HasEnumCheckConstraint(false));
        await using SqliteConnection connection = await OpenAsync();
        await using (StoredOrderContext writer = await CreateAsync(connection, model))
        {
            writer.Orders.Add(SampleOrder());
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);
        await ExecuteAsync(reader, "UPDATE orders SET \"Status\" = 'SHIPPED'");

        (await reader.Orders.SingleAsync()).Status.Should().Be(StoredOrderStatus.Shipped);
    }

    [Fact]
    public async Task Query_Should_NameColumn_When_StoredValueIsUnknownAndThereIsNoFallback()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(Query_Should_NameColumn_When_StoredValueIsUnknownAndThereIsNoFallback),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(order => order.Status).HasEnumCheckConstraint(false));
        await using SqliteConnection connection = await OpenAsync();
        await using (StoredOrderContext writer = await CreateAsync(connection, model))
        {
            writer.Orders.Add(SampleOrder());
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);
        await ExecuteAsync(reader, "UPDATE orders SET \"Status\" = 'cancelled'");

        Func<Task> read = () => reader.Orders.SingleAsync();

        (await read.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("orders.Status").And.Contain("cancelled");
    }

    [Fact]
    public async Task SaveChanges_Should_Throw_When_ValueIsUndefined()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(SaveChanges_Should_Throw_When_ValueIsUndefined)));
        StoredOrder order = SampleOrder();
        order.Status = (StoredOrderStatus)42;
        context.Orders.Add(order);

        Func<Task> save = () => context.SaveChangesAsync();

        (await save.Should().ThrowAsync<DbUpdateException>())
            .WithInnerException<EnumValueException>().Which.Error.Path.Should().Contain("orders.Status");
    }

    private static StoredOrder SampleOrder() => new()
    {
        Id = 1,
        Status = StoredOrderStatus.PendingApproval,
        PreviousStatus = StoredOrderStatus.Shipped,
        Shipment = StoredShipment.InTransit,
        Priority = StoredPriority.High,
        Permissions = StoredPermissions.Read | StoredPermissions.Write,
        SharedPermissions = StoredPermissions.Read | StoredPermissions.Delete,
        Color = PlainColor.Green,
        ManualStatus = StoredOrderStatus.PendingApproval,
        History = [StoredOrderStatus.Pending, StoredOrderStatus.Shipped],
        Priorities = [StoredPriority.Low, StoredPriority.High],
        Details = new StoredOrderDetails { Status = StoredOrderStatus.Shipped, Priority = StoredPriority.Medium, Color = PlainColor.Green },
    };

    private static async Task<SqliteConnection> OpenAsync()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<StoredOrderContext> CreateAsync(SqliteConnection connection, StoredOrderModel model)
    {
        StoredOrderContext context = StoredOrderContext.Sqlite(connection, model);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static Task<string?> ScalarAsync(DbContext context, string column)
    {
        string sql = "SELECT CAST(" + column + " AS TEXT) AS \"Value\" FROM orders";
        return context.Database.SqlQueryRaw<string?>(sql).SingleAsync();
    }

    private static Task<int> ExecuteAsync(DbContext context, string sql) => context.Database.ExecuteSqlRawAsync(sql);
}
