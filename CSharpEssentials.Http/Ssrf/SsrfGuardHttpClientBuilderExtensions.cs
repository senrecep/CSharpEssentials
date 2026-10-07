#if NET9_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Http;

public static class SsrfGuardHttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddSsrfGuard(this IHttpClientBuilder builder, Action<SsrfGuardOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        string name = builder.Name;
        OptionsBuilder<SsrfGuardOptions> options = builder.Services.AddOptions<SsrfGuardOptions>(name);
        if (configure is not null)
            options.Configure(configure);

        builder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            SsrfGuardOptions guardOptions = serviceProvider.GetRequiredService<IOptionsMonitor<SsrfGuardOptions>>().Get(name);
            IOutboundAddressPolicy addressPolicy = guardOptions.AddressPolicy
                ?? serviceProvider.GetService<IOutboundAddressPolicy>()
                ?? DefaultOutboundAddressPolicy.Instance;
            var connector = new SsrfConnector(guardOptions, addressPolicy);

            var socketsHandler = new SocketsHttpHandler
            {
                UseProxy = false,
                Proxy = null,
                AllowAutoRedirect = false,
                UseCookies = false,
                ConnectCallback = connector.ConnectAsync,
            };
            return new SsrfPrimaryHandler(socketsHandler, ResolveRequestPolicy(serviceProvider, guardOptions));
        });

        builder.AddHttpMessageHandler(serviceProvider =>
        {
            SsrfGuardOptions guardOptions = serviceProvider.GetRequiredService<IOptionsMonitor<SsrfGuardOptions>>().Get(name);
            return new SsrfGuardHandler(guardOptions, ResolveRequestPolicy(serviceProvider, guardOptions));
        });

        return builder;
    }

    private static IOutboundRequestPolicy ResolveRequestPolicy(IServiceProvider serviceProvider, SsrfGuardOptions options) =>
        options.RequestPolicy
            ?? serviceProvider.GetService<IOutboundRequestPolicy>()
            ?? new DefaultOutboundRequestPolicy(options);
}
#endif
