using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>Enum columns against a real PostgreSQL database: text columns, text[] flags, arrays, jsonb and check constraints.</summary>
public sealed class EnumStoragePostgresTests(PostgresEnumFixture postgres) : IClassFixture<PostgresEnumFixture>
{
    private const string CheckViolation = "23514";

    [Fact]
    public async Task EnsureCreated_Should_CreateEnumCheckConstraints()
    {
        string connectionString = await CreateAsync(nameof(EnsureCreated_Should_CreateEnumCheckConstraints));

        List<string> constraints = await ColumnAsync<string>(
            connectionString,
            "SELECT conname || ': ' || pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conrelid = 'orders'::regclass AND contype = 'c' ORDER BY conname");

        constraints.Should().HaveCount(8);
        constraints.Should().ContainMatch("ck_orders_Status_enum: CHECK ((\"Status\" = ANY (ARRAY['pending'::text, 'pending_approval'::text, 'shipped'::text])))");
        constraints.Should().ContainMatch("ck_orders_SharedPermissions_enum: CHECK ((\"SharedPermissions\" <@ ARRAY['none'::text, 'read'::text, 'write'::text, 'delete'::text]))");
        constraints.Should().ContainMatch("ck_orders_History_enum: CHECK ((\"History\" <@ ARRAY[*'pending'::text*]))");
        constraints.Should().ContainMatch("ck_orders_Priorities_enum: CHECK ((\"Priorities\" <@ ARRAY[0, 1, 2]))");
        constraints.Should().ContainMatch("ck_orders_Permissions_enum: CHECK (((\"Permissions\" & (~ 7)) = 0))");
        constraints.Should().ContainMatch("ck_orders_PreviousStatus_enum: *");
        constraints.Should().ContainMatch("ck_orders_Shipment_enum: *'unknown'::text*");
        constraints.Should().ContainMatch("ck_orders_Priority_enum: CHECK ((\"Priority\" = ANY (ARRAY[0, 1, 2])))");
    }

    [Fact]
    public async Task EnsureCreated_Should_UseTextAndArrayColumns()
    {
        string connectionString = await CreateAsync(nameof(EnsureCreated_Should_UseTextAndArrayColumns));

        List<string> columns = await ColumnAsync<string>(
            connectionString,
            "SELECT column_name || ' ' || udt_name AS \"Value\" FROM information_schema.columns WHERE table_name = 'orders' ORDER BY column_name");

        columns.Should().Contain(
        [
            "Status text",
            "PreviousStatus text",
            "Priority int4",
            "Permissions int4",
            "SharedPermissions _text",
            "Color int4",
            "History _text",
            "Priorities _int4",
            "details jsonb",
        ]);
    }

    [Fact]
    public async Task SaveChanges_Should_StoreCanonicalValues()
    {
        string connectionString = await CreateAsync(nameof(SaveChanges_Should_StoreCanonicalValues));
        await SaveAsync(connectionString, SampleOrder(), nameof(SaveChanges_Should_StoreCanonicalValues));

        List<string> row = await ColumnAsync<string>(
            connectionString,
            """
            SELECT unnest(ARRAY["Status", "PreviousStatus", "Priority"::text, "Permissions"::text, array_to_string("SharedPermissions", ','),
                "Color"::text, array_to_string("History", ','), array_to_string("Priorities", ','), details->>'Status', details->>'Priority']) AS "Value"
            FROM orders
            """);

        row.Should().Equal("pending_approval", "shipped", "2", "3", "read,delete", "1", "pending,shipped", "0,2", "shipped", "1");
    }

    [Fact]
    public async Task Query_Should_RoundTripEveryEnumShape()
    {
        string connectionString = await CreateAsync(nameof(Query_Should_RoundTripEveryEnumShape));
        await SaveAsync(connectionString, SampleOrder(), nameof(Query_Should_RoundTripEveryEnumShape));

        await using StoredOrderContext reader = StoredOrderContext.Postgres(connectionString, Model(nameof(Query_Should_RoundTripEveryEnumShape)));
        StoredOrder order = await reader.Orders.SingleAsync();

        order.Should().BeEquivalentTo(SampleOrder());
    }

    [Fact]
    public async Task Query_Should_TranslateEnumFilters()
    {
        string connectionString = await CreateAsync(nameof(Query_Should_TranslateEnumFilters));
        await SaveAsync(connectionString, SampleOrder(), nameof(Query_Should_TranslateEnumFilters));
        await using StoredOrderContext reader = StoredOrderContext.Postgres(connectionString, Model(nameof(Query_Should_TranslateEnumFilters)));

        int count = await reader.Orders.CountAsync(order =>
            order.Status == StoredOrderStatus.PendingApproval &&
            order.History.Contains(StoredOrderStatus.Shipped) &&
            order.Details.Status == StoredOrderStatus.Shipped);

        count.Should().Be(1);
    }

