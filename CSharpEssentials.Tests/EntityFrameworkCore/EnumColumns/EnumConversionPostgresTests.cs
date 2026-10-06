using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>The enum conversions against PostgreSQL, plus the jsonb path conversion that only PostgreSQL supports.</summary>
public sealed class EnumConversionPostgresTests(PostgresEnumFixture postgres) : EnumConversionTests, IClassFixture<PostgresEnumFixture>
{
    private readonly string _connectionString = postgres.ConnectionString($"conversion_{Guid.NewGuid():N}");

    protected override string Provider => "Npgsql.EntityFrameworkCore.PostgreSQL";

    [Fact]
    public async Task FlagsTextToInteger_Should_DropTheTemporaryColumn()
    {
        StoredOrderModel before = Model("flags-text", Legacy(EnumStoredAs.Integer, EnumStoredAs.FlagsText));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 0, 'Read, Write')");

        await MigrateAsync(before, after, nameof(ConvertedOrder.Permissions), builder =>
            builder.ConvertEnumColumn<StoredPermissions>(Table, nameof(ConvertedOrder.Permissions), from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        (await QueryAsync<string>($"SELECT column_name || ' ' || udt_name || ' ' || is_nullable AS \"Value\" FROM information_schema.columns WHERE table_name = '{Table}' ORDER BY column_name"))
            .Should().Equal("Id int4 NO", "Permissions int4 NO", "Status int4 NO");
    }

    [Fact]
    public async Task ConvertEnumJsonPath_Should_RewriteNumbersAsWireNames()
    {
        await CreateJsonAsync("('{\"order\": {\"status\": 1}}'), ('{\"order\": {\"status\": \"Shipped\"}}'), ('{\"order\": {\"status\": 9}}'), ('{\"order\": {}}'), (NULL)");

        await RunAsync(JsonModel(), builder => builder.ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", ["order", "status"]));

        (await JsonAsync()).Should().Equal(
            "{\"order\": {\"status\": \"pending_approval\"}}",
            "{\"order\": {\"status\": \"shipped\"}}",
            "{\"order\": {\"status\": 9}}",
            "{\"order\": {}}",
            null);
    }

    [Fact]
    public async Task ConvertEnumJsonPath_Should_RestoreNumbers_When_MigratingDown()
    {
        await CreateJsonAsync("('{\"order\": {\"status\": \"pending_approval\"}}'), ('{\"order\": {\"status\": \"shipped\"}}'), ('{\"order\": {\"status\": \"bogus\"}}')");

        await RunAsync(JsonModel(), builder => builder.ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", ["order", "status"], to: EnumStorage.Integer));

        (await JsonAsync()).Should().Equal(
            "{\"order\": {\"status\": 1}}",
            "{\"order\": {\"status\": 2}}",
            "{\"order\": {\"status\": \"bogus\"}}");
    }

    [Fact]
    public async Task ConvertEnumJsonPath_Should_ChangeNothing_When_RunTwice()
    {
        await CreateJsonAsync("('{\"order\": {\"status\": 0}}'), ('{\"order\": {\"status\": \"PendingApproval\"}}')");
        await RunAsync(JsonModel(), builder => builder.ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", ["order", "status"]));
        List<string?> first = await JsonAsync();

        await RunAsync(JsonModel(), builder => builder.ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", ["order", "status"]));

        (await JsonAsync()).Should().Equal(first).And.Equal("{\"order\": {\"status\": \"pending\"}}", "{\"order\": {\"status\": \"pending_approval\"}}");
    }

    protected override StoredOrderContext Open(StoredOrderModel model) => StoredOrderContext.Postgres(_connectionString, model);

    protected override async Task<string> ConstraintsAsync() =>
        string.Join("\n", await QueryAsync<string>($"SELECT conname AS \"Value\" FROM pg_constraint WHERE conrelid = '{Table}'::regclass AND contype = 'c' ORDER BY conname"));

    private StoredOrderModel JsonModel() => Model("json", Legacy(EnumStoredAs.Integer));

    private async Task CreateJsonAsync(string rows)
    {
        await CreateAsync(JsonModel(), "(1, 0, 0)");
        await using StoredOrderContext context = Open(JsonModel());
        await context.Database.ExecuteSqlRawAsync("CREATE TABLE documents (id serial PRIMARY KEY, data jsonb)");
        string insert = $"INSERT INTO documents (data) VALUES {rows.Replace("{", "{{").Replace("}", "}}")}";
        await context.Database.ExecuteSqlRawAsync(insert);
    }

    private Task<List<string?>> JsonAsync() =>
        QueryAsync<string?>("SELECT CAST(data AS text) AS \"Value\" FROM documents ORDER BY id");
}
