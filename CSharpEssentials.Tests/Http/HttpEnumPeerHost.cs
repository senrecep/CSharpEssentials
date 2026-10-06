using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AspResults = Microsoft.AspNetCore.Http.Results;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Http;

public sealed record HttpEchoResponse(string[] Raw, long[] Parsed);

public sealed record HttpBodyEchoResponse(JsonElement Raw, HttpOrderPayload Order);

public sealed record HttpStatusResponse(HttpOrderStatus Status);

/// <summary>
/// A TestServer peer with strict input parsing (<see cref="EnumReadMode.Input"/>) that writes enums in the format chosen by StartAsync:
/// <see cref="EnumWireFormat.Number"/> for a pre-5.0 producer, <see cref="EnumWireFormat.String"/> for a 5.0 producer.
/// </summary>
internal sealed class HttpEnumPeerHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private HttpEnumPeerHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public static async Task<HttpEnumPeerHost> StartAsync(EnumWireFormat writeAs, EnumConventions? conventions = null)
    {
        EnumConventions serverConventions = conventions ?? EnumConventions.Default;
        JsonSerializerOptions requestOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .AddEnumConventions(serverConventions, EnumReadMode.Input);
        JsonSerializerOptions responseOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .AddEnumConventions(serverConventions, EnumReadMode.Data, writeAs);

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        WebApplication app = builder.Build();

        app.MapGet("/route/{status}", (string status) => ParseStatuses(serverConventions, [status]));
        app.MapGet("/query", (HttpContext context) =>
        {
            string[] statuses = context.Request.Query["status"].ToArray()!;
            string[] permissions = context.Request.Query["permissions"].ToArray()!;
            return ParseQuery(serverConventions, statuses, permissions) is { } echo
                ? AspResults.Json(echo)
                : AspResults.StatusCode(StatusCodes.Status400BadRequest);
        });
        app.MapPost("/body", async (HttpContext context) =>
        {
            string raw = await new StreamReader(context.Request.Body).ReadToEndAsync();
            HttpOrderPayload? order;
            try
            {
                order = JsonSerializer.Deserialize<HttpOrderPayload>(raw, requestOptions);
            }
            catch (JsonException)
            {
                return AspResults.StatusCode(StatusCodes.Status400BadRequest);
            }

            using JsonDocument document = JsonDocument.Parse(raw);
            string json = JsonSerializer.Serialize(new HttpBodyEchoResponse(document.RootElement.Clone(), order!), responseOptions);
            return AspResults.Text(json, "application/json");
        });
        app.MapGet("/status", (string value) => AspResults.Text("""{"status":""" + value + "}", "application/json"));
        app.MapGet("/status/current", () => AspResults.Text(JsonSerializer.Serialize(new HttpStatusResponse(HttpOrderStatus.PendingApproval), responseOptions), "application/json"));

        await app.StartAsync();
        return new HttpEnumPeerHost(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }

    private static IResult ParseStatuses(EnumConventions conventions, string[] values) =>
        ParseQuery(conventions, values, []) is { } echo ? AspResults.Json(echo) : AspResults.StatusCode(StatusCodes.Status400BadRequest);

    private static HttpEchoResponse? ParseQuery(EnumConventions conventions, string[] statuses, string[] permissions)
    {
        List<long> parsed = [];
        foreach (string status in statuses)
        {
            if (!EnumValueParser.TryParse(status, EnumReadMode.Input, conventions, out HttpOrderStatus value, out _))
                return null;
            parsed.Add((long)value);
        }

        if (permissions.Length > 0)
        {
            HttpPermissions flags = HttpPermissions.None;
            foreach (string permission in permissions)
            {
                if (!EnumValueParser.TryParse(permission, EnumReadMode.Input, conventions, out HttpPermissions value, out _))
                    return null;
                flags |= value;
            }

            parsed.Add((long)flags);
        }

        return new HttpEchoResponse([.. statuses, .. permissions], [.. parsed]);
    }
}
