using System.Text;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class QueryStringExtensions
{
    public static Result<string> ToQueryString(this Dictionary<string, string?> parameters)
    {
        if (parameters.Count == 0)
            return string.Empty;

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

    public static Result<string> ToQueryString(this object source)
    {
        if (source is null)
            return Error.Validation("QueryString.SourceRequired", "Source cannot be null.");

        var properties = source.GetType().GetProperties()
            .Where(p => p.CanRead)
            .Select(p => new KeyValuePair<string, string?>(p.Name, p.GetValue(source)?.ToString()))
            .ToDictionary(p => p.Key, p => p.Value);

        return properties.ToQueryString();
    }

    public static Result<Uri> WithQueryString(this Uri uri, Dictionary<string, string?> parameters)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");

        Result<string> queryResult = parameters.ToQueryString();
        if (queryResult.IsFailure)
            return queryResult.Errors;

        if (string.IsNullOrEmpty(queryResult.Value))
            return uri;

        var builder = new UriBuilder(uri);
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? queryResult.Value
            : builder.Query.TrimStart('?') + "&" + queryResult.Value;

        return builder.Uri;
    }

    public static Result<Uri> WithQueryString(this Uri uri, object parameters)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");
        if (parameters is null)
            return Error.Validation("QueryString.ParametersRequired", "Parameters cannot be null.");

        Result<string> queryResult = parameters.ToQueryString();
        if (queryResult.IsFailure)
            return queryResult.Errors;

        if (string.IsNullOrEmpty(queryResult.Value))
            return uri;

        var builder = new UriBuilder(uri);
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? queryResult.Value
            : builder.Query.TrimStart('?') + "&" + queryResult.Value;

        return builder.Uri;
    }

    public static Result<Uri> WithQueryString(this Uri uri, string name, string value)
    {
        if (uri is null)
            return Error.Validation("QueryString.UriRequired", "URI cannot be null.");
        if (string.IsNullOrEmpty(name))
            return Error.Validation("QueryString.NameRequired", "Query parameter name cannot be null or empty.");

        var builder = new UriBuilder(uri);
        string encoded = Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value);
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? encoded
            : builder.Query.TrimStart('?') + "&" + encoded;

        return builder.Uri;
    }
}
