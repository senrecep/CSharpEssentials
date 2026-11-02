using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Intercepts SaveChanges to automatically populate audit fields and handle soft deletes.
/// <list type="bullet">
///   <item><see cref="EntityState.Added"/>: sets <see cref="ICreationAudit.SetCreatedInfo"/>.</item>
///   <item><see cref="EntityState.Modified"/>: sets <see cref="IModificationAudit.SetUpdatedInfo"/>.</item>
///   <item><see cref="EntityState.Deleted"/> on <see cref="ISoftDeletable"/> (non-hard), converts to soft delete.</item>
/// </list>
/// <para>
/// When an entity is soft-deleted, tracked entries that EF already cascade-deleted with it (owned types and
/// dependents of a <see cref="DeleteBehavior.Cascade"/> or <see cref="DeleteBehavior.ClientCascade"/> relationship)
/// are kept as well: an <see cref="ISoftDeletable"/> dependent is soft-deleted, any other is set back to
/// <see cref="EntityState.Unchanged"/>. A dependent marked with <see cref="ISoftDeletable.MarkAsHardDeleted"/> is
/// still deleted. A dependent removed explicitly in the same save cannot be told apart from a cascaded one and is
/// kept too; save that removal separately.
/// </para>
/// </summary>
public sealed class AuditInterceptor(
    TimeProvider timeProvider,
    IServiceScopeFactory serviceScopeFactory) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            UpdateAuditableEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            UpdateAuditableEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IAuditUserIdProvider userProvider = scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>();
        string userId = userProvider.GetCurrentUserId();
        DateTimeOffset now = timeProvider.GetUtcNow();

        List<EntityEntry> entries = [.. context.ChangeTracker.Entries()];
        List<EntityEntry> softDeleted = [];
        foreach (EntityEntry entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAudit creationAudit)
                creationAudit.SetCreatedInfo(now, userId);
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudit modificationAudit)
                modificationAudit.SetUpdatedInfo(now, userId);
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable && !softDeletable.IsHardDeleted)
            {
                softDeletable.MarkAsDeleted(now, userId);
                entry.State = EntityState.Modified;
                softDeleted.Add(entry);
            }
        }

        if (softDeleted.Count > 0)
            RestoreCascadedDependents(entries, softDeleted);
    }

    private static void RestoreCascadedDependents(
        List<EntityEntry> entries, List<EntityEntry> kept)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (EntityEntry entry in entries)
            {
                if (entry.State != EntityState.Deleted
                    || entry.Entity is ISoftDeletable { IsHardDeleted: true }
                    || !IsCascadedFromKept(entry, kept))
                    continue;

                entry.State = EntityState.Unchanged;
                entry.DetectChanges();
                kept.Add(entry);
                changed = true;
            }
        }
    }

    private static bool IsCascadedFromKept(EntityEntry dependent, List<EntityEntry> kept)
    {
        foreach (IForeignKey foreignKey in dependent.Metadata.GetForeignKeys())
        {
            if (!foreignKey.IsOwnership
                && foreignKey.DeleteBehavior is not (DeleteBehavior.Cascade or DeleteBehavior.ClientCascade))
                continue;

            foreach (EntityEntry principal in kept)
            {
                if (foreignKey.PrincipalEntityType.IsAssignableFrom(principal.Metadata)
                    && KeysMatch(foreignKey, dependent, principal))
                    return true;
            }
        }

        return false;
    }

    private static bool KeysMatch(IForeignKey foreignKey, EntityEntry dependent, EntityEntry principal)
    {
        for (int i = 0; i < foreignKey.Properties.Count; i++)
        {
            object? dependentValue = dependent.Property(foreignKey.Properties[i].Name).CurrentValue;
            object? principalValue = principal.Property(foreignKey.PrincipalKey.Properties[i].Name).CurrentValue;
            if (dependentValue is null || !dependentValue.Equals(principalValue))
                return false;
        }

        return true;
    }
}
