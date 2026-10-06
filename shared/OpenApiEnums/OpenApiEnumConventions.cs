using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The enum conventions as the OpenAPI document generators see them: which enums are handled (the same selection as the JSON
/// converter) and the output format of every operation and document (design section 10.1).
/// </summary>
internal sealed partial class OpenApiEnumConventions
{
    internal const string WireFormatExtension = "x-enum-wire-format";
    internal const string WireFormatHeaderExtension = "x-enum-wire-format-header";

    private static readonly ConditionalWeakTable<IServiceProvider, OpenApiEnumConventions> _instances = [];

    // Keyed by the host's logger factory (a root singleton): the conventions can be created per request scope.
    private static readonly ConditionalWeakTable<ILoggerFactory, ConcurrentDictionary<(string Document, Type EnumType), bool>> _mixedWarnings = [];

    private readonly IServiceProvider _services;
    private readonly EnumConventionsRegistration? _registration;
    private readonly EnumConventions _conventions;
    private readonly ConcurrentDictionary<Type, IEnumInfo?> _infos = [];
    private readonly ConcurrentDictionary<(Type, EnumWireFormat), EnumSchemaContent> _contents = [];
    private readonly ConcurrentDictionary<string, DocumentEnumFormat> _documents = new(StringComparer.Ordinal);

    private OpenApiEnumConventions(IServiceProvider services)
    {
        _services = services;
        _registration = services.GetService<EnumConventionsRegistration>();
        _conventions = _registration?.Conventions ?? services.GetService<EnumConventions>() ?? EnumConventions.Default;
    }

    /// <summary>One instance per service provider, shared by the filters or transformers of a host.</summary>
    public static OpenApiEnumConventions For(IServiceProvider services) =>
        _instances.GetValue(services, static provider => new OpenApiEnumConventions(provider));

    /// <summary>The global output format, <see cref="EnumConventions.WriteAs"/>.</summary>
    public EnumWireFormat WriteAs => _conventions.WriteAs;

    /// <summary>
    /// The metadata of an enum the conventions handle, or <see langword="null"/> for an enum left to the framework. Without
    /// <c>AddEnumConventions</c> the defaults apply (generated metadata or <see cref="StringEnumAttribute"/>).
    /// </summary>
    public IEnumInfo? Resolve(Type enumType) => _infos.GetOrAdd(enumType, ResolveCore);

    /// <summary>The component content of <paramref name="info"/> in <paramref name="format"/>.</summary>
    public EnumSchemaContent GetContent(IEnumInfo info, EnumWireFormat format) =>
        _contents.GetOrAdd((info.EnumType, format), static (_, state) => EnumSchemaContent.Create(state.info, state.format), (info, format));

    /// <summary>
    /// The enum format of a document: <see cref="EnumWireFormat.Number"/> when every operation of the document writes numbers,
    /// otherwise <see cref="EnumWireFormat.String"/> (a mixed document marks its number operations); <see cref="WriteAs"/> for a
    /// document without operations. Cached per document until the API descriptions change.
    /// </summary>
    public DocumentEnumFormat GetDocumentFormat(string documentName, Func<ApiDescription, bool> include)
    {
        ApiDescriptionGroupCollection groups = _services.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups;
        if (_documents.TryGetValue(documentName, out DocumentEnumFormat? cached) && cached.Version == groups.Version)
            return cached;

        Dictionary<ActionDescriptor, Endpoint> endpoints = IndexEndpoints();
        Dictionary<ActionDescriptor, OperationEnumFormat> operations = [with(ReferenceEqualityComparer.Instance)];
        List<ApiDescription> descriptions = [];
        foreach (ApiDescriptionGroup group in groups.Items)
        {
            foreach (ApiDescription description in group.Items)
            {
                if (!include(description))
                    continue;
                operations[description.ActionDescriptor] = GetOperationFormat(description.ActionDescriptor, endpoints);
                descriptions.Add(description);
            }
        }

        EnumWireFormat format = WriteAs;
        if (operations.Count > 0)
            format = operations.Values.All(static operation => operation.Format == EnumWireFormat.Number) ? EnumWireFormat.Number : EnumWireFormat.String;
        if (format == EnumWireFormat.String && operations.Values.Any(static operation => operation.Format == EnumWireFormat.Number))
            WarnMixedEnums(documentName, descriptions, operations);
        var document = new DocumentEnumFormat(groups.Version, format, operations);
        _documents[documentName] = document;
        return document;
    }

    /// <summary>
    /// Logs once per document and enum when the enum is written as strings by some operations and as numbers by others: its one
    /// component shows the string form, so clients of the number operations must read the value table (design section 10.1).
    /// </summary>
    private void WarnMixedEnums(string documentName, List<ApiDescription> descriptions, Dictionary<ActionDescriptor, OperationEnumFormat> operations)
    {
        JsonSerializerOptions jsonOptions = _services.GetService<IOptions<HttpJsonOptions>>()?.Value.SerializerOptions ?? JsonSerializerOptions.Default;
        HashSet<Type> stringEnums = [];
        HashSet<Type> numberEnums = [];
        HashSet<Type> stringVisited = [];
        HashSet<Type> numberVisited = [];
        foreach (ApiDescription description in descriptions)
        {
            bool number = operations[description.ActionDescriptor].Format == EnumWireFormat.Number;
            HashSet<Type> enums = number ? numberEnums : stringEnums;
            HashSet<Type> visited = number ? numberVisited : stringVisited;
            foreach (ApiParameterDescription parameter in description.ParameterDescriptions)
            {
                // MVC reports string as the Type of a parameter whose type converts from string (enums do); its model type is the enum.
                CollectEnums(parameter.Type == typeof(string) ? parameter.ModelMetadata?.ModelType : parameter.Type, jsonOptions, enums, visited);
            }

            foreach (ApiResponseType response in description.SupportedResponseTypes)
                CollectEnums(response.Type, jsonOptions, enums, visited);
        }

        stringEnums.IntersectWith(numberEnums);
        if (stringEnums.Count == 0)
            return;

        if (_services.GetService<ILoggerFactory>() is not { } loggerFactory)
            return;
        ILogger logger = loggerFactory.CreateLogger<OpenApiEnumConventions>();
        ConcurrentDictionary<(string Document, Type EnumType), bool> warned = _mixedWarnings.GetValue(loggerFactory, static _ => []);
        foreach (Type enumType in stringEnums.OrderBy(static type => type.FullName, StringComparer.Ordinal))
        {
            if (warned.TryAdd((documentName, enumType), true))
                LogMixedEnum(logger, documentName, enumType.FullName ?? enumType.Name);
        }
    }

    private void CollectEnums(Type? type, JsonSerializerOptions jsonOptions, HashSet<Type> enums, HashSet<Type> visited)
    {
        if (type is null || type == typeof(void) || type.IsPointer || type.IsByRef || type.ContainsGenericParameters)
            return;
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (!visited.Add(type))
            return;
        if (type.IsEnum)
        {
            if (Resolve(type) is not null)
                enums.Add(type);
            return;
        }

        JsonTypeInfo typeInfo;
        try
        {
            typeInfo = jsonOptions.GetTypeInfo(type);
        }
        catch (Exception exception) when (exception is NotSupportedException or InvalidOperationException or ArgumentException)
        {
            // Types the serializer cannot describe (Stream, delegates, ...) carry no enum values.
            return;
        }

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
                CollectEnums(property.PropertyType, jsonOptions, enums, visited);
            foreach (JsonDerivedType derived in typeInfo.PolymorphismOptions?.DerivedTypes ?? [])
                CollectEnums(derived.DerivedType, jsonOptions, enums, visited);
        }
        else if (typeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary)
        {
            CollectEnums(typeInfo.KeyType, jsonOptions, enums, visited);
            CollectEnums(typeInfo.ElementType, jsonOptions, enums, visited);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "OpenAPI document '{DocumentName}' writes enum {EnumType} as strings in some operations and as numbers in others; " +
            "its schema shows the string form and the number operations are marked with x-enum-wire-format: number.")]
    private static partial void LogMixedEnum(ILogger logger, string documentName, string enumType);

    private IEnumInfo? ResolveCore(Type enumType)
    {
        if (_registration is not null)
            return _registration.Resolve(enumType);
        return EnumMetadata.TryGet(enumType, out IEnumInfo? info) && _conventions.CanHandle(enumType) ? info : null;
    }

    private Dictionary<ActionDescriptor, Endpoint> IndexEndpoints()
    {
        Dictionary<ActionDescriptor, Endpoint> index = [with(ReferenceEqualityComparer.Instance)];
        if (_services.GetService<EndpointDataSource>() is not { } dataSource)
            return index;
        foreach (Endpoint endpoint in dataSource.Endpoints)
        {
            if (endpoint.Metadata.GetMetadata<ActionDescriptor>() is { } action)
                index.TryAdd(action, endpoint);
        }

        return index;
    }

    private OperationEnumFormat GetOperationFormat(ActionDescriptor action, Dictionary<ActionDescriptor, Endpoint> endpoints)
    {
        if (FindMetadata(action, endpoints) is not { } metadata)
            return new OperationEnumFormat(WriteAs, null);

        // A header selector is described by its default branch: the format of a request without the header.
        EnumWireFormat format = metadata.SelectFormat(new DefaultHttpContext { RequestServices = _services });
        return new OperationEnumFormat(format, metadata.VaryHeader);
    }

    private static IEnumWireFormatMetadata? FindMetadata(ActionDescriptor action, Dictionary<ActionDescriptor, Endpoint> endpoints)
    {
        // Same precedence as the response filters: action > controller > endpoint metadata (group, MapControllers()).
        if (action is ControllerActionDescriptor controllerAction)
        {
            if (controllerAction.MethodInfo.GetCustomAttribute<EnumWireFormatAttribute>(inherit: true) is { } method)
                return method;
            if (controllerAction.ControllerTypeInfo.GetCustomAttribute<EnumWireFormatAttribute>(inherit: true) is { } controller)
                return controller;
        }

        if (endpoints.TryGetValue(action, out Endpoint? endpoint) && endpoint.Metadata.GetMetadata<IEnumWireFormatMetadata>() is { } metadata)
            return metadata;
        return action.EndpointMetadata?.OfType<IEnumWireFormatMetadata>().LastOrDefault();
    }
}

