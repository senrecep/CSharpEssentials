using System.Collections.ObjectModel;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// A hand-written domain event list: <see cref="Snapshot"/> copies to an array (no copy when empty), <see cref="View"/> is
/// a cached live read-only view that never copies.
/// </summary>
public sealed class HandWrittenEventHolder
{
    private readonly List<IDomainEvent> _events = [];

    public HandWrittenEventHolder() => View = _events.AsReadOnly();

    public IReadOnlyList<IDomainEvent> Snapshot => _events.Count == 0 ? [] : _events.ToArray();

    public ReadOnlyCollection<IDomainEvent> View { get; }

    public void Raise(IDomainEvent domainEvent) => _events.Add(domainEvent);

    public void Clear() => _events.Clear();
}
