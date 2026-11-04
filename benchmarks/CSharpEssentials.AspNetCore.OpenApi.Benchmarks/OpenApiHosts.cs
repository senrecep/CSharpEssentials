using System.Text.Json.Serialization;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Benchmarks;

/// <summary>
/// One build function per row, shared by the warm class, the first-document class and the self-check. Every host
/// registers the same app services (idempotency in Enforce mode and the CSE enum conventions) and maps the same sample
/// API; the rows differ only in the document options. The StringEnums row registers <c>JsonStringEnumConverter</c>
/// instead of the CSE enum conventions.
/// </summary>
internal static class OpenApiHosts
{
    public const string PlainEnum01 = """component {"description":"Enum 01 of the sample API: six defined, contiguous members."}; query status {"$ref":"#/components/schemas/Enum01"}""";
    public const string PlainStringEnumsEnum01 = """component {"enum":["Alpha","Beta","Gamma","Delta","Epsilon","Zeta"],"description":"Enum 01 of the sample API: six defined, contiguous members."}; query status {"$ref":"#/components/schemas/Enum01"}""";

    public const string PlainStage = """{"oneOf":[{"type":"null"},{"description":"A nullable enum.","$ref":"#/components/schemas/Enum02"}]}""";

    public static readonly PlainShapes PlainShapes = new(PlainEnum01, "\"beta\"", PlainStage);

    public static readonly PlainShapes StringEnumsShapes = new(PlainStringEnumsEnum01, "\"Beta\"", PlainStage);

    public static ApiDocsHost Plain() => Start(static _ => { });

    public static ApiDocsHost PlainStringEnums() => Start(static _ => { }, stringEnums: true);

    public static ApiDocsHost EnumConventions() => Start(static o => o.AddEnumConventions());

    public static ApiDocsHost IdempotencyConventions() => Start(static o => o.AddIdempotencyConventions());

    public static ApiDocsHost OptionalRouteParameters() => Start(static o => o.AddOptionalRouteParameters());

    public static ApiDocsHost AllConventions() => Start(static o => o.AddEnumConventions().AddIdempotencyConventions().AddOptionalRouteParameters());

    private static ApiDocsHost Start(Action<OpenApiOptions> document, bool stringEnums = false) => ApiDocsHost.Start(
        builder =>
        {
            builder.Services.AddIdempotency(static o => o.Mode = IdempotencyMode.Enforce);
            if (stringEnums)
                builder.Services.ConfigureHttpJsonOptions(static o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            else
                builder.Services.AddEnumConventions();
            builder.Services.AddOpenApi("v1", document);
        },
        static app =>
        {
            app.MapSampleApi();
            app.MapOpenApi();
        });
}
