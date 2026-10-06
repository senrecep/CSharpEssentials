using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>One row of the cross-layer golden table: a value and the wire text every layer must produce or accept for it.</summary>
public sealed record CrossLayerRow
{
    /// <summary>The test case name.</summary>
    public required string Name { get; init; }

    /// <summary>The endpoint group <c>/golden/{Shape}</c> of <see cref="CrossLayerApi"/> and the EF Core property that holds the value.</summary>
    public required string Shape { get; init; }

    /// <summary>The declared CLR type, nullable for the nullable rows.</summary>
    public required Type Type { get; init; }

    public required object? Value { get; init; }

    /// <summary>The output format of every layer; the shape's endpoints use the same format.</summary>
    public EnumWireFormat Format { get; init; } = EnumWireFormat.String;

    /// <summary>System.Text.Json and the ASP.NET Core response body.</summary>
    public required string Json { get; init; }

    /// <summary>The route value (unescaped) written by <c>HttpRequestBuilder.WithRoute</c> and bound by ASP.NET Core; <see langword="null"/>: no route value.</summary>
    public required string? Route { get; init; }

    /// <summary>The query string written by the HTTP query helpers and bound by ASP.NET Core; empty: no parameter.</summary>
    public required string Query { get; init; }

    /// <summary>The header value bound by ASP.NET Core; <see langword="null"/>: no header.</summary>
    public required string? Header { get; init; }

    /// <summary>The stored EF Core column value as text; <see langword="null"/>: SQL <c>NULL</c>.</summary>
    public required string? Column { get; init; }

    /// <summary>The schema of the bound value parameter (route, query and header), keys sorted (both OpenAPI 3.0 outputs).</summary>
    public required string OpenApiParameter { get; init; }

    /// <summary>
    /// The Microsoft.AspNetCore.OpenApi parameter schema in an OpenAPI 3.1 document where it differs from <see cref="OpenApiParameter"/>:
    /// only for nullable rows, which 3.1 describes with <c>oneOf</c> and <c>{"type":"null"}</c> instead of <c>nullable</c>.
    /// </summary>
    public string? OpenApi31Parameter { get; init; }

    /// <summary>The <c>type</c> and <c>enum</c> of the enum component (both OpenAPI 3.0 outputs unless <see cref="MicrosoftOpenApiComponent"/> is set).</summary>
    public required string OpenApiComponent { get; init; }

    /// <summary>
    /// The Microsoft.AspNetCore.OpenApi component where it differs from <see cref="OpenApiComponent"/> (Swashbuckle): only for plain
    /// enums, whose framework schemas the conventions leave alone (Swashbuckle lists the numbers, Microsoft.AspNetCore.OpenApi does not).
    /// </summary>
    public string? MicrosoftOpenApiComponent { get; init; }

    /// <summary>The OpenAPI document that describes the shape: <c>v1</c> writes numbers, <c>v2</c> strings.</summary>
    public string Document => Format == EnumWireFormat.Number ? "v1" : "v2";

    /// <summary>The enum type without <see cref="Nullable{T}"/>.</summary>
    public Type EnumType => Nullable.GetUnderlyingType(Type) ?? Type;

    public override string ToString() => Name;
}
