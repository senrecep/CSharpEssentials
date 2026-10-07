using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Describes optional route parameters with valid OpenAPI (see <see cref="OptionalRouteParameterMode.SplitPaths"/> and
/// <see cref="OptionalRouteParameterMode.RequiredOnly"/>). Every path parameter of an operation with an optional or
/// catch-all route parameter is marked required. In <see cref="OptionalRouteParameterMode.SplitPaths"/>, an operation whose
/// route ends with one to <see cref="MaxSplitParameters"/> optional parameters is also described, as a copy, on the paths
/// without them. A form whose path and method another operation already has is skipped. An operation with more trailing optional
/// parameters is logged once per document as a warning.
/// </summary>
internal sealed partial class OptionalRouteParameterDocumentFilter(IServiceProvider services, OptionalRouteParameterSettings settings) : IDocumentFilter
{
    internal const int MaxSplitParameters = 3;
    private const char PathSeparator = '/';

    // Keyed by the host's logger factory (a root singleton): the filter can be created per document generation.
    private static readonly ConditionalWeakTable<ILoggerFactory, ConcurrentDictionary<(string Document, string Path, HttpMethod Method), bool>> _warnings = [];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Paths is not { Count: > 0 } paths)
            return;

        Func<ApiDescription, string> pathGroupSelector = services.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value.PathGroupSelector;
        HashSet<string> operationIds = new(
            paths.Values.SelectMany(static item => item.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
                .Select(static operation => operation.OperationId)
                .OfType<string>()
                .Where(static id => id.Length > 0),
            StringComparer.Ordinal);
        HashSet<(string Path, HttpMethod Method)> processed = [];
        Dictionary<string, OpenApiPathItem> newItems = [with(StringComparer.Ordinal)];
        Dictionary<string, List<string>> newPathsByAnchor = [with(StringComparer.Ordinal)];

        foreach (ApiDescription description in context.ApiDescriptions)
        {
            if (description.HttpMethod is not { } httpMethod || description.RelativePath is not { } relativePath)
                continue;

            string path = PathSeparator + pathGroupSelector(description);
            HttpMethod method = new(httpMethod);
            if (!processed.Add((path, method))
                || !paths.TryGetValue(path, out IOpenApiPathItem? item)
                || item.Operations is null
                || !item.Operations.TryGetValue(method, out OpenApiOperation? operation))
                continue;

            if (RouteShape.Analyze(description, relativePath) is not { HasOptionalOrCatchAll: true } shape)
                continue;

            MarkPathParametersRequired(operation);
            if (settings.Mode != OptionalRouteParameterMode.SplitPaths || shape.TrailingOptional.Count == 0)
                continue;
            if (shape.TrailingOptional.Count > MaxSplitParameters)
            {
                WarnNotSplit(context.DocumentName, path, method, shape.TrailingOptional.Count);
                continue;
            }
            if (!EndsWithParameters(path, shape.TrailingOptional))
                continue;

            string[] segments = path.Split(PathSeparator);
            for (int omit = shape.TrailingOptional.Count; omit >= 1; omit--)
            {
                string shortPath = string.Join(PathSeparator, segments, 0, segments.Length - omit);
                if (shortPath.Length == 0)
                    shortPath = PathSeparator.ToString();
                if (HasOperation(paths, shortPath, method) || newItems.TryGetValue(shortPath, out OpenApiPathItem? pending) && pending.Operations?.ContainsKey(method) == true)
                    continue;

                paths.TryGetValue(shortPath, out IOpenApiPathItem? existing);
                if (existing is not null and not OpenApiPathItem)
                    continue;

                IReadOnlyList<string> omitted = [.. shape.TrailingOptional.Skip(shape.TrailingOptional.Count - omit)];
                OpenApiOperation form = CreateForm(operation, omitted, operationIds);
                if (existing is OpenApiPathItem concrete)
                {
                    (concrete.Operations ??= [])[method] = form;
                    continue;
                }

                if (!newItems.TryGetValue(shortPath, out OpenApiPathItem? newItem))
                {
                    newItem = new OpenApiPathItem { Operations = [] };
                    newItems[shortPath] = newItem;
                    if (!newPathsByAnchor.TryGetValue(path, out List<string>? anchored))
                        newPathsByAnchor[path] = anchored = [];
                    anchored.Add(shortPath);
                }
                newItem.Operations![method] = form;
            }
        }

        if (newItems.Count > 0)
            swaggerDoc.Paths = InsertBeforeAnchors(paths, newItems, newPathsByAnchor);
    }

    private OpenApiOperation CreateForm(OpenApiOperation operation, IReadOnlyList<string> omitted, HashSet<string> operationIds)
    {
        OpenApiOperation form = OpenApiOperationCloner.Clone(operation);
        if (form.Parameters is { } parameters)
        {
            for (int index = parameters.Count - 1; index >= 0; index--)
            {
                if (parameters[index] is { In: ParameterLocation.Path, Name: { } name } && omitted.Contains(name, StringComparer.OrdinalIgnoreCase))
                    parameters.RemoveAt(index);
            }
        }
        MarkPathParametersRequired(form);

        if (string.IsNullOrEmpty(operation.OperationId))
        {
            form.OperationId = null;
            return form;
        }

        string operationId = settings.OperationIdSelector is { } selector
            ? selector(operation.OperationId, omitted)
            : operation.OperationId + "Without" + string.Join("And", omitted.Select(ToPascalCase));
        if (!operationIds.Add(operationId))
        {
            throw new InvalidOperationException(
                $"The operationId '{operationId}' generated for operation '{operation.OperationId}' without the optional route parameter(s) "
                + $"'{string.Join("', '", omitted)}' is already used. Rename one of the operations or pass an operationIdSelector to AddOptionalRouteParameters.");
        }
        form.OperationId = operationId;
        return form;
    }

    private void WarnNotSplit(string documentName, string path, HttpMethod method, int count)
    {
        if (services.GetService<ILoggerFactory>() is not { } loggerFactory)
            return;
        if (_warnings.GetValue(loggerFactory, static _ => []).TryAdd((documentName, path, method), true))
            LogNotSplit(loggerFactory.CreateLogger<OptionalRouteParameterDocumentFilter>(), documentName, method.Method, path, count, MaxSplitParameters);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "OpenAPI document '{DocumentName}': {Method} {Path} ends with {Count} optional route parameters, more than the {MaxSplitParameters} " +
            "that SplitPaths describes as separate paths; it is described on one path with every parameter required.")]
    private static partial void LogNotSplit(ILogger logger, string documentName, string method, string path, int count, int maxSplitParameters);

    private static void MarkPathParametersRequired(OpenApiOperation operation)
    {
        foreach (IOpenApiParameter candidate in operation.Parameters ?? [])
        {
            if (candidate is not OpenApiParameter { In: ParameterLocation.Path } parameter)
                continue;

            parameter.Required = true;
            parameter.AllowEmptyValue = false;
            if (parameter.Schema is not OpenApiSchema schema)
                continue;

            bool nullType = schema.Type is { } type && type.HasFlag(JsonSchemaType.Null);
            bool nullDefault = schema.Default is { } value && (JsonNullSentinel.IsJsonNullSentinel(value) || value.GetValueKind() == JsonValueKind.Null);
            if (!nullType && !nullDefault)
                continue;

            // A path segment is never empty, so a required path parameter cannot be null.
            var copy = (OpenApiSchema)schema.CreateShallowCopy();
            if (nullType)
                copy.Type &= ~JsonSchemaType.Null;
            if (nullDefault)
                copy.Default = null;
            parameter.Schema = copy;
        }
    }

    private static bool EndsWithParameters(string path, IReadOnlyList<string> names)
    {
        string[] segments = path.Split(PathSeparator);
        if (segments.Length - 1 < names.Count)
            return false;
        for (int index = 0; index < names.Count; index++)
        {
            if (!string.Equals(segments[segments.Length - names.Count + index], "{" + names[index] + "}", StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    private static bool HasOperation(OpenApiPaths paths, string path, HttpMethod method) =>
        paths.TryGetValue(path, out IOpenApiPathItem? item) && item.Operations?.ContainsKey(method) == true;

    private static OpenApiPaths InsertBeforeAnchors(OpenApiPaths paths, Dictionary<string, OpenApiPathItem> newItems, Dictionary<string, List<string>> newPathsByAnchor)
    {
        var ordered = new OpenApiPaths { Extensions = paths.Extensions };
        foreach (KeyValuePair<string, IOpenApiPathItem> pair in paths)
        {
            if (newPathsByAnchor.TryGetValue(pair.Key, out List<string>? anchored))
            {
                foreach (string shortPath in anchored.Where(shortPath => !ordered.ContainsKey(shortPath)))
                    ordered.Add(shortPath, newItems[shortPath]);
            }
            ordered.Add(pair.Key, pair.Value);
        }
        return ordered;
    }

    private static string ToPascalCase(string name) =>
        name.Length == 0 ? name : char.ToUpper(name[0], CultureInfo.InvariantCulture) + name[1..];

    private sealed record RouteShape(bool HasOptionalOrCatchAll, IReadOnlyList<string> TrailingOptional)
    {
        public static RouteShape? Analyze(ApiDescription description, string relativePath)
        {
            // Parsing is skipped for routes that cannot have an optional or catch-all parameter.
            if (relativePath.AsSpan().IndexOfAny('?', '=', '*') < 0
                && !description.ParameterDescriptions.Any(static candidate => candidate.RouteInfo?.IsOptional == true))
                return null;

            RoutePattern pattern;
            try
            {
                pattern = RoutePatternFactory.Parse(relativePath);
            }
            catch (RoutePatternException)
            {
                return null;
            }

            bool hasOptionalOrCatchAll = pattern.Parameters.Any(parameter => parameter.IsCatchAll || IsOptional(description, parameter));

            List<string> trailing = [];
            for (int index = pattern.PathSegments.Count - 1; index >= 0; index--)
            {
                RoutePatternPathSegment segment = pattern.PathSegments[index];
                if (!segment.IsSimple || segment.Parts[0] is not RoutePatternParameterPart parameter || parameter.IsCatchAll || !IsOptional(description, parameter))
                    break;
                trailing.Insert(0, parameter.Name);
            }
            return new RouteShape(hasOptionalOrCatchAll, trailing);
        }

        // MVC reports a sanitized RelativePath ("{id}" for "{id?}"), so its optional parameters are known from RouteInfo only.
        private static bool IsOptional(ApiDescription description, RoutePatternParameterPart parameter) =>
            !parameter.IsCatchAll
            && (parameter.IsOptional
                || parameter.Default is not null
                || description.ParameterDescriptions.Any(candidate =>
                    candidate.RouteInfo is { IsOptional: true } && string.Equals(candidate.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)));
    }
}
