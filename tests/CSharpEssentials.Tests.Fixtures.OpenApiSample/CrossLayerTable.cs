using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The cross-layer golden table (#68), taken from the normative matrix of <c>docs/design/CSharpEssentials.Enums-DESIGN.md</c>
/// (sections 5, 5.1, 10.1, 11 and 13). Every layer runs every row: System.Text.Json, ASP.NET Core binding, EF Core storage,
/// the HTTP client helpers (CSharpEssentials.Tests) and both OpenAPI outputs (Swashbuckle in CSharpEssentials.Tests,
/// Microsoft.AspNetCore.OpenApi in CSharpEssentials.AspNetCore.OpenApi.Tests). A layer that drifts fails its row.
/// </summary>
public static class CrossLayerTable
{
    private const string StatusComponent = """{"enum":["pending","pending_approval","unknown"],"type":"string"}""";
    private const string NullableStatusParameter = """{"allOf":[{"$ref":"#/components/schemas/GoldenOrderStatus"}],"nullable":true}""";
    private const string NullableStatusParameter31 = """{"oneOf":[{"$ref":"#/components/schemas/GoldenOrderStatus"},{"type":"null"}]}""";

    public static IReadOnlyList<CrossLayerRow> Rows { get; } =
    [
        // Plain enum: framework defaults everywhere (numbers in JSON and storage, ToString() in URLs), nothing throws.
        new()
        {
            Name = "plain",
            Shape = "Plain",
            Type = typeof(GoldenPlain),
            Value = GoldenPlain.InProgress,
            Json = "1",
            Route = "InProgress",
            Query = "value=InProgress",
            Header = "InProgress",
            Column = "1",
            OpenApiParameter = """{"$ref":"#/components/schemas/GoldenPlain"}""",
            OpenApiComponent = """{"enum":[0,1],"type":"integer"}""",
            MicrosoftOpenApiComponent = """{"type":"integer"}""",
        },
        // [StringEnum] with the default naming (snake_case_lower).
        new()
        {
            Name = "string-enum",
            Shape = "Status",
            Type = typeof(GoldenOrderStatus),
            Value = GoldenOrderStatus.PendingApproval,
            Json = "\"pending_approval\"",
            Route = "pending_approval",
            Query = "value=pending_approval",
            Header = "pending_approval",
            Column = "pending_approval",
            OpenApiParameter = """{"$ref":"#/components/schemas/GoldenOrderStatus"}""",
            OpenApiComponent = StatusComponent,
        },
        // A custom member name ([JsonStringEnumMemberName]).
        new()
        {
            Name = "custom-name",
            Shape = "Naming",
            Type = typeof(GoldenNaming),
            Value = GoldenNaming.Original,
            Json = "\"custom-name\"",
            Route = "custom-name",
            Query = "value=custom-name",
            Header = "custom-name",
            Column = "custom-name",
            OpenApiParameter = """{"$ref":"#/components/schemas/GoldenNaming"}""",
            OpenApiComponent = """{"enum":["custom-name","plain_value"],"type":"string"}""",
        },
        // [Flags]: the composite ReadWrite is written as its single flags; stored as an integer (FlagsStorage = Integer).
        new()
        {
            Name = "flags",
            Shape = "Permissions",
            Type = typeof(GoldenPermissions),
            Value = GoldenPermissions.ReadWrite,
            Json = """["read","write"]""",
            Route = "read,write",
            Query = "value=read&value=write",
            Header = "read,write",
            Column = "3",
            OpenApiParameter = """{"items":{"$ref":"#/components/schemas/GoldenPermissions"},"type":"array","uniqueItems":true}""",
            OpenApiComponent = """{"enum":["none","read","write","delete","read_write"],"type":"string"}""",
        },
        // Numeric wire: WriteAs = Number on every layer, Storage = Integer.
        new()
        {
            Name = "numeric-wire",
            Shape = "Priority",
            Type = typeof(GoldenPriority),
            Value = GoldenPriority.High,
            Format = EnumWireFormat.Number,
            Json = "2",
            Route = "2",
            Query = "value=2",
            Header = "2",
            Column = "2",
            OpenApiParameter = """{"$ref":"#/components/schemas/GoldenPriority"}""",
            OpenApiComponent = """{"enum":[0,1,2],"type":"integer"}""",
        },
        // Nullable with a value: the same wire text as the non-nullable enum; nullability lives on the usage site.
        new()
        {
            Name = "nullable",
            Shape = "OptionalStatus",
            Type = typeof(GoldenOrderStatus?),
            Value = GoldenOrderStatus.Pending,
            Json = "\"pending\"",
            Route = "pending",
            Query = "value=pending",
            Header = "pending",
            Column = "pending",
            OpenApiParameter = NullableStatusParameter,
            OpenApi31Parameter = NullableStatusParameter31,
            OpenApiComponent = StatusComponent,
        },
        // Nullable without a value: JSON null, no route value, no query parameter, no header, SQL NULL.
        new()
        {
            Name = "nullable-null",
            Shape = "OptionalStatus",
            Type = typeof(GoldenOrderStatus?),
            Value = null,
            Json = "null",
            Route = null,
            Query = "",
            Header = null,
            Column = null,
            OpenApiParameter = NullableStatusParameter,
            OpenApi31Parameter = NullableStatusParameter31,
            OpenApiComponent = StatusComponent,
        },
    ];

    /// <summary>The row named <paramref name="name"/>.</summary>
    public static CrossLayerRow Get(string name) => Rows.Single(row => row.Name == name);
}
