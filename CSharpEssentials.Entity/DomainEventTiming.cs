namespace CSharpEssentials.Entity;

/// <summary>
/// Specifies when a domain event should be published relative to the database save operation.
/// </summary>
public enum DomainEventTiming
{
    /// <summary>
    /// Event is dispatched after the <see cref="BeforeSave"/> events. This is the default.
    /// Use it for notifications, projections and integrations. Whether handlers run before or after the
    /// data is written depends on the dispatcher: <c>BaseDbContext.DispatchDomainEventsOnSaveChanges</c>
    /// dispatches after the save succeeds, <c>DomainEventInterceptor</c> before the changes are written.
    /// </summary>
    AfterSave = 0,

    /// <summary>
    /// Event is published before the database transaction commits.
    /// Use it for validation, enrichment or side effects that must take part
    /// in the same transaction. If a handler throws, the save is aborted.
    /// </summary>
    BeforeSave = 1
}
