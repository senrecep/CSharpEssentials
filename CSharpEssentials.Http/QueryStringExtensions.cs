using System.Collections;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Globalization;
using System.Text;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class QueryStringExtensions
{
    public static Result<string> ToQueryString(this Dictionary<string, string?> parameters) =>
        BuildQuery(parameters);

#if NET7_0_OR_GREATER
    [RequiresUnreferencedCode("Reads the public properties of the source type through reflection.")]
    [RequiresDynamicCode("Reads the public properties of the source type through reflection.")]
#endif
    public static Result<string> ToQueryString(this object? source) =>
        source.ToQueryString(EnumConventions.Default);

    public static Result<string> ToQueryString(this object? source, EnumConventions conventions, EnumWireFormat? format = null)
    {
        if (source is null)
            return Error.Validation("QueryString.SourceRequired", "Source cannot be null.");
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));

        List<KeyValuePair<string, string?>> pairs = [];
        try
        {
            foreach (System.Reflection.PropertyInfo property in source.GetType().GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
                AddValues(pairs, property.Name, property.GetValue(source), conventions, format);
        }
        catch (EnumValueException exception)
        {
            return Error.Validation("QueryString.InvalidEnumValue", exception.Message);
        }

        return BuildQuery(pairs);
    }

    public static Result<Uri> WithQueryString(this Uri? uri, Dictionary<string, string?> parameters)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");

        return uri.AppendQuery(parameters.ToQueryString());
    }

    public static Result<Uri> WithQueryString(this Uri? uri, object? parameters) =>
        uri.WithQueryString(parameters, EnumConventions.Default);

    public static Result<Uri> WithQueryString(this Uri? uri, object? parameters, EnumConventions conventions, EnumWireFormat? format = null)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");
        if (parameters is null)
            return Error.Validation("QueryString.ParametersRequired", "Parameters cannot be null.");

        return uri.AppendQuery(parameters.ToQueryString(conventions, format));
    }

    public static Result<Uri> WithQueryString(this Uri? uri, string name, string value)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");
        if (string.IsNullOrEmpty(name))
            return Error.Validation("QueryString.NameRequired", "Query parameter name cannot be null or empty.");

        return uri.AppendQuery(Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value));
    }

    public static Result<Uri> WithQueryString(this Uri? uri, string name, object? value, EnumConventions? conventions = null, EnumWireFormat? format = null)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");
        if (string.IsNullOrEmpty(name))
            return Error.Validation("QueryString.NameRequired", "Query parameter name cannot be null or empty.");

        List<KeyValuePair<string, string?>> pairs = [];
        try
        {
            AddValues(pairs, name, value, conventions ?? EnumConventions.Default, format);
        }
        catch (EnumValueException exception)
        {
            return Error.Validation("QueryString.InvalidEnumValue", exception.Message);
        }

        return uri.AppendQuery(BuildQuery(pairs));
    }

    internal static Result<string> BuildQuery(IEnumerable<KeyValuePair<string, string?>> parameters)
    {
        var builder = new StringBuilder();
        foreach (KeyValuePair<string, string?> parameter in parameters)
        {
            if (string.IsNullOrEmpty(parameter.Key))
                return Error.Validation("QueryString.EmptyKey", "Query parameter key cannot be null or empty.");

            if (parameter.Value is null)
                continue;

            if (builder.Length > 0)
                builder.Append('&');

            builder.Append(Uri.EscapeDataString(parameter.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(parameter.Value));
        }

        return builder.ToString();
    }

    internal static void AddValues(
        List<KeyValuePair<string, string?>> pairs,
        string name,
        object? value,
        EnumConventions conventions,
        EnumWireFormat? format)
    {
        if (value is null)
            return;

        if (value is not string)
        {
            List<string> formatted = [];
            if (EnumValueFormatter.TryFormatMany(value, conventions, formatted, format))
            {
                foreach (string text in formatted)
                    pairs.Add(new KeyValuePair<string, string?>(name, text));
                return;
            }

            if (value is IEnumerable items)
            {
                foreach (object? item in items)
                    AddItem(pairs, name, item, conventions, format);
                return;
            }
        }

        AddItem(pairs, name, value, conventions, format);
    }

    private static void AddItem(
        List<KeyValuePair<string, string?>> pairs,
        string name,
        object? value,
        EnumConventions conventions,
        EnumWireFormat? format)
    {
        if (value is null)
            return;

        string? text = EnumValueFormatter.TryFormat(value, conventions, out string? formatted, format)
            ? formatted
            : Convert.ToString(value, CultureInfo.InvariantCulture);
        pairs.Add(new KeyValuePair<string, string?>(name, text));
    }

    internal static Result<Uri> AppendQuery(this Uri uri, Result<string> queryResult)
    {
        if (queryResult.IsFailure)
            return queryResult.Errors;

        return uri.AppendQuery(queryResult.Value);
    }

    private static Result<Uri> AppendQuery(this Uri uri, string query)
    {
        if (string.IsNullOrEmpty(query))
            return uri;

        var builder = new UriBuilder(uri);
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? query
            : builder.Query.TrimStart('?') + "&" + query;

        return builder.Uri;
    }
}
