using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Describes optional route parameters with valid OpenAPI. Every path parameter of an operation with an optional or catch-all
/// route parameter is marked required. With split paths, an operation whose route ends with one to
/// <see cref="MaxSplitParameters"/> optional parameters is also described, as a copy, on the paths without them. A form whose
/// path and method another operation already has is skipped. An operation with more trailing optional parameters is logged once
/// per document, path and method as a warning.
/// </summary>
internal sealed partial class OptionalRouteParameterSplitter(
    bool splitPaths,
    Func<string, IReadOnlyList<string>, string>? operationIdSelector,
    ILoggerFactory? loggerFactory,
    string loggerCategory)
{
    internal const int MaxSplitParameters = 3;
    private const char PathSeparator = '/';

    // Keyed by the host's logger factory (a root singleton): the filter or transformer can be created per document generation.
    private static readonly ConditionalWeakTable<ILoggerFactory, ConcurrentDictionary<(string Document, string Path, HttpMethod Method), bool>> _warnings = [];

    /// <summary>Applies the optional route parameter forms to <paramref name="document"/>.</summary>
    /// <param name="document">The generated document.</param>
    /// <param name="documentName">The document name, for the warning.</param>
    /// <param name="descriptions">The API descriptions of the document.</param>
    /// <param name="pathOf">The key of the description's path in <see cref="OpenApiDocument.Paths"/>.</param>
    public void Apply(OpenApiDocument document, string documentName, IEnumerable<ApiDescription> descriptions, Func<ApiDescription, string> pathOf)
    {
        if (document.Paths is not { Count: > 0 } paths)
            return;

        HashSet<string> operationIds = new(
            paths.Values.SelectMany(static item => item.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
                .Select(static operation => operation.OperationId)
                .OfType<string>()
                .Where(static id => id.Length > 0),
            StringComparer.Ordinal);
        HashSet<(string Path, HttpMethod Method)> processed = [];
        Dictionary<string, OpenApiPathItem> newItems = [with(StringComparer.Ordinal)];
        Dictionary<string, List<string>> newPathsByAnchor = [with(StringComparer.Ordinal)];

        foreach (ApiDescription description in descriptions)
        {
            if (description.HttpMethod is not { } httpMethod || description.RelativePath is not { } relativePath)
                continue;

            string path = pathOf(description);
            HttpMethod method = new(httpMethod);
            if (!processed.Add((path, method))
                || !paths.TryGetValue(path, out IOpenApiPathItem? item)
                || item.Operations is null
                || !item.Operations.TryGetValue(method, out OpenApiOperation? operation))
                continue;

            if (OptionalRouteShape.Analyze(description, relativePath) is not { HasOptionalOrCatchAll: true } shape)
                continue;

            MarkPathParametersRequired(operation);
            if (!splitPaths || shape.TrailingOptional.Count == 0)
                continue;
            if (shape.TrailingOptional.Count > MaxSplitParameters)
            {
                WarnNotSplit(documentName, path, method, shape.TrailingOptional.Count);
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
            document.Paths = InsertBeforeAnchors(paths, newItems, newPathsByAnchor);
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

        string operationId = operationIdSelector is { } selector
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
        if (loggerFactory is null)
            return;
        if (_warnings.GetValue(loggerFactory, static _ => []).TryAdd((documentName, path, method), true))
            LogNotSplit(loggerFactory.CreateLogger(loggerCategory), documentName, method.Method, path, count, MaxSplitParameters);
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
}
