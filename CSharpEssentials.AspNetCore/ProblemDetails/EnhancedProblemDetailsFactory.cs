using CSharpEssentials.Errors;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

internal static class EnhancedProblemDetailsFactory
{
    internal const string ErrorsKey = "errors";

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
            ProblemDetailsEnrichment.GetOptions(services));

    public static EnhancedProblemDetails Create(
        IReadOnlyList<Error> errors,
        ErrorMetadata? extensions,
        int? statusCode,
        IErrorStatusCodeMapper mapper,
        EnhancedProblemDetailsOptions options)
    {
        Error primary = mapper.SelectPrimaryError(errors);
        int status = statusCode ?? mapper.GetStatusCode(primary);

        var problemDetails = new EnhancedProblemDetails
        {
            Status = status,
            Title = mapper.GetTitle(primary, status),
            Detail = primary.Description,
            Type = options.TypeUriResolver(status),
            Errors = errors as Error[] ?? [.. errors],
        };

        if (extensions is not null)
            foreach (KeyValuePair<string, object?> item in extensions)
                problemDetails.Extensions[item.Key] = item.Value;

        ProblemErrorFields fields = options.ErrorFields;
        if ((fields & ProblemErrorFields.Codes) != 0)
            problemDetails.ErrorCodes = [.. errors.Select(e => e.Code)];
        if ((fields & ProblemErrorFields.Messages) != 0)
            problemDetails.ErrorMessages = [.. errors.Select(e => e.Description)];

        if ((fields & ProblemErrorFields.AllErrors) != 0)
            problemDetails.Extensions[ErrorsKey] = problemDetails.Errors;
        else if ((fields & ProblemErrorFields.ValidationErrors) != 0)
            AddValidationErrors(problemDetails, errors, options.ValidationErrorsFormat);

        return problemDetails;
    }

    private static void AddValidationErrors(EnhancedProblemDetails problemDetails, IReadOnlyList<Error> errors, ValidationErrorsFormat format)
    {
        Error[] validationErrors = [.. errors.Where(e => e.Type == ErrorType.Validation)];
        if (validationErrors.Length == 0)
            return;

        problemDetails.Extensions[ErrorsKey] = format == ValidationErrorsFormat.Dictionary
            ? validationErrors
                .GroupBy(e => e.Code, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.Ordinal)
            : validationErrors.Select(e => new ProblemValidationError(e.Code, e.Description)).ToList();
    }
}
