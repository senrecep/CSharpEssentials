using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The endpoints both OpenAPI packages describe in the bearer security tests, and the extract compared with the shared
/// <c>bearer-security</c> golden files: <c>components.securitySchemes</c> and the <c>security</c> of every operation
/// (<c>null</c> when it has none).
/// </summary>
public static class BearerSecurityApi
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    /// <summary>The controllers of the bearer security tests.</summary>
    public static IReadOnlyList<Type> Controllers { get; } = [typeof(BearerSecurityController), typeof(BearerSecurityAnonymousController)];

    /// <summary>Maps the Minimal API endpoints of the bearer security tests.</summary>
    /// <param name="app">The application.</param>
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/bearer/secure", static () => "secure").RequireAuthorization();
        app.MapGet("/bearer/anonymous", static () => "anonymous").RequireAuthorization().AllowAnonymous();
        app.MapGet("/bearer/plain", static () => "plain");
        app.MapGet("/bearer/policy", static () => "policy").RequireAuthorization(static policy => policy.RequireAuthenticatedUser());
        app.MapGet("/bearer/policy-object", static () => "policy-object")
            .RequireAuthorization(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        RouteGroupBuilder group = app.MapGroup("/bearer/group").RequireAuthorization();
        group.MapGet("/secure", static () => "secure");
        group.MapGet("/anonymous", static () => "anonymous").AllowAnonymous();
    }

    /// <summary>The security extract of <paramref name="openApiJson"/>, indented, with <c>\n</c> line ends.</summary>
    /// <param name="openApiJson">The OpenAPI document.</param>
    /// <returns>The extract.</returns>
    public static string Extract(string openApiJson)
    {
        JsonNode root = JsonNode.Parse(openApiJson) ?? throw new ArgumentException("Not a JSON document.", nameof(openApiJson));
        var operations = new JsonObject();
        foreach ((string path, JsonNode? item) in root["paths"]!.AsObject().OrderBy(static p => p.Key, StringComparer.Ordinal))
        {
            foreach ((string method, JsonNode? operation) in item!.AsObject())
                operations[$"{method.ToUpperInvariant()} {path}"] = operation!["security"]?.DeepClone();
        }

        var extract = new JsonObject
        {
            ["securitySchemes"] = root["components"]?["securitySchemes"]?.DeepClone(),
            ["operations"] = operations,
        };
        return extract.ToJsonString(_indented).Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}
