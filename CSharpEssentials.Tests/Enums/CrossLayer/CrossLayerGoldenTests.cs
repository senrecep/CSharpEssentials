using System.Reflection;
using System.Text.Json;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Http;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.AspNetCore;
using CSharpEssentials.Tests.AspNetCore.EnumIntegration;
using CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.Enums.CrossLayer;

/// <summary>
/// The cross-layer golden table (<see cref="CrossLayerTable"/>, #68) against System.Text.Json, ASP.NET Core binding, EF Core
/// storage (SQLite), the HTTP client helpers and the Swashbuckle output. The Microsoft.AspNetCore.OpenApi output runs the same
/// table in CSharpEssentials.AspNetCore.OpenApi.Tests (net10.0).
/// </summary>
public class CrossLayerGoldenTests
{
    private static readonly Lazy<Task<IReadOnlyDictionary<string, string>>> SwashbuckleDocuments = new(() =>
        SwashbuckleEnumGoldenTests.GetDocumentsAsync(addEnumConventions: true, configureApp: static app => app.MapCrossLayerApi()));

    public static TheoryData<string> Rows() => [.. CrossLayerTable.Rows.Select(static row => row.Name)];

    [Fact]
    public void Table_Should_CoverEveryShapeAndLayerColumn()
    {
        string[] shapes = [.. typeof(CrossLayerRecord).GetProperties().Select(static property => property.Name).Where(static name => name != "Id")];

        CrossLayerTable.Rows.Select(static row => row.Shape).Distinct().Should().BeEquivalentTo(shapes);
        CrossLayerTable.Rows.Select(static row => row.Name).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void Table_Should_ListEveryJsonTokenInTheOpenApiEnum(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        using JsonDocument json = JsonDocument.Parse(row.Json);
        using JsonDocument component = JsonDocument.Parse(row.OpenApiComponent);
        string[] listed = [.. component.RootElement.GetProperty("enum").EnumerateArray().Select(static value => value.GetRawText())];

        string[] tokens = json.RootElement.ValueKind switch
        {
            JsonValueKind.Array => [.. json.RootElement.EnumerateArray().Select(static value => value.GetRawText())],
            JsonValueKind.Null => [],
            _ => [json.RootElement.GetRawText()],
        };

        tokens.Except(listed).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void Json_Should_WriteAndReadTheWireText(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        JsonSerializerOptions data = new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default, EnumReadMode.Data, row.Format);
        JsonSerializerOptions input = new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default, EnumReadMode.Input, row.Format);

        string written = JsonSerializer.Serialize(row.Value, row.Type, data);

        written.Should().Be(row.Json);
        JsonSerializer.Deserialize(row.Json, row.Type, data).Should().Be(row.Value);
        JsonSerializer.Deserialize(row.Json, row.Type, input).Should().Be(row.Value);
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task Binding_Should_BindTheWireTextAndEchoTheJson(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: static app => app.MapCrossLayerApi());
        string group = "/golden/" + row.Shape;

        EcResponse route = await host.GetAsync(row.Route is null ? group + "/route" : group + "/route/" + Uri.EscapeDataString(row.Route));
        EcResponse query = await host.GetAsync(row.Query.Length == 0 ? group + "/query" : group + "/query?" + row.Query);
        EcResponse header = row.Header is null
            ? await host.GetAsync(group + "/header")
            : await host.GetAsync(group + "/header", (CrossLayerApi.Header, row.Header));

        foreach (EcResponse response in (EcResponse[])[route, query, header])
        {
            response.Status.Should().Be(200, response.Body);
            response.Body.Should().Be(row.Json);
        }
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task EfCore_Should_StoreTheColumnTextAndReadTheValueBack(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        StoredOrderModel model = new(
            "CrossLayer",
            static builder => builder.ConfigureEnumConventions(EnumConventions.Default),
            static modelBuilder =>
            {
                modelBuilder.Ignore<StoredOrder>();
                modelBuilder.Entity<CrossLayerRecord>().ToTable("golden");
            });
        PropertyInfo property = typeof(CrossLayerRecord).GetProperty(row.Shape)!;
        await using SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        await using (StoredOrderContext writer = StoredOrderContext.Sqlite(connection, model))
        {
            await writer.Database.EnsureCreatedAsync();
            var record = new CrossLayerRecord();
            property.SetValue(record, row.Value);
            writer.Set<CrossLayerRecord>().Add(record);
            await writer.SaveChangesAsync();
        }

        await using StoredOrderContext reader = StoredOrderContext.Sqlite(connection, model);
        CrossLayerRecord stored = await reader.Set<CrossLayerRecord>().SingleAsync();

        (await ColumnAsync(reader, row.Shape)).Should().Be(row.Column);
        property.GetValue(stored).Should().Be(row.Value);
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void Http_Should_WriteTheRouteAndQueryText(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);

        Result<Uri> query = new Uri("http://localhost/golden").WithQueryString("value", row.Value, EnumConventions.Default, row.Format);
        Result<HttpRequestMessage> builtQuery = HttpRequestBuilder.Get("http://localhost/golden")
            .WithQuery("value", row.Value)
            .WithEnumConventions(EnumConventions.Default, row.Format)
            .Build();
        Result<HttpRequestMessage> builtRoute = HttpRequestBuilder.Get("http://localhost/golden/{value}")
            .WithRoute("value", row.Value)
            .WithEnumConventions(EnumConventions.Default, row.Format)
            .Build();
        // The Refit adapter of the guide (design section 13.1): TryFormat, else the default ToString().
        string? refit = EnumValueFormatter.TryFormat(row.Value, EnumConventions.Default, out string? text, row.Format)
            ? text
            : row.Value?.ToString();

        query.Value.Query.TrimStart('?').Should().Be(row.Query);
        builtQuery.Value.RequestUri!.Query.TrimStart('?').Should().Be(row.Query);
        if (row.Route is null)
            builtRoute.IsFailure.Should().BeTrue();
        else
            Uri.UnescapeDataString(builtRoute.Value.RequestUri!.AbsolutePath["/golden/".Length..]).Should().Be(row.Route);
        refit.Should().Be(row.Route);
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task Swashbuckle_Should_DescribeTheParameterAndComponent(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        IReadOnlyDictionary<string, string> documents = await SwashbuckleDocuments.Value;

        string document = documents[row.Document];

        CrossLayerOpenApi.Parameter(document, row).Should().Be(row.OpenApiParameter);
        CrossLayerOpenApi.Component(document, row).Should().Be(row.OpenApiComponent);
    }

    private static Task<string?> ColumnAsync(DbContext context, string column)
    {
        string sql = "SELECT CAST(\"" + column + "\" AS TEXT) AS \"Value\" FROM golden";
        return context.Database.SqlQueryRaw<string?>(sql).SingleAsync();
    }
}
