namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;

internal sealed class DefaultRequestResponseWithHandlerMiddleware(RequestDelegate next,
                                                   Func<RequestResponseContext, Task> reqResHandler,
                                                   ILogWriter logWriter,
                                                   string[] ignoredPaths) : BaseMiddleware(logWriter, ignoredPaths)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (IsIgnoredPath(httpContext))
        {
            await next(httpContext);
            return;
        }

        RequestResponseContext reqResContext = await InvokeMiddleware(next, httpContext);

        await reqResHandler.Invoke(reqResContext);
    }
}
