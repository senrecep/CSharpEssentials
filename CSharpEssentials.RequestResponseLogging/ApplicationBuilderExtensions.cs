using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.RequestResponseLogging;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder AddRequestResponseLogging(this IApplicationBuilder appBuilder,
                                                                       Action<RequestResponseOptions> options)
    {
        var opt = new RequestResponseOptions();
        options(opt);


        ILogWriter logWriter = opt.LoggerFactory is null
                                ? new NullLogWriter()
                                : new LoggerFactoryLogWriter(opt.LoggerFactory, opt.LoggingOptions);

        if (opt.HandlerUsing)
            appBuilder.UseMiddleware<DefaultRequestResponseWithHandlerMiddleware>(opt.ReqResHandler, logWriter, opt.IgnoredPaths);
        else
            appBuilder.UseMiddleware<DefaultRequestResponseMiddleware>(logWriter, opt.IgnoredPaths);

        return appBuilder;
    }



}
