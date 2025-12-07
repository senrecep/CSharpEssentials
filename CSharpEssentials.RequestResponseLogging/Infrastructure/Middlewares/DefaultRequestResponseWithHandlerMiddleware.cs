namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;

internal sealed class DefaultRequestResponseWithHandlerMiddleware : BaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Func<RequestResponseContext, Task> _reqResHandler;

    public DefaultRequestResponseWithHandlerMiddleware(RequestDelegate next,
                                                   Func<RequestResponseContext, Task> reqResHandler,
                                                   ILogWriter logWriter,
                                                   string[] ignoredPaths) : base(logWriter, ignoredPaths)
    {
        _next = next;
        _reqResHandler = reqResHandler;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (IsIgnoredPath(httpContext))
        {
            await _next(httpContext);
            return;
        }

        RequestResponseContext reqResContext = await InvokeMiddleware(_next, httpContext);

        await _reqResHandler.Invoke(reqResContext);
    }
}
