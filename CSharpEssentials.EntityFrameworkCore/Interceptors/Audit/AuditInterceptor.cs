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
/// <para>
/// <see cref="IAuditUserIdProvider"/> and the clock are read once per save, and only when the save has an entry to
/// stamp; a save with no auditable entries creates no DI scope and does not call the provider.
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
        (DateTimeOffset Now, string UserId)? stamp = null;
        List<EntityEntry> entries = [.. context.ChangeTracker.Entries()];
        List<EntityEntry> softDeleted = [];
        foreach (EntityEntry entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAudit creationAudit)
            {
                stamp ??= CreateStamp();
                creationAudit.SetCreatedInfo(stamp.Value.Now, stamp.Value.UserId);
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudit modificationAudit)
            {
                stamp ??= CreateStamp();
                modificationAudit.SetUpdatedInfo(stamp.Value.Now, stamp.Value.UserId);
            }
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable && !softDeletable.IsHardDeleted)
            {
                stamp ??= CreateStamp();
                softDeletable.MarkAsDeleted(stamp.Value.Now, stamp.Value.UserId);
                entry.State = EntityState.Modified;
                softDeleted.Add(entry);
            }
        }

        if (softDeleted.Count > 0)
            RestoreCascadedDependents(entries, softDeleted);
    }

    private (DateTimeOffset Now, string UserId) CreateStamp()
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        string userId = scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>().GetCurrentUserId();
        return (timeProvider.GetUtcNow(), userId);
    }

    private static void RestoreCascadedDependents(List<EntityEntry> entries, List<EntityEntry> softDeleted)
    {
        Dictionary<(IKey Key, object?[] Values), EntityEntry> entriesByKey = [with(KeyValuesComparer.Instance)];
        List<EntityEntry> pending = [];
        foreach (EntityEntry entry in entries)
        {
            foreach (IKey key in entry.Metadata.GetKeys())
            {
                if (ReadValues(entry, key.Properties) is { } values)
                    entriesByKey.TryAdd((key, values), entry);
            }

            if (entry.State == EntityState.Deleted && entry.Entity is not ISoftDeletable { IsHardDeleted: true })
                pending.Add(entry);
        }

        HashSet<EntityEntry> kept = [with(ReferenceEqualityComparer.Instance), .. softDeleted];
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                EntityEntry entry = pending[i];
                if (!IsCascadedFromKept(entry, entriesByKey, kept))
                    continue;

                entry.State = EntityState.Unchanged;
                entry.DetectChanges();
                kept.Add(entry);
                pending.RemoveAt(i);
                changed = true;
            }
        }
    }

    private static bool IsCascadedFromKept(
        EntityEntry dependent,
        Dictionary<(IKey Key, object?[] Values), EntityEntry> entriesByKey,
        HashSet<EntityEntry> kept)
    {
        foreach (IForeignKey foreignKey in dependent.Metadata.GetForeignKeys())
        {
            if (!foreignKey.IsOwnership
                && foreignKey.DeleteBehavior is not (DeleteBehavior.Cascade or DeleteBehavior.ClientCascade))
                continue;

            if (ReadValues(dependent, foreignKey.Properties) is { } values
                && entriesByKey.TryGetValue((foreignKey.PrincipalKey, values), out EntityEntry? principal)
                && kept.Contains(principal))
                return true;
        }

        return false;
    }

    private static object?[]? ReadValues(EntityEntry entry, IReadOnlyList<IProperty> properties)
    {
        object?[] values = new object?[properties.Count];
        for (int i = 0; i < properties.Count; i++)
        {
            values[i] = entry.Property(properties[i]).CurrentValue;
            if (values[i] is null)
                return null;
        }

        return values;
    }

    private sealed class KeyValuesComparer : IEqualityComparer<(IKey Key, object?[] Values)>
    {
        public static readonly KeyValuesComparer Instance = new();

        public bool Equals((IKey Key, object?[] Values) x, (IKey Key, object?[] Values) y)
        {
            if (!ReferenceEquals(x.Key, y.Key))
                return false;

            for (int i = 0; i < x.Values.Length; i++)
            {
                if (!x.Key.Properties[i].GetKeyValueComparer().Equals(x.Values[i], y.Values[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode((IKey Key, object?[] Values) obj)
        {
            HashCode hash = new();
            hash.Add(obj.Key);
            for (int i = 0; i < obj.Values.Length; i++)
                hash.Add(obj.Key.Properties[i].GetKeyValueComparer().GetHashCode(obj.Values[i]));

            return hash.ToHashCode();
        }
    }
}
