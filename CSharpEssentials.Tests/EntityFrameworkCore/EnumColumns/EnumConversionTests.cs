using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>
/// <c>ConvertEnumColumn</c> against a real database, in the order design section 12.1 prescribes: the generated
/// <c>DropCheckConstraint</c>, the conversion in place of the generated <c>AlterColumn</c>, the generated <c>AddCheckConstraint</c>.
/// Every test runs on PostgreSQL and SQLite.
/// </summary>
public abstract class EnumConversionTests
{
    protected const string Table = "orders";

    protected abstract string Provider { get; }

    [Fact]
    public async Task IntegerToString_Should_WriteWireNamesAndAddConstraint()
    {
        StoredOrderModel before = Model("int", Legacy(EnumStoredAs.Integer));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, "(1, 0, 0), (2, 1, 3), (3, 2, 7)");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Integer, to: EnumStorage.String));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("pending", "pending_approval", "shipped");
        (await StatusesAsync(after)).Should().Equal(StoredOrderStatus.Pending, StoredOrderStatus.PendingApproval, StoredOrderStatus.Shipped);
        (await ConstraintsAsync()).Should().Contain("ck_orders_Status_enum");
    }

    [Fact]
    public async Task StringToInteger_Should_RestoreNumbers_When_MigratingDown()
    {
        StoredOrderModel before = Model("int", Legacy(EnumStoredAs.Integer));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, "(1, 0, 0), (2, 1, 3), (3, 2, 7)");
        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Integer, to: EnumStorage.String));

        await MigrateAsync(after, before, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.Integer));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("0", "1", "2");
        (await StatusesAsync(before)).Should().Equal(StoredOrderStatus.Pending, StoredOrderStatus.PendingApproval, StoredOrderStatus.Shipped);
        (await ConstraintsAsync()).Should().NotContain("ck_orders_Status_enum");
    }

    [Theory]
    [InlineData(EnumStoredAs.MemberName, "Pending", "PendingApproval")]
    [InlineData(EnumStoredAs.CamelCase, "pending", "pendingApproval")]
    [InlineData(EnumStoredAs.LegacySnakeCase, "pending", "pending_approval")]
    public async Task LegacyText_Should_RoundTripThroughWireNames(EnumStoredAs format, string pending, string pendingApproval)
    {
        StoredOrderModel before = Model($"legacy-{format}", Legacy(format));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, $"(1, '{pending}', 0), (2, '{pendingApproval}', 0)");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: format, to: EnumStorage.String));
        List<string?> up = await TextsAsync(nameof(ConvertedOrder.Status));
        await MigrateAsync(after, before, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: format));

        up.Should().Equal("pending", "pending_approval");
        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal(pending, pendingApproval);
        (await StatusesAsync(before)).Should().Equal(StoredOrderStatus.Pending, StoredOrderStatus.PendingApproval);
    }

    [Fact]
    public async Task TextToString_Should_NormalizeEverySpelling()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, "(1, 'PendingApproval', 0), (2, 'pendingApproval', 0), (3, 'PENDING', 0), (4, '2', 0), (5, ' shipped ', 0), (6, 'pending_approval', 0)");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.String));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("pending_approval", "pending_approval", "pending", "shipped", "shipped", "pending_approval");
    }

    [Fact]
    public async Task TextToString_Should_ChangeNothing_When_RunTwice()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, "(1, 'PendingApproval', 0), (2, 'Shipped', 0)");
        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.String));
        List<string?> first = await TextsAsync(nameof(ConvertedOrder.Status));

        await RunAsync(after, builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.String));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal(first);
    }

    [Fact]
    public async Task IntegerToString_Should_KeepUndefinedValues()
    {
        StoredOrderModel before = Model("int", Legacy(EnumStoredAs.Integer));
        StoredOrderModel after = Model("string-unchecked", Current(statusConstraint: false));
        await CreateAsync(before, "(1, 1, 0), (2, 7, 0)");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Integer, to: EnumStorage.String));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("pending_approval", "7");
    }

    [Fact]
    public async Task IntegerToString_Should_FailOnConstraint_When_ValueIsUndefined()
    {
        StoredOrderModel before = Model("int", Legacy(EnumStoredAs.Integer));
        StoredOrderModel after = Model("string", Current());
        await CreateAsync(before, "(1, 1, 0), (2, 7, 0)");

        Func<Task> migrate = () => MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Integer, to: EnumStorage.String));

        (await migrate.Should().ThrowAsync<Exception>()).Which.Message.Should().ContainEquivalentOf("ck_orders_Status_enum");
    }

    [Fact]
    public async Task TextToInteger_Should_KeepUndefinedNumbers()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 'Shipped', 0), (2, '7', 0), (3, 'pending_approval', 0)");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.Integer));

        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("2", "7", "1");
    }

    [Fact]
    public async Task TextToInteger_Should_FailLoudly_When_ValueIsUnknown()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 'Shipped', 0), (2, 'bogus', 0)");

        Func<Task> migrate = () => MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.Integer));

        (await migrate.Should().ThrowAsync<Exception>()).Which.Message.Should()
            .Contain("cannot convert orders.Status to StoredOrderStatus, unknown value: bogus");
        (await TextsAsync(nameof(ConvertedOrder.Status))).Should().Equal("Shipped", "bogus");
    }

    [Fact]
    public async Task FlagsTextToInteger_Should_CombineFlagsAndAddConstraint()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 0, 'Read, Write'), (2, 0, 'read,delete'), (3, 0, 'None'), (4, 0, ''), (5, 0, 'Write, Write'), (6, 0, '2')");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        (await TextsAsync(nameof(ConvertedOrder.Permissions))).Should().Equal("3", "5", "0", "0", "2", "2");
        (await PermissionsAsync(after)).Should().Equal(
            StoredPermissions.Read | StoredPermissions.Write,
            StoredPermissions.Read | StoredPermissions.Delete,
            StoredPermissions.None,
            StoredPermissions.None,
            StoredPermissions.Write,
            StoredPermissions.Write);
        (await ConstraintsAsync()).Should().Contain("ck_orders_Permissions_enum");
    }

    [Fact]
    public async Task IntegerToFlagsText_Should_RestoreMemberNames_When_MigratingDown()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 0, 'Read, Write'), (2, 0, 'read,delete'), (3, 0, 'None'), (4, 0, 'Write, Read, Delete')");
        await MigrateAsync(before, after, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        await MigrateAsync(after, before, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.Integer, to: EnumStoredAs.FlagsText));

        (await TextsAsync(nameof(ConvertedOrder.Permissions))).Should().Equal("Read, Write", "Read, Delete", "None", "Read, Write, Delete");
        (await PermissionsAsync(before)).Should().Equal(
            StoredPermissions.Read | StoredPermissions.Write,
            StoredPermissions.Read | StoredPermissions.Delete,
            StoredPermissions.None,
            StoredPermissions.Read | StoredPermissions.Write | StoredPermissions.Delete);
    }

    [Fact]
    public async Task IntegerToFlagsText_Should_KeepUndefinedBitsAsNumbers()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        StoredOrderModel after = Model("int-unchecked", Legacy(EnumStoredAs.Integer), permissionsConstraint: false);
        await CreateAsync(after, "(1, 0, 9), (2, 0, 1)");

        await MigrateAsync(after, before, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.Integer, to: EnumStoredAs.FlagsText));

        (await TextsAsync(nameof(ConvertedOrder.Permissions))).Should().Equal("9", "Read");
    }

    [Fact]
    public async Task FlagsTextToInteger_Should_FailLoudly_When_TokenIsUnknown()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 0, 'Read'), (2, 0, 'Read, Bogus')");

        Func<Task> migrate = () => MigrateAsync(before, after, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        (await migrate.Should().ThrowAsync<Exception>()).Which.Message.Should()
            .Contain("cannot convert orders.Permissions to StoredPermissions, unknown value:").And.Contain("Bogus");
    }

    [Fact]
    public async Task Audit_Should_ListValuesThatMatchNoSpelling()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        await CreateAsync(before, "(1, 'Pending', 0), (2, 'bogus', 0), (3, 'bogus', 0), (4, '7', 0), (5, 'PENDING_APPROVAL', 0), (6, '1', 0)");

        List<EnumAuditRow> rows = await AuditAsync(EnumDataAudit.Sql<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), provider: Provider));

        rows.Select(row => (row.Value, row.Count)).Should().Equal(("7", 1L), ("bogus", 2L));
    }

    [Fact]
    public async Task Audit_Should_ListFlagsTextWithUnknownTokens()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        await CreateAsync(before, "(1, 0, 'Read, Write'), (2, 0, 'Read, Bogus'), (3, 0, ''), (4, 0, 'read,')");

        List<EnumAuditRow> rows = await AuditAsync(
            EnumDataAudit.Sql<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), storedAs: EnumStoredAs.FlagsText, provider: Provider));

        rows.Select(row => (row.Value, row.Count)).Should().Equal(("Read, Bogus", 1L));
    }

    protected abstract StoredOrderContext Open(StoredOrderModel model);

    protected abstract Task<string> ConstraintsAsync();

    protected static Action<EntityTypeBuilder<ConvertedOrder>> Legacy(EnumStoredAs status, EnumStoredAs? permissions = null) =>
        order =>
        {
            order.Property(o => o.Status).HasLegacyEnumStorage(status);
            if (permissions is { } format)
                order.Property(o => o.Permissions).HasLegacyEnumStorage(format);
        };

    protected static Action<EntityTypeBuilder<ConvertedOrder>> Current(bool statusConstraint = true) =>
        order => order.Property(o => o.Status).HasEnumCheckConstraint(statusConstraint);

    protected StoredOrderModel Model(string name, Action<EntityTypeBuilder<ConvertedOrder>> configure, bool permissionsConstraint = true) =>
        new(
            $"{GetType().Name}.{name}",
            builder => builder.ConfigureEnumConventions(EnumConventions.Default),
            modelBuilder =>
            {
                modelBuilder.Ignore<StoredOrder>();
                EntityTypeBuilder<ConvertedOrder> order = modelBuilder.Entity<ConvertedOrder>();
                order.ToTable(Table);
                order.Property(o => o.Id).ValueGeneratedNever();
                order.Property(o => o.Permissions).HasEnumCheckConstraint(permissionsConstraint);
                configure(order);
            });

    protected async Task CreateAsync(StoredOrderModel model, string rows)
    {
        await using StoredOrderContext context = Open(model);
        await context.Database.EnsureCreatedAsync();
        string insert = $"INSERT INTO \"{Table}\" (\"Id\", \"Status\", \"Permissions\") VALUES {rows}";
        await context.Database.ExecuteSqlRawAsync(insert);
    }

    /// <summary>Runs the model difference with the conversion in place of the generated <c>AlterColumn</c> of <paramref name="column"/>.</summary>
    protected async Task MigrateAsync(StoredOrderModel from, StoredOrderModel to, string column, Action<MigrationBuilder> convert)
    {
        await using StoredOrderContext source = Open(from);
        await using StoredOrderContext target = Open(to);
        IReadOnlyList<MigrationOperation> differences = target.GetService<IMigrationsModelDiffer>().GetDifferences(
            source.GetService<IDesignTimeModel>().Model.GetRelationalModel(),
            target.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        MigrationBuilder builder = new(Provider);
        builder.Operations.AddRange(differences.OfType<DropCheckConstraintOperation>());
        convert(builder);
        builder.Operations.AddRange(differences.Where(operation =>
            operation is not DropCheckConstraintOperation && !(operation is AlterColumnOperation alter && alter.Name == column)));

        await ExecuteAsync(target, builder);
    }

    /// <summary>Runs only the operations of <paramref name="convert"/>, as a second run of the same migration would.</summary>
    protected async Task RunAsync(StoredOrderModel model, Action<MigrationBuilder> convert)
    {
        await using StoredOrderContext context = Open(model);
        MigrationBuilder builder = new(Provider);
        convert(builder);
        await ExecuteAsync(context, builder);
    }

    protected Task<List<string?>> TextsAsync(string column) =>
        QueryAsync<string?>($"SELECT CAST(\"{column}\" AS text) AS \"Value\" FROM \"{Table}\" ORDER BY \"Id\"");

    protected async Task<List<T>> QueryAsync<T>(string sql)
    {
        await using StoredOrderContext context = Open(Model("raw", Legacy(EnumStoredAs.Integer)));
        return await context.Database.SqlQueryRaw<T>(sql).ToListAsync();
    }

    private async Task<List<EnumAuditRow>> AuditAsync(string sql) => await QueryAsync<EnumAuditRow>(sql.TrimEnd(';'));

    private async Task<List<StoredOrderStatus>> StatusesAsync(StoredOrderModel model)
    {
        await using StoredOrderContext context = Open(model);
        return await context.Set<ConvertedOrder>().OrderBy(order => order.Id).Select(order => order.Status).ToListAsync();
    }

    private async Task<List<StoredPermissions>> PermissionsAsync(StoredOrderModel model)
    {
        await using StoredOrderContext context = Open(model);
        return await context.Set<ConvertedOrder>().OrderBy(order => order.Id).Select(order => order.Permissions).ToListAsync();
    }

    private static async Task ExecuteAsync(StoredOrderContext context, MigrationBuilder builder)
    {
        IReadOnlyList<MigrationCommand> commands = context.GetService<IMigrationsSqlGenerator>()
            .Generate(builder.Operations, context.GetService<IDesignTimeModel>().Model);
        await context.GetService<IMigrationCommandExecutor>().ExecuteNonQueryAsync(commands, context.GetService<IRelationalConnection>());
    }
}