/// <summary>The output format of one operation; <see cref="Header"/> is the request header of a header selector.</summary>
internal sealed record OperationEnumFormat(EnumWireFormat Format, string? Header);

/// <summary>The enum format of one document and of each of its operations.</summary>
internal sealed class DocumentEnumFormat(int version, EnumWireFormat format, IReadOnlyDictionary<ActionDescriptor, OperationEnumFormat> operations)
{
    internal const string NumberNote =
        "Enum values in this operation are written as numbers; the enum schemas of this document show the string form " +
        "(the numbers are listed in each enum's value table).";

    public int Version { get; } = version;

    /// <summary>The format the enum components of the document are described in.</summary>
    public EnumWireFormat Format { get; } = format;

    /// <summary>
    /// The markers of an operation: <c>x-enum-wire-format: number</c> when it writes numbers in a string document, and
    /// <c>x-enum-wire-format-header</c> when a request header selects the format; <see langword="null"/> when it has none.
    /// </summary>
    public OperationEnumMarkers? GetMarkers(ActionDescriptor action)
    {
        if (!operations.TryGetValue(action, out OperationEnumFormat? operation))
            return null;

        bool differs = operation.Format != Format;
        if (!differs && operation.Header is null)
            return null;

        string? wireFormat = null;
        if (differs)
            wireFormat = operation.Format == EnumWireFormat.Number ? "number" : "string";
        List<string> notes = [];
        if (differs && operation.Format == EnumWireFormat.Number)
            notes.Add(NumberNote);
        if (operation.Header is { } header)
            notes.Add($"The enum output format of this operation depends on the `{header}` request header; the schemas show the format of a request without it.");
        return new OperationEnumMarkers(wireFormat, operation.Header, string.Join(" ", notes));
    }
}

/// <summary>The extensions and the description note of an operation (see <see cref="DocumentEnumFormat.GetMarkers"/>).</summary>
internal sealed record OperationEnumMarkers(string? WireFormat, string? Header, string Note)
{
    /// <summary>Appends <see cref="Note"/> to the operation description once.</summary>
    public string AppendNote(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Note;
        return description.Contains(Note, StringComparison.Ordinal) ? description : $"{description.TrimEnd()}\n\n{Note}";
    }
}
