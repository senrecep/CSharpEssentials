using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Intercepts SaveChanges to automatically populate audit fields and handle soft deletes.
/// <list type="bullet">
///   <item><see cref="EntityState.Added"/> — sets <see cref="ICreationAudit.SetCreatedInfo"/>.</item>
///   <item><see cref="EntityState.Modified"/> — sets <see cref="IModificationAudit.SetUpdatedInfo"/>.</item>
///   <item><see cref="EntityState.Deleted"/> on <see cref="ISoftDeletable"/> (non-hard) — converts to soft delete.</item>
/// </list>
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

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAudit creationAudit)
                creationAudit.SetCreatedInfo(now, userId);
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudit modificationAudit)
                modificationAudit.SetUpdatedInfo(now, userId);
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable && !softDeletable.IsHardDeleted)
            {
                softDeletable.MarkAsDeleted(now, userId);
                entry.State = EntityState.Modified;
            }
        }
    }
}
