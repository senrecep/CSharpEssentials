namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;

internal sealed class DefaultRequestResponseMiddleware : BaseMiddleware
{
    private readonly RequestDelegate _next;

    public DefaultRequestResponseMiddleware(RequestDelegate next, ILogWriter logWriter, string[] ignoredPaths) : base(logWriter, ignoredPaths) => _next = next;

    public Task InvokeAsync(HttpContext httpContext) => IsIgnoredPath(httpContext) ? _next(httpContext) : InvokeMiddleware(_next, httpContext);
}
