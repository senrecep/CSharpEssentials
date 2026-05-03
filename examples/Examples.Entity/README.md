# CSharpEssentials.Entity Example

This console application demonstrates entity base classes from `CSharpEssentials.Entity`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **EntityBase** | Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) |
| **SoftDeletableEntityBase** | Soft delete with `IsDeleted`, `DeletedAt`, `Restore()` |
| **EntityBase<TId>** | Typed identifier support |
| **Domain Events** | Raise and clear domain events for DDD patterns |
| **Domain Event Timing** | `[DomainEventTiming]` attribute for BeforeSave / AfterSave control |

## Running

```bash
cd examples/Examples.Entity
dotnet run
```
