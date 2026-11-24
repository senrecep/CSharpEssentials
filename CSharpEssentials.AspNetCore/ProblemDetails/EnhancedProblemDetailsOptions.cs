namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Controls what goes into problem responses written by CSharpEssentials and by the framework
/// (status code pages, exception handler) once <c>AddEnhancedProblemDetails</c> is registered.
/// Defaults are privacy-first; <see cref="UseLegacyDefaults"/> restores the 3.x output.
/// </summary>
public sealed class EnhancedProblemDetailsOptions
{
    /// <summary>
    /// Format of the <c>traceId</c> extension. Default: <see cref="TraceIdFormat.W3CTraceId"/>.
    /// </summary>
    public TraceIdFormat TraceId { get; set; } = TraceIdFormat.W3CTraceId;

    /// <summary>
    /// Writes <c>user</c> (the authenticated identity name). Default: <see langword="false"/>.
    /// </summary>
    public bool IncludeUser { get; set; }

    /// <summary>
    /// Writes <c>spanId</c> and <c>parentSpanId</c>. Default: <see langword="false"/>.
    /// </summary>
    public bool IncludeSpanIds { get; set; }

    /// <summary>
    /// Writes <c>requestId</c> (<c>HttpContext.TraceIdentifier</c>). Default: <see langword="false"/>.
    /// </summary>
    public bool IncludeRequestId { get; set; }

    /// <summary>
    /// How <c>instance</c> is filled. Default: <see cref="ProblemInstanceFormat.Path"/>.
    /// </summary>
    public ProblemInstanceFormat Instance { get; set; } = ProblemInstanceFormat.Path;

    /// <summary>
    /// Error collections written for problems built from errors.
    /// Default: <see cref="ProblemErrorFields.Codes"/> | <see cref="ProblemErrorFields.ValidationErrors"/>.
    /// </summary>
    public ProblemErrorFields ErrorFields { get; set; } = ProblemErrorFields.Codes | ProblemErrorFields.ValidationErrors;

    /// <summary>
    /// Shape of <c>errors</c> for <see cref="ProblemErrorFields.ValidationErrors"/>. Default: <see cref="ValidationErrorsFormat.List"/>.
    /// </summary>
    public ValidationErrorsFormat ValidationErrorsFormat { get; set; } = ValidationErrorsFormat.List;

    /// <summary>
    /// Maps a status code to <c>type</c>. Used when <c>type</c> is empty or is a framework default link.
    /// Return <see langword="null"/> to leave <c>type</c> empty. Default: <see cref="ProblemTypeUris.Rfc9110"/>.
    /// </summary>
    public Func<int, string?> TypeUriResolver { get; set; } = ProblemTypeUris.Rfc9110;

    /// <summary>
    /// Writes an <c>exception</c> extension (type, message, stack trace) for unhandled exceptions, and keeps the
    /// error description and the exception metadata (<c>exceptionType</c>, <c>exceptionMessage</c>,
    /// <c>exceptionStackTrace</c>, <c>innerException</c>, <c>exception</c>, <c>stackTrace</c>) in 5xx problems built
    /// from errors. When <see langword="false"/>, a 5xx problem gets a fixed <c>detail</c>, its 5xx errors get the
    /// same fixed description and those metadata keys are removed.
    /// Enable only in development. Default: <see langword="false"/>.
    /// </summary>
    public bool ExposeExceptionDetails { get; set; }

    /// <summary>
    /// Restores the 3.x output: traceparent <c>traceId</c>, <c>user</c>, span ids, <c>requestId</c>,
    /// <c>"METHOD /path"</c> instance, every error collection and RFC 7231 type links.
    /// </summary>
    public EnhancedProblemDetailsOptions UseLegacyDefaults()
    {
        TraceId = TraceIdFormat.TraceparentHeader;
        IncludeUser = true;
        IncludeSpanIds = true;
        IncludeRequestId = true;
        Instance = ProblemInstanceFormat.MethodAndPath;
        ErrorFields = ProblemErrorFields.All;
        TypeUriResolver = ProblemTypeUris.Rfc7231;
        return this;
    }
}
