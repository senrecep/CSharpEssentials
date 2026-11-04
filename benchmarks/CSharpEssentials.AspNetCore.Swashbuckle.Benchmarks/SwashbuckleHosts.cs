using System.Text.Json.Serialization;
using CSharpEssentials.Idempotency;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

/// <summary>
/// One build function per row, shared by the warm class, the first-document class and the self-check. Every host
/// registers the same app services (idempotency in Enforce mode, the CSE enum conventions and the endpoint API
/// explorer) and maps the same sample API; the rows differ only in the document options. The StringEnums row registers
/// <c>JsonStringEnumConverter</c> instead of the CSE enum conventions.
/// </summary>
internal static class SwashbuckleHosts
{
    public const string PlainEnum01 = """component {"enum":["alpha","beta","gamma","delta","epsilon","zeta"],"type":"string"}; query status {"$ref":"#/components/schemas/Enum01"}""";
    public const string PlainStringEnumsEnum01 = """component {"enum":["Alpha","Beta","Gamma","Delta","Epsilon","Zeta"],"type":"string"}; query status {"$ref":"#/components/schemas/Enum01"}""";

    public const string PlainStage = """{"$ref":"#/components/schemas/Enum02"}""";

    public static readonly PlainShapes PlainShapes = new(PlainEnum01, "<none>", PlainStage);

    public static readonly PlainShapes StringEnumsShapes = new(PlainStringEnumsEnum01, "<none>", PlainStage);

    public static ApiDocsHost Plain() => Start(static _ => { });

    public static ApiDocsHost PlainStringEnums() => Start(static _ => { }, stringEnums: true);

    public static ApiDocsHost EnumConventions() => Start(static o => o.AddEnumConventions());

    public static ApiDocsHost IdempotencyConventions() => Start(static o => o.AddIdempotencyConventions());

    public static ApiDocsHost OptionalRouteParameters() => Start(static o => o.AddOptionalRouteParameters());

    public static ApiDocsHost AllConventions() => Start(static o => o.AddEnumConventions().AddIdempotencyConventions().AddOptionalRouteParameters());

    public static ApiDocsHost AddSwagger() => ApiDocsHost.Start(
        static builder =>
        {
            AddAppServices(builder, stringEnums: false);
            builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(SecuritySchemes.JwtBearerSchemeName, SecuritySchemes.JwtBearerTokenSecurity);
        },
        static app =>
        {
            app.MapSampleApi();
            app.UseSwagger(static s => s.RouteTemplate = "swagger/{documentName}/swagger.json");
        });

    private static ApiDocsHost Start(Action<SwaggerGenOptions> document, bool stringEnums = false) => ApiDocsHost.Start(
        builder =>
        {
            AddAppServices(builder, stringEnums);
            builder.Services.AddSwaggerGen(document);
        },
        static app =>
        {
            app.MapSampleApi();
            app.UseSwagger();
        });

    private static void AddAppServices(WebApplicationBuilder builder, bool stringEnums)
    {
        builder.Services.AddIdempotency(static o => o.Mode = IdempotencyMode.Enforce);
        if (stringEnums)
            builder.Services.ConfigureHttpJsonOptions(static o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        else
            builder.Services.AddEnumConventions();
        builder.Services.AddEndpointsApiExplorer();
    }
}
