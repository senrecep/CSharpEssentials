using System.Globalization;
using System.Text.Json.Nodes;
using CSharpEssentials.Idempotency;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Describes the <c>Idempotency-Key</c> contract of an idempotent endpoint in the Microsoft.OpenApi 2.x model, for both the
/// Swashbuckle and the Microsoft.AspNetCore.OpenApi packages: the header parameter, the <c>Idempotency-Replayed</c> header
/// of the 2xx responses, and the <c>400</c>, <c>409</c> (<see cref="IdempotencyErrorCodes.InFlight"/>, with <c>Retry-After</c>)
/// and <c>422</c> problem responses with their <c>code</c> values. An endpoint that does not enforce its key (no
/// <see cref="IdempotentAttribute"/>, <see cref="IdempotencyMode.Off"/> or <see cref="IdempotencyMode.Observe"/>) and a method
/// outside <see cref="IdempotencyOptions.Methods"/> stay unchanged.
/// </summary>
internal static class OpenApiIdempotency
{
    private const string ProblemMediaType = "application/problem+json";

    public static void Apply(OpenApiOperation operation, string? httpMethod, IEnumerable<object>? endpointMetadata, IdempotencyOptions options, string owner)
    {
        if (httpMethod is null || endpointMetadata is null || !options.Methods.Contains(httpMethod))
            return;

        IdempotencyEndpointOptions merged = new();
        bool idempotent = false;
        foreach (IdempotentAttribute attribute in endpointMetadata.OfType<IdempotentAttribute>())
        {
            idempotent = true;
            merged.MergeFrom(attribute.Overrides);
        }

        if (!idempotent)
            return;

        var settings = IdempotencyEndpointSettings.Resolve(options, merged, owner);
        if (settings.Mode != IdempotencyMode.Enforce)
            return;

        AddKeyParameter(operation, options, settings.RequireKey);
        AddReplayedHeader(operation, options);
        AddBadRequest(operation, options, settings);
        AddInFlight(operation);
        AddKeyReused(operation);
    }

    private static void AddKeyParameter(OpenApiOperation operation, IdempotencyOptions options, bool required)
    {
        string description =
            $"Makes the request safe to retry: a repeat with the same key and the same request returns the stored response with the {options.ReplayedHeaderName} header, "
            + $"and the same key with a different request is rejected. 1 to {options.MaxKeyLength.ToString(CultureInfo.InvariantCulture)} printable ASCII characters other than space, '\"' and '\\', optionally quoted.";

        operation.Parameters ??= [];
        OpenApiParameter? existing = operation.Parameters
            .OfType<OpenApiParameter>()
            .FirstOrDefault(parameter => parameter.In == ParameterLocation.Header && string.Equals(parameter.Name, options.HeaderName, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.Required |= required;
            if (string.IsNullOrWhiteSpace(existing.Description))
                existing.Description = description;
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = options.HeaderName,
            In = ParameterLocation.Header,
            Required = required,
            Description = description,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                MinLength = 1,
                MaxLength = options.MaxKeyLength,
            },
        });
    }

    private static void AddReplayedHeader(OpenApiOperation operation, IdempotencyOptions options)
    {
        if (operation.Responses is null)
            return;

        foreach ((string statusCode, IOpenApiResponse candidate) in operation.Responses)
        {
            if (candidate is not OpenApiResponse response || statusCode.Length != 3 || statusCode[0] != '2')
                continue;
            response.Headers ??= new Dictionary<string, IOpenApiHeader>(StringComparer.Ordinal);
            response.Headers[options.ReplayedHeaderName] = new OpenApiHeader
            {
                Description = "true when this response is the stored response of the first request that used the idempotency key.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Enum = [JsonValue.Create("true")] },
            };
        }
    }

    private static void AddBadRequest(OpenApiOperation operation, IdempotencyOptions options, IdempotencyEndpointSettings settings)
    {
        List<string> codes = [IdempotencyErrorCodes.KeyInvalid];
        string description = $"{IdempotencyErrorCodes.KeyInvalid}: the {options.HeaderName} header is not a valid key";
        if (settings.RequireKey)
        {
            codes.Add(IdempotencyErrorCodes.KeyMissing);
            description += $"; {IdempotencyErrorCodes.KeyMissing}: the header is required and was not sent";
        }

        if (settings.UnscopedKeyBehavior == UnscopedKeyBehavior.Reject)
        {
            codes.Add(IdempotencyErrorCodes.ScopeRequired);
            description += $"; {IdempotencyErrorCodes.ScopeRequired}: the caller's scope cannot be resolved";
        }

        AddProblem(operation, "400", description + ".", codes, headers: null);
    }

    private static void AddInFlight(OpenApiOperation operation)
    {
        var headers = new Dictionary<string, IOpenApiHeader>(StringComparer.Ordinal)
        {
            ["Retry-After"] = new OpenApiHeader
            {
                Description = "Seconds to wait before sending the same request again.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", Minimum = "1" },
            },
        };

        AddProblem(
            operation,
            "409",
            $"{IdempotencyErrorCodes.InFlight}: a request with the same idempotency key is still being processed. Retry the same request after the delay in Retry-After.",
            [IdempotencyErrorCodes.InFlight],
            headers);
    }

    private static void AddKeyReused(OpenApiOperation operation) =>
        AddProblem(
            operation,
            "422",
            $"{IdempotencyErrorCodes.KeyReused}: the idempotency key was already used for a different request. Send a new key for a different request.",
            [IdempotencyErrorCodes.KeyReused],
            headers: null);

    private static void AddProblem(OpenApiOperation operation, string statusCode, string description, IReadOnlyList<string> codes, Dictionary<string, IOpenApiHeader>? headers)
    {
        OpenApiSchema schema = new()
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string>(StringComparer.Ordinal) { "code" },
            Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal)
            {
                ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["code"] = new OpenApiSchema { Type = JsonSchemaType.String, Enum = [.. codes.Select(static code => (JsonNode)JsonValue.Create(code))] },
            },
        };

        operation.Responses ??= [];
        if (!operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? candidate) || candidate is not OpenApiResponse existing)
        {
            operation.Responses[statusCode] = new OpenApiResponse
            {
                Description = description,
                Headers = headers,
                Content = new Dictionary<string, OpenApiMediaType> { [ProblemMediaType] = new OpenApiMediaType { Schema = schema } },
            };
            return;
        }

        existing.Description = string.IsNullOrWhiteSpace(existing.Description) ? description : $"{existing.Description}\n\n{description}";
        if (headers is not null)
        {
            existing.Headers ??= new Dictionary<string, IOpenApiHeader>(StringComparer.Ordinal);
            foreach ((string name, IOpenApiHeader header) in headers)
                existing.Headers[name] = header;
        }

        existing.Content ??= new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal);
        if (existing.Content.TryGetValue(ProblemMediaType, out OpenApiMediaType? media) && media.Schema is { } current)
            media.Schema = new OpenApiSchema { OneOf = [current, schema] };
        else
            existing.Content[ProblemMediaType] = new OpenApiMediaType { Schema = schema };
    }
}
