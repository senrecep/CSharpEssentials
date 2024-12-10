namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;

internal sealed class DefaultRequestResponseMiddleware(RequestDelegate next, ILogWriter logWriter, string[] ignoredPaths) : BaseMiddleware(logWriter, ignoredPaths)
{
    public Task InvokeAsync(HttpContext httpContext) => IsIgnoredPath(httpContext) ? next(httpContext) : InvokeMiddleware(next, httpContext);
}
