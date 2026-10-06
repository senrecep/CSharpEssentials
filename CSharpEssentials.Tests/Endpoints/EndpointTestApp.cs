using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Endpoints;

internal static class EndpointTestApp
{
    public static WebApplication Create(CapturingLoggerProvider? loggerProvider = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        if (loggerProvider is not null)
        {
            builder.Logging.AddProvider(loggerProvider);
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
        }

        return builder.Build();
    }

    public static RouteEndpoint[] Endpoints(IEndpointRouteBuilder app) =>
        [.. app.DataSources.SelectMany(static source => source.Endpoints).OfType<RouteEndpoint>()];

    public static RouteEndpoint Single(IEndpointRouteBuilder app) => Endpoints(app).Single();
}
