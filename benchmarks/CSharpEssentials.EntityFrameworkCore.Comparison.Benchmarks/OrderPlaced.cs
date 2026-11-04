using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The domain event raised by <see cref="AuditedOrder"/> in the domain event rows.</summary>
public sealed record OrderPlaced(int Ordinal) : IDomainEvent;