    [Fact]
    public async Task SaveChanges_Should_StoreNull_When_NullableEnumIsNull()
    {
        string connectionString = await CreateAsync(nameof(SaveChanges_Should_StoreNull_When_NullableEnumIsNull));
        StoredOrder order = SampleOrder();
        order.PreviousStatus = null;
        await SaveAsync(connectionString, order, nameof(SaveChanges_Should_StoreNull_When_NullableEnumIsNull));

        await using StoredOrderContext reader = StoredOrderContext.Postgres(connectionString, Model(nameof(SaveChanges_Should_StoreNull_When_NullableEnumIsNull)));

        (await ColumnAsync<bool>(connectionString, "SELECT \"PreviousStatus\" IS NULL AS \"Value\" FROM orders")).Should().Equal(true);
        (await reader.Orders.SingleAsync()).PreviousStatus.Should().BeNull();
    }

    [Theory]
    [InlineData("\"Status\" = 'cancelled'", "ck_orders_Status_enum")]
    [InlineData("\"Priority\" = 7", "ck_orders_Priority_enum")]
    [InlineData("\"Permissions\" = 8", "ck_orders_Permissions_enum")]
    [InlineData("\"SharedPermissions\" = ARRAY['read', 'admin']", "ck_orders_SharedPermissions_enum")]
    [InlineData("\"History\" = ARRAY['pending', 'cancelled']", "ck_orders_History_enum")]
    [InlineData("\"Priorities\" = ARRAY[0, 9]", "ck_orders_Priorities_enum")]
    public async Task CheckConstraint_Should_RejectUnknownValue(string assignment, string constraint)
    {
        string database = $"{nameof(CheckConstraint_Should_RejectUnknownValue)}_{constraint}";
        string connectionString = await CreateAsync(database);
        await SaveAsync(connectionString, SampleOrder(), database);

        Func<Task> update = () => ExecuteAsync(connectionString, $"UPDATE orders SET {assignment}");

        PostgresException error = (await update.Should().ThrowAsync<PostgresException>()).Which;
        error.SqlState.Should().Be(CheckViolation);
        error.ConstraintName.Should().Be(constraint);
    }

    [Fact]
    public async Task Query_Should_ReadFallbackMember_When_StoredValueIsUnknown()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            $"{nameof(EnumStoragePostgresTests)}.{nameof(Query_Should_ReadFallbackMember_When_StoredValueIsUnknown)}",
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(order => order.Shipment).HasEnumCheckConstraint(false));
        string connectionString = postgres.ConnectionString(nameof(Query_Should_ReadFallbackMember_When_StoredValueIsUnknown));
        await using (StoredOrderContext writer = StoredOrderContext.Postgres(connectionString, model))
        {
            await writer.Database.EnsureCreatedAsync();
            writer.Orders.Add(SampleOrder());
            await writer.SaveChangesAsync();
        }

        await ExecuteAsync(connectionString, "UPDATE orders SET \"Shipment\" = 'lost_at_sea'");
        await using StoredOrderContext reader = StoredOrderContext.Postgres(connectionString, model);

        (await reader.Orders.SingleAsync()).Shipment.Should().Be(StoredShipment.Unknown);
    }

    private static StoredOrderModel Model(string name) =>
        StoredOrderContext.Default($"{nameof(EnumStoragePostgresTests)}.{name}");

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

    private async Task<string> CreateAsync(string name)
    {
        string connectionString = postgres.ConnectionString(name);
        await using StoredOrderContext context = StoredOrderContext.Postgres(connectionString, Model(name));
        await context.Database.EnsureCreatedAsync();
        return connectionString;
    }

    private static async Task SaveAsync(string connectionString, StoredOrder order, string name)
    {
        await using StoredOrderContext context = StoredOrderContext.Postgres(connectionString, Model(name));
        context.Orders.Add(order);
        await context.SaveChangesAsync();
    }

    private static async Task<List<T>> ColumnAsync<T>(string connectionString, string sql)
    {
        await using StoredOrderContext context = StoredOrderContext.Postgres(connectionString, Model("raw"));
        return await context.Database.SqlQueryRaw<T>(sql).ToListAsync();
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using StoredOrderContext context = StoredOrderContext.Postgres(connectionString, Model("raw"));
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
