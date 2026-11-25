using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CSharpEssentials.Errors;

public sealed class ErrorMetadata : Dictionary<string, object?>
{
    /// <summary>
    /// The key of the property path a validation error belongs to (for example <c>Address.City</c>).
    /// CSharpEssentials.Validation sets it on every error a rule chain records.
    /// </summary>
    public const string FieldKey = "field";

    public ErrorMetadata() { }
    public ErrorMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }
    public ErrorMetadata(params IEnumerable<KeyValuePair<string, object?>> collection) : base(collection) { }

    public ErrorMetadata(KeyValuePair<string, object?> keyValuePair) : this([keyValuePair]) { }
    public ErrorMetadata(string key, object? value) : this(new KeyValuePair<string, object?>(key, value)) { }

    public static ErrorMetadata CreateEmpty() => [];
    public static ErrorMetadata CreateWithStackTrace() => new("stackTrace", Environment.StackTrace);
    public static ErrorMetadata CreateWithException(Exception ex) => new("exception", CreateWithExceptionDetailed(ex));
    public static ErrorMetadata CreateWithExceptionDetailed(Exception exception)
    {
        var metadata = new ErrorMetadata()
        {
                { "exceptionType", exception.GetType().Name},
                { "exceptionStackTrace", exception.StackTrace },
                { "exceptionMessage", exception.Message },
        };
        if (exception.InnerException is not null)
            metadata.Add("innerException", CreateWithExceptionDetailed(exception.InnerException));

        return metadata;
    }

    /// <summary>
    /// Returns the <see cref="FieldKey"/> value, or <see langword="null"/> when it is missing or not a string.
    /// A string read back from JSON (a <see cref="JsonElement"/>) is returned too.
    /// </summary>
    public string? GetField() =>
        TryGetValue(FieldKey, out object? value)
            ? value switch
            {
                string field => field,
                JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
                _ => null,
            }
            : null;

    public ErrorMetadata AddMetadata(string key, object? value)
    {
        TryAdd(key, value);
        return this;
    }


    public ErrorMetadata AddStackTrace()
    {
        TryAdd("stackTrace", Environment.StackTrace);
        return this;
    }

    public ErrorMetadata AddException(Exception ex)
    {
        TryAdd("exception", CreateWithExceptionDetailed(ex));
        return this;
    }

    public ErrorMetadata AddExceptionDetailed(Exception exception)
    {
        TryAdd("exceptionType", exception.GetType().Name);
        TryAdd("exceptionStackTrace", exception.StackTrace);
        TryAdd("exceptionMessage", exception.Message);

        if (exception.InnerException is not null)
            TryAdd("innerException", CreateWithExceptionDetailed(exception.InnerException));

        return this;
    }

    public ErrorMetadata Combine(ErrorMetadata? metadata)
    {
        if (metadata is null)
            return this;
        foreach ((string key, object value) in metadata)
            TryAdd(key, value);
        return this;
    }

    public override string ToString()
    {
        var sb = new StringBuilder("[");
        foreach ((string key, object? value) in this)
        {
            if (sb.Length > 1)
                sb.Append(", ");
#if NET6_0_OR_GREATER
            sb.Append(CultureInfo.InvariantCulture, $"{{ {key}: {value} }}");
#else
            sb.AppendFormat(CultureInfo.InvariantCulture, "{{ {0}: {1} }}", key, value);
#endif
        }
        return sb.Append(']').ToString();
    }
}
