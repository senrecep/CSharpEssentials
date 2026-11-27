using CSharpEssentials.EntityFrameworkCore.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Hosting;

public static class OutboxHostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="OutboxProcessorHostedService"/>. Call <c>AddOutboxProcessor&lt;TContext&gt;()</c> first: it
    /// registers the <see cref="IOutboxProcessor"/> and the <see cref="OutboxProcessorOptions"/> the service reads.
    /// Each application instance runs its own loop; the claimer keeps instances from processing the same row.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddOutboxProcessorHostedService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHostedService<OutboxProcessorHostedService>();
    }
}
