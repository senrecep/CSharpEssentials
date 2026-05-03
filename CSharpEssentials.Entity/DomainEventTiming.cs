namespace CSharpEssentials.Entity;

/// <summary>
/// Specifies when a domain event should be published relative to the database save operation.
/// </summary>
public enum DomainEventTiming
{
    /// <summary>
    /// Event is published after the database transaction commits successfully.
    /// This is the default and safest option — guarantees data is persisted before
    /// handlers run. Use for notifications, projections, and integrations.
    /// </summary>
    AfterSave = 0,

    /// <summary>
    /// Event is published before the database transaction commits.
    /// Use for validation, enrichment, or side effects that must participate
    /// in the same transaction. If a handler throws, the save is aborted.
    /// </summary>
    BeforeSave = 1
}
