using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="DbContextOptionsBuilder"/> to configure CQRS contexts.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the DbContext as a write context with full change tracking.
    /// </summary>
    public static DbContextOptionsBuilder UseAsWriteContext(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }

    /// <summary>
    /// Configures the DbContext as a read-only context with no change tracking.
    /// </summary>
    public static DbContextOptionsBuilder UseAsReadContext(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Configures the DbContext as a read-only context with no change tracking and no tracking for identity resolution.
    /// </summary>
    public static DbContextOptionsBuilder UseAsReadContextWithIdentityResolution(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    }
}
