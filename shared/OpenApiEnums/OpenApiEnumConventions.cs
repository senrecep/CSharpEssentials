using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The enum conventions as the OpenAPI document generators see them: which enums are handled (the same selection as the JSON
/// converter) and the output format of every operation and document (design section 10.1).
/// </summary>
internal sealed class OpenApiEnumConventions
{
    internal const string WireFormatExtension = "x-enum-wire-format";
    internal const string WireFormatHeaderExtension = "x-enum-wire-format-header";

    private static readonly ConditionalWeakTable<IServiceProvider, OpenApiEnumConventions> _instances = [];

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
        foreach (ApiDescriptionGroup group in groups.Items)
        {
            foreach (ApiDescription description in group.Items)
            {
                if (include(description))
                    operations[description.ActionDescriptor] = GetOperationFormat(description.ActionDescriptor, endpoints);
            }
        }

        EnumWireFormat format = WriteAs;
        if (operations.Count > 0)
            format = operations.Values.All(static operation => operation.Format == EnumWireFormat.Number) ? EnumWireFormat.Number : EnumWireFormat.String;
        var document = new DocumentEnumFormat(groups.Version, format, operations);
        _documents[documentName] = document;
        return document;
    }

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
