using System.Globalization;
using System.Reflection;
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

[StringEnum]
internal enum SwStatus
{
    Active = 0,
    InProgress = 1,
    HTTPStatus = 2,
}

/// <summary>Not marked with [StringEnum]: must stay an integer schema.</summary>
internal enum SwPlain
{
    First = 0,
    Second = 1,
}

internal sealed class SwQueryDto
{
    public SwStatus Status { get; set; }

    public SwStatus? Optional { get; set; }

    public SwPlain Plain { get; set; }
}

/// <summary>
/// Internal so the default <c>ControllerFeatureProvider</c> never discovers the controller in other test hosts.
/// </summary>
internal static class SwaggerParameterControllers
{
    [ApiController]
    [Route("")]
    internal sealed class SwApiController : ControllerBase
    {
        [HttpGet("items/{status}")]
        public string Route(SwStatus status) => status.ToString();

        [HttpGet("query")]
        public string Query([FromQuery] SwStatus status) => status.ToString();

        [HttpGet("nullable")]
        public string Nullable([FromQuery] SwStatus? status) => status.ToString();

        [HttpGet("list")]
        public string List([FromQuery] List<SwStatus> status) => status.Count.ToString(CultureInfo.InvariantCulture);

        [HttpGet("dto")]
        public string Dto([FromQuery] SwQueryDto dto) => dto.Status.ToString();

        [HttpGet("plain")]
        public string Plain([FromQuery] SwPlain plain) => plain.ToString();

        [HttpGet("plain/{plain}")]
        public string PlainRoute(SwPlain plain) => plain.ToString();
    }
}

public class EnumSchemaFilterParameterTests
{
    private static readonly string[] _expectedNames = ["active", "in_progress", "http_status"];

    public enum Kind
    {
        MinimalApi,
        Mvc,
    }

    public static TheoryData<Kind, string, string, bool> StringEnumParameters()
    {
        var data = new TheoryData<Kind, string, string, bool>();
        foreach (Kind kind in Enum.GetValues<Kind>())
        {
            data.Add(kind, "/items/{status}", "status", false);
            data.Add(kind, "/query", "status", false);
            data.Add(kind, "/nullable", "status", false);
            data.Add(kind, "/list", "status", true);
            data.Add(kind, "/dto", "status", false);
            data.Add(kind, "/dto", "optional", false);
        }
        return data;
    }

    public static TheoryData<Kind, string, string> IntegerEnumParameters()
    {
        var data = new TheoryData<Kind, string, string>();
        foreach (Kind kind in Enum.GetValues<Kind>())
        {
            data.Add(kind, "/plain", "plain");
            data.Add(kind, "/plain/{plain}", "plain");
            data.Add(kind, "/dto", "plain");
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(StringEnumParameters))]
    public async Task Parameter_Should_BeStringEnumSchema_When_EnumIsMarkedWithStringEnum(Kind kind, string path, string name, bool isArray)
    {
        using JsonDocument document = await GetSwaggerAsync(kind);

        JsonElement schema = ResolveParameterSchema(document, path, name, isArray);

        schema.GetProperty("type").GetString().Should().Be("string");
        schema.TryGetProperty("format", out _).Should().BeFalse();
        schema.GetProperty("enum").EnumerateArray().Select(e => e.GetString()).Should().Equal(_expectedNames);
    }

    [Theory]
    [MemberData(nameof(StringEnumParameters))]
    public async Task Parameter_Should_ListWireNamesInDescription_When_EnumIsMarkedWithStringEnum(Kind kind, string path, string name, bool isArray)
    {
        using JsonDocument document = await GetSwaggerAsync(kind);

        JsonElement schema = ResolveParameterSchema(document, path, name, isArray);

        schema.GetProperty("description").GetString().Should().Be(
            "| value | number | description |\n|---|---|---|\n| `active` | 0 |  |\n| `in_progress` | 1 |  |\n| `http_status` | 2 |  |");
    }

    [Theory]
    [MemberData(nameof(IntegerEnumParameters))]
    public async Task Parameter_Should_StayInteger_When_EnumIsNotMarkedWithStringEnum(Kind kind, string path, string name)
    {
        using JsonDocument document = await GetSwaggerAsync(kind);

        JsonElement schema = ResolveParameterSchema(document, path, name, isArray: false);

        schema.GetProperty("type").GetString().Should().Be("integer");
        schema.TryGetProperty("enum", out JsonElement values).Should().BeTrue();
        values.EnumerateArray().Select(e => e.GetInt32()).Should().Equal(0, 1);
    }

    private static async Task<JsonDocument> GetSwaggerAsync(Kind kind)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        var scheme = new OpenApiSecurityScheme
        {
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
        };
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(SecuritySchemes.JwtBearerSchemeName, scheme, typeof(EnumSchemaFilter).Assembly);

        if (kind == Kind.Mvc)
        {
            builder.Services.AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                        manager.FeatureProviders.Remove(provider);
                    manager.FeatureProviders.Add(new SingleControllerFeatureProvider(typeof(SwaggerParameterControllers.SwApiController)));
                });
        }

        await using WebApplication app = builder.Build();
        app.UseVersionableSwagger();
        if (kind == Kind.MinimalApi)
            MapMinimalApi(app);
        else
            app.MapControllers();

        await app.StartAsync();
        using HttpClient client = app.GetTestClient();
        using HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");
        string body = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue(body);
        await app.StopAsync();
        return JsonDocument.Parse(body);
    }

    private static void MapMinimalApi(WebApplication app)
    {
        app.MapGet("/items/{status}", (SwStatus status) => status.ToString());
        app.MapGet("/query", (SwStatus status) => status.ToString());
        app.MapGet("/nullable", (SwStatus? status) => status.ToString());
        app.MapGet("/list", (SwStatus[] status) => status.Length.ToString(CultureInfo.InvariantCulture));
        app.MapGet("/dto", ([AsParameters] SwQueryDto p) => p.Status.ToString());
        app.MapGet("/plain", (SwPlain plain) => plain.ToString());
        app.MapGet("/plain/{plain}", (SwPlain plain) => plain.ToString());
    }

    private static JsonElement ResolveParameterSchema(JsonDocument document, string path, string name, bool isArray)
    {
        JsonElement parameters = document.RootElement.GetProperty("paths").GetProperty(path).GetProperty("get").GetProperty("parameters");
        JsonElement parameter = parameters.EnumerateArray()
            .Single(p => string.Equals(p.GetProperty("name").GetString(), name, StringComparison.OrdinalIgnoreCase));
        JsonElement schema = Resolve(document, parameter.GetProperty("schema"));
        if (isArray)
        {
            schema.GetProperty("type").GetString().Should().Be("array");
            schema = Resolve(document, schema.GetProperty("items"));
        }
        return schema;
    }

    private static JsonElement Resolve(JsonDocument document, JsonElement schema)
    {
        while (true)
        {
            if (schema.TryGetProperty("$ref", out JsonElement reference))
            {
                string id = reference.GetString()!.Split('/')[^1];
                schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(id);
            }
            else if (schema.TryGetProperty("allOf", out JsonElement allOf) && allOf.GetArrayLength() == 1)
            {
                schema = allOf[0];
            }
            else
            {
                return schema;
            }
        }
    }

    private sealed class SingleControllerFeatureProvider(Type controller) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo info = controller.GetTypeInfo();
            if (!feature.Controllers.Contains(info))
                feature.Controllers.Add(info);
        }
    }
}
