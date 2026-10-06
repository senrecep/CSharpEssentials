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

        (await ScalarAsync(connection, "\"Status\"")).Should().Be("pending_approval");
        (await ScalarAsync(connection, "\"PreviousStatus\"")).Should().Be("shipped");
        (await ScalarAsync(connection, "\"Priority\"")).Should().Be(2L);
        (await ScalarAsync(connection, "\"Permissions\"")).Should().Be(3L);
        (await ScalarAsync(connection, "\"SharedPermissions\"")).Should().Be("[\"read\",\"delete\"]");
        (await ScalarAsync(connection, "\"Color\"")).Should().Be(1L);
        (await ScalarAsync(connection, "\"ManualStatus\"")).Should().Be("PendingApproval");
        (await ScalarAsync(connection, "\"History\"")).Should().Be("[\"pending\",\"shipped\"]");
        (await ScalarAsync(connection, "\"Priorities\"")).Should().Be("[0,2]");
        (await ScalarAsync(connection, "\"details\"")).Should().Be("{\"Color\":1,\"Priority\":1,\"Status\":\"shipped\"}");
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

        (await ScalarAsync(connection, "\"PreviousStatus\"")).Should().Be(DBNull.Value);
        (await reader.Orders.SingleAsync()).PreviousStatus.Should().BeNull();
    }

    [Fact]
    public async Task CheckConstraint_Should_RejectUnknownWireName()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(CheckConstraint_Should_RejectUnknownWireName)));
        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        Func<Task> update = () => ExecuteAsync(connection, "UPDATE orders SET \"Status\" = 'cancelled'");

        (await update.Should().ThrowAsync<SqliteException>()).Which.Message.Should().Contain("ck_orders_Status_enum");
    }

    [Fact]
    public async Task CheckConstraint_Should_RejectUnknownInteger()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using StoredOrderContext context = await CreateAsync(connection, StoredOrderContext.Default(nameof(CheckConstraint_Should_RejectUnknownInteger)));
        context.Orders.Add(SampleOrder());
        await context.SaveChangesAsync();

        Func<Task> priority = () => ExecuteAsync(connection, "UPDATE orders SET \"Priority\" = 7");
        Func<Task> permissions = () => ExecuteAsync(connection, "UPDATE orders SET \"Permissions\" = 8");

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

        await ExecuteAsync(connection, "UPDATE orders SET \"Shipment\" = 'lost_at_sea'");
        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);

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

        await ExecuteAsync(connection, "UPDATE orders SET \"Status\" = 'SHIPPED'");
        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);

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

        await ExecuteAsync(connection, "UPDATE orders SET \"Status\" = 'cancelled'");
        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);

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

    private static async Task<object?> ScalarAsync(SqliteConnection connection, string column)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT {column} FROM orders";
        return await command.ExecuteScalarAsync();
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
