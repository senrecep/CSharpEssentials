using System.Text;
using System.Text.Json;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

internal static class EnhancedProblemDetailsFactory
{
    internal const string ErrorsKey = "errors";
    internal const string ServerErrorDetail = "An unexpected error occurred.";

    public static EnhancedProblemDetails Create(
        IReadOnlyList<Error> errors,
        ErrorMetadata? extensions,
        int? statusCode,
        IServiceProvider? services) =>
        Create(
            errors,
            extensions,
            statusCode,
            services?.GetService<IErrorStatusCodeMapper>() ?? DefaultErrorStatusCodeMapper.Instance,
            ProblemDetailsEnrichment.GetOptions(services),
            GetFieldNamingPolicy(services));

    /// <summary>
    /// The naming policy of the <see cref="ValidationErrorsFormat.ByField"/> keys: the <c>PropertyNamingPolicy</c> of the
    /// HTTP JSON options, which also write the problem body when an <see cref="IProblemDetailsService"/> is registered.
    /// </summary>
    internal static JsonNamingPolicy? GetFieldNamingPolicy(IServiceProvider? services) =>
        services?.GetService<IOptions<HttpJsonOptions>>()?.Value.SerializerOptions.PropertyNamingPolicy;

    public static EnhancedProblemDetails Create(
        IReadOnlyList<Error> errors,
        ErrorMetadata? extensions,
        int? statusCode,
        IErrorStatusCodeMapper mapper,
        EnhancedProblemDetailsOptions options,
        JsonNamingPolicy? fieldNamingPolicy = null)
    {
        Error primary = mapper.SelectPrimaryError(errors);
        int status = statusCode ?? mapper.GetStatusCode(primary);
        bool redact = status >= StatusCodes.Status500InternalServerError && !options.ExposeExceptionDetails;
        if (redact)
            errors = Redact(errors, mapper);

        var problemDetails = new EnhancedProblemDetails
        {
            Status = status,
            Title = mapper.GetTitle(primary, status),
            Detail = redact ? ServerErrorDetail : primary.Description,
            Type = options.TypeUriResolver(status),
            Errors = errors as Error[] ?? [.. errors],
        };

        if (extensions is not null)
            foreach (KeyValuePair<string, object?> item in extensions)
                if (!redact || !IsExceptionKey(item.Key))
                    problemDetails.Extensions[item.Key] = item.Value;

        ProblemErrorFields fields = options.ErrorFields;
        if ((fields & ProblemErrorFields.Codes) != 0)
            problemDetails.ErrorCodes = [.. errors.Select(e => e.Code)];
        if ((fields & ProblemErrorFields.Messages) != 0)
            problemDetails.ErrorMessages = [.. errors.Select(e => e.Description)];

        if ((fields & ProblemErrorFields.AllErrors) != 0)
            problemDetails.Extensions[ErrorsKey] = problemDetails.Errors;
        else if ((fields & ProblemErrorFields.ValidationErrors) != 0)
            AddValidationErrors(problemDetails, errors, options.ValidationErrorsFormat, fieldNamingPolicy);

        return problemDetails;
    }

    private static Error[] Redact(IReadOnlyList<Error> errors, IErrorStatusCodeMapper mapper)
    {
        var redacted = new Error[errors.Count];
        for (int i = 0; i < errors.Count; i++)
        {
            Error error = errors[i];
            if (mapper.GetStatusCode(error) >= StatusCodes.Status500InternalServerError)
                error = error with { Description = ServerErrorDetail };
            if (error.Metadata is { } metadata && metadata.Keys.Any(IsExceptionKey))
            {
                var kept = new ErrorMetadata(metadata.Where(item => !IsExceptionKey(item.Key)));
                error = error.WithMetadata(kept.Count == 0 ? null : kept);
            }
            redacted[i] = error;
        }
        return redacted;
    }

    private static bool IsExceptionKey(string key) =>
        key is "exception" or "exceptionType" or "exceptionStackTrace" or "exceptionMessage" or "innerException" or "stackTrace";

    private static void AddValidationErrors(EnhancedProblemDetails problemDetails, IReadOnlyList<Error> errors, ValidationErrorsFormat format, JsonNamingPolicy? fieldNamingPolicy)
    {
        Error[] validationErrors = [.. errors.Where(e => e.Type == ErrorType.Validation)];
        if (validationErrors.Length == 0)
            return;

        problemDetails.Extensions[ErrorsKey] = format switch
        {
            ValidationErrorsFormat.Dictionary => GroupDescriptions(validationErrors, static e => e.Code),
            ValidationErrorsFormat.ByField => GroupDescriptions(validationErrors, e => e.Metadata?.GetField() is { } field ? ConvertPath(field, fieldNamingPolicy) : e.Code),
            _ => validationErrors.Select(e => new ProblemValidationError(e.Code, e.Description)).ToList(),
        };
    }

    /// <summary>
    /// Applies the naming policy to the name part of each dotted segment and keeps indexers:
    /// <c>Lines[1].Sku</c> becomes <c>lines[1].sku</c> under camelCase.
    /// </summary>
    internal static string ConvertPath(string path, JsonNamingPolicy? policy)
    {
        if (policy is null)
            return path;
        var builder = new StringBuilder(path.Length);
        string[] segments = path.Split('.');
        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i];
            if (i > 0)
                builder.Append('.');
            int indexer = segment.IndexOf('[', StringComparison.Ordinal);
            string name = indexer < 0 ? segment : segment[..indexer];
            builder.Append(name.Length == 0 ? name : policy.ConvertName(name));
            if (indexer >= 0)
                builder.Append(segment, indexer, segment.Length - indexer);
        }
        return builder.ToString();
    }

    private static Dictionary<string, string[]> GroupDescriptions(Error[] errors, Func<Error, string> key) =>
        errors
            .GroupBy(key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.Ordinal);
}
