using System.Globalization;
using System.Text;

namespace CSharpEssentials.Errors;

public sealed class ErrorMetadata : Dictionary<string, object?>
{
    public ErrorMetadata() { }
    public ErrorMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }
    public ErrorMetadata(params IEnumerable<KeyValuePair<string, object?>> collection) : base(collection) { }

    public ErrorMetadata(KeyValuePair<string, object?> keyValuePair) : this([keyValuePair]) { }
    public ErrorMetadata(string key, object? value) : this(new KeyValuePair<string, object?>(key, value)) { }

    public static ErrorMetadata CreateEmpty() => [];
    public static ErrorMetadata CreateWithStackTrace() => new("stackTrace", Environment.StackTrace);
    public static ErrorMetadata CreateWithException(Exception ex) => new("exception", ex);
    public static ErrorMetadata CreateWithExceptionDetailed(Exception exception)
    {
        var metadata = new ErrorMetadata()
        {
                { "exceptionType", exception.GetType().Name},
                { "exceptionStackTrace", exception.StackTrace ?? Environment.StackTrace},
                { "exceptionMessage", exception.Message },
        };
        if (exception.InnerException is not null)
            metadata.Add("innerException", CreateWithExceptionDetailed(exception.InnerException));

        return metadata;
    }

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
        TryAdd("exception", ex);
        return this;
    }

    public ErrorMetadata AddExceptionDetailed(Exception exception)
    {
        TryAdd("exceptionType", exception.GetType().Name);
        TryAdd("exceptionStackTrace", exception.StackTrace ?? Environment.StackTrace);
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
        var sb = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            (string? key, object? value) = this.ElementAtOrDefault(i);
            sb.Append(string.Create(CultureInfo.InvariantCulture, $"{{ {key}: {value} }}"));
            if (i < Count - 1)
                sb.Append(", ");
        }
        return $"[{sb}]";
    }
}
