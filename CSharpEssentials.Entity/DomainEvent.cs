using CSharpEssentials.Core;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;

/// <summary>
/// Base record for domain events with an identity: <see cref="EventId"/> is a version 7 GUID stamped with
/// <see cref="OccurredAt"/>, so ids sort by creation time. Both properties are <c>init</c>, so a serializer such as
/// System.Text.Json restores the original values instead of making new ones.
/// </summary>
/// <example>
/// <code>
/// public sealed record OrderPlaced(Guid OrderId) : DomainEvent;
/// public sealed record OrderShipped(Guid OrderId, TimeProvider Clock) : DomainEvent(Clock);
/// </code>
/// </example>
public abstract record DomainEvent : IIdentifiedDomainEvent
{
    /// <summary>Stamps the event with <see cref="TimeProvider.System"/>.</summary>
    protected DomainEvent()
        : this(TimeProvider.System)
    {
    }

    /// <summary>Stamps the event with the current time of <paramref name="timeProvider"/>; a fake clock gives a deterministic <see cref="OccurredAt"/>.</summary>
    /// <param name="timeProvider">The clock to read.</param>
    /// <exception cref="ArgumentNullException"><paramref name="timeProvider"/> is <see langword="null"/>.</exception>
    protected DomainEvent(TimeProvider timeProvider)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(timeProvider);
#else
        if (timeProvider is null)
            throw new ArgumentNullException(nameof(timeProvider));
#endif

        OccurredAt = timeProvider.GetUtcNow();
        EventId = Guider.NewGuid(OccurredAt);
    }

    /// <inheritdoc />
    public Guid EventId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; init; }
}
