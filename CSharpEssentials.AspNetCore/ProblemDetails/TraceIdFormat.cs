namespace CSharpEssentials.AspNetCore;

/// <summary>
/// How the <c>traceId</c> extension of a problem response is written.
/// </summary>
public enum TraceIdFormat
{
    /// <summary>
    /// The 32 hex character W3C trace id (<c>Activity.TraceId</c>). Falls back to <c>HttpContext.TraceIdentifier</c>
    /// when there is no W3C activity (for example when tracing is disabled), so the response always carries a trace id.
    /// </summary>
    W3CTraceId = 0,

    /// <summary>
    /// The full <c>traceparent</c> value (<c>Activity.Id</c>), falling back to <c>HttpContext.TraceIdentifier</c>.
    /// This is the ASP.NET Core default and the 3.x behavior.
    /// </summary>
    TraceparentHeader = 1,

    /// <summary>
    /// No <c>traceId</c> extension.
    /// </summary>
    None = 2,
}
