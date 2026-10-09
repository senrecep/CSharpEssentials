# CSharpEssentials.Entity Example

This console application demonstrates entity base classes from `CSharpEssentials.Entity`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **EntityBase** | Audit fields (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) set with `SetCreatedInfo` / `SetUpdatedInfo` (`User`, `Order`) |
| **SoftDeletableEntityBase** | Soft delete with `MarkAsDeleted`, `IsDeleted`, `DeletedAt`, `DeletedBy` and `Restore()` (`Product`) |
| **Hard Delete** | `MarkAsHardDeleted()` sets `IsHardDeleted` |
| **`EntityBase<TId>`** | Typed identifier: `Category : EntityBase<Guid>` sets `Id` in its constructor (the setter is protected) |
| **Domain Events** | `Raise`, `DomainEvents` and `ClearDomainEvents` with an `IDomainEvent` record (`OrderCreatedEvent`) |
| **Interfaces** | `ICreationAudit`, `IModificationAudit` and `ISoftDeletable` used to write code against any entity |

The example does not use `[DomainEventTiming]`; see the [package README](../../CSharpEssentials.Entity/Readme.MD) for `BeforeSave` / `AfterSave`, which `CSharpEssentials.EntityFrameworkCore` applies.

## Running

```bash
cd examples/Examples.Entity
dotnet run
```
